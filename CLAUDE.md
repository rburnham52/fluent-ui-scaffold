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

### Deferred Execution Chain (Page Object Model)

**`Page<TSelf>`** — the core abstraction. Acts as a deferred execution chain builder:
- Single self-referencing generic (`TSelf : Page<TSelf>`) enables fluent API with correct return types
- **Custom awaitable**: implements `GetAwaiter()` returning `TaskAwaiter` — chains execute when awaited
- **`Enqueue(Func<Task>)`**: queues a deferred action (no DI)
- **`Enqueue<T>(Func<T, Task>)`**: queues an action with a DI-resolved service (resolved at execution time)
- **`NavigateTo<TTarget>()`**: freezes current page, creates target sharing the action list
- **Freeze-on-navigate**: frozen pages throw `FrozenPageException` if you try to enqueue more actions
- **DEBUG finalizer**: warns via `Trace.TraceWarning` if a chain with actions is never awaited
- Example: `samples/SampleApp.Tests/Pages/HomePage.cs`

### Plugin + Session Architecture

**`IUITestingPlugin`** — plugin contract for UI testing frameworks:
- `ConfigureServices(IServiceCollection)`: registers shared services into DI
- `InitializeAsync(FluentUIScaffoldOptions, CancellationToken)`: one-time init (e.g., launches browser)
- `CreateSessionAsync(IServiceProvider rootProvider)`: creates an isolated `IBrowserSession` per test

**`IBrowserSession`** — per-test isolation boundary:
- `NavigateToUrlAsync(Uri)`: navigates the session's page
- `ServiceProvider`: scoped provider with session-local services (IPage, IBrowserContext, IBrowser)
- Implements `IAsyncDisposable` for cleanup

**`PlaywrightPlugin`** — implements `IUITestingPlugin`:
- Owns `IPlaywright` and `IBrowser` as singletons
- `CreateSessionAsync()` creates a new `IBrowserContext` + `IPage` per test
- Uses `DOMContentLoaded` instead of `NetworkIdle` for navigation (saves 500ms+)

**`SessionServiceProvider`** — lightweight wrapper `IServiceProvider`:
- Checks session-local dict (IPage, IBrowserContext, IBrowser) first
- Falls back to root provider for other services
- Implements `IServiceProviderIsService` for `ActivatorUtilities` compatibility

### Application Orchestration

**`AppScaffold<TWebApp>`** — the unified async-first application orchestrator:
- Async lifecycle with `StartAsync()` and `IAsyncDisposable`
- **Session lifecycle**: `CreateSessionAsync()` / `DisposeSessionAsync()` per test
- **Instance field** for `IBrowserSession` tracking (AsyncLocal and ThreadStatic both failed due to MSTest thread scheduling; works because AppScaffold is shared via a static accessor)
- Page navigation: `NavigateTo<TPage>()`, `NavigateTo<TPage>(routeParams)`, `On<TPage>()`
- Service resolution via `GetService<T>()`
- Built via `FluentUIScaffoldBuilder` fluent API

### Hosting Strategies

**IHostingStrategy** — pluggable abstraction for managing application hosts:

1. **DotNetHostingStrategy**: Manages .NET application lifecycle via `dotnet run`
2. **NodeHostingStrategy**: Manages Node.js application lifecycle via `npm run`
3. **ExternalHostingStrategy**: For pre-started servers (CI environments, staging)
4. **AspireHostingStrategy**: Wraps `DistributedApplicationTestingBuilder`

### Aspire Integration

**Components** (`src/FluentUIScaffold.AspireHosting/`):
- `AspireHostingExtensions.UseAspireHosting<TEntryPoint>()`: Configures `FluentUIScaffoldBuilder` for Aspire
  - Auto-discovers base URL from named resource
  - Optional `baseUrlPrefix` parameter for hash-based SPA routing

### Configuration

**FluentUIScaffoldOptions**:
- `BaseUrl`: Application under test URL
- `HeadlessMode`: Explicit headless control (auto-detect if null based on debugger attachment)
- `SlowMo`: Browser slow-motion delay
- `EnvironmentVariables`: Custom env vars for hosted applications
- `EnvironmentName`: Logical environment (default: "Testing")
- `SpaProxyEnabled`: ASP.NET SPA dev server proxy toggle

**FluentUIScaffoldBuilder**: Instance-based fluent configuration builder:
- `UsePlugin(IUITestingPlugin)`: Register a UI testing framework plugin
- `UsePlaywright()`: Convenience method to register Playwright plugin
- `UseAspireHosting<TEntryPoint>()`: Configure Aspire-based hosting
- `UseDotNetHosting()`, `UseNodeHosting()`, `UseExternalServer()`: Hosting strategies
- `Web<TApp>()`: Configure web application options
- `Build<TApp>()`: Build the `AppScaffold<TApp>` instance

## Test Patterns

### Standard Test Pattern

```csharp
[TestClass]
public static class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost:5000"))
            .Build<WebApp>();
        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null) await _app.DisposeAsync();
    }

    public static AppScaffold<WebApp> App => _app!;
}
```

### Test with Session Lifecycle

```csharp
[TestMethod]
public async Task NavigateToHomePage_DisplaysWelcome()
{
    var session = await TestAssemblyHooks.App.CreateSessionAsync();
    try
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .VerifyWelcomeVisible();
    }
    finally
    {
        await TestAssemblyHooks.App.DisposeSessionAsync();
    }
}
```

### Aspire Testing Pattern

```csharp
_app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseAspireHosting<Projects.SampleApp_AppHost>(
        appHost => { },
        "sampleapp")
    .Web<WebApp>(options => { })
    .Build<WebApp>();
await _app.StartAsync();
```

## Important Code Patterns

### Page Object Implementation

Pages use `Enqueue<IPage>()` to inject Playwright's `IPage` for direct browser interaction:

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public HomePage ClickCounter()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
        });
    }

    public HomePage VerifyWelcomeVisible()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
        });
    }
}
```

### Cross-Page Navigation (Fluent Chaining)

```csharp
await app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .NavigateTo<LoginPage>()    // freezes HomePage, shares action list
    .ClickLoginTab()
    .EnterEmail("test@example.com");
```

### Parameterized Routes

```csharp
[Route("/users/{userId}")]
public class UserPage : Page<UserPage>
{
    protected UserPage(IServiceProvider sp) : base(sp) { }
}

var userPage = app.NavigateTo<UserPage>(new { userId = "123" });
// Navigates to: http://localhost:5000/users/123
```

### Plugin Registration

```csharp
var builder = new FluentUIScaffoldBuilder()
    .UsePlaywright();  // Convenience extension

// Or register directly:
var builder = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin());
```

## Multi-Targeting

- **Core projects** target: net6.0, net7.0, net8.0
- **Playwright projects** target: net6.0, net7.0, net8.0
- **Aspire projects** target: net8.0, net9.0
- **Sample/Test projects** typically target: net8.0
- When adding features, ensure compatibility across all target frameworks

## Sample Application Structure

- **SampleApp**: ASP.NET Core backend (net8.0) with Vite+Svelte frontend
  - Uses `Microsoft.AspNetCore.SpaProxy` for reverse proxy to `http://localhost:5173`
  - Frontend: `samples/SampleApp/ClientApp/` (npm-based)
- **SampleApp.AppHost**: Aspire AppHost for distributed testing scenarios
- **SampleApp.Tests**: Standard .NET app testing (MSTest, no Aspire)
- **SampleApp.AspireTests**: Aspire-hosted testing (requires Docker)

## Key Architectural Decisions

1. **Deferred execution chain** — `Page<TSelf>` queues actions that execute on `await`, no sync-over-async
2. **Single self-referencing generic** (`Page<TSelf>`) provides clean fluent API with correct return types
3. **Direct Playwright access** — pages use `Enqueue<IPage>()` for native Playwright API, no wrapper layer
4. **Plugin + session architecture** — `IUITestingPlugin` owns browser, creates `IBrowserSession` per test
5. **Instance field for session tracking** — `IBrowserSession` stored as a plain instance field on `AppScaffold`; works because `AppScaffold` is shared via a static accessor (AsyncLocal/ThreadStatic both failed under MSTest thread scheduling)
6. **Assembly-level server lifecycle** — start once, reuse across tests for efficiency
7. **Pluggable hosting strategies** — support for .NET, Node, External, and Aspire hosts
8. **Aspire as first-class citizen** — full integration with distributed app testing
9. **`ConfigureAwait(false)`** on all internal awaits; NOT on Task returned by `GetAwaiter()`

## Code Formatting

The `.cursor/rules/dotnet-format.mdc` rule automatically runs `dotnet format` on C# changes. Always ensure code is formatted before commits.
