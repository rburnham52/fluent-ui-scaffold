# FluentUIScaffold Initialization Pattern

## Overview

The FluentUIScaffold framework uses a simplified initialization pattern with auto-discovery of plugins and pages, similar to FluentTestScaffold. This eliminates the need for manual service configuration and makes the framework much easier to use.

## Constructor

The constructor takes a `FluentUIScaffoldOptions` object and automatically:

1. **Auto-discovers plugins** from loaded assemblies
2. **Auto-discovers pages** from loaded assemblies  
3. **Configures the IOC container** with all discovered components
4. **Creates the driver** using the discovered plugins

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = TestConfiguration.BaseUri,
    DefaultWaitTimeout = TimeSpan.FromSeconds(10),
    LogLevel = LogLevel.Information,
    HeadlessMode = true, // Run in headless mode for CI/CD
    DebugMode = false, // Set to true for debugging (disables headless, sets SlowMo = 1000)
    // Optional: Enable web server launching
    EnableWebServerLaunch = true,
    WebServerProjectPath = "path/to/your/web/app",
    ReuseExistingServer = false
};

var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
await fluentUIApp.InitializeAsync(options); // Initialize with web server launch if enabled
```

## Auto-Discovery Features

### Plugin Auto-Discovery

The framework automatically discovers and registers plugins from all loaded assemblies:

- **Skips test assemblies** to avoid test plugins
- **Skips plugins with "Test" or "Mock" in their names** to avoid test plugins
- **Falls back to MockPlugin** if no valid plugins are found
- **Handles exceptions gracefully** and continues with other plugins

### Web Server Launching

The framework supports automatic web server launching for applications that need to be started before testing:

- **Automatic Launch**: Uses `dotnet run` to launch the web application
- **Wait for Readiness**: Waits for the server to be accessible at the configured base URL
- **Process Management**: Automatically cleans up the server process when tests complete
- **Reuse Option**: Can reuse an already running server to avoid conflicts

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("https://localhost:5001"),
    EnableWebServerLaunch = true,
    WebServerProjectPath = "path/to/your/web/app",
    ReuseExistingServer = false
};
```

### Debug Mode

The framework supports a debug mode that makes testing easier during development:

- **Non-Headless Mode**: Automatically disables headless mode to show the browser window
- **SlowMo**: Sets SlowMo to 1000ms to slow down interactions for better visibility
- **Detailed Logging**: Provides enhanced logging of browser actions
- **Automatic Detection**: Automatically enables when a debugger is attached (no configuration needed!)

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("https://localhost:5001"),
    // DebugMode automatically enables when debugger is attached
    // You can also explicitly set it: DebugMode = true
    // When DebugMode is true:
    // - HeadlessMode is automatically set to false
    // - SlowMo is automatically set to 1000ms
};
```

**Debug Mode Behavior:**
- **Development**: Automatically activates when you run tests in debug mode (F5 in Visual Studio, or when a debugger is attached)
- **CI/CD**: Remains disabled in CI/CD environments where no debugger is attached
- **Manual Control**: You can explicitly set `DebugMode = true` for manual control
- **Fallback**: You can still set `HeadlessMode` and `FrameworkOptions["SlowMo"]` manually for more control

### Page Auto-Discovery

The framework automatically discovers and registers pages from all loaded assemblies:

- **Finds all types inheriting from `BasePageComponent<,>`**
- **Registers them as transient services** in the IOC container
- **Determines URL patterns** using convention-based mapping
- **Handles exceptions gracefully** and continues with other pages

### URL Pattern Convention

Pages are automatically mapped to URLs using this convention:

- **HomePage** → `/home`
- **LoginPage** → `/login`  
- **RegistrationPage** → `/registration`
- **DashboardPage** → `/dashboard`

The framework converts PascalCase to kebab-case and removes the "Page" suffix.

## Fluent API Usage

With the initialization, you can use a clean fluent API:

```csharp
fluentUIApp.NavigateTo<HomePage>() // Resolves page from IOC and returns its fluent API
    .WaitFor(e => e.UsernameInput) // Uses base PageObject's common methods
    .LoginAsAdmin() // Use HomePage's custom method
    .WaitFor<Dashboard>() // Resolve from IOC and returns dashboard FluentAPI
    .VerifyUserIsLoggedIn(); // Use Dashboard's custom method
```

### WaitFor Methods

The framework provides convenient `WaitFor` methods:

```csharp
// Wait for a specific element on a page
fluentUIApp.WaitFor<HomePage>(e => e.CounterButton);

// Wait for a page to be ready
fluentUIApp.WaitFor<Dashboard>();
```

## Benefits

1. **Simplified Setup**: No manual service configuration required
2. **Auto-Discovery**: Plugins and pages are automatically found and registered
3. **Convention-Based**: URL patterns are automatically determined
4. **Error Handling**: Graceful handling of discovery failures
5. **Fallback Support**: MockPlugin fallback ensures tests always work
6. **Clean API**: Fluent API with `WaitFor` methods for better readability

## Usage Examples

### Basic Setup

```csharp
[Test]
public async Task BasicTest()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = TestConfiguration.BaseUri,
        DefaultWaitTimeout = TimeSpan.FromSeconds(10),
        LogLevel = LogLevel.Information,
        HeadlessMode = true
    };

    using var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
    await fluentUIApp.InitializeAsync(options);
    
    // Use fluent API...
    fluentUIApp.NavigateTo<HomePage>()
        .WaitFor(e => e.CounterButton)
        .ClickCounter();
}
```

### Advanced Usage with Web Server Launching

```csharp
[Test]
public async Task AdvancedTest()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = TestConfiguration.BaseUri,
        DefaultWaitTimeout = TimeSpan.FromSeconds(10),
        LogLevel = LogLevel.Information,
        HeadlessMode = true,
        // Enable web server launching
        EnableWebServerLaunch = true,
        WebServerProjectPath = "path/to/your/web/app",
        ReuseExistingServer = false
    };

    using var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
    await fluentUIApp.InitializeAsync(options); // This will launch the web server
    
    // Navigate and perform actions
    fluentUIApp.NavigateTo<HomePage>()
        .WaitFor(e => e.UsernameInput)
        .LoginAsAdmin()
        .WaitFor<Dashboard>()
        .VerifyUserIsLoggedIn();
}
``` 