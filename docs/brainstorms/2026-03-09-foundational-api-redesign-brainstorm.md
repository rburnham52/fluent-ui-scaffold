# Foundational API Redesign

**Date:** 2026-03-09
**Status:** Brainstorm
**Supersedes:** [future-enhancement-framework-escape-hatch.md](../future-enhancement-framework-escape-hatch.md)

## What We're Building

A ground-up redesign of FluentUIScaffold's core API that:

1. **Removes the `IUIDriver` / `Element` / `IElement` abstraction layer** — page objects interact with the underlying framework (Playwright) directly
2. **Makes `Page<TSelf>` an async deferred execution chain builder** — page methods queue actions, `GetAwaiter()` executes them
3. **Injects framework services into action lambdas** via `Enqueue<T>()` generic overloads with DI resolution
4. **Separates browser lifecycle from sessions** — plugin owns the browser (singleton), a factory creates isolated sessions (per-test context + page)
5. **Keeps Core framework-agnostic** — `Page<TSelf>` and `Enqueue()` live in Core with no Playwright dependency. Framework coupling is in user-written page objects and the plugin package.

## Why This Approach

The current API reimplements ~19 methods on `IUIDriver` that mirror what Playwright already provides. This creates:
- Maintenance overhead without value (the fluent Page layer is what tests actually use)
- Sync-over-async throughout (`PlaywrightDriver` blocks on every async Playwright call)
- Three disconnected wait systems (`Element.WaitFor()`, `PlaywrightWaitStrategy`, `VerificationContext` polling)
- Inconsistent delegation (some `Page` methods go through `IElement`, others bypass to `IUIDriver` directly)
- Dead code (`PluginManager`, `IUIDriver.NavigateTo<T>()`, most wait strategy implementations)

The framework's real value is **hosting orchestration + structured page objects + fluent chaining**, not wrapping Playwright's API. The new design doubles down on that value.

## Key Decisions

### 1. Page IS the chain builder (async deferred execution)

`Page<TSelf>` itself is the awaitable chain builder — no separate `PageActions` wrapper type. Page methods return `TSelf`, queue actions internally, and `Page<TSelf>` implements `GetAwaiter()` to execute all queued actions when awaited:

```csharp
await app.NavigateTo<ProductsPage>()
    .SearchFor("laptop")           // queues action, returns ProductsPage
    .FilterByCategory("Electronics") // queues action, returns ProductsPage
    .AddProductToCart(1);           // queues action, await executes all 3
```

Pages use a shared internal action queue. `NavigateTo<TTarget>()` creates the target page with the same queue reference, then **freezes** the current page — any subsequent calls on the old page throw a clear error. This prevents stale reference bugs:

```csharp
// Correct: continuous chain
await app.NavigateTo<HomePage>()
    .SearchFor("laptop")               // HomePage method
    .NavigateTo<SearchResultsPage>()    // freezes HomePage, returns SearchResultsPage
    .VerifyResultCount(5);              // SearchResultsPage method

// Error: using frozen page
var home = app.NavigateTo<HomePage>();
var results = home.NavigateTo<SearchResultsPage>();
home.SearchFor("laptop"); // throws: "Page frozen after NavigateTo<SearchResultsPage>()"
```

Navigation and service resolution are deferred — `app.NavigateTo<T>()` synchronously creates the page and queues the navigation action, but actual browser navigation happens when the chain is awaited.

### 2. Enqueue<T> with DI injection into lambdas

Page methods use `Enqueue<T>()` generic overloads that resolve services from DI at execution time:

```csharp
[Route("/products")]
public class ProductsPage : Page<ProductsPage>
{
    // IPage resolved from DI at execution time
    public ProductsPage SearchFor(string query)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByPlaceholder("Search...").FillAsync(query);
            await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        });

    // Multiple services injected
    public ProductsPage AddToCart(string productId)
        => Enqueue(async (IPage page, ILogger<ProductsPage> logger) =>
        {
            logger.LogInformation("Adding {Id} to cart", productId);
            await page.ClickAsync($"[data-id='{productId}'] .add-to-cart");
        });
}
```

Core provides overloads: `Enqueue(Func<Task>)`, `Enqueue<T1>(Func<T1, Task>)`, `Enqueue<T1,T2>(Func<T1,T2,Task>)`, etc. Core doesn't know what types will be injected — it just resolves them from `IServiceProvider`.

This guarantees all interactions happen inside deferred execution (no accidental side effects in constructors or property getters).

### 3. Framework-coupled page objects (by design)

Page objects use Playwright's API directly inside `Enqueue` lambdas. This is intentional:

- **No element abstraction on the base class** — no `IElement`, `Element`, `ElementBuilder`, `WaitStrategy` enum
- **No built-in verify methods** — page authors use Playwright's `Assertions.Expect()` which has built-in auto-retry
- **Locators are defined using Playwright's native API** — `page.GetByTestId()`, `page.Locator()`, `page.GetByRole()`, etc.
- **Core stays framework-agnostic** — the coupling is in test code (page objects), not in the library

If someone switches from Playwright to Selenium, they rewrite page object internals. The page structure, routing, chaining, and hosting are the portable parts.

### 4. Plugin + BrowserSession architecture

**`IUITestingPlugin`** — singleton, owns the browser:
```csharp
public interface IUITestingPlugin : IAsyncDisposable
{
    void ConfigureServices(IServiceCollection services);
    Task InitializeAsync(UIScaffoldOptions options);
}
```

**`IBrowserSessionFactory`** — registered by plugin, creates isolated sessions:
```csharp
public interface IBrowserSessionFactory
{
    Task<IBrowserSession> CreateSessionAsync();
}
```

**`IBrowserSession`** — one per test/scope, owns context + page:
```csharp
public interface IBrowserSession : IAsyncDisposable
{
    Task NavigateToUrlAsync(Uri url);
    T GetService<T>(); // scoped DI: IPage, IBrowserContext for THIS session
}
```

For Playwright: plugin launches one `IBrowser` (shared), factory creates `IBrowserContext` + `IPage` per session. Sessions are isolated (like incognito windows), cheap to create, and independently disposable.

### 5. Chain scope switching for page-to-page navigation

`NavigateTo<TTarget>()` on a page changes the chain's type mid-flow. It freezes the current page, creates the target page with the shared action queue, and queues the navigation:

```csharp
await app.NavigateTo<HomePage>()
    .SearchFor("laptop")                // HomePage method
    .NavigateTo<SearchResultsPage>()    // freezes HomePage, returns SearchResultsPage
    .VerifyResultCount(5)               // SearchResultsPage method
    .ClickProduct(0)
    .NavigateTo<ProductDetailPage>()    // freezes SearchResultsPage, returns ProductDetailPage
    .VerifyPrice("$999");               // await executes entire chain in order
```

Internally, `NavigateTo<TTarget>()` queues: navigate to TTarget's `[Route]` URL, then returns the new page instance (resolved from DI) that shares the same action queue.

### 6. No IUIDriver

`IUIDriver` is removed entirely. Its responsibilities are redistributed:
- **Element interactions** → Playwright API directly (in Enqueue lambdas)
- **Browser lifecycle** → `IUITestingPlugin.InitializeAsync()`
- **Navigation** → `IBrowserSession.NavigateToUrlAsync()`
- **Framework access** → DI injection into Enqueue lambdas (replaces `GetFrameworkDriver<T>()`)
- **Script execution, screenshots** → Playwright's `IPage` directly

## Architecture Diagram

```
FluentUIScaffoldBuilder
    .UsePlugin(new PlaywrightPlugin())       → configures DI
    .UseAspireHosting<T>(...)                → configures hosting
    .Build<WebApp>()                         → creates AppScaffold

AppScaffold<TWebApp>
    StartAsync()          → starts hosting + plugin.InitializeAsync()
    NavigateTo<TPage>()   → factory.CreateSession() or reuse, navigate, return TPage
    DisposeAsync()        → dispose sessions, hosting, plugin

IUITestingPlugin (e.g., PlaywrightPlugin)
    ConfigureServices()   → registers IBrowserSessionFactory, shared services
    InitializeAsync()     → launches browser (singleton)

IBrowserSessionFactory → IBrowserSession
    CreateSession()       → new IBrowserContext + IPage (isolated)
    NavigateToUrlAsync()  → session-scoped navigation
    GetService<T>()       → scoped DI (IPage, IBrowserContext for this session)

Page<TSelf>  (Core — framework-agnostic, IS the chain builder)
    [Route("/path")]      → URL template with {param} placeholders
    Enqueue(Func<Task>)   → deferred action (no DI), returns TSelf
    Enqueue<T>(...)       → deferred action with DI injection, returns TSelf
    NavigateTo<T>()       → scope switch, freezes self, returns TTarget
    GetAwaiter()          → executes all queued actions sequentially
```

## What Gets Removed

| Current | Replacement |
|---------|-------------|
| `IUIDriver` (22 members) | Plugin + BrowserSession + direct DI |
| `Element` / `IElement` | Playwright's `ILocator` / `IPage` API directly |
| `ElementBuilder` | Not needed — no element abstraction |
| `WaitStrategy` enum | Playwright's built-in auto-waiting |
| `PlaywrightWaitStrategy` (unused) | Playwright's built-in auto-waiting |
| `VerificationContext<TSelf>` | Custom page methods using `Assertions.Expect()` |
| `PluginManager` (dead code) | `IUITestingPlugin` registered directly |
| `PluginRegistry` (dead code) | Direct DI registration |
| `PlaywrightAdvancedFeatures` | `IPage` / `IBrowserContext` directly via DI |
| `Page.Focus/Hover/Clear` (bypass Element) | Unified — all via Enqueue lambdas |
| `Page.ClickElement/TypeText/etc.` (bypass Element) | Unified — all via Enqueue lambdas |

## What Gets Kept / Evolved

| Current | Evolution |
|---------|-----------|
| `Page<TSelf>` (CRTP pattern) | Kept — becomes the chain builder with `Enqueue<T>()`, `GetAwaiter()`, loses element methods |
| `[Route]` attribute | Kept — with `{param}` placeholder support |
| `AppScaffold<TWebApp>` | Kept — gains session management, async navigation |
| `FluentUIScaffoldBuilder` | Kept — same builder pattern |
| `IHostingStrategy` | Kept as-is — hosting is orthogonal to this redesign |
| `FluentUIScaffoldOptions` | Simplified — remove element-related options |
| `IUITestingFrameworkPlugin` | Evolved → `IUITestingPlugin` (simpler contract) |

## Full Example: End-to-End

```csharp
// === Test Setup (AssemblyInitialize) ===

[TestClass]
public class TestSetup
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task Init(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .UseAspireHosting<Projects.MyApp_AppHost>(
                appHost => { /* configure */ },
                "myapp")
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task Cleanup() => await _app!.DisposeAsync();

    public static AppScaffold<WebApp> App => _app!;
}

// === Page Objects ===

[Route("/")]
public class HomePage : Page<HomePage>
{
    public HomePage SearchFor(string query)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByPlaceholder("Search products...").FillAsync(query);
            await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        });

    public SearchResultsPage SearchAndNavigate(string query)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByPlaceholder("Search products...").FillAsync(query);
            await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await page.WaitForURLAsync("**/search**");
        }).NavigateTo<SearchResultsPage>();

    public HomePage SelectItemFromCarousel(int index)
        => Enqueue(async (IPage page) =>
        {
            var carousel = page.Locator(".carousel");
            var targetItem = carousel.Locator($"[data-index='{index}']");

            // Use Playwright's full API — conditionals, waits, everything
            if (!await targetItem.IsVisibleAsync())
            {
                var nextBtn = carousel.GetByRole(AriaRole.Button, new() { Name = "Next" });
                while (!await targetItem.IsVisibleAsync())
                    await nextBtn.ClickAsync();
            }

            await targetItem.ClickAsync();
        });
}

[Route("/search")]
public class SearchResultsPage : Page<SearchResultsPage>
{
    public SearchResultsPage VerifyResultCount(int expected)
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.Locator(".result-count"))
                .ToHaveTextAsync($"{expected} results");
        });

    public SearchResultsPage ClickProduct(int index)
        => Enqueue(async (IPage page) =>
        {
            await page.Locator($".product-card:nth-child({index + 1})").ClickAsync();
        });
}

[Route("/users/{userId}")]
public class UserProfilePage : Page<UserProfilePage>
{
    public UserProfilePage VerifyDisplayName(string expected)
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.GetByTestId("display-name"))
                .ToHaveTextAsync(expected);
        });
}

// === Tests ===

[TestClass]
public class SearchTests
{
    [TestMethod]
    public async Task SearchForProduct_ShowsResults()
    {
        await TestSetup.App.NavigateTo<HomePage>()
            .SearchAndNavigate("laptop")
            .VerifyResultCount(5);
    }

    [TestMethod]
    public async Task CarouselSelection_NavigatesToProduct()
    {
        await TestSetup.App.NavigateTo<HomePage>()
            .SelectItemFromCarousel(3)
            .NavigateTo<ProductDetailPage>()
            .VerifyPrice("$999");
    }

    [TestMethod]
    public async Task ParameterizedRoute_NavigatesCorrectly()
    {
        await TestSetup.App.NavigateTo<UserProfilePage>(new { userId = "42" })
            .VerifyDisplayName("Jane Doe");
    }
}
```

## Resolved Questions

1. **Enqueue overload limit** — Provide up to `Enqueue<T1,T2,T3,T4>` (4 generic overloads, matching the standard `Func<>`/`Action<>` convention). For rare cases needing 5+ services, inject `IServiceProvider` and resolve manually.

2. **Session lifecycle per test** — Configurable, default per-test. Each test method gets a fresh `IBrowserSession` (new context + page) for full isolation. Opt-in to shared session per test class via attribute (e.g., `[SharedBrowserSession]`) for speed when isolation isn't needed.

3. **Chain error handling** — Fail-fast. First exception stops the chain immediately; subsequent queued actions are skipped. If login fails, don't try to click the dashboard.

4. **Return values from chains** — Closures only. Use local variables captured in lambda closures. No special `GetValue<T>()` API. If you need a value from one chain to use in another, `await` the first chain, then start a new one.

5. **Custom page base classes** — Supported naturally by the CRTP (Curiously Recurring Template Pattern). `AuthenticatedPage<TSelf> : Page<TSelf> where TSelf : AuthenticatedPage<TSelf>` works out of the box. No special design needed — just document the pattern.

6. **Chaining mechanism** — `Page<TSelf>` IS the chain builder (no separate `PageActions` type). Pages use a shared internal action queue. `NavigateTo<T>()` freezes the current page (throws on subsequent use) and returns the target page sharing the same queue. `GetAwaiter()` on the page executes all queued actions sequentially.

## Open Questions

_(None — all questions resolved during brainstorming.)_
