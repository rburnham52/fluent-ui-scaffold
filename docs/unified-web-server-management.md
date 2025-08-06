# Unified Web Server Management

## Overview

The FluentUIScaffold framework now provides a unified approach to web server management that eliminates the redundancy between `TestAssemblyHooks` and `WebServerProjectPath` configurations.

## Problem

Previously, the framework had two different web server launch methods:

1. **Manual `TestAssemblyHooks`** - MSTest-specific, assembly-level server management
2. **Framework-managed `WebServerProjectPath`** - Per-test initialization with automatic cleanup

This created confusion and redundancy, as both approaches essentially did the same thing but in different ways.

## Solution

The new unified approach uses `TestAssemblyWebHook` as the foundation for all web server management:

### 1. Framework-Agnostic Components

```csharp
// Located in: src/FluentUIScaffold.Core/Configuration/WebServerLauncher.cs
public class WebServerLauncher : IDisposable
{
    // Framework-agnostic web server launcher
    public async Task LaunchWebServerAsync(string projectPath, Uri baseUrl, TimeSpan timeout)
}

// Located in: src/FluentUIScaffold.Core/Configuration/TestAssemblyWebHook.cs
public class TestAssemblyWebHook : IDisposable
{
    // Singleton pattern for assembly-level management
    public static TestAssemblyWebHook GetInstance(FluentUIScaffoldOptions options, ILogger? logger = null)
    
    // Unified server start method
    public static async Task StartServerAsync(FluentUIScaffoldOptions options)
    
    // Unified server stop method
    public static void StopServer()
}
```

### 2. MSTest-Specific Implementation

```csharp
// Located in: samples/SampleApp.Tests/TestAssemblyHooks.cs
[TestClass]
public class TestAssemblyHooks
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        StartServerAsync().Wait();
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        TestAssemblyWebHook.StopServer();
    }
}
```

### 3. Framework Integration

The `FluentUIScaffoldApp` now focuses on test-specific initialization:

```csharp
public async Task InitializeAsync()
{
    // Web server management is handled by TestAssemblyHooks at the assembly level
    // This method is kept for future extensibility
    await Task.CompletedTask;
}
```

## Benefits

### 1. **Single Source of Truth**
- All web server management goes through `TestAssemblyWebHook`
- Uses the proven `WebServerLauncher` internally
- Consistent behavior across all test frameworks

### 2. **Framework Agnostic**
- Works with MSTest, NUnit, xUnit, etc.
- No framework-specific dependencies
- Easy to adapt for different test runners

### 3. **Simplified Configuration**
- Remove redundant `WebServerProjectPath` from individual tests
- Assembly-level management is the preferred approach
- Cleaner test setup code

### 4. **Better Resource Management**
- Singleton pattern prevents multiple server instances
- Automatic cleanup and disposal
- Proper process management

## Usage Examples

### MSTest Assembly-Level (Recommended)

```csharp
// TestAssemblyHooks.cs
[AssemblyInitialize]
public static void AssemblyInitialize(TestContext context)
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = TestConfiguration.BaseUri,
        DefaultWaitTimeout = TimeSpan.FromSeconds(30),
        WebServerProjectPath = "path/to/your/project"
    };
    await TestAssemblyWebHook.StartServerAsync(options);
}

// Individual tests - no web server configuration needed
[TestMethod]
public async Task MyTest()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = TestConfiguration.BaseUri,
        DefaultWaitTimeout = TimeSpan.FromSeconds(10),
        LogLevel = LogLevel.Information,
        HeadlessMode = true
    };
    
    using var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
    await fluentUIApp.InitializeAsync();
    // Test logic...
}
```



## Migration Guide

### From Manual Web Server Management

1. **Replace manual process management** with `TestAssemblyWebHook`
2. **Remove hardcoded project paths** and use configuration
3. **Simplify cleanup logic** - handled automatically



## Configuration Options

| Option | Purpose | Default |
|--------|---------|---------|
| `BaseUrl` | The URL where the server should be accessible | Required |
| `WebServerProjectPath` | Path to the ASP.NET Core project | Required |
| `DefaultWaitTimeout` | Timeout for server startup | 30 seconds |
| `LogLevel` | Logging level for debugging | Information |

**Note:** These options are configured in the `TestAssemblyHooks` class, not in individual tests.

## Best Practices

1. **Use assembly-level management** - this is the only approach now
2. **Configure once** in `TestAssemblyHooks`, not in individual tests
3. **Keep test configuration minimal** - focus on test-specific settings
4. **Use proper cleanup** - the framework handles this automatically
5. **Monitor server output** - logs are captured for debugging

## Troubleshooting

### Server Won't Start
- Check `WebServerProjectPath` is correct
- Verify `BaseUrl` is accessible
- Check for port conflicts
- Review server output logs

### Multiple Server Instances
- Ensure singleton pattern is working
- Check for multiple assembly hooks
- Verify cleanup is called properly

### Performance Issues
- Assembly-level management is the only approach
- Monitor resource usage
- Consider server reuse strategies 