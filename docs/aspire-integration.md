# Aspire Integration

FluentUIScaffold integrates with [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) for testing distributed applications. Aspire manages the full application lifecycle -- including databases, caches, and other dependencies -- so your tests run against a realistic environment without manual process management.

## Prerequisites

- **.NET 8.0 or later**
- **Docker** (Aspire uses containers for dependencies)
- **Aspire workload** installed:

```bash
dotnet workload install aspire
```

## Package Installation

Add the Aspire hosting package alongside the Playwright plugin:

```bash
dotnet add package FluentUIScaffold.AspireHosting
dotnet add package FluentUIScaffold.Playwright
```

The `FluentUIScaffold.AspireHosting` package targets net8.0 and net9.0.

## AppHost Setup

Your solution needs an Aspire AppHost project that defines the distributed application. A typical AppHost `Program.cs` looks like:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var app = builder.AddProject<Projects.MyApp>("myapp");

builder.Build().Run();
```

The string `"myapp"` is the **resource name** -- you will reference this when configuring FluentUIScaffold to discover the base URL.

## Test Project Setup

Create a test project and reference your AppHost project so that the `Projects.MyApp_AppHost` type is available:

```xml
<ProjectReference Include="..\MyApp.AppHost\MyApp.AppHost.csproj" />
```

Then set up your test assembly hooks:

```csharp
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;
using FluentUIScaffold.AspireHosting;

[TestClass]
public static class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        if (!TestEnvironmentHelper.CanRunAspireTests)
        {
            Assert.Inconclusive(
                "Aspire tests require Docker and the Aspire workload. Skipping.");
            return;
        }

        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .WithHeadlessMode(true)
            .UseAspireHosting<Projects.MyApp_AppHost>(
                appHost => { },
                "myapp")
            .Web<WebApp>(options => { })
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

## Docker Detection

Not every environment has Docker available. The `TestEnvironmentHelper.CanRunAspireTests` check lets you gracefully skip Aspire tests when prerequisites are missing:

```csharp
if (!TestEnvironmentHelper.CanRunAspireTests)
{
    Assert.Inconclusive(
        "Aspire tests require Docker and the Aspire workload. Skipping.");
    return;
}
```

This is particularly useful in CI pipelines where some build agents may not have Docker installed. Tests are reported as "Inconclusive" rather than failed.

## UseAspireHosting API

The `UseAspireHosting<TEntryPoint>` extension method configures Aspire-based hosting:

```csharp
.UseAspireHosting<Projects.MyApp_AppHost>(
    Action<IDistributedApplicationTestingBuilder> configure,
    string? baseUrlResourceName = null,
    string? baseUrlPrefix = null)
```

**Parameters:**

| Parameter | Type | Description |
|---|---|---|
| `configure` | `Action<IDistributedApplicationTestingBuilder>` | Callback to configure the Aspire testing builder |
| `baseUrlResourceName` | `string?` | Aspire resource name used to discover the application's base URL |
| `baseUrlPrefix` | `string?` | Prefix appended to the base URL (e.g., `"/#"` for hash-based SPA routing) |

`TEntryPoint` is the AppHost entry point class (e.g., `Projects.MyApp_AppHost`) and must be a reference type (`where TEntryPoint : class`).

## Base URL Discovery

When you provide `baseUrlResourceName`, FluentUIScaffold automatically discovers the base URL from the Aspire distributed application at startup. You do not need to set `BaseUrl` manually in the options -- it is resolved from the running Aspire resource.

```csharp
.UseAspireHosting<Projects.MyApp_AppHost>(
    appHost => { },
    "myapp")  // Discovers URL from the "myapp" resource
.Web<WebApp>(options => { })  // No BaseUrl needed
```

This eliminates hardcoded URLs and ensures tests connect to whatever port Aspire assigns.

## SPA Routing Prefix

If your application uses hash-based SPA routing, use the `baseUrlPrefix` parameter to prepend a prefix to route paths:

```csharp
.UseAspireHosting<Projects.MyApp_AppHost>(
    appHost => { },
    "myapp",
    "/#")  // Routes become http://host:port/#/path
```

This ensures that `NavigateTo<TPage>()` generates correct URLs for your SPA routing scheme.

## Accessing Aspire Resources

Use the `CreateHttpClient` extension method to get an `HttpClient` for any Aspire resource. This is useful for setting up test data or verifying API responses directly:

```csharp
HttpClient client = app.CreateHttpClient<WebApp>("myapi");
var response = await client.GetAsync("/api/health");
```

The method resolves `DistributedApplication` from DI and calls `CreateHttpClient(resourceName)` on it, returning a pre-configured `HttpClient` pointing at the correct address.

## Configuring the Aspire Builder

The `configure` callback gives you access to the `IDistributedApplicationTestingBuilder` for customizing the Aspire environment:

```csharp
.UseAspireHosting<Projects.MyApp_AppHost>(
    appHost =>
    {
        // Configure Aspire testing builder here
        // e.g., override settings, add test-specific configuration
    },
    "myapp")
```

## Differences from Standard Hosting

| Aspect | Standard Hosting | Aspire Hosting |
|---|---|---|
| **Process management** | FluentUIScaffold starts/stops the app via `dotnet run` or `npm run` | Aspire manages the full lifecycle |
| **Base URL** | Must be configured manually | Auto-discovered from Aspire resource |
| **Dependencies** | Must be started separately | Aspire orchestrates all dependencies |
| **Environment variables** | Applied as process-level vars by the hosting strategy | Applied as process-level vars during Aspire bootstrap |
| **Docker requirement** | Not required | Required |
| **Minimum .NET version** | .NET 6.0 | .NET 8.0 |

## Writing Tests

Once the assembly hooks are set up, tests look identical to standard FluentUIScaffold tests:

```csharp
[TestClass]
public class HomePageTests
{
    [TestMethod]
    public async Task HomePage_DisplaysWelcome()
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
}
```

Page objects, deferred execution chains, and cross-page navigation all work the same way regardless of the hosting strategy.

## Further Reading

- [Getting Started](getting-started.md) -- general project setup and hosting strategies
- [Page Object Pattern](page-object-pattern.md) -- building page objects and fluent chains
- [API Reference](api-reference.md) -- complete API documentation
