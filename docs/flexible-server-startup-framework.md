# Flexible Server Startup Framework

> **Note:** This document describes the legacy `WebServerManager` approach. The recommended approach is to use `IHostingStrategy` implementations via `FluentUIScaffoldBuilder`. See the [Architecture](architecture.md) documentation for the new hosting strategies.

## Overview

The Flexible Server Startup Framework is a comprehensive solution for launching web servers in test scenarios. It provides a flexible, extensible architecture that supports multiple server types and configurable startup options.

## New Recommended Approach: IHostingStrategy

The `IHostingStrategy` abstraction provides a unified way to manage application hosting:

```csharp
// Using the new builder pattern with hosting strategies
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .UseDotNetHosting(new Uri("http://localhost:5000"), "/path/to/project.csproj", opts =>
    {
        opts.WithFramework("net8.0");
        opts.WithConfiguration("Release");
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

Available hosting strategies:
- `DotNetHostingStrategy` - For .NET applications (`dotnet run`)
- `NodeHostingStrategy` - For Node.js applications (`npm run`)
- `ExternalHostingStrategy` - For pre-started servers
- `AspireHostingStrategy` - For Aspire distributed applications

## Legacy Architecture

### Core Components

1. **`WebServerManager`** - The main orchestrator that manages server lifecycle
2. **`IServerLauncher`** - Strategy interface for different server types
3. Removed: automatic project detection
4. **`ServerLauncherFactory`** - Factory for creating and managing launchers and detectors
5. **`ServerConfiguration`** - Configuration class for server startup parameters
6. **`IProcessRunner` / `IProcess`** - Abstractions to start and manage external processes (testable wrapper over `System.Diagnostics.Process`)
7. **`IClock`** - Time abstraction to control delays and timing in tests
8. **`IReadinessProbe`** - Strategy interface to centralize server readiness checks (default: `HttpReadinessProbe`)
9. **`LaunchPlan`** - Value object describing the planned launch (executable, arguments, environment)

### Design Patterns

- **Strategy Pattern**: `IServerLauncher` and `IProjectDetector` implementations; `IReadinessProbe`
- **Factory Pattern**: `ServerLauncherFactory` manages component registration and creation
- **Singleton Pattern**: `WebServerManager` provides singleton access for test scenarios

## Benefits

- **Framework Agnostic**: Works with any UI testing framework (MSTest, NUnit, xUnit)
- **Multi-Server Support**: Built-in support for ASP.NET Core and Aspire App Host
- **Automatic Project Detection**: Finds projects without relying on Git repositories
- **Flexible Configuration**: Supports explicit configuration or automatic detection
- **Extensible**: Easy to add new server types and project detectors
- **Robust Error Handling**: Comprehensive logging and error recovery

## Usage Examples

### Basic Usage

```csharp
var serverConfig = ServerConfiguration.CreateDotNetServer(
        new Uri("http://localhost:5000"),
        "/path/to/project.csproj")
    .WithFramework("net8.0")
    .WithConfiguration("Release")
    .WithHealthCheckEndpoints("/", "/index.html")
    .Build();

await WebServerManager.StartServerAsync(serverConfig);
```

### Explicit Configuration

```csharp
var serverConfig = ServerConfiguration.CreateDotNetServer(
        new Uri("http://localhost:5000"),
        "/path/to/project.csproj")
    .WithConfiguration("Release")
    .WithSpaProxy(true)
    .WithAspNetCoreEnvironment("Development")
    .WithStartupTimeout(TimeSpan.FromSeconds(90))
    .Build();

await WebServerManager.StartServerAsync(serverConfig);
```

### MSTest Integration

```csharp
[TestClass]
public class TestAssemblyHooks
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        var serverConfig = ServerConfiguration.CreateDotNetServer(
                new Uri("http://localhost:5000"),
                "./path/to/project.csproj")
            .WithFramework("net8.0")
            .WithConfiguration("Release")
            .WithAspNetCoreEnvironment("Development")
            .Build();

        WebServerManager.StartServerAsync(serverConfig).Wait();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        WebServerManager.StopServer();
    }
}
```

## Server Types

### ASP.NET Core Application

```csharp
var config = ServerConfiguration.CreateDotNetServer(
    new Uri("http://localhost:5000"),
    "/path/to/project.csproj"
);
```

### Aspire App Host Application

```csharp
var config = ServerConfiguration.CreateAspireServer(
    new Uri("http://localhost:5000"),
    "/path/to/apphost.csproj"
);
```

## Project Detection

The framework includes two project detectors:

1. **`EnvironmentBasedProjectDetector`** - Checks environment variables and configuration files
2. **`GitBasedProjectDetector`** - Finds projects relative to Git repository root

### Custom Project Detector

```csharp
public class CustomProjectDetector : IProjectDetector
{
    public string Name => "CustomProjectDetector";
    public int Priority => 100;

    public string? DetectProject(ProjectDetectionContext context)
    {
        // Custom detection logic
        return detectedProjectPath;
    }
}

// Register the detector
var factory = new ServerLauncherFactory();
factory.RegisterDetector(new CustomProjectDetector());
```

## Configuration Options

### FluentUIScaffoldOptions

- `BaseUrl` - The base URL the tests will use
- `DefaultWaitTimeout`, `HeadlessMode`, `SlowMo`, `RequestedDriverType` – framework runtime settings

### ServerConfiguration

- `ServerType` - Type of server (aspnetcore, aspire)
- `ProjectPath` - Path to the project file
- `WorkingDirectory` - Working directory for the server process
- `BaseUrl` - Base URL for the server
- `Arguments` - Additional command line arguments
- `EnvironmentVariables` - Environment variables to set
- `EnableSpaProxy` - Whether to enable SPA proxy
- `StartupTimeout` - Timeout for server startup
- `HealthCheckEndpoints` - Endpoints to check for server readiness

### Readiness Probes

- Default readiness is provided by `HttpReadinessProbe`, which queries `BaseUrl` and any configured `HealthCheckEndpoints` until the server responds with a success status or times out.
- Custom readiness strategies can be implemented by providing your own `IReadinessProbe` and registering/injecting it for your launcher.

## Migration Guide

### From TestAssemblyWebHook

Replace:
```csharp
TestAssemblyWebHook.StartServerAsync(options);
```

With:
```csharp
WebServerManager.StartServerAsync(options);
```

### From WebServerLauncher

The new framework provides more flexibility and better error handling. Update your configuration to use `ServerConfiguration` and `WebServerManager`.

## Best Practices

1. **Use Explicit Configuration for CI/CD**: Provide explicit project paths in CI environments
2. **Enable Logging**: Use `ILogger` for debugging server startup issues
3. **Set Appropriate Timeouts**: Configure `StartupTimeout` based on your application's startup time
4. **Handle Cleanup**: Always call `StopServer()` in test cleanup
5. **Use Health Checks**: Configure appropriate health check endpoints for your application

## Troubleshooting

### Common Issues

1. **Server Startup Timeout**
   - Increase `StartupTimeout` in configuration
   - Check if the server is actually starting (logs, process list)
   - Verify health check endpoints are correct

2. **Project Not Found**
   - Enable logging to see detection attempts
   - Provide explicit `ProjectPath` in configuration
   - Check `AdditionalSearchPaths` configuration

3. **Port Conflicts**
   - The framework automatically checks for running servers
   - Kill existing processes manually if needed
   - Use different ports for different test runs

### Debugging

Enable detailed logging:

```csharp
var loggerFactory = LoggerFactory.Create(builder =>
    builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

var serverConfig = ServerConfiguration.CreateDotNetServer(
        new Uri("http://localhost:5000"),
        "/path/to/project.csproj")
    .Build();

await WebServerManager.StartServerAsync(serverConfig);
```

## Future Enhancements

- **Container Support**: Launch servers in Docker containers
- **Load Balancing**: Support for multiple server instances
- **Health Check Plugins**: Customizable health check strategies
- **Performance Monitoring**: Built-in performance metrics
- **Configuration Validation**: Validate configuration before startup
