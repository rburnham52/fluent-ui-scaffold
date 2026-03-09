# FluentUIScaffold E2E Testing Framework - API Specification

## Overview

FluentUIScaffold is a framework-agnostic E2E testing library that provides a deferred execution chain pattern for building maintainable UI test automation. Pages queue actions via `Enqueue<T>` that execute sequentially when the chain is awaited. The framework abstracts the underlying testing framework (currently Playwright) while giving tests direct access to the native API.

The framework's core value is **hosting orchestration + structured page objects + fluent deferred execution chains** -- not wrapping Playwright's API.

## Core Architecture

### Entry Points

```csharp
// Build and configure the application scaffold
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
    })
    .Build<WebApp>();

// Start hosting + initialize browser (async-first design)
await app.StartAsync();

// Per-test: create an isolated browser session
await app.CreateSessionAsync();

// Navigate and interact via awaitable page chains
await app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .ClickGetStarted();

// Per-test: dispose the session
await app.DisposeSessionAsync();

// Clean up when done (assembly teardown)
await app.DisposeAsync();
```

### Configuration Options

```csharp
public class FluentUIScaffoldOptions
{
    public Uri? BaseUrl { get; set; }
    public bool? HeadlessMode { get; set; } = null;  // null = auto (debugger/CI)
    public int? SlowMo { get; set; } = null;         // null = auto (debugger/CI)
    public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
```

## Plugin System

### Plugin Interface

```csharp
public interface IUITestingPlugin : IAsyncDisposable
{
    void ConfigureServices(IServiceCollection services);
    Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken ct = default);
    Task<IBrowserSession> CreateSessionAsync(IServiceProvider rootProvider);
}
```

The plugin owns the browser singleton, configures shared DI services, and acts as a factory for per-test browser sessions. There is no separate factory interface -- `CreateSessionAsync()` lives directly on the plugin.

### Plugin Registration

```csharp
// Explicit registration
var builder = new FluentUIScaffoldBuilder();
builder.UsePlugin(new PlaywrightPlugin());

// Convenience extension method
builder.UsePlaywright();
```

### PlaywrightPlugin

```csharp
public class PlaywrightPlugin : IUITestingPlugin
{
    // InitializeAsync: creates IPlaywright, launches IBrowser (singleton)
    // CreateSessionAsync: creates IBrowserContext + IPage (isolated per test)
    // DisposeAsync: closes browser, disposes Playwright
}
```

## Browser Sessions

### IBrowserSession

```csharp
public interface IBrowserSession : IAsyncDisposable
{
    Task NavigateToUrlAsync(Uri url);
    IServiceProvider ServiceProvider { get; }
}
```

Each session represents an isolated browser context (like an incognito window) with its own cookies, localStorage, and cache. Sessions are cheap to create (~70-130ms) and independently disposable.

### Session Service Provider

Each session has a `SessionServiceProvider` that resolves session-scoped services first, then falls back to the root provider:

| Session Service | Description |
|----------------|-------------|
| `IPage` | Playwright page for this test |
| `IBrowserContext` | Playwright browser context for this test |
| `IBrowser` | Shared Playwright browser instance |

Root services (logging, options, hosting, Aspire `DistributedApplication`) are resolved via fallback. There is no dual-provider concept -- pages receive a single `IServiceProvider` that handles both session and root resolution.

### Session Lifecycle

Sessions are created and disposed per test. `AppScaffold` tracks the current session via `AsyncLocal<IBrowserSession>` for parallel test safety.

```csharp
// In TestInitialize
await app.CreateSessionAsync();

// In TestCleanup
await app.DisposeSessionAsync();
```

## AppScaffold

### Application Orchestrator

```csharp
public class AppScaffold<TWebApp> : IAsyncDisposable
{
    public IServiceProvider ServiceProvider { get; }

    // Lifecycle
    public Task StartAsync(CancellationToken ct = default);
    public ValueTask DisposeAsync();

    // Session management (per-test)
    public Task<IBrowserSession> CreateSessionAsync();
    public Task DisposeSessionAsync();

    // Page navigation (requires active session)
    public TPage NavigateTo<TPage>() where TPage : Page<TPage>;
    public TPage NavigateTo<TPage>(object routeParams) where TPage : Page<TPage>;
    public TPage On<TPage>() where TPage : Page<TPage>;

    // Service resolution
    public T GetService<T>() where T : notnull;
}
```

- `NavigateTo<TPage>()` synchronously creates the page via `ActivatorUtilities.CreateInstance` with the session provider, enqueues navigation to the page's `[Route]` URL, and returns the page for chaining.
- `NavigateTo<TPage>()` throws `InvalidOperationException` if no session is active: `"No browser session is active. Call CreateSessionAsync() in [TestInitialize] before navigating."`
- `NavigateTo<TPage>(object routeParams)` supports parameterized routes with `{param}` placeholders.
- `On<TPage>()` resolves a page without enqueuing navigation (for asserting current page state).

## Page Base Class

### Deferred Execution Chain Builder

`Page<TSelf>` is the core abstraction. It is an awaitable chain builder -- page methods queue actions internally, and `GetAwaiter()` executes them all when the chain is awaited.

```csharp
public abstract class Page<TSelf> where TSelf : Page<TSelf>
{
    // Public constructor -- AppScaffold.NavigateTo creates initial page with fresh action list
    protected Page(IServiceProvider serviceProvider);

    // Internal constructor -- Page.NavigateTo passes shared action list for cross-page chains
    internal Page(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions);

    // Properties
    protected IServiceProvider ServiceProvider { get; }
    protected TSelf Self { get; }

    // Enqueue actions for deferred execution
    protected TSelf Enqueue(Func<Task> action);             // no DI
    protected TSelf Enqueue<T>(Func<T, Task> action);       // 1 service resolved from DI

    // Cross-page navigation (freezes current page, shares action queue)
    public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;

    // Makes the chain awaitable
    public TaskAwaiter GetAwaiter();
}
```

### Key Behaviors

- **Deferred execution:** Actions enqueued via `Enqueue()` do not execute immediately. They run sequentially when the chain is `await`ed.
- **Fluent chaining:** All `Enqueue` calls return `TSelf`, enabling method chaining.
- **DI resolution at execution time:** `Enqueue<T>()` resolves the service from `IServiceProvider` when the action runs, not when it is enqueued.
- **Shared action queue:** `NavigateTo<TTarget>()` freezes the current page and creates the target page sharing the same action list. The entire chain executes as one unit when any page in the chain is awaited.
- **Freeze semantics:** After `NavigateTo<T>()`, the source page is frozen. Any subsequent `Enqueue` or `NavigateTo` calls on a frozen page throw `FrozenPageException`.
- **Fail-fast:** The first exception stops the chain immediately; subsequent queued actions are skipped.
- **Double-await safety:** Awaiting an already-consumed chain is a no-op (consistent with `Task` behavior).
- **Unawaited chain detection:** In DEBUG builds, a finalizer warns if actions were queued but never executed.
- **ConfigureAwait:** All internal awaits use `ConfigureAwait(false)`. The `Task` returned by `GetAwaiter()` does NOT -- the caller's `SynchronizationContext` controls the final continuation.

### Two Enqueue Overloads

Only two overloads are provided. For actions needing multiple services, resolve from `IServiceProvider` directly:

```csharp
// No DI -- simple deferred action
protected TSelf Enqueue(Func<Task> action);

// 1 service -- covers the vast majority of cases
protected TSelf Enqueue<T>(Func<T, Task> action);

// For 2+ services, use the no-DI overload with manual resolution:
Enqueue(async () =>
{
    var page = ServiceProvider.GetRequiredService<IPage>();
    var logger = ServiceProvider.GetRequiredService<ILogger<MyPage>>();
    // ...
});
```

### Route Attribute

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RouteAttribute : Attribute
{
    public string Path { get; }
    public RouteAttribute(string path);
}
```

#### Static Routes

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    public LoginPage(IServiceProvider services) : base(services) { }
}

// Navigates to: BaseUrl + "/login"
await app.NavigateTo<LoginPage>();
```

#### Parameterized Routes

```csharp
[Route("/users/{userId}")]
public class UserProfilePage : Page<UserProfilePage>
{
    public UserProfilePage(IServiceProvider services) : base(services) { }

    public UserProfilePage VerifyDisplayName(string expected)
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.GetByTestId("display-name"))
                .ToHaveTextAsync(expected);
        });
}

// Navigates to: BaseUrl + "/users/42"
await app.NavigateTo<UserProfilePage>(new { userId = "42" })
    .VerifyDisplayName("Jane Doe");

// Multiple parameters
[Route("/users/{userId}/posts/{postId}")]
public class UserPostPage : Page<UserPostPage> { /* ... */ }

await app.NavigateTo<UserPostPage>(new { userId = "456", postId = "789" });
```

## Example Page Implementations

### Basic Page Object

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    public HomePage(IServiceProvider services) : base(services) { }

    public HomePage VerifyWelcomeVisible()
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.GetByTestId("welcome-message"))
                .ToBeVisibleAsync();
        });

    public HomePage ClickGetStarted()
        => Enqueue(async (IPage page) =>
        {
            await page.GetByRole(AriaRole.Link, new() { Name = "Get Started" })
                .ClickAsync();
        });

    public HomePage SearchFor(string query)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByPlaceholder("Search products...").FillAsync(query);
            await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        });

    // Cross-page navigation: search and transition to results
    public SearchResultsPage SearchAndNavigate(string query)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByPlaceholder("Search products...").FillAsync(query);
            await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
            await page.WaitForURLAsync("**/search**");
        }).NavigateTo<SearchResultsPage>();
}
```

### Page with Complex Interactions

```csharp
[Route("/")]
public class CarouselPage : Page<CarouselPage>
{
    public CarouselPage(IServiceProvider services) : base(services) { }

    public CarouselPage SelectItemFromCarousel(int index)
        => Enqueue(async (IPage page) =>
        {
            var carousel = page.Locator(".carousel");
            var targetItem = carousel.Locator($"[data-index='{index}']");

            // Use Playwright's full API -- conditionals, waits, everything
            if (!await targetItem.IsVisibleAsync())
            {
                var nextBtn = carousel.GetByRole(AriaRole.Button, new() { Name = "Next" });
                while (!await targetItem.IsVisibleAsync())
                    await nextBtn.ClickAsync();
            }

            await targetItem.ClickAsync();
        });
}
```

### Page with Verification

Verification uses Playwright's `Assertions.Expect()` directly, which provides built-in auto-retry:

```csharp
[Route("/search")]
public class SearchResultsPage : Page<SearchResultsPage>
{
    public SearchResultsPage(IServiceProvider services) : base(services) { }

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
```

### Form Interaction Page

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    public LoginPage(IServiceProvider services) : base(services) { }

    public LoginPage EnterCredentials(string username, string password)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByLabel("Username").FillAsync(username);
            await page.GetByLabel("Password").FillAsync(password);
        });

    public LoginPage Submit()
        => Enqueue(async (IPage page) =>
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        });

    public HomePage LoginAs(string username, string password)
        => EnterCredentials(username, password)
            .Submit()
            .NavigateTo<HomePage>();
}
```

### Domain-Specific Page with Composition

```csharp
[Route("/roster/details/{rosterId}")]
public class RosterDetailsPage : Page<RosterDetailsPage>
{
    public RosterDetailsPage(IServiceProvider sp) : base(sp) { }

    public RosterDetailsPage ClickAddShift() => Enqueue(async (IPage page) =>
    {
        await page.ClickAsync("[data-testid='add-shift-btn']");
    });

    public RosterDetailsPage SelectEmployee(string employeeId) => Enqueue(async (IPage page) =>
    {
        await page.SelectOptionAsync("[data-testid='employee-dropdown']", employeeId);
    });

    public RosterDetailsPage EnterStartTime(string time) => Enqueue(async (IPage page) =>
    {
        await page.FillAsync("#start-time", time);
    });

    public RosterDetailsPage EnterEndTime(string time) => Enqueue(async (IPage page) =>
    {
        await page.FillAsync("#end-time", time);
    });

    public RosterDetailsPage Save() => Enqueue(async (IPage page) =>
    {
        await page.ClickAsync("[data-testid='save-btn']");
    });

    // Compose smaller actions into domain-level operations
    public RosterDetailsPage AddShift(string employeeId, string start, string end) =>
        ClickAddShift()
            .SelectEmployee(employeeId)
            .EnterStartTime(start)
            .EnterEndTime(end)
            .Save();

    public RosterDetailsPage VerifyShiftAdded(string employeeId) => Enqueue(async (IPage page) =>
    {
        var shiftRow = page.Locator($".shift-row[data-employee-id='{employeeId}']");
        await Assertions.Expect(shiftRow).ToBeVisibleAsync();
    });

    public HomePage NavigateToHome() => NavigateTo<HomePage>();
}
```

## Cross-Page Navigation Chains

`NavigateTo<TTarget>()` on a page freezes the current page and returns the target page sharing the same action queue. The entire chain executes when awaited:

```csharp
// Continuous chain across multiple pages
await app.NavigateTo<HomePage>()
    .SearchFor("laptop")               // HomePage method
    .NavigateTo<SearchResultsPage>()   // freezes HomePage, returns SearchResultsPage
    .VerifyResultCount(5)              // SearchResultsPage method
    .ClickProduct(0)
    .NavigateTo<ProductDetailPage>()   // freezes SearchResultsPage, returns ProductDetailPage
    .VerifyPrice("$999");              // await executes entire chain in order
```

## Component Transitions

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    public HomePage(IServiceProvider sp) : base(sp) { }

    public HomePage VerifyRosterScheduleVisible() => Enqueue(async (IPage page) =>
    {
        await Assertions.Expect(page.Locator(".roster-schedule")).ToBeVisibleAsync();
    });

    public RosterDetailsPage OpenRosterDetails(int rosterId)
    {
        Enqueue(async (IPage page) =>
        {
            await page.ClickAsync($".roster-link[data-roster-id='{rosterId}']");
        });
        return NavigateTo<RosterDetailsPage>();
    }

    public ConfirmationDialog DeleteRoster(int rosterId)
    {
        Enqueue(async (IPage page) =>
        {
            await page.ClickAsync($".delete-roster[data-roster-id='{rosterId}']");
        });
        return NavigateTo<ConfirmationDialog>();
    }
}

public class ConfirmationDialog : Page<ConfirmationDialog>
{
    public ConfirmationDialog(IServiceProvider sp) : base(sp) { }

    public HomePage Confirm()
    {
        Enqueue(async (IPage page) =>
        {
            await page.ClickAsync("[data-testid='confirm']");
        });
        return NavigateTo<HomePage>();
    }

    public HomePage Cancel()
    {
        Enqueue(async (IPage page) =>
        {
            await page.ClickAsync("[data-testid='cancel']");
        });
        return NavigateTo<HomePage>();
    }
}
```

## Hosting Strategies

Hosting is orthogonal to the plugin/session architecture. All strategies implement `IHostingStrategy`:

### DotNetHostingStrategy
Manages .NET application lifecycle via `dotnet run` with HTTP readiness probing.

### NodeHostingStrategy
Manages Node.js application lifecycle via `npm run` with environment variable configuration.

### ExternalHostingStrategy
For pre-started servers (CI environments, staging) with health check only.

### AspireHostingStrategy
Wraps `DistributedApplicationTestingBuilder` for full Aspire distributed application lifecycle with resource-based URL discovery.

## Usage Examples

### Standard Test Setup

```csharp
[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task Init(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost:5000"))
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task Cleanup() => await _app!.DisposeAsync();

    public static AppScaffold<WebApp> App => _app!;
}

[TestClass]
public class TestBase
{
    [TestInitialize]
    public async Task TestSetup()
        => await TestAssemblyHooks.App.CreateSessionAsync();

    [TestCleanup]
    public async Task TestCleanup()
        => await TestAssemblyHooks.App.DisposeSessionAsync();
}
```

### Aspire Test Setup

```csharp
[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task Init(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UseAspireHosting<Projects.MyApp_AppHost>(
                appHost => { /* configure distributed app */ },
                "myapp")
            .Web<WebApp>(options => { options.UsePlaywright(); })
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task Cleanup() => await _app!.DisposeAsync();

    public static AppScaffold<WebApp> App => _app!;
}
```

### Test Examples

```csharp
[TestClass]
public class SearchTests : TestBase
{
    [TestMethod]
    public async Task SearchForProduct_ShowsResults()
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .SearchAndNavigate("laptop")
            .VerifyResultCount(5);
    }

    [TestMethod]
    public async Task CarouselSelection_NavigatesToProduct()
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .SelectItemFromCarousel(3)
            .NavigateTo<ProductDetailPage>()
            .VerifyPrice("$999");
    }

    [TestMethod]
    public async Task ParameterizedRoute_NavigatesCorrectly()
    {
        await TestAssemblyHooks.App.NavigateTo<UserProfilePage>(new { userId = "42" })
            .VerifyDisplayName("Jane Doe");
    }

    [TestMethod]
    public async Task LoginFlow_RedirectsToHome()
    {
        await TestAssemblyHooks.App.NavigateTo<LoginPage>()
            .LoginAs("testuser", "password123")
            .VerifyWelcomeVisible();
    }

    [TestMethod]
    public async Task Can_Add_Shift_To_Roster()
    {
        await TestAssemblyHooks.App.NavigateTo<RosterDetailsPage>(new { rosterId = 123 })
            .AddShift("456", "09:00", "17:00")
            .VerifyShiftAdded("456");
    }
}
```

## Framework-Specific Access

Playwright's API is accessed directly inside `Enqueue<T>()` lambdas. There is no `IUIDriver` abstraction. The framework coupling is in user-written page objects, not in the library:

```csharp
// Playwright's IPage injected via DI into the lambda
public HomePage TakeScreenshot(string path)
    => Enqueue(async (IPage page) =>
    {
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
    });

// Access IBrowserContext for cookie manipulation
public HomePage ClearCookies()
    => Enqueue(async () =>
    {
        var context = ServiceProvider.GetRequiredService<IBrowserContext>();
        await context.ClearCookiesAsync();
    });
```

## Custom Page Base Classes

The CRTP pattern supports custom base classes for shared behavior:

```csharp
public abstract class AuthenticatedPage<TSelf> : Page<TSelf>
    where TSelf : AuthenticatedPage<TSelf>
{
    protected AuthenticatedPage(IServiceProvider services) : base(services) { }

    public TSelf VerifyLoggedIn()
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.GetByTestId("user-avatar"))
                .ToBeVisibleAsync();
        });

    public LoginPage Logout()
        => Enqueue(async (IPage page) =>
        {
            await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
        }).NavigateTo<LoginPage>();
}

[Route("/dashboard")]
public class DashboardPage : AuthenticatedPage<DashboardPage>
{
    public DashboardPage(IServiceProvider services) : base(services) { }

    public DashboardPage VerifyWidgetCount(int expected)
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.Locator(".widget"))
                .ToHaveCountAsync(expected);
        });
}
```

## Error Handling

### FrozenPageException

Thrown when attempting to enqueue actions on a page that has been frozen by `NavigateTo<T>()`. Message includes the source page type and the target page type.

```csharp
public class FrozenPageException : InvalidOperationException
{
    public Type PageType { get; }
}
```

### InvalidOperationException

Thrown by `AppScaffold.NavigateTo<TPage>()` when no browser session is active.

### Chain Errors

Chains fail fast. The first exception stops execution and propagates to the `await` site. Subsequent queued actions are skipped.

## Common Mistakes

```csharp
// WRONG: Missing await -- actions never execute!
app.NavigateTo<HomePage>().ClickGetStarted();

// CORRECT: Always await the chain
await app.NavigateTo<HomePage>().ClickGetStarted();

// WRONG: Using a page after NavigateTo (page is frozen)
var home = app.NavigateTo<HomePage>();
var results = home.NavigateTo<SearchResultsPage>();
home.SearchFor("laptop"); // throws FrozenPageException

// CORRECT: Continuous chain
await app.NavigateTo<HomePage>()
    .SearchFor("laptop")
    .NavigateTo<SearchResultsPage>()
    .VerifyResultCount(5);

// WRONG: Forgetting to create a session
await app.NavigateTo<HomePage>(); // throws InvalidOperationException

// CORRECT: Create session first
await app.CreateSessionAsync();
await app.NavigateTo<HomePage>().VerifyWelcomeVisible();

// WRONG: Conditional logic inside a chain (not supported)
await app.NavigateTo<HomePage>()
    .MaybeDoSomething(); // chains are linear

// CORRECT: Break chain with await, use C# if, start new chain
await app.NavigateTo<HomePage>();
if (someCondition)
    await app.On<HomePage>().DoSomething();
```

This specification describes the deferred execution chain architecture of FluentUIScaffold, where pages use `Enqueue<IPage>` for direct Playwright access and chains must be awaited to execute.
