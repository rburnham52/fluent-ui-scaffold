# Story 1.2.1: Fluent Entry Point

## Story Information
- **Epic**: 1.2 Fluent API Foundation
- **Priority**: Critical
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD

## User Story
**As a** test developer  
**I want** a fluent API entry point that's intuitive and chainable  
**So that** I can quickly set up and configure my UI tests

## Acceptance Criteria
- [ ] `FluentUIScaffold<TApp>` main entry point implemented
- [ ] Support fluent configuration with `FluentUIScaffoldOptions`
- [ ] Plugin registration system implemented
- [ ] Support both generic and explicit web application types
- [ ] Provide clear error messages for misconfiguration
- [ ] All public APIs have comprehensive unit tests
- [ ] API is intuitive and follows fluent patterns
- [ ] Documentation with examples is complete

## Technical Tasks

### Task 1.2.1.1: Implement Main FluentUIScaffold Class
- [ ] Create `FluentUIScaffold<TApp>` generic class
- [ ] Implement static factory methods for Web and Mobile
- [ ] Add fluent configuration methods
- [ ] Implement plugin management
- [ ] Add navigation methods

```csharp
public class FluentUIScaffold<TApp> where TApp : class
{
    private readonly FluentUIScaffoldOptions _options;
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    
    // Static factory methods
    public static FluentUIScaffold<WebApp> Web(Action<FluentUIScaffoldOptions> configureOptions = null)
    public static FluentUIScaffold<MobileApp> Mobile(Action<FluentUIScaffoldOptions> configureOptions = null)
    
    // Fluent configuration
    public FluentUIScaffold<TApp> Configure(Action<FluentUIScaffoldOptions> configureOptions)
    public FluentUIScaffold<TApp> RegisterPlugin<TPlugin>() where TPlugin : IUITestingFrameworkPlugin
    public FluentUIScaffold<TApp> WithBaseUrl(string baseUrl)
    public FluentUIScaffold<TApp> WithTimeout(TimeSpan timeout)
    public FluentUIScaffold<TApp> WithWaitStrategy(WaitStrategy strategy)
    
    // Navigation
    public TPage NavigateTo<TPage>() where TPage : BasePageComponent<TApp>
    public FluentUIScaffold<TApp> NavigateToUrl(string url)
    
    // Framework access
    public TDriver Framework<TDriver>() where TDriver : class
}
```

### Task 1.2.1.2: Implement Plugin Registration System
- [ ] Create plugin discovery mechanism
- [ ] Implement automatic plugin registration via reflection
- [ ] Add manual plugin registration methods
- [ ] Implement plugin validation
- [ ] Add plugin conflict resolution

```csharp
public class PluginManager
{
    private readonly List<IUITestingFrameworkPlugin> _plugins = new();
    
    public void RegisterPlugin<TPlugin>() where TPlugin : IUITestingFrameworkPlugin
    public void RegisterPlugin(IUITestingFrameworkPlugin plugin)
    public void DiscoverPlugins()
    public IUIDriver CreateDriver(Type driverType, FluentUIScaffoldOptions options)
}
```

### Task 1.2.1.3: Implement Configuration Builder Pattern
- [ ] Create `FluentUIScaffoldOptionsBuilder` class
- [ ] Implement fluent configuration methods
- [ ] Add validation for configuration options
- [ ] Support framework-specific options
- [ ] Implement configuration inheritance

```csharp
public class FluentUIScaffoldOptionsBuilder
{
    public FluentUIScaffoldOptionsBuilder WithBaseUrl(string baseUrl)
    public FluentUIScaffoldOptionsBuilder WithTimeout(TimeSpan timeout)
    public FluentUIScaffoldOptionsBuilder WithRetryInterval(TimeSpan interval)
    public FluentUIScaffoldOptionsBuilder WithWaitStrategy(WaitStrategy strategy)
    public FluentUIScaffoldOptionsBuilder WithLogLevel(LogLevel level)
    public FluentUIScaffoldOptionsBuilder WithScreenshotPath(string path)
    public FluentUIScaffoldOptionsBuilder WithFrameworkOption(string key, object value)
    public FluentUIScaffoldOptions Build()
}
```

### Task 1.2.1.4: Implement Application Type Support
- [ ] Create `WebApp` and `MobileApp` marker classes
- [ ] Implement type-specific configuration
- [ ] Add validation for application types
- [ ] Support custom application types
- [ ] Implement type-specific defaults

```csharp
public class WebApp
{
    public static readonly WebApp Instance = new();
    private WebApp() { }
}

public class MobileApp
{
    public static readonly MobileApp Instance = new();
    private MobileApp() { }
}
```

### Task 1.2.1.5: Add Error Handling and Validation
- [ ] Implement configuration validation
- [ ] Add meaningful error messages
- [ ] Implement plugin validation
- [ ] Add URL validation
- [ ] Implement timeout validation

```csharp
public class FluentUIScaffoldValidationException : FluentUIScaffoldException
{
    public FluentUIScaffoldValidationException(string message, string property) : base(message)
    {
        Property = property;
    }
    
    public string Property { get; }
}
```

### Task 1.2.1.6: Implement Service Provider Integration
- [ ] Integrate with Microsoft.Extensions.DependencyInjection
- [ ] Implement service registration for plugins
- [ ] Add service resolution capabilities
- [ ] Support custom service providers
- [ ] Implement service lifecycle management

## Dependencies
- Story 1.1.1 (Project Structure Setup)
- Story 1.1.2 (Core Interfaces & Abstractions)

## Definition of Done
- [ ] FluentUIScaffold class is implemented and tested
- [ ] Plugin registration system works correctly
- [ ] Configuration builder is fluent and intuitive
- [ ] All public APIs have unit tests with >90% coverage
- [ ] Error messages are clear and helpful
- [ ] Documentation with examples is complete
- [ ] Integration tests demonstrate usage patterns

## Notes
- Follow fluent API design patterns from fluent-test-scaffold
- Ensure method chaining is intuitive
- Consider performance implications of fluent API
- Plan for async operations in future iterations
- Make error messages developer-friendly

## Related Documentation
- [Fluent API Design Patterns](https://github.com/rburnham52/fluent-test-scaffold)
- [.NET Dependency Injection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Builder Pattern in C#](https://docs.microsoft.com/en-us/dotnet/standard/modern-web-apps-azure-architecture/architectural-principles#the-builder-pattern) 