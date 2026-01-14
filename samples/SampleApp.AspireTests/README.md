# SampleApp.AspireTests

This test project demonstrates **FluentUIScaffold with Aspire Hosting** using the unified API. It showcases how to use `UseAspireHosting<T>()` for automatic application lifecycle management during UI tests.

## Overview

These tests demonstrate the recommended approach for Aspire-based E2E testing:

- **Unified API** using `FluentUIScaffoldBuilder` with `UseAspireHosting<T>()`
- **Automatic server lifecycle** via Aspire's `DistributedApplicationTestingBuilder`
- **Fluent page interactions** and verifications
- **CI/headless mode** support with automatic detection

## Test Categories

### Server Lifecycle Tests
- `EnsureStartedAsync_WithAspireAppHost_StartsServerSuccessfully` - Basic startup with Aspire
- `HealthCheckValidation_WithCustomEndpoints_ValidatesServerReadiness` - Endpoint health checking
- `MultiplePageNavigation_WithAspireHosting_WorksCorrectly` - Page navigation

### CI/CD Integration
- `CIHeadlessMode_WithAutomaticDetection_ConfiguresAppropriately` - Automatic CI detection

### Performance Tests
- `AppScaffoldReuse_WithSameConfiguration_WorksCorrectly` - App reuse validation

## Running the Tests

### Prerequisites

1. .NET 8.0 SDK
2. Node.js (for SPA development server)
3. Aspire workload: `dotnet workload install aspire`
4. Docker (for Aspire resource containers)

### Command Line

```bash
# Run all Aspire tests
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj

# Run specific test category
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --filter TestCategory=Aspire

# Run with verbose output
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --logger "console;verbosity=detailed"
```

## Recommended Pattern

### Test Setup (Assembly Level)

```csharp
[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UseAspireHosting<Projects.SampleApp_AppHost>(
                appHost => { /* configure if needed */ },
                "sampleapp")
            .Web<WebApp>(options =>
            {
                options.UsePlaywright();
                options.HeadlessMode = true;
            })
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

### Writing Tests

```csharp
[TestMethod]
public void Can_Navigate_And_Interact()
{
    var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();

    homePage
        .WaitForVisible(p => p.CounterButton)
        .Click(p => p.CounterButton)
        .Verify.TextContains(p => p.CounterValue, "1");
}
```

## Configuration

### Using Aspire Hosting

The `UseAspireHosting<T>()` method:
- Creates a `DistributedApplicationTestingBuilder` from Aspire.Hosting.Testing
- Auto-discovers the base URL from the named resource
- Manages the full distributed application lifecycle

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.SampleApp_AppHost>(
        appHost => { /* configure distributed app */ },
        "sampleapp")  // Resource name to get base URL from
    .Web<WebApp>(options =>
    {
        options.UsePlaywright();
        options.HeadlessMode = true;
    })
    .Build<WebApp>();
```

### Headless Mode

For CI environments:

```csharp
.Web<WebApp>(options =>
{
    options.UsePlaywright();
    options.HeadlessMode = true;  // Force headless
    // Or leave null for auto-detection based on environment
})
```

## Troubleshooting

### Common Issues

1. **Aspire workload not found**: Install with `dotnet workload install aspire`
2. **Docker not running**: Aspire requires Docker for resource containers
3. **Timeout issues**: Increase timeout in options or test configuration
4. **Port conflicts**: Aspire auto-allocates ports, but check Docker containers

### Debugging

- Set `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` for local debugging
- Use `--logger "console;verbosity=detailed"` for verbose test output

## Architecture

```
SampleApp.AspireTests
├── Uses: FluentUIScaffold.AspireHosting (UseAspireHosting extension)
├── Uses: FluentUIScaffold.Core (Unified API)
├── Uses: FluentUIScaffold.Playwright (UI automation)
├── Hosts: SampleApp.AppHost (Aspire orchestration)
└── Tests: SampleApp (Web application)
```
