# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FluentUIScaffold is a framework-agnostic E2E testing library providing a fluent API for building maintainable UI test automation. It abstracts underlying testing frameworks (currently Playwright) with a consistent developer experience.

## Build & Test Commands

### Building
```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build src/FluentUIScaffold.Core/FluentUIScaffold.Core.csproj

# Format code (run before commits per Cursor rules)
dotnet format
```

### Testing
```bash
# Run all tests in solution
dotnet test

# Run specific test project
dotnet test tests/FluentUIScaffold.Core.Tests/FluentUIScaffold.Core.Tests.csproj
dotnet test tests/FluentUIScaffold.Playwright.Tests/FluentUIScaffold.Playwright.Tests.csproj
dotnet test tests/FluentUIScaffold.AspireHosting.Tests/FluentUIScaffold.AspireHosting.Tests.csproj

# Run sample tests (standard)
dotnet test samples/SampleApp.Tests/SampleApp.Tests.csproj

# Run sample tests (Aspire - requires Docker)
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj

# Run single test by filter
dotnet test --filter "FullyQualifiedName~TestMethodName"
dotnet test --filter "TestCategory=Integration"
```

### Running Sample App
```bash
# Standard ASP.NET Core app with Vite+Svelte frontend
cd samples/SampleApp/ClientApp
npm install
cd ..
dotnet run

# Aspire-hosted version (requires Docker)
cd samples/SampleApp.AppHost
dotnet run
```

## Code Architecture

### Core Abstraction Layers

**Element Interaction Pattern (3-layer)**:
- `IElement` interface: Contract for UI element interactions (Click, Type, SelectOption, etc.) with wait strategies
- `Element` class: Implementation that delegates to `IUIDriver` with wait/retry logic
- `ElementBuilder`: Fluent builder for configuring elements (`WithWaitStrategy()`, `WithTimeout()`, etc.)

**Page Object Model**:
- `Page<TSelf>`: Base class for all page objects
  - Single self-referencing generic (`TSelf : Page<TSelf>`) enables fluent API
  - Abstract `ConfigureElements()` method for element setup in derived classes
  - Fluent methods: `Click()`, `Type()`, `Select()`, `WaitForVisible()`, `NavigateTo<TTarget>()`
  - Verification context via `Verify` property for assertions
  - Access to `Driver` (IUIDriver) and `IServiceProvider` for DI
- Example: `samples/SampleApp.Tests/Pages/HomePage.cs`

**Driver Abstraction**:
- `IUIDriver`: Framework-agnostic interface for browser automation (navigation, clicks, waits, state checks)
- `PlaywrightDriver`: Concrete implementation managing Playwright lifecycle (IPlaywright → IBrowser → IBrowserContext → IPage)
  - Auto-detects headless mode based on debugger attachment
  - Implements all IUIDriver contract methods

### Plugin System

**Architecture**:
- `IUITestingFrameworkPlugin`: Interface for adding new UI frameworks
  - `CreateDriver()`: Factory method for driver instantiation
  - `ConfigureServices()`: DI registration hook
  - `SupportedDriverTypes` and `CanHandle()` for driver type matching
- `PluginManager`: Routes driver creation to appropriate plugin based on `FluentUIScaffoldOptions.RequestedDriverType`
- `PluginRegistry`: Global static registry for shared plugins

**PlaywrightPlugin** (`src/FluentUIScaffold.Playwright/`):
- Registers `PlaywrightDriver`, `IPage`, `IBrowser`, `IBrowserContext` with DI
- Supports .NET 6, 7, 8, 9

### Application Orchestration

**AppScaffold<TWebApp>** - The unified async-first application orchestrator:
- Async lifecycle with `StartAsync()` and `IAsyncDisposable`
- Startup logic as pluggable `Func<IServiceProvider, Task>` actions
- Built via `FluentUIScaffoldBuilder` fluent API
- Page navigation: `NavigateTo<TPage>()`, `On<TPage>()`, `WaitFor<TPage>()`
- Service resolution via `GetService<T>()` and `Framework<TResult>()`
- Base URL configuration via `WithBaseUrl()` and `NavigateToUrl()`

### Hosting Strategies

**IHostingStrategy** - Pluggable abstraction for managing application hosts:

1. **DotNetHostingStrategy**: Manages .NET application lifecycle via `dotnet run`
   - Process launching with configurable project path
   - HTTP readiness probing for health checks
   - Configuration hashing for server reuse

2. **NodeHostingStrategy**: Manages Node.js application lifecycle via `npm run`
   - npm/node-specific process handling
   - Environment variable configuration (PORT, NODE_ENV)

3. **ExternalHostingStrategy**: For pre-started servers (CI environments, staging)
   - Health check only, no process management
   - HTTP readiness probing to verify availability

4. **AspireHostingStrategy**: Wraps `DistributedApplicationTestingBuilder`
   - Full Aspire distributed application lifecycle
   - Resource-based URL discovery

### Aspire Integration

**Components** (`src/FluentUIScaffold.AspireHosting/`):
- `AspireHostingExtensions.UseAspireHosting<TEntryPoint>()`: Configures `FluentUIScaffoldBuilder` for Aspire
  - Creates `DistributedApplicationTestingBuilder` from Aspire.Hosting.Testing
  - Auto-discovers base URL from named resource
  - Stores `DistributedApplication` instance in DI via `DistributedApplicationHolder`
  - Optional `baseUrlPrefix` parameter to append a prefix to auto-discovered BaseUrl (e.g., `"#"` for hash-based SPA routing, `"/app"` for a common base path)
- `AspireResourceExtensions`: Helpers for creating HTTP clients from Aspire resources

### Configuration

**FluentUIScaffoldOptions**:
- `BaseUrl`: Application under test URL
- `DefaultWaitTimeout`: Global element timeout (default 30s)
- `HeadlessMode`: Explicit headless control (auto-detect if null)
- `SlowMo`: Browser slow-motion delay (auto-detect if null)
- `RequestedDriverType`: Hints plugin selection

**FluentUIScaffoldBuilder**: Instance-based fluent configuration builder:
- `UsePlugin()`: Register UI testing framework plugins
- `UsePlaywright()`: Convenience method to register Playwright plugin
- `UseAspireHosting<TEntryPoint>()`: Configure Aspire-based hosting
- `Web<TApp>()`: Configure web application options
- `WithAutoPageDiscovery()`: Enable automatic page class discovery
- `RegisterPage<TPage>()`: Manually register page classes
- `Build<TApp>()`: Build the `AppScaffold<TApp>` instance

## Test Patterns

### Standard Test Pattern

Pattern: Build AppScaffold with plugin and hosting strategy:

```csharp
[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost:5000"))
            .WithAutoPageDiscovery()
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

### Aspire Testing Pattern

Pattern: Build AppScaffold with Aspire hosting:

```csharp
[AssemblyInitialize]
public static async Task AssemblyInitialize(TestContext context)
{
    _sessionApp = new FluentUIScaffoldBuilder()
        .UseAspireHosting<Projects.SampleApp_AppHost>(
            appHost => { /* configure distributed app */ },
            "sampleapp")
        .Web<WebApp>(options => { options.UsePlaywright(); })
        .Build<WebApp>();

    await _sessionApp.StartAsync();
}

[AssemblyCleanup]
public static async Task AssemblyCleanup()
{
    await _sessionApp.DisposeAsync();
}
```

### Test Example

```csharp
[TestMethod]
public void NavigateToHomePage_DisplaysWelcomeMessage()
{
    var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();

    homePage.Verify
        .TitleContains("Welcome")
        .Visible(p => p.WelcomeMessage)
        .And
        .Click(p => p.GetStartedButton);
}
```

## Multi-Targeting

- **Core projects** target: net6.0, net7.0, net8.0
- **Playwright projects** target: net6.0, net7.0, net8.0, net9.0
- **Sample/Test projects** typically target: net6.0, net7.0, net8.0
- When adding features, ensure compatibility across all target frameworks

## Sample Application Structure

- **SampleApp**: ASP.NET Core backend (net8.0) with Vite+Svelte frontend
  - Uses `Microsoft.AspNetCore.SpaProxy` for reverse proxy to `http://localhost:5173`
  - Frontend: `samples/SampleApp/ClientApp/` (npm-based)
  - Automatic npm install in Debug builds if node_modules missing
- **SampleApp.AppHost**: Aspire AppHost for distributed testing scenarios
- **SampleApp.Tests**: Standard .NET app testing (no Aspire)
- **SampleApp.AspireTests**: Aspire-hosted testing (requires Docker)

## Important Code Patterns

### Page Object Implementation with Route Attribute

Use the `[Route]` attribute to define the URL path for a page. The route is combined with `BaseUrl`:

```csharp
[Route("/login")]  // Page URL will be: BaseUrl + "/login"
public class LoginPage : Page<LoginPage>
{
    public IElement SubmitButton { get; private set; } = null!;
    public IElement UsernameField { get; private set; } = null!;

    public LoginPage(IServiceProvider serviceProvider, Uri pageUrl)
        : base(serviceProvider, pageUrl)
    {
    }

    protected override void ConfigureElements()
    {
        SubmitButton = Element("[data-testid='submit']")
            .WithDescription("Submit Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();

        UsernameField = Element("#username")
            .WithDescription("Username Field")
            .Build();
    }

    public LoginPage EnterUsername(string username)
    {
        return Type(p => p.UsernameField, username);
    }

    public LoginPage SubmitForm()
    {
        return Click(p => p.SubmitButton);
    }
}
```

### Parameterized Routes

For pages with dynamic URL segments, use placeholders in the `[Route]` attribute:

```csharp
[Route("/users/{userId}")]
public class UserPage : Page<UserPage>
{
    public UserPage(IServiceProvider serviceProvider, Uri pageUrl)
        : base(serviceProvider, pageUrl)
    {
    }

    protected override void ConfigureElements() { /* ... */ }
}

// Navigate with parameters:
var userPage = app.NavigateTo<UserPage>(new { userId = "123" });
// Navigates to: http://localhost:5000/users/123

// Multiple parameters:
[Route("/users/{userId}/posts/{postId}")]
public class UserPostPage : Page<UserPostPage> { /* ... */ }

var postPage = app.NavigateTo<UserPostPage>(new { userId = "456", postId = "789" });
// Navigates to: http://localhost:5000/users/456/posts/789
```

### Wait Strategies

Available strategies: `None`, `Visible`, `Hidden`, `Clickable`, `Enabled`, `Disabled`, `TextPresent`, `Smart`

Smart waiting is preferred - framework handles retries/timeouts automatically.

### Fluent Page Actions

All page actions return `TSelf` for fluent chaining:

```csharp
homePage
    .Click(p => p.LoginLink)
    .WaitForVisible(p => p.LoginForm)
    .Type(p => p.Username, "testuser")
    .Type(p => p.Password, "password")
    .Click(p => p.SubmitButton);
```

### Verification Context

Use the `Verify` property for assertions with fluent chaining:

```csharp
page.Verify
    .TitleContains("Dashboard")
    .UrlContains("/dashboard")
    .Visible(p => p.WelcomeMessage)
    .TextContains(p => p.UserGreeting, "Hello")
    .And  // Returns to page for continued interaction
    .Click(p => p.LogoutButton);
```

### Plugin Registration

Plugins are registered on builder instances:

```csharp
var builder = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin());

// Or use convenience method
var builder = new FluentUIScaffoldBuilder()
    .UsePlaywright();
```

## Key Architectural Decisions

1. **Element abstraction** allows uniform interaction across any driver implementation
2. **Single self-referencing generic** (`Page<TSelf>`) provides clean fluent API with correct return types
3. **Explicit plugin registration** - no implicit assembly scanning for plugins
4. **Assembly-level server lifecycle** - start once, reuse across tests for efficiency
5. **DI-based page resolution** - pages auto-instantiated with required dependencies
6. **Async-first design** - `AppScaffold<TApp>` uses async lifecycle (`StartAsync`/`DisposeAsync`)
7. **Pluggable hosting strategies** - support for .NET, Node, External, and Aspire hosts
8. **Aspire as first-class citizen** - full integration with distributed app testing

## Code Formatting

The `.cursor/rules/dotnet-format.mdc` rule automatically runs `dotnet format` on C# changes. Always ensure code is formatted before commits.
