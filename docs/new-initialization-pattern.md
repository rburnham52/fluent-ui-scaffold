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
    HeadlessMode = true // Run in headless mode for CI/CD
};

var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
```

## Auto-Discovery Features

### Plugin Auto-Discovery

The framework automatically discovers and registers plugins from all loaded assemblies:

- **Skips test assemblies** to avoid test plugins
- **Skips plugins with "Test" or "Mock" in their names** to avoid test plugins
- **Falls back to MockPlugin** if no valid plugins are found
- **Handles exceptions gracefully** and continues with other plugins

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
public void BasicTest()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = TestConfiguration.BaseUri,
        DefaultWaitTimeout = TimeSpan.FromSeconds(10),
        LogLevel = LogLevel.Information,
        HeadlessMode = true
    };

    using var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
    
    // Use fluent API...
    fluentUIApp.NavigateTo<HomePage>()
        .WaitFor(e => e.CounterButton)
        .ClickCounter();
}
```

### Advanced Usage

```csharp
[Test]
public void AdvancedTest()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = TestConfiguration.BaseUri,
        DefaultWaitTimeout = TimeSpan.FromSeconds(10),
        LogLevel = LogLevel.Information,
        HeadlessMode = true
    };

    using var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);
    
    // Navigate and perform actions
    fluentUIApp.NavigateTo<HomePage>()
        .WaitFor(e => e.UsernameInput)
        .LoginAsAdmin()
        .WaitFor<Dashboard>()
        .VerifyUserIsLoggedIn();
}
``` 