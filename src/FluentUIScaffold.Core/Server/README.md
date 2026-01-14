# FluentUIScaffold Server Lifecycle Management

This directory contains the server lifecycle management system for FluentUIScaffold, providing deterministic startup, health checking, and cleanup for .NET applications started via `dotnet run`.

> **Note:** For Aspire applications, use `UseAspireHosting<T>()` in `FluentUIScaffold.AspireHosting` which delegates to Aspire's `DistributedApplicationTestingBuilder`. The components in this directory are used for non-Aspire scenarios.

## Overview

The server lifecycle management system provides:

- **Process persistence**: Reuse existing healthy servers across test runs
- **Configuration drift detection**: Restart servers when configuration changes
- **Health checking**: Wait for servers to become ready before proceeding with tests
- **Orphan cleanup**: Automatically clean up stale server processes
- **CI/headless support**: Disable SpaProxy and build assets when needed
- **Port management**: Support fixed ports for deterministic environments

## Components

### Core Interfaces

- **`IServerManager`**: Main interface for server lifecycle management
- **`ServerStatus`**: Immutable record representing server state
- **`IProcessRegistry`**: Handles persistent server state across runs
- **`IProcessLauncher`**: Manages process creation and output streaming
- **`IHealthWaiter`**: Waits for servers to become healthy

### Implementations

- **`DotNetServerManager`**: Main implementation for .NET applications
- **`ProcessRegistry`**: File-based process state persistence
- **`ProcessLauncher`**: Process creation with output streaming
- **`HealthWaiter`**: Health checking using readiness probes
- **`ConfigHasher`**: Deterministic configuration hashing

## Usage Examples

### Basic Usage

```csharp
// Create server configuration
var serverConfig = ServerConfiguration.CreateAspireServer(
    new Uri("http://localhost:5000"),
    "/path/to/AppHost.csproj")
    .WithHealthCheckEndpoints("/", "/health")
    .WithHeadless(true)
    .WithFixedPorts(new Dictionary<string, int> { ["web"] = 5000 })
    .Build();

// Configure FluentUIScaffold with server management
var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
{
    options.WithServerConfiguration(serverConfig);
    options.WithBaseUrl(new Uri("http://localhost:5000"));
});
```

### Advanced Configuration

```csharp
// Create server configuration with all options
var serverConfig = ServerConfiguration.CreateAspireServer(
    new Uri("http://localhost:5174"),
    "./src/MyApp.AppHost/MyApp.AppHost.csproj")
    .WithHealthCheckEndpoints("/", "/health", "/ready")
    .WithForceRestartOnConfigChange(true)
    .WithKillOrphansOnStart(true)
    .WithFixedPorts(new Dictionary<string, int> 
    { 
        ["web"] = 5174, 
        ["api"] = 5175 
    })
    .WithHeadless(true)
    .WithAssetsBuild(async ct =>
    {
        // Custom SPA build logic
        await BuildSpaAssetsAsync(ct);
    })
    .Build();

// Use with custom service provider
var services = new ServiceCollection();
services.AddSingleton<IServerManager, DotNetServerManager>();
// ... register other services

var serviceProvider = services.BuildServiceProvider();

var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
{
    options.WithServerConfiguration(serverConfig);
    options.WithServiceProvider(serviceProvider);
});
```

### Manual Server Management

```csharp
// Direct server manager usage
var serverManager = new DotNetServerManager();
var logger = loggerFactory.CreateLogger<DotNetServerManager>();

try
{
    var status = await serverManager.EnsureStartedAsync(launchPlan, logger);
    Console.WriteLine($"Server running on PID {status.Pid} at {status.BaseUrl}");
    
    // Run tests...
    
    await serverManager.RestartAsync(); // If needed
}
finally
{
    await serverManager.StopAsync();
}
```

## Configuration Hash

The system computes a deterministic hash of server configuration to detect changes:

- Executable path and arguments
- Environment variables (sorted)
- Base URL and timeout settings
- Health check configuration
- Working directory

When configuration changes, the server is automatically restarted.

## State Persistence

Server state is persisted to platform-specific locations:

- **Windows**: `%LOCALAPPDATA%/FluentUIScaffold/servers/{configHash}/state.json`
- **Linux/macOS**: `${XDG_RUNTIME_DIR}/fluentuiscaffold/servers/{configHash}/state.json`

State includes process ID, start time, configuration hash, and health status.

## CI/Headless Support

The system automatically detects CI environments and:

- Disables SpaProxy (`ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=""`)
- Enables headless mode
- Builds SPA assets if `dist/` folder is missing
- Uses in-memory databases when configured

Environment variables detected:
- `CI=true`
- `GITHUB_ACTIONS`
- `AZURE_PIPELINES`
- `JENKINS_URL`

## Migration from WebServerManager

The new system coexists with the existing `WebServerManager`. To migrate:

1. Replace `WebServerManager.StartServerAsync()` calls with server configuration
2. Use `WithServerConfiguration()` in options builder
3. Remove manual server lifecycle code from tests

```csharp
// Old approach
await WebServerManager.StartServerAsync(launchPlan);

// New approach
var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
{
    options.WithServerConfiguration(launchPlan);
});
```

## Error Handling

The system provides detailed error information:

- Process startup failures with stderr output
- Health check timeouts with diagnostic information
- Configuration validation errors
- Port binding conflicts

All errors are logged through the provided `ILogger` instance.

## Testing

Comprehensive test coverage is provided for all components:

- `ConfigHasherTests`: Configuration hashing logic
- `ProcessRegistryTests`: State persistence and cleanup
- `DotNetServerManagerTests`: Full lifecycle management scenarios

Run tests with:
```bash
dotnet test --filter "FullyQualifiedName~Server"
```