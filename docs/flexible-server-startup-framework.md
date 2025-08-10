# Flexible Server Startup Framework

## Overview

The Flexible Server Startup Framework is a comprehensive solution for launching web servers in test scenarios. It provides a flexible, extensible architecture that supports multiple server types and configurable startup options.

## Architecture

### Core Components

1. **`WebServerManager`** - The main orchestrator that manages server lifecycle
2. **`IServerLauncher`** - Strategy interface for different server types
3. Removed: automatic project detection
4. **`ServerLauncherFactory`** - Factory for creating and managing launchers and detectors
5. **`ServerConfiguration`** - Configuration class for server startup parameters

### Design Patterns

- **Strategy Pattern**: `IServerLauncher` and `IProjectDetector` implementations
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

### Basic Usage with Automatic Project Detection

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("http://localhost:5000"),
    EnableWebServerLaunch = true,
        // Project detection is not used; specify ServerConfiguration or WebServerProjectPath explicitly
    ServerType = "aspnetcore"
};

await WebServerManager.StartServerAsync(options);
```

### Explicit Configuration

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("http://localhost:5000"),
    EnableWebServerLaunch = true,
    ServerConfiguration = new ServerConfiguration
    {
        ProjectPath = "/path/to/project.csproj",
        WorkingDirectory = "/path/to/project",
        ServerType = "aspnetcore",
        EnableSpaProxy = true,
        StartupTimeout = TimeSpan.FromSeconds(90),
        EnvironmentVariables =
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development"
        }
    }
};

await WebServerManager.StartServerAsync(options);
```

### MSTest Integration

```csharp
[TestClass]
public class TestAssemblyHooks
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        var options = new FluentUIScaffoldOptions
        {
            BaseUrl = new Uri("http://localhost:5000"),
            EnableWebServerLaunch = true,
            // Project detection removed in favor of explicit configuration
            ServerType = "aspnetcore"
        };

        WebServerManager.StartServerAsync(options).Wait();
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
var config = ServerConfiguration.CreateAspNetCore(
    new Uri("http://localhost:5000"),
    "/path/to/project.csproj"
);
```

### Aspire App Host Application

```csharp
var config = ServerConfiguration.CreateAspire(
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

- `BaseUrl` - The base URL for the web server
- `EnableWebServerLaunch` - Whether to launch a web server
Removed: `EnableProjectDetection` (explicit configuration is required)
- `ServerType` - The type of server to launch
- `AdditionalSearchPaths` - Additional paths to search for projects
- `ServerConfiguration` - Explicit server configuration

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

var options = new FluentUIScaffoldOptions
{
    // ... other options
};

await WebServerManager.StartServerAsync(options);
```

## Future Enhancements

- **Container Support**: Launch servers in Docker containers
- **Load Balancing**: Support for multiple server instances
- **Health Check Plugins**: Customizable health check strategies
- **Performance Monitoring**: Built-in performance metrics
- **Configuration Validation**: Validate configuration before startup
