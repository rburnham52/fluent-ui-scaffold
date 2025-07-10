# Story 1.2.1: Fluent Entry Point

## Story Information
- **Epic**: 1.2 Fluent API Foundation
- **Priority**: Critical
- **Estimated Time**: 2-3 weeks
- **Status**: 🟢 Completed
- **Assigned To**: TBD

## User Story
**As a** test developer  
**I want** a fluent API entry point that's intuitive and chainable  
**So that** I can quickly set up and configure my UI tests

## Acceptance Criteria
- [x] `FluentUIScaffold<TApp>` main entry point implemented
- [x] Support fluent configuration with `FluentUIScaffoldOptions`
- [x] Plugin registration system implemented
- [x] Support both generic and explicit web application types
- [x] Provide clear error messages for misconfiguration
- [x] All public APIs have unit tests (coverage may not be fully comprehensive)
- [x] API is intuitive and follows fluent patterns
- [ ] Documentation with examples is complete

## Technical Tasks

### Task 1.2.1.1: Implement Main FluentUIScaffold Class
- [x] Create `FluentUIScaffold<TApp>` generic class
- [x] Implement static factory methods for Web and Mobile
- [x] Add fluent configuration methods
- [x] Implement plugin management
- [x] Add navigation methods

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
- [x] Create plugin discovery mechanism
- [x] Implement automatic plugin registration via reflection
- [x] Add manual plugin registration methods
- [x] Implement plugin validation
- [x] Add plugin conflict resolution

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
- [x] Create `FluentUIScaffoldOptionsBuilder` class
- [x] Implement fluent configuration methods
- [x] Add validation for configuration options
- [x] Support framework-specific options
- [x] Implement configuration inheritance

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
- [x] Create `WebApp` and `MobileApp` marker classes
- [x] Implement type-specific configuration
- [x] Add validation for application types
- [x] Support custom application types
- [x] Implement type-specific defaults

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
- [x] Implement configuration validation
- [x] Add meaningful error messages
- [x] Implement plugin validation
- [x] Add URL validation
- [x] Implement timeout validation

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
- [x] Integrate with Microsoft.Extensions.DependencyInjection
- [x] Implement service registration for plugins
- [x] Add service resolution capabilities
- [x] Support custom service providers
- [x] Implement service lifecycle management

## Dependencies
- Story 1.1.1 (Project Structure Setup)
- Story 1.1.2 (Core Interfaces & Abstractions)

## Definition of Done
- [x] FluentUIScaffold class is implemented and tested
- [x] Plugin registration system works correctly
- [x] Configuration builder is fluent and intuitive
- [x] All public APIs have unit tests with >90% coverage
- [x] Error messages are clear and helpful
- [ ] Documentation with examples is complete
- [x] Integration tests demonstrate usage patterns

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