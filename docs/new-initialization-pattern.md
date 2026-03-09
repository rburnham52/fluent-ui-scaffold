# FluentUIScaffold Initialization Pattern

## Overview

The FluentUIScaffold framework uses a builder-based initialization pattern with `AppScaffold<TApp>` as the central orchestrator. This provides an async-first design with pluggable hosting strategies and per-test browser session isolation.

## Builder Pattern

The `FluentUIScaffoldBuilder` provides a fluent API for configuring and building the application scaffold:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
        opts.HeadlessMode = true;
    })
    .Build<WebApp>();

await app.StartAsync();
```

## Hosting Strategies

The framework supports pluggable hosting strategies for different application types:

### DotNetHostingStrategy

For .NET applications started with `dotnet run`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseDotNetHosting(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
        opts.ProjectPath = "path/to/project.csproj";
    })
    .Build<WebApp>();

await app.StartAsync();
```

### NodeHostingStrategy

For Node.js applications started with `npm run`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseNodeHosting(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:3000");
        opts.ProjectPath = "path/to/client-app";
    })
    .Build<WebApp>();

await app.StartAsync();
```

### ExternalHostingStrategy

For pre-started servers (CI environments, staging):

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseExternalServer(new Uri("https://staging.example.com"))
    .Build<WebApp>();

await app.StartAsync();
```

### AspireHostingStrategy

For Aspire distributed applications:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.MyApp_AppHost>(
        appHost => { /* configure */ },
        "myapp")
    .UsePlaywright()
    .Build<WebApp>();

await app.StartAsync();
```

## Plugin Registration

A UI testing plugin must be registered. Currently Playwright is the supported plugin:

```csharp
// Convenience extension method (preferred)
builder.UsePlaywright();
```

## Environment Configuration

The builder provides methods for configuring the hosting environment:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .WithEnvironmentName("Testing")
    .WithSpaProxy(false)
    .WithHeadlessMode(true)
    .WithEnvironmentVariable("MY_VAR", "val")
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
    })
    .Build<WebApp>();
```

## Debug Mode

The framework automatically detects debugging for Playwright configuration:

- **Debugger attached**: `HeadlessMode = false` (visible browser), `SlowMo = 50ms`
- **No debugger (CI)**: `HeadlessMode = true` (headless), `SlowMo = 0ms`

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
        // HeadlessMode auto-resolves at Build() time
    })
    .Build<WebApp>();
```

## Session Lifecycle

Each test gets an isolated browser session (`IBrowserContext` + `IPage`). Sessions are created and disposed around tests:

```csharp
// Create session in test setup
await app.CreateSessionAsync();

// Navigate and interact in tests (chains must be awaited)
await app.NavigateTo<HomePage>()
    .ClickButton();

// Dispose session in test cleanup
await app.DisposeSessionAsync();
```

## Page Objects

Page objects use the `[Route]` attribute for URL mapping and `Enqueue<T>()` for all browser interactions. The constructor takes `IServiceProvider`:

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    protected LoginPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public LoginPage EnterUsername(string user)
        => Enqueue(async (IPage page) =>
        {
            await page.FillAsync("[data-testid='username']", user);
        });

    public LoginPage EnterPassword(string password)
        => Enqueue(async (IPage page) =>
        {
            await page.FillAsync("[data-testid='password']", password);
        });

    public LoginPage SubmitForm()
        => Enqueue(async (IPage page) =>
        {
            await page.ClickAsync("[data-testid='submit']");
        });
}
```

### Parameterized Routes

For pages with dynamic URL segments, use placeholders in the `[Route]` attribute:

```csharp
[Route("/users/{userId}")]
public class UserProfilePage : Page<UserProfilePage>
{
    protected UserProfilePage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public UserProfilePage VerifyDisplayName(string expected)
        => Enqueue(async (IPage page) =>
        {
            await Assertions.Expect(page.GetByTestId("display-name"))
                .ToHaveTextAsync(expected);
        });
}

// Navigate with parameters:
await app.NavigateTo<UserProfilePage>(new { userId = "42" })
    .VerifyDisplayName("Jane Doe");
```

### Injecting Multiple Services

`Enqueue<T>()` supports multiple generic parameters for injecting additional services from DI:

```csharp
[Route("/checkout")]
public class CheckoutPage : Page<CheckoutPage>
{
    protected CheckoutPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public CheckoutPage PlaceOrder()
        => Enqueue(async (IPage page, ILogger<CheckoutPage> logger) =>
        {
            logger.LogInformation("Placing order");
            await page.GetByRole(AriaRole.Button, new() { Name = "Place Order" }).ClickAsync();
            await page.WaitForURLAsync("**/confirmation**");
        });

    // Direct framework access for advanced scenarios (network mocking, etc.)
    public CheckoutPage MockPaymentApi()
        => Enqueue(async (IPage page) =>
        {
            await page.RouteAsync("**/api/payment", route => route.FulfillAsync(new()
            {
                Body = "{\"status\": \"success\", \"transactionId\": \"test-123\"}"
            }));
        });
}
```

## Fluent API Usage

All page methods return `TSelf` for chaining. Chains are deferred and execute when awaited:

```csharp
await app.NavigateTo<HomePage>()
    .SearchFor("laptop")
    .NavigateTo<SearchResultsPage>()
    .VerifyResultCount(5)
    .ClickProduct(0)
    .NavigateTo<ProductDetailPage>()
    .VerifyPrice("$999");
```

### Page Navigation and Freezing

When `NavigateTo<TTarget>()` is called on a page, the current page is frozen (subsequent calls on it throw an error) and the target page is returned sharing the same action queue:

```csharp
// Correct: continuous chain
await app.NavigateTo<HomePage>()
    .SearchFor("laptop")               // HomePage method
    .NavigateTo<SearchResultsPage>()   // freezes HomePage, returns SearchResultsPage
    .VerifyResultCount(5);             // SearchResultsPage method

// Error: using frozen page
var home = app.NavigateTo<HomePage>();
var results = home.NavigateTo<SearchResultsPage>();
home.SearchFor("laptop"); // throws: "Page frozen after NavigateTo<SearchResultsPage>()"
```

## Benefits

1. **Async-First**: Modern async lifecycle with `StartAsync()` and `DisposeAsync()`
2. **Pluggable Hosting**: Support for .NET, Node.js, External servers, and Aspire
3. **Session Isolation**: Each test gets its own browser context and page
4. **Deferred Execution**: Actions queue and execute on `await` for predictable ordering
5. **Direct Framework Access**: `Enqueue<IPage>` gives full Playwright API access
6. **Debug Support**: Automatic headless/SlowMo detection for development

## Usage Examples

### Basic Test Setup

```csharp
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
                opts.BaseUrl = new Uri("https://localhost:5001");
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

### Writing Tests

```csharp
[TestClass]
public class HomePageTests
{
    [TestInitialize]
    public async Task Setup()
    {
        await TestAssemblyHooks.App.CreateSessionAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await TestAssemblyHooks.App.DisposeSessionAsync();
    }

    [TestMethod]
    public async Task Can_Search_And_View_Results()
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .SearchAndNavigate("laptop")
            .VerifyResultCount(5)
            .ClickProduct(0)
            .NavigateTo<ProductDetailPage>()
            .VerifyPrice("$999");
    }

    [TestMethod]
    public async Task Parameterized_Route_Navigates_Correctly()
    {
        await TestAssemblyHooks.App.NavigateTo<UserProfilePage>(new { userId = "42" })
            .VerifyDisplayName("Jane Doe");
    }
}
```

### Aspire Testing Setup

```csharp
[TestClass]
public class AspireTestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UseAspireHosting<Projects.SampleApp_AppHost>(
                appHost => { /* configure */ },
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

    public static AppScaffold<WebApp> App => _app!;
}
```
