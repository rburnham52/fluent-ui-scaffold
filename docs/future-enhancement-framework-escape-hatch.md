# Future Enhancement: Framework Escape Hatch & IUIDriver Refactoring

## Problem Statement

`IUIDriver` currently reimplements 19 methods that mirror what Playwright (and other frameworks) already provide — Click, Type, Hover, Focus, SelectOption, etc. This creates a thin abstraction that adds maintenance overhead without much value, since the fluent `Page<TSelf>` and `Element` layers already provide the framework-agnostic API that test code actually uses.

Meanwhile, when tests need framework-specific capabilities (network interception, geolocation, device emulation), there's no clean pattern to "escape" from the fluent API into the native framework.

## Vision

1. **Slim down `IUIDriver`** to only browser-level operations that don't have fluent equivalents (navigation, script execution, screenshots, lifecycle)
2. **Add a framework escape hatch** so tests can drop into native Playwright (or Selenium, etc.) code when needed, with full DI support

## Design: Framework Escape Hatch

### Option A: Lambda-based access on AppScaffold

```csharp
// Inject any registered Playwright services into the lambda
app.Framework<PlaywrightDriver>(driver =>
{
    // Full access to Playwright's IPage, IBrowser, IBrowserContext
    var page = driver.GetFrameworkDriver<IPage>();
    await page.RouteAsync("**/api/*", route => route.FulfillAsync(new()
    {
        Body = "{\"mocked\": true}"
    }));
});

// With return value
var cookies = app.Framework<PlaywrightDriver, IReadOnlyList<BrowserContextCookiesResult>>(
    async driver =>
    {
        var context = driver.GetFrameworkDriver<IBrowserContext>();
        return await context.CookiesAsync();
    });
```

### Option B: Direct DI resolution (current pattern, enhanced)

```csharp
// Already works today via Framework<T>()
var page = app.Framework<IPage>();
await page.EvaluateAsync("localStorage.clear()");

// Could register PlaywrightAdvancedFeatures in DI
var advanced = app.Framework<PlaywrightAdvancedFeatures>();
await advanced.TakeScreenshotAsync("debug.png");
await advanced.InterceptNetworkRequests("/api/*", handler);
```

### Option C: Page-level escape hatch

```csharp
// From within a Page object, access the native framework
public class MyPage : Page<MyPage>
{
    public async Task ClearStorageAsync()
    {
        // Escape hatch from within page context
        await UseFramework<IPage>(async page =>
        {
            await page.EvaluateAsync("localStorage.clear()");
        });
    }
}
```

### Recommendation

**Option B (enhanced)** is the simplest path — register `PlaywrightAdvancedFeatures` in DI via `PlaywrightPlugin.ConfigureServices()`, and tests access it via `app.Framework<PlaywrightAdvancedFeatures>()`. No new API surface needed.

**Option C** is valuable longer-term for keeping framework access scoped to page objects rather than scattered in test methods.

## Design: Slimming IUIDriver

### Current IUIDriver (19 methods + 1 property)

```
Navigation:     NavigateToUrl, NavigateTo<T>
Element State:  Click, Type, SelectOption, Focus, Hover, Clear,
                GetText, GetAttribute, GetValue, IsVisible, IsEnabled
Waits:          WaitForElement, WaitForElementToBeVisible, WaitForElementToBeHidden
Page-Level:     GetPageTitle, CurrentUrl
Framework:      GetFrameworkDriver<T>
```

### Proposed Minimal IUIDriver

Keep only what the `Element` and `Page<TSelf>` layers actually need, plus browser-level ops:

```csharp
public interface IUIDriver : IDisposable
{
    // Browser-level (no fluent equivalent)
    Uri? CurrentUrl { get; }
    void NavigateToUrl(Uri url);
    string GetPageTitle();
    Task ExecuteScriptAsync(string script);
    Task<T> ExecuteScriptAsync<T>(string script);
    Task TakeScreenshotAsync(string filePath);

    // Element operations (used by Element class internally)
    void Click(string selector);
    void Type(string selector, string text);
    void SelectOption(string selector, string value);
    void Clear(string selector);
    void Focus(string selector);
    void Hover(string selector);
    string GetText(string selector);
    string GetAttribute(string selector, string attributeName);
    string GetValue(string selector);
    bool IsVisible(string selector);
    bool IsEnabled(string selector);

    // Wait operations (used by Element class internally)
    void WaitForElement(string selector);
    void WaitForElementToBeVisible(string selector);
    void WaitForElementToBeHidden(string selector);

    // Framework access
    TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
}
```

**Note:** After review, the element operations are still needed because `Element` class delegates to `IUIDriver` directly. The real refactoring opportunity is to consider whether `Element` should instead hold a reference to the native framework driver directly, but that's a larger architectural change.

### Migration Path

The refactoring would be a **major version** change:

1. Add new async methods (`ExecuteScriptAsync`, `TakeScreenshotAsync`) — **non-breaking, do this now**
2. Register `PlaywrightAdvancedFeatures` in DI — **non-breaking, do this soon**
3. Add page-level `UseFramework<T>()` escape hatch — **non-breaking, do later**
4. Evaluate whether `IUIDriver` element methods can be internalized — **breaking, major version**

## Implementation Notes

### Registering PlaywrightAdvancedFeatures in DI

In `PlaywrightPlugin.ConfigureServices()`:

```csharp
services.AddSingleton<PlaywrightAdvancedFeatures>(provider =>
{
    var page = provider.GetRequiredService<IPage>();
    return new PlaywrightAdvancedFeatures(page);
});
```

### Page-level UseFramework (future)

```csharp
// In Page<TSelf> base class
protected async Task UseFramework<TFramework>(Func<TFramework, Task> action)
    where TFramework : class
{
    var framework = ServiceProvider.GetRequiredService<TFramework>();
    await action(framework);
}

protected async Task<TResult> UseFramework<TFramework, TResult>(
    Func<TFramework, Task<TResult>> action)
    where TFramework : class
{
    var framework = ServiceProvider.GetRequiredService<TFramework>();
    return await action(framework);
}
```

## Related

- Brainstorm: [2026-02-15-browser-interaction-apis-brainstorm.md](brainstorms/2026-02-15-browser-interaction-apis-brainstorm.md)
- Current `PlaywrightAdvancedFeatures`: `src/FluentUIScaffold.Playwright/PlaywrightAdvancedFeatures.cs`
- Current `IUIDriver`: `src/FluentUIScaffold.Core/Interfaces/IUIDriver.cs`
