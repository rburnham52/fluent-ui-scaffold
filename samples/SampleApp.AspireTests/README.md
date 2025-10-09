# SampleApp.AspireTests

This test project demonstrates the new **FluentUIScaffold Aspire Server Lifecycle Management** system in action. It showcases how to use Aspire AppHost for hosting applications during UI tests with automatic server lifecycle management.

## Overview

These tests demonstrate:

- **Automatic server startup** using Aspire AppHost configuration
- **Server lifecycle management** with health checking and readiness validation
- **Configuration drift detection** and automatic server restarts
- **CI/headless mode** support with automatic detection
- **Performance optimizations** through server reuse across test runs
- **Manual server management** for advanced scenarios

## Test Categories

### Server Lifecycle Tests
- `EnsureStartedAsync_WithAspireAppHost_StartsServerSuccessfully` - Basic server startup with Aspire
- `ManualServerManagement_WithAspireAppHost_DemonstratesFullLifecycle` - Manual lifecycle control
- `ConfigurationDriftDetection_WithDifferentConfigurations_RestartsServerAppropriately` - Config change handling

### Health Check Validation
- `HealthCheckValidation_WithCustomEndpoints_ValidatesServerReadiness` - Multiple endpoint health checking

### CI/CD Integration
- `CIHeadlessMode_WithAutomaticDetection_ConfiguresAppropriately` - Automatic CI environment detection

### Performance Tests
- `ServerReusePerformance_WithSameConfiguration_ReusesFastly` - Server reuse performance validation

## Running the Tests

### Prerequisites

1. .NET 8.0 SDK
2. Node.js (for SPA development server)
3. Aspire workload: `dotnet workload install aspire`

### Command Line

```bash
# Run all Aspire lifecycle tests
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj

# Run specific test category
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --filter TestCategory=ServerLifecycle

# Run with verbose output
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --logger "console;verbosity=detailed"

# Run in parallel (faster execution)
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj --parallel
```

### IDE

Open the solution in Visual Studio or Rider and run the tests through the Test Explorer.

## Key Features Demonstrated

### 1. Automatic Server Management

```csharp
var serverConfig = ServerConfiguration.CreateAspireServer(
    TestBaseUrl,
    Path.Combine(projectRoot, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
    .WithHealthCheckEndpoints("/", "/weatherforecast")
    .WithHeadless(true)
    .Build();

using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
{
    options.WithServerConfiguration(serverConfig);
    options.WithBaseUrl(TestBaseUrl);
});
// Server automatically started and will be stopped when disposed
```

### 2. Manual Lifecycle Control

```csharp
var serverManager = new AspireServerManager();
try
{
    var status = await serverManager.EnsureStartedAsync(launchPlan, logger);
    // Use the server...
    await serverManager.RestartAsync(); // If needed
}
finally
{
    await serverManager.StopAsync();
}
```

### 3. CI/Headless Integration

```csharp
var serverConfig = ServerConfiguration.CreateAspireServer(baseUrl, projectPath)
    .WithAutoCI() // Automatically configures for CI environments
    .WithHeadless(true)
    .Build();
```

## Configuration Options

### Health Checks
- Custom endpoints for readiness validation
- Timeout configuration for startup validation
- Multiple endpoint support

### CI/CD Support
- Automatic headless mode detection
- SPA asset building in CI environments
- Environment variable configuration

### Performance Optimizations
- Server process reuse across test runs
- Configuration drift detection
- Orphan process cleanup

## Integration with Existing Tests

These tests complement the existing `SampleApp.Tests` project by demonstrating:

1. **Migration path** from WebServerManager to the new lifecycle system
2. **Advanced scenarios** not covered in basic tests
3. **Performance characteristics** of the new system
4. **CI/CD integration patterns**

## Troubleshooting

### Common Issues

1. **Port conflicts**: Tests use ports 5100-5105. Ensure these are available.
2. **Aspire workload**: Install with `dotnet workload install aspire`
3. **Node.js requirements**: SPA components need Node.js for asset building
4. **Timeout issues**: Increase `TestTimeout` for slower environments

### Debugging

- Set `ASPIRE_ALLOW_UNSECURED_TRANSPORT=true` for local debugging
- Use `--logger "console;verbosity=detailed"` for verbose test output
- Check server logs in console output during test execution

### CI/CD Configuration

For CI environments, ensure:
- `CI=true` environment variable is set
- Node.js is available for SPA asset building  
- Sufficient timeout values for server startup
- Headless browser capabilities are installed

## Architecture

The tests demonstrate the complete Aspire integration stack:

```
SampleApp.AspireTests
├── Uses: FluentUIScaffold.Core (Server lifecycle management)
├── Uses: FluentUIScaffold.Playwright (UI automation)
├── Hosts: SampleApp.AppHost (Aspire orchestration)
└── Tests: SampleApp (Web application)
```

This provides a complete end-to-end testing solution with managed server lifecycle, making tests more reliable and faster through server reuse.