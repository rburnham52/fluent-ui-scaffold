# Getting Started with FluentUIScaffold

FluentUIScaffold is a framework-agnostic E2E testing library that provides a fluent API for building maintainable UI test automation. It abstracts underlying testing frameworks (currently Playwright) behind a consistent developer experience.

## Prerequisites

- **.NET 6.0 or later** (.NET 8.0+ required for Aspire integration)
- **Playwright browsers** installed (see below)
- A web application to test

## Project Setup

Create a test project and install the required NuGet packages. At minimum, you need the Core package and a UI testing plugin (Playwright):

```bash
dotnet new mstest -n MyApp.Tests
cd MyApp.Tests
dotnet add package FluentUIScaffold.Core
dotnet add package FluentUIScaffold.Playwright
```

After adding the Playwright package, install the browser binaries:

```bash
pwsh bin/Debug/net8.0/playwright.ps1 install
```

For Aspire integration, also add:

```bash
dotnet add package FluentUIScaffold.AspireHosting
```

## Core Concepts

FluentUIScaffold is built around four key ideas:

1. **Builder** (`FluentUIScaffoldBuilder`) -- configures everything: hosting strategy, UI plugin, options.
2. **AppScaffold** (`AppScaffold<TWebApp>`) -- manages the full application and browser lifecycle.
3. **Pages** (`Page<TSelf>`) -- define UI interactions as deferred execution chains using the page object pattern.
4. **Sessions** (`IBrowserSession`) -- provide per-test browser isolation.

## Test Assembly Setup

Use `[AssemblyInitialize]` and `[AssemblyCleanup]` (MSTest) to start the application once and share it across all tests:

```csharp
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

[TestClass]
public static class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .WithHeadlessMode(true)
            .WithEnvironmentName("Development")
            .WithSpaProxy(false)
            .UseDotNetHosting(opts =>
            {
                opts.BaseUrl = new Uri("http://localhost:5000");
                opts.ProjectPath = "../MyApp/MyApp.csproj";
                opts.Framework = "net8.0";
                opts.Configuration = "Release";
                opts.HealthCheckEndpoints = new[] { "/" };
                opts.StartupTimeout = TimeSpan.FromSeconds(120);
            })
            .Web<WebApp>(options =>
            {
                options.BaseUrl = new Uri("http://localhost:5000");
            })
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

> `WebApp` is a marker class that identifies your application under test. It can be an empty class.

## Your First Page Object

Define a page object that maps to a route in your application. Each method queues a browser action that executes when the chain is awaited:

```csharp
using FluentUIScaffold.Core.Pages;
using Microsoft.Playwright;

[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public HomePage VerifyHeadingVisible()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator("h1").WaitForAsync().ConfigureAwait(false);
        });
    }
}
```

Key points:
- The constructor must be `protected` and accept `IServiceProvider`.
- `[Route("/")]` declares the URL path this page maps to.
- `Enqueue<T>(...)` queues an action that receives a DI-resolved service at execution time. `Enqueue<IPage>` is the most common usage, injecting Playwright's `IPage`.
- Always use `.ConfigureAwait(false)` on awaits inside `Enqueue` callbacks.

For a deeper dive into page objects, see [Page Object Pattern](page-object-pattern.md).

## Your First Test

Each test creates a session (isolated browser context), navigates, interacts, and then disposes the session:

```csharp
[TestClass]
public class HomePageTests
{
    [TestMethod]
    public async Task HomePage_DisplaysHeading()
    {
        var session = await TestAssemblyHooks.App.CreateSessionAsync();
        try
        {
            await TestAssemblyHooks.App.NavigateTo<HomePage>()
                .VerifyHeadingVisible();
        }
        finally
        {
            await TestAssemblyHooks.App.DisposeSessionAsync();
        }
    }
}
```

`CreateSessionAsync()` opens a fresh browser context. `DisposeSessionAsync()` closes it. The session is stored as an instance field on `AppScaffold`, so each test must create and dispose its own session.

## Hosting Strategies

FluentUIScaffold supports four hosting strategies for managing the application under test.

### DotNet Hosting

`UseDotNetHosting` manages a .NET application via `dotnet run`. This is the most common strategy for testing ASP.NET Core apps.

```csharp
.UseDotNetHosting(opts =>
{
    opts.BaseUrl = new Uri("http://localhost:5000");
    opts.ProjectPath = "../MyApp/MyApp.csproj";
    opts.Framework = "net8.0";
    opts.Configuration = "Release";
    opts.HealthCheckEndpoints = new[] { "/", "/index.html" };
    opts.StartupTimeout = TimeSpan.FromSeconds(120);
    opts.ProcessName = "MyApp";
    opts.WorkingDirectory = "/path/to/working/dir";
})
```

**DotNetHostingOptions:**

| Property | Default | Description |
|---|---|---|
| `ProjectPath` | `""` | Path to the .csproj file (required) |
| `BaseUrl` | `null` | URL the app listens on (required) |
| `Framework` | `"net8.0"` | Target framework |
| `Configuration` | `"Release"` | Build configuration |
| `StartupTimeout` | 60 seconds | Time to wait for the app to start |
| `HealthCheckEndpoints` | `["/"]` | Endpoints to poll for readiness |
| `WorkingDirectory` | `null` | Working directory for the process |
| `ProcessName` | `null` | Process name for identification |

### Node Hosting

`UseNodeHosting` manages a Node.js application via `npm run`. Useful when your frontend runs as a standalone Node server.

```csharp
.UseNodeHosting(opts =>
{
    opts.BaseUrl = new Uri("http://localhost:3000");
    opts.ProjectPath = "../frontend";
    opts.Script = "start";
    opts.HealthCheckEndpoints = new[] { "/" };
    opts.StartupTimeout = TimeSpan.FromSeconds(60);
})
```

**NodeHostingOptions:**

| Property | Default | Description |
|---|---|---|
| `ProjectPath` | `""` | Path to the Node.js project directory (required) |
| `BaseUrl` | `null` | URL the app listens on (required) |
| `Script` | `"start"` | npm script to run |
| `StartupTimeout` | 60 seconds | Time to wait for the app to start |
| `HealthCheckEndpoints` | `["/"]` | Endpoints to poll for readiness |
| `WorkingDirectory` | `null` | Working directory for the process |

### External Server

`UseExternalServer` connects to a server that is already running. Ideal for CI environments or staging servers.

```csharp
.UseExternalServer(
    new Uri("http://localhost:5000"),
    "/health", "/ready")
```

The URI is the base URL and the additional arguments are health check endpoints to verify availability.

### Aspire Hosting

`UseAspireHosting<TEntryPoint>` integrates with .NET Aspire for distributed application testing. Aspire manages the full application lifecycle including dependencies.

```csharp
.UseAspireHosting<Projects.MyApp_AppHost>(
    appHost => { },
    "myapp")
```

For full details, see [Aspire Integration](aspire-integration.md).

## Configuration Options

The builder provides several configuration methods:

### HeadlessMode

```csharp
.WithHeadlessMode(true)   // Always headless
.WithHeadlessMode(false)  // Always visible
.WithHeadlessMode(null)   // Auto-detect: headless unless debugger is attached
```

When set to `null` (the default), the browser runs headless unless a debugger is attached, which is convenient for local development.

### SlowMo

Add a delay between browser actions for debugging:

```csharp
.Web<WebApp>(options =>
{
    options.SlowMo = 500; // 500ms delay between actions
})
```

### EnvironmentName

Set the logical environment name (defaults to `"Testing"`):

```csharp
.WithEnvironmentName("Development")
```

> Note: `"Production"` is rejected to prevent accidental production testing.

### SPA Proxy

Enable or disable ASP.NET Core's SPA development proxy:

```csharp
.WithSpaProxy(true)
```

### Environment Variables

Pass environment variables to the hosted application:

```csharp
.WithEnvironmentVariable("MY_SETTING", "value")
```

Or set them in bulk via the options:

```csharp
.Web<WebApp>(options =>
{
    options.EnvironmentVariables["MY_SETTING"] = "value";
})
```

## Next Steps

- [Page Object Pattern](page-object-pattern.md) -- deep dive into building page objects, deferred execution, and cross-page navigation
- [Aspire Integration](aspire-integration.md) -- testing distributed applications with .NET Aspire
- [API Reference](api-reference.md) -- complete API documentation
