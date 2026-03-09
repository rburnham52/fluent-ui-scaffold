# Getting Started with FluentUIScaffold

## Prerequisites

- **.NET 6.0 or later** - [Download .NET](https://dotnet.microsoft.com/download)
- **A test framework** - MSTest, xUnit, or NUnit
- **Playwright browsers** installed (handled automatically on first run)

## Installation

```bash
dotnet add package FluentUIScaffold.Core
dotnet add package FluentUIScaffold.Playwright
```

For Aspire-hosted applications, also add:

```bash
dotnet add package FluentUIScaffold.AspireHosting
```

## Quick Start

### 1. Set Up Assembly Hooks with Session Lifecycle

FluentUIScaffold uses an assembly-level `AppScaffold` for the application lifecycle and per-test session management for browser isolation.

```csharp
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
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

    public static AppScaffold<WebApp> App => _app!;
}
```

### 2. Create a Base Test Class with Session Lifecycle

Each test gets its own browser session via `CreateSessionAsync()` and `DisposeSessionAsync()`:

```csharp
[TestClass]
public class UITestBase
{
    [TestInitialize]
    public async Task TestSetup()
    {
        await TestAssemblyHooks.App.CreateSessionAsync();
    }

    [TestCleanup]
    public async Task TestTeardown()
    {
        await TestAssemblyHooks.App.DisposeSessionAsync();
    }
}
```

### 3. Write Your First Test

Tests `await` the page chain, which triggers deferred execution of all enqueued actions:

```csharp
[TestClass]
public class HomePageTests : UITestBase
{
    [TestMethod]
    public async Task Can_Navigate_To_Home_Page()
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>();
    }
}
```

### 4. Run Your Tests

```bash
dotnet test

# Run a specific test
dotnet test --filter "Can_Navigate_To_Home_Page"
```

## Creating Page Objects

Pages inherit from `Page<TSelf>` and use the `Enqueue<IPage>()` method to schedule Playwright interactions. The page acts as a deferred execution chain builder -- actions are collected and only run when the chain is awaited.

```csharp
using FluentUIScaffold.Core.Pages;
using Microsoft.Playwright;

[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public HomePage ClickLoginButton()
    {
        Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("#login-button");
        });
        return this;
    }

    public HomePage EnterSearchTerm(string term)
    {
        Enqueue<IPage>(async page =>
        {
            await page.FillAsync("#search-input", term);
        });
        return this;
    }

    public HomePage VerifyWelcomeMessageVisible()
    {
        Enqueue<IPage>(async page =>
        {
            await Expect(page.Locator("#welcome-message")).ToBeVisibleAsync();
        });
        return this;
    }
}
```

Key points:
- The `[Route("/")]` attribute defines the page URL (combined with `BaseUrl`).
- The constructor takes only `IServiceProvider`.
- `Enqueue<IPage>()` schedules an async action against the Playwright `IPage` instance.
- Methods return `this` (typed as `TSelf`) for fluent chaining.
- Nothing executes until the chain is awaited via `GetAwaiter()`.

### Fluent Chaining in Tests

```csharp
[TestMethod]
public async Task Can_Search_And_See_Results()
{
    await TestAssemblyHooks.App.NavigateTo<HomePage>()
        .EnterSearchTerm("fluent ui")
        .ClickSearchButton()
        .VerifyResultsVisible();
}
```

## Cross-Page Navigation

Use `NavigateTo<TTarget>()` within a page to navigate to another page. This freezes the current page's chain and starts building the target page's chain:

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public LoginPage GoToLogin()
    {
        Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("#login-link");
        });
        return NavigateTo<LoginPage>();
    }
}

[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    protected LoginPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public LoginPage EnterEmail(string email)
    {
        Enqueue<IPage>(async page =>
        {
            await page.FillAsync("#email", email);
        });
        return this;
    }

    public LoginPage EnterPassword(string password)
    {
        Enqueue<IPage>(async page =>
        {
            await page.FillAsync("#password", password);
        });
        return this;
    }

    public LoginPage SubmitForm()
    {
        Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("#login-btn");
        });
        return this;
    }
}
```

Chain across pages in a single awaited expression:

```csharp
[TestMethod]
public async Task Can_Login_From_Home_Page()
{
    await TestAssemblyHooks.App.NavigateTo<HomePage>()
        .GoToLogin()
        .EnterEmail("user@example.com")
        .EnterPassword("password123")
        .SubmitForm();
}
```

Use `On<TPage>()` to resolve the current page without triggering navigation:

```csharp
[TestMethod]
public async Task Can_Verify_Dashboard_After_Login()
{
    await TestAssemblyHooks.App.NavigateTo<LoginPage>()
        .EnterEmail("user@example.com")
        .EnterPassword("password123")
        .SubmitForm();

    await TestAssemblyHooks.App.On<DashboardPage>()
        .VerifyWelcomeMessage();
}
```

## Parameterized Routes

Pages with dynamic URL segments use placeholders in the `[Route]` attribute. Pass parameters as an anonymous object to `NavigateTo`:

```csharp
[Route("/users/{userId}")]
public class UserPage : Page<UserPage>
{
    protected UserPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    // ... page methods
}

// Navigate with a parameter:
await app.NavigateTo<UserPage>(new { userId = "123" });
// Navigates to: http://localhost:5000/users/123
```

Multiple parameters work the same way:

```csharp
[Route("/users/{userId}/posts/{postId}")]
public class UserPostPage : Page<UserPostPage>
{
    protected UserPostPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}

await app.NavigateTo<UserPostPage>(new { userId = "456", postId = "789" });
// Navigates to: http://localhost:5000/users/456/posts/789
```

## Hosting Strategies

### DotNet Hosting

For .NET web applications managed by the framework:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
    })
    .Build<WebApp>();
```

### Aspire Hosting

For distributed applications using .NET Aspire:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.SampleApp_AppHost>(
        appHost => { /* configure distributed app */ },
        "sampleapp")
    .Web<WebApp>(options => { options.UsePlaywright(); })
    .Build<WebApp>();
```

You can append a prefix to the auto-discovered base URL for hash-based SPA routing or a common base path:

```csharp
.UseAspireHosting<Projects.SampleApp_AppHost>(
    appHost => { /* configure */ },
    baseUrlResourceName: "sampleapp",
    baseUrlPrefix: "#")  // Results in: http://localhost:port/#
```

### External Server

For pre-started servers in CI or staging environments:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://staging.your-app.com");
    })
    .Build<WebApp>();
```

## Headless Mode Configuration

By default, FluentUIScaffold auto-detects headless mode based on whether a debugger is attached:
- **Debugger attached**: Browser window is visible, SlowMo is enabled for easier observation.
- **No debugger (CI)**: Headless mode, no SlowMo.

Override explicitly when needed:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = false; // Force visible browser
        opts.SlowMo = 250;        // Milliseconds between actions
    })
    .Build<WebApp>();
```

## Next Steps

- Explore the [sample application](../samples/) for real-world usage
- See the [API Reference](api-reference.md) for the full API surface
