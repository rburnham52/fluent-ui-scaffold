# Browser Interactions

## Overview

FluentUIScaffold uses a deferred execution chain pattern where page objects queue browser actions that execute when the chain is awaited. Interactions are performed using `Enqueue<IPage>`, which provides direct access to Playwright's `IPage` API within a DI-injected lambda.

There is no element abstraction layer, no `ElementBuilder`, and no `WaitStrategy` enum. Pages extend `Page<TSelf>` and use Playwright's native API directly inside `Enqueue` lambdas. The framework's value is hosting orchestration, structured page objects, and fluent chaining -- not wrapping Playwright.

## Core Concept: Enqueue\<T>

The `Enqueue<T>(Func<T, Task>)` method on `Page<TSelf>` queues a deferred action. The type parameter `T` is resolved from the session's DI container at execution time (not at enqueue time). For browser interactions, `T` is Playwright's `IPage`.

```csharp
public HomePage ClickButton() => Enqueue<IPage>(async page =>
{
    await page.ClickAsync("[data-testid='submit']").ConfigureAwait(false);
});
```

Actions execute only when the chain is awaited via the custom `GetAwaiter()` on `Page<TSelf>`. Until then, they are just queued.

```csharp
// Actions are queued here...
var chain = app.NavigateTo<HomePage>().ClickButton();

// ...and execute here when awaited
await chain;

// Or more commonly, in a single line:
await app.NavigateTo<HomePage>().ClickButton();
```

## Basic Interactions

### Clicking Elements

```csharp
public HomePage ClickSubmit() => Enqueue<IPage>(async page =>
{
    await page.ClickAsync("[data-testid='submit']").ConfigureAwait(false);
});

public HomePage ClickLoginButton() => Enqueue<IPage>(async page =>
{
    await page.GetByRole(AriaRole.Button, new() { Name = "Login" })
        .ClickAsync().ConfigureAwait(false);
});
```

### Filling Text Fields

```csharp
public LoginPage EnterEmail(string email) => Enqueue<IPage>(async page =>
{
    await page.FillAsync("[data-testid='email-input']", email).ConfigureAwait(false);
});

public LoginPage EnterSearch(string text) => Enqueue<IPage>(async page =>
{
    await page.GetByPlaceholder("Search...").FillAsync(text).ConfigureAwait(false);
});
```

### Selecting Options

```csharp
public TodosPage SelectPriority(string priority) => Enqueue<IPage>(async page =>
{
    await page.SelectOptionAsync("[data-testid='priority-select']", priority)
        .ConfigureAwait(false);
});
```

### Combining Multiple Actions

A single `Enqueue` lambda can contain multiple steps when they logically belong together:

```csharp
public TodosPage AddTodo(string text, string priority = "medium") => Enqueue<IPage>(async page =>
{
    await page.FillAsync("[data-testid='new-todo-input']", text).ConfigureAwait(false);
    await page.SelectOptionAsync("[data-testid='priority-select']", priority).ConfigureAwait(false);
    await page.ClickAsync("[data-testid='add-todo-btn']").ConfigureAwait(false);
});
```

## Waiting for Elements

### Auto-Waiting

Playwright actions like `ClickAsync`, `FillAsync`, and `SelectOptionAsync` automatically wait for elements to be actionable. No additional wait configuration is needed in most cases.

### Explicit Waits with WaitForAsync

For elements that appear dynamically, use Playwright's `WaitForAsync` on a locator:

```csharp
public HomePage VerifyWelcomeVisible() => Enqueue<IPage>(async page =>
{
    await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
});

public MyPage WaitForLoadingToFinish() => Enqueue<IPage>(async page =>
{
    await page.Locator(".loading-spinner")
        .WaitForAsync(new() { State = WaitForSelectorState.Hidden })
        .ConfigureAwait(false);
});
```

### Playwright Expect Assertions

Use `Assertions.Expect()` for assertions that auto-retry until a timeout:

```csharp
public MyPage VerifyVisible(string testId) => Enqueue<IPage>(async page =>
{
    await Assertions.Expect(page.GetByTestId(testId))
        .ToBeVisibleAsync().ConfigureAwait(false);
});

public MyPage VerifyText(string testId, string expected) => Enqueue<IPage>(async page =>
{
    await Assertions.Expect(page.GetByTestId(testId))
        .ToHaveTextAsync(expected).ConfigureAwait(false);
});

public MyPage VerifyUrl(string expected) => Enqueue<IPage>(async page =>
{
    await Assertions.Expect(page).ToHaveURLAsync(expected).ConfigureAwait(false);
});
```

### Wait for Load State

```csharp
public MyPage WaitForDOMReady() => Enqueue<IPage>(async page =>
{
    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded).ConfigureAwait(false);
});
```

Prefer `DOMContentLoaded` over `NetworkIdle` -- it saves 500ms+ per navigation and is sufficient for most SPA interactions.

## Getting Text and Attribute Values

Read values from the page within an `Enqueue` lambda and use them for assertions or further logic:

```csharp
public HomePage VerifyTitle(string expectedText) => Enqueue<IPage>(async page =>
{
    var title = await page.TitleAsync().ConfigureAwait(false);
    if (!title.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
        throw new Exception($"Expected title to contain '{expectedText}' but was '{title}'.");
});

public MyPage VerifyAttribute(string testId, string attr, string expected) => Enqueue<IPage>(async page =>
{
    var value = await page.GetByTestId(testId)
        .GetAttributeAsync(attr).ConfigureAwait(false);
    Assert.AreEqual(expected, value);
});

public MyPage VerifyInputValue(string testId, string expected) => Enqueue<IPage>(async page =>
{
    var value = await page.GetByTestId(testId)
        .InputValueAsync().ConfigureAwait(false);
    Assert.AreEqual(expected, value);
});
```

## Custom DI Services via Enqueue\<TService>

`Enqueue<T>` is not limited to `IPage`. Any service registered in the session or root DI container can be injected.

### IBrowserContext

```csharp
public MyPage ClearCookies() => Enqueue<IBrowserContext>(async context =>
{
    await context.ClearCookiesAsync().ConfigureAwait(false);
});
```

### FluentUIScaffoldOptions

```csharp
public MyPage NavigateToSpecialPage() => Enqueue<IPage>(async page =>
{
    // Access options from the session provider
    var options = ServiceProvider.GetRequiredService<FluentUIScaffoldOptions>();
    var url = options.BaseUrl + "/special-page";
    await page.GotoAsync(url).ConfigureAwait(false);
});
```

### Multiple Services

For actions requiring multiple services, use the parameterless `Enqueue` and resolve from `ServiceProvider` directly:

```csharp
public MyPage ComplexAction() => Enqueue(async () =>
{
    var page = ServiceProvider.GetRequiredService<IPage>();
    var options = ServiceProvider.GetRequiredService<FluentUIScaffoldOptions>();

    await page.GotoAsync(options.BaseUrl + "/dashboard").ConfigureAwait(false);
});
```

Only two `Enqueue` overloads exist: `Enqueue(Func<Task>)` (no DI) and `Enqueue<T>(Func<T, Task>)` (one service). This covers the vast majority of cases. Use `ServiceProvider` directly when you need two or more services.

## Complete Page Object Example

```csharp
using FluentUIScaffold.Core.Pages;
using Microsoft.Playwright;

[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    protected LoginPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public LoginPage EnterEmail(string email) => Enqueue<IPage>(async page =>
    {
        await page.FillAsync("[data-testid='email-input']", email).ConfigureAwait(false);
    });

    public LoginPage EnterPassword(string password) => Enqueue<IPage>(async page =>
    {
        await page.FillAsync("[data-testid='password-input']", password).ConfigureAwait(false);
    });

    public LoginPage Submit() => Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("[data-testid='login-submit']").ConfigureAwait(false);
    });

    public LoginPage VerifyErrorMessage(string expected) => Enqueue<IPage>(async page =>
    {
        await Assertions.Expect(page.GetByTestId("error-message"))
            .ToHaveTextAsync(expected).ConfigureAwait(false);
    });

    public DashboardPage LoginAs(string email, string password)
    {
        return EnterEmail(email)
            .EnterPassword(password)
            .Submit()
            .NavigateTo<DashboardPage>();
    }
}
```

## Fluent Chaining

All `Enqueue` methods return `TSelf`, enabling fluent chaining. The entire chain executes sequentially when awaited:

```csharp
await app.NavigateTo<LoginPage>()
    .EnterEmail("admin@example.com")
    .EnterPassword("secret")
    .Submit();
```

### Cross-Page Chaining

`NavigateTo<TTarget>()` freezes the current page and returns a new page that shares the same action queue. Subsequent calls enqueue on the target page:

```csharp
await app.NavigateTo<LoginPage>()
    .EnterEmail("admin@example.com")
    .EnterPassword("secret")
    .LoginAs("admin@example.com", "secret")  // Returns DashboardPage
    .VerifyWelcomeText("Hello, Admin");       // Executes on DashboardPage
```

Once a page is frozen (after `NavigateTo<T>()` is called on it), any attempt to enqueue on it throws `FrozenPageException`.

## Best Practices

### 1. Use data-testid Selectors

Selectors based on `data-testid` attributes are stable across refactors and decoupled from presentation:

```csharp
// Preferred: stable, decoupled from CSS and layout
public MyPage ClickSubmit() => Enqueue<IPage>(async page =>
{
    await page.ClickAsync("[data-testid='submit-button']").ConfigureAwait(false);
});

// Avoid: brittle, tied to CSS structure
public MyPage ClickSubmit() => Enqueue<IPage>(async page =>
{
    await page.ClickAsync("div.form > button.btn-primary:last-child").ConfigureAwait(false);
});
```

### 2. Always Use ConfigureAwait(false)

All `await` calls inside `Enqueue` lambdas should use `.ConfigureAwait(false)`. The framework's internal execution uses `ConfigureAwait(false)` throughout; your lambdas should follow the same pattern:

```csharp
public MyPage DoSomething() => Enqueue<IPage>(async page =>
{
    await page.ClickAsync("[data-testid='btn']").ConfigureAwait(false);
    await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded).ConfigureAwait(false);
});
```

### 3. Keep Methods Small and Focused

Each page method should do one logical thing. Compose via fluent chaining:

```csharp
// Good: each method does one thing
public MyPage EnterEmail(string email) => Enqueue<IPage>(async page =>
{
    await page.FillAsync("[data-testid='email']", email).ConfigureAwait(false);
});

public MyPage ClickSubmit() => Enqueue<IPage>(async page =>
{
    await page.ClickAsync("[data-testid='submit']").ConfigureAwait(false);
});

// Compose via chaining
public MyPage SubmitEmail(string email) =>
    EnterEmail(email).ClickSubmit();
```

### 4. Always Await the Chain

Forgetting `await` means your actions are queued but never executed. In DEBUG builds, a finalizer warning will be emitted if a chain with queued actions is garbage collected without being awaited:

```csharp
// Correct: chain is awaited, actions execute
await app.NavigateTo<HomePage>().ClickButton();

// Wrong: actions are queued but never execute!
app.NavigateTo<HomePage>().ClickButton(); // Missing await
```

### 5. Use Playwright Assertions Over Manual Checks

Playwright's `Assertions.Expect()` auto-retries until timeout, making tests more resilient to timing issues:

```csharp
// Preferred: auto-retries until element has expected text
public MyPage VerifyGreeting(string name) => Enqueue<IPage>(async page =>
{
    await Assertions.Expect(page.GetByTestId("greeting"))
        .ToHaveTextAsync($"Hello, {name}").ConfigureAwait(false);
});

// Less resilient: single-shot check that can be flaky
public MyPage VerifyGreeting(string name) => Enqueue<IPage>(async page =>
{
    var text = await page.GetByTestId("greeting").TextContentAsync().ConfigureAwait(false);
    Assert.AreEqual($"Hello, {name}", text);
});
```

## Error Handling

Exceptions inside `Enqueue` lambdas propagate to the `await` site. The chain uses fail-fast behavior: the first exception stops execution and subsequent actions are skipped.

```csharp
public MyPage ClickWithFallback() => Enqueue<IPage>(async page =>
{
    try
    {
        await page.ClickAsync("[data-testid='primary-btn']", new() { Timeout = 5000 })
            .ConfigureAwait(false);
    }
    catch (TimeoutException)
    {
        await page.ClickAsync("[data-testid='secondary-btn']").ConfigureAwait(false);
    }
});
```

If `Enqueue<T>` cannot resolve the requested service from the session provider, it throws an `InvalidOperationException` at execution time with a message identifying both the service type and the page type.

## Advanced Interactions

### Keyboard and Mouse

```csharp
public MyPage PressEnter() => Enqueue<IPage>(async page =>
{
    await page.Keyboard.PressAsync("Enter").ConfigureAwait(false);
});

public MyPage TypeSlowly(string text) => Enqueue<IPage>(async page =>
{
    await page.GetByTestId("username")
        .PressSequentiallyAsync(text).ConfigureAwait(false);
});

public MyPage HoverOverMenu() => Enqueue<IPage>(async page =>
{
    await page.HoverAsync("[data-testid='menu-trigger']").ConfigureAwait(false);
});
```

### JavaScript Execution

```csharp
public MyPage ScrollToBottom() => Enqueue<IPage>(async page =>
{
    await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)")
        .ConfigureAwait(false);
});

public MyPage ClearLocalStorage() => Enqueue<IPage>(async page =>
{
    await page.EvaluateAsync("localStorage.clear()").ConfigureAwait(false);
});
```

### Screenshots

```csharp
public MyPage TakeScreenshot(string path) => Enqueue<IPage>(async page =>
{
    await page.ScreenshotAsync(new PageScreenshotOptions { Path = path })
        .ConfigureAwait(false);
});
```

For more information, see the [API Reference](api-reference.md) and [Playwright Integration](playwright-integration.md) guides.
