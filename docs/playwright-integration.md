# Playwright Integration

## Overview

FluentUIScaffold integrates with Microsoft Playwright through the `PlaywrightPlugin`, which implements `IUITestingPlugin`. The plugin owns a shared Chromium browser instance and creates isolated per-test sessions via `PlaywrightBrowserSession`. Page objects interact with the browser using `Enqueue<IPage>`, giving you direct access to Playwright's full API while preserving the deferred execution chain pattern.

## 1. Setup and Registration

### Install the Package

```bash
dotnet add package FluentUIScaffold.Playwright
```

Playwright browsers must also be installed:

```bash
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### Register the Plugin

The `UsePlaywright()` extension method is the recommended way to register the Playwright plugin. It is a convenience wrapper around `.UsePlugin(new PlaywrightPlugin())`.

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = true;
    })
    .Build<WebApp>();

await app.StartAsync();
```

Explicit plugin registration is equivalent:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
    })
    .Build<WebApp>();
```

### Core Components

**`PlaywrightPlugin`** implements `IUITestingPlugin`:

```csharp
public class PlaywrightPlugin : IUITestingPlugin
{
    public void ConfigureServices(IServiceCollection services);
    public Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken ct = default);
    public Task<IBrowserSession> CreateSessionAsync(IServiceProvider rootProvider);
    public ValueTask DisposeAsync();
}
```

- `ConfigureServices` is called once during builder configuration.
- `InitializeAsync` launches Chromium with the configured headless mode and SlowMo settings. The browser instance is shared across all test sessions.
- `CreateSessionAsync` creates a new `IBrowserContext` and `IPage` for each test.
- `DisposeAsync` closes the browser and disposes the Playwright instance.

**`PlaywrightBrowserSession`** implements `IBrowserSession`:

```csharp
public class PlaywrightBrowserSession : IBrowserSession
{
    public IServiceProvider ServiceProvider { get; }
    public Task NavigateToUrlAsync(Uri url);
    public ValueTask DisposeAsync();
}
```

The session's `ServiceProvider` is a `SessionServiceProvider` that resolves session-scoped Playwright services, falling back to the root provider for everything else:

| Service Type | Scope | Description |
|---|---|---|
| `IPage` | Per-session | The Playwright page for this test |
| `IBrowserContext` | Per-session | The browser context for this test |
| `IBrowser` | Shared | The browser instance owned by the plugin |

## 2. Session Lifecycle

Each test gets an isolated browser context and page. Create a session in `[TestInitialize]` and dispose it in `[TestCleanup]`:

```csharp
[TestClass]
public class HomeTests
{
    private static AppScaffold<WebApp> _app;

    [AssemblyInitialize]
    public static async Task AssemblyInit(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .Web<WebApp>(opts =>
            {
                opts.BaseUrl = new Uri("http://localhost:5000");
            })
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }

    [TestInitialize]
    public async Task TestSetup()
    {
        await _app.CreateSessionAsync();
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        await _app.DisposeSessionAsync();
    }

    [TestMethod]
    public async Task Home_ShowsWelcome()
    {
        await _app.NavigateTo<HomePage>()
            .VerifyTitle("Welcome");
    }
}
```

**Lifecycle summary:**

1. `app.StartAsync()` calls `PlaywrightPlugin.InitializeAsync()`, which launches Chromium once.
2. `app.CreateSessionAsync()` calls `PlaywrightPlugin.CreateSessionAsync()`, which creates a fresh `IBrowserContext` and `IPage`.
3. `app.NavigateTo<TPage>()` resolves the page object, injecting the session's `IServiceProvider`.
4. `app.DisposeSessionAsync()` closes the browser context, releasing all session resources.
5. `app.DisposeAsync()` calls `PlaywrightPlugin.DisposeAsync()`, closing the browser.

## 3. Using Playwright API in Page Objects via Enqueue\<IPage>

All browser interactions in page objects use `Enqueue<IPage>`. The `IPage` is resolved from the session's service provider at execution time (not at enqueue time). Actions are deferred and execute sequentially when the chain is awaited.

### Basic Page Object

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public HomePage ClickCounter() => Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
    });

    public HomePage VerifyTitle(string expected) => Enqueue<IPage>(async page =>
    {
        await Assertions.Expect(page).ToHaveTitleAsync(expected).ConfigureAwait(false);
    });
}
```

### Locators

Inside `Enqueue<IPage>` you have access to all of Playwright's locator strategies:

```csharp
public MyPage UseLocators() => Enqueue<IPage>(async page =>
{
    // Role-based
    await page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();

    // Label-based
    await page.GetByLabel("Email").FillAsync("test@example.com");

    // Test ID
    await page.GetByTestId("user-card").ClickAsync();

    // Text-based
    await page.GetByText("Welcome back").IsVisibleAsync();

    // Placeholder-based
    await page.GetByPlaceholder("Search...").FillAsync("query");
});
```

### Assertions

Use Playwright's built-in auto-retrying assertions:

```csharp
public MyPage VerifyDashboard() => Enqueue<IPage>(async page =>
{
    await Assertions.Expect(page).ToHaveTitleAsync("Dashboard");
    await Assertions.Expect(page.GetByTestId("welcome")).ToBeVisibleAsync();
    await Assertions.Expect(page.GetByTestId("user-name")).ToHaveTextAsync("John");
});
```

### Fluent Chaining and Navigation

Page methods return `TSelf`, enabling fluent chains. Use `NavigateTo<TTarget>()` to transition between pages:

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    public LoginPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public LoginPage EnterUsername(string username) => Enqueue<IPage>(async page =>
    {
        await page.FillAsync("[data-testid='username']", username);
    });

    public LoginPage EnterPassword(string password) => Enqueue<IPage>(async page =>
    {
        await page.FillAsync("[data-testid='password']", password);
    });

    public LoginPage SubmitForm() => Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("[data-testid='submit']");
    });

    public DashboardPage LoginAs(string user, string pass) =>
        EnterUsername(user)
            .EnterPassword(pass)
            .SubmitForm()
            .NavigateTo<DashboardPage>();
}
```

### Browser Context Access

Use `Enqueue<IBrowserContext>` for context-level operations:

```csharp
public MyPage ClearCookies() => Enqueue<IBrowserContext>(async context =>
{
    await context.ClearCookiesAsync();
});

public MyPage SetCookie() => Enqueue<IBrowserContext>(async context =>
{
    await context.AddCookiesAsync(new[]
    {
        new Cookie
        {
            Name = "session",
            Value = "abc123",
            Domain = "localhost",
            Path = "/",
        }
    });
});
```

## 4. Screenshots and Script Execution

### Screenshots

Capture full-page or element-level screenshots via `IPage`:

```csharp
public MyPage TakeScreenshot(string path) => Enqueue<IPage>(async page =>
{
    await page.ScreenshotAsync(new PageScreenshotOptions { Path = path });
});

public MyPage ScreenshotElement(string selector, string path) => Enqueue<IPage>(async page =>
{
    await page.Locator(selector).ScreenshotAsync(new() { Path = path });
});
```

### Script Execution

Execute arbitrary JavaScript via `IPage.EvaluateAsync`:

```csharp
public MyPage ClearStorage() => Enqueue<IPage>(async page =>
{
    await page.EvaluateAsync("localStorage.clear(); sessionStorage.clear()");
});

public MyPage CheckLocation() => Enqueue<IPage>(async page =>
{
    var href = await page.EvaluateAsync<string>("window.location.href");
    var count = await page.EvaluateAsync<int>("document.querySelectorAll('h1').length");
});
```

## 5. Network Interception

Use `IPage.RouteAsync` to intercept and mock network requests:

```csharp
public MyPage MockApiResponse() => Enqueue<IPage>(async page =>
{
    await page.RouteAsync("**/api/users", async route =>
    {
        await route.FulfillAsync(new RouteFulfillOptions
        {
            Body = "[{\"id\": 1, \"name\": \"John Doe\"}]",
            ContentType = "application/json",
        });
    });
});

public MyPage BlockImages() => Enqueue<IPage>(async page =>
{
    await page.RouteAsync("**/*.{png,jpg,jpeg,gif}", async route =>
    {
        await route.AbortAsync();
    });
});

public MyPage InterceptAndModify() => Enqueue<IPage>(async page =>
{
    await page.RouteAsync("**/api/config", async route =>
    {
        var response = await route.FetchAsync();
        var body = await response.TextAsync();
        var modified = body.Replace("\"feature\": false", "\"feature\": true");

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Response = response,
            Body = modified,
        });
    });
});
```

## 6. Headless Mode and SlowMo Auto-Detection

`PlaywrightPlugin.InitializeAsync` automatically configures headless mode and SlowMo based on whether a debugger is attached:

| Condition | HeadlessMode | SlowMo |
|---|---|---|
| Debugger attached (default) | `false` | `50ms` |
| No debugger / CI (default) | `true` | `0ms` |
| Explicit value set | Uses provided value | Uses provided value |

Auto-detection applies when `HeadlessMode` or `SlowMo` are left as `null` in options.

```csharp
// Auto-detect: headless in CI, headed when debugging
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        // HeadlessMode and SlowMo auto-detect when not set
    })
    .Build<WebApp>();
```

```csharp
// Explicit: always show browser with slow interactions
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = false;
        opts.SlowMo = 1000;
    })
    .Build<WebApp>();
```

## 7. Aspire Hosting Integration

Combine Playwright with Aspire for distributed application testing. The `UseAspireHosting` method auto-discovers the base URL from a named Aspire resource:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.SampleApp_AppHost>(
        appHost => { /* configure distributed app */ },
        "sampleapp")
    .UsePlaywright()
    .Build<WebApp>();

await app.StartAsync();
```

Full test class with Aspire:

```csharp
[TestClass]
public class AspireTests
{
    private static AppScaffold<WebApp> _app;

    [AssemblyInitialize]
    public static async Task AssemblyInit(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UseAspireHosting<Projects.SampleApp_AppHost>(
                appHost => { },
                "sampleapp")
            .UsePlaywright()
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }

    [TestInitialize]
    public async Task TestSetup() => await _app.CreateSessionAsync();

    [TestCleanup]
    public async Task TestCleanup() => await _app.DisposeSessionAsync();

    [TestMethod]
    public async Task App_Loads()
    {
        await _app.NavigateTo<HomePage>()
            .VerifyTitle("Sample App");
    }
}
```

## 8. Advanced Features

### Tracing

Capture Playwright traces for debugging failed tests. Traces include screenshots, DOM snapshots, and source code:

```csharp
public MyPage StartTrace() => Enqueue<IBrowserContext>(async context =>
{
    await context.Tracing.StartAsync(new TracingStartOptions
    {
        Screenshots = true,
        Snapshots = true,
        Sources = true,
    });
});

public MyPage StopTrace(string path) => Enqueue<IBrowserContext>(async context =>
{
    await context.Tracing.StopAsync(new TracingStopOptions
    {
        Path = path,
    });
});
```

View traces with:

```bash
pwsh bin/Debug/net8.0/playwright.ps1 show-trace trace.zip
```

### Mobile Emulation

Configure device emulation when creating the browser context. Since `PlaywrightPlugin` creates the context internally, use `Enqueue<IPage>` to set viewport and user agent after session creation:

```csharp
public MyPage EmulatePhone() => Enqueue<IPage>(async page =>
{
    await page.SetViewportSizeAsync(375, 812);
});
```

For full device emulation (user agent, touch, etc.), create a custom plugin that overrides context creation, or access the context directly:

```csharp
public MyPage SetMobileUserAgent() => Enqueue<IBrowserContext>(async context =>
{
    // Note: user agent is set at context creation time.
    // For full device emulation, consider extending PlaywrightPlugin
    // to accept BrowserNewContextOptions.
});
```

### Direct Playwright Access (Escape Hatch)

For scenarios where the page object pattern is not suitable, resolve `IPage` directly from the app's current session:

```csharp
[TestMethod]
public async Task DirectPlaywrightAccess()
{
    await _app.CreateSessionAsync();

    var page = _app.GetService<IPage>();
    await page.GotoAsync("http://localhost:5000");
    await page.ClickAsync("text=Login");

    var title = await page.TitleAsync();
    Assert.AreEqual("Login", title);

    await _app.DisposeSessionAsync();
}
```

### Error Handling with Screenshot Capture

```csharp
[TestMethod]
public async Task WithScreenshotOnFailure()
{
    try
    {
        await _app.NavigateTo<TestPage>()
            .PerformAction();
    }
    catch (PlaywrightException)
    {
        var page = _app.GetService<IPage>();
        await page.ScreenshotAsync(new() { Path = "failure.png" });
        throw;
    }
}
```

## Best Practices

### 1. Use Session Lifecycle Correctly

Always create a session in `[TestInitialize]` and dispose it in `[TestCleanup]`. This ensures each test gets a clean browser context.

```csharp
[TestInitialize]
public async Task Setup() => await _app.CreateSessionAsync();

[TestCleanup]
public async Task Cleanup() => await _app.DisposeSessionAsync();
```

### 2. Always Await Page Chains

Page chains use deferred execution. Forgetting `await` means actions never run:

```csharp
// Correct: actions execute
await app.NavigateTo<HomePage>().ClickButton();

// Wrong: actions are enqueued but never executed
app.NavigateTo<HomePage>().ClickButton(); // Missing await!
```

### 3. Use Playwright's Auto-Retrying Assertions

Prefer `Assertions.Expect` over manual polling. Playwright assertions automatically retry until the condition is met or the timeout expires:

```csharp
public MyPage VerifyText() => Enqueue<IPage>(async page =>
{
    await Assertions.Expect(page.GetByTestId("message"))
        .ToHaveTextAsync("Success");
});
```

### 4. Keep Page Methods Focused

Write small, composable methods and combine them for workflows:

```csharp
// Small, composable methods
public LoginPage EnterUsername(string user) => Enqueue<IPage>(async page =>
    await page.FillAsync("#username", user));

public LoginPage EnterPassword(string pass) => Enqueue<IPage>(async page =>
    await page.FillAsync("#password", pass));

public LoginPage Submit() => Enqueue<IPage>(async page =>
    await page.ClickAsync("#submit"));

// Compose for common workflows
public DashboardPage LoginAs(string user, string pass) =>
    EnterUsername(user).EnterPassword(pass).Submit().NavigateTo<DashboardPage>();
```

### 5. Prefer Test IDs Over CSS Selectors

Test IDs are resilient to UI changes:

```csharp
// Preferred: stable across refactors
await page.GetByTestId("submit-button").ClickAsync();

// Fragile: breaks when CSS classes or DOM structure change
await page.ClickAsync(".btn.btn-primary.submit");
```

For more information, see the [API Reference](api-reference.md).
