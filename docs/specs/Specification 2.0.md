# Fluent UI Scaffold V2.0 Specification

## Overview

Fluent UI Scaffold V2.0 is a framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. Unlike V1, which tightly coupled to specific frameworks, V2.0 focuses on the control flow and structure while leaving framework implementations as pluggable extensions.

## Key Design Principles

1. **Framework Agnostic**: Core framework is completely independent of any specific testing framework
2. **Dependency Injection First**: All components are resolved through IoC container
3. **Explicit Framework Exposure**: Base page components expose underlying framework drivers
4. **Base Element Actions**: Consistent, framework-agnostic element interactions
5. **Fluent API Control Flow**: Focus on structuring reusable actions and verification tasks
6. **Dual Generic Types**: Maintains fluent API context and type safety throughout method chains
7. **Separation of Concerns**: Clear boundaries between core framework and framework-specific implementations
8. **Internal Fluent Verification**: Chained verification methods for complex scenarios

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                    FluentUIScaffold V2.0                   │
├─────────────────────────────────────────────────────────────┤
│  Core Framework (Framework Agnostic)                       │
│  ├── FluentUIScaffoldBuilder                             │
│  ├── BasePageComponent<TDriver, TPage>                    │
│  ├── IUIDriver (Interface)                                │
│  ├── Base Element Actions (Click, Type, Select, etc.)     │
│  ├── Verification System (Verify, VerifyElement)          │
│  └── Plugin System                                        │
├─────────────────────────────────────────────────────────────┤
│  Framework Plugins (Framework Specific)                   │
│  ├── PlaywrightPlugin                                     │
│  ├── SeleniumPlugin                                       │
│  └── Future: Mobile, Desktop, etc.                       │
└─────────────────────────────────────────────────────────────┘
```

### Key Architectural Features

1. **Base Element Actions**: Framework-agnostic element interactions
2. **Fluent Verification API**: Internal fluent verification builder
3. **Dual Generic Types**: Maintains fluent API context and type safety
4. **Plugin Architecture**: Extensible framework support
5. **Dependency Injection**: IoC container for all components

## Core Components

### 1. FluentUIScaffoldBuilder

The main entry point that handles initialization and configuration:

```csharp
public static class FluentUIScaffoldBuilder
{
    public static FluentUIScaffoldApp<TApp> Web<TApp>(
        Action<FluentUIScaffoldOptions> configureOptions,
        Action<FrameworkOptions> configureFramework = null) 
        where TApp : class
    {
        var options = new FluentUIScaffoldOptions();
        configureOptions?.Invoke(options);

        var services = new ServiceCollection();
        ConfigureServices(services, options, configureFramework);
        var serviceProvider = services.BuildServiceProvider();

        var driver = serviceProvider.GetRequiredService<IUIDriver>();
        var logger = serviceProvider.GetRequiredService<ILogger<FluentUIScaffoldApp<TApp>>>();

        return new FluentUIScaffoldApp<TApp>(serviceProvider, driver, logger);
    }
}
```

### 2. BasePageComponent<TDriver, TPage>

The foundation for all page components that exposes the underlying framework and maintains fluent API context:

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    protected TDriver Driver { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected FluentUIScaffoldOptions Options { get; }

    protected BasePageComponent(IServiceProvider serviceProvider, Uri urlPattern)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Driver = serviceProvider.GetRequiredService<TDriver>();
        Logger = serviceProvider.GetRequiredService<ILogger<BasePageComponent<TDriver, TPage>>>();
        Options = serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
        
        UrlPattern = urlPattern;
        NavigateToUrl(urlPattern);
        ConfigureElements();
    }

    public Uri UrlPattern { get; }
    
    protected abstract void ConfigureElements();
    
    // Framework-agnostic element interaction methods
    protected virtual void ClickElement(string selector) => Driver.Click(selector);
    protected virtual void TypeText(string selector, string text) => Driver.Type(selector, text);
    protected virtual void SelectOption(string selector, string value) => Driver.SelectOption(selector, value);
    protected virtual string GetElementText(string selector) => Driver.GetText(selector);
    protected virtual bool IsElementVisible(string selector) => Driver.IsVisible(selector);
    protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);
    
    // Base element interaction methods for fluent API
    public virtual TPage Click(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.Click(element.Selector);
        return (TPage)this;
    }
    
    public virtual TPage Type(Func<TPage, IElement> elementSelector, string text)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.Type(element.Selector, text);
        return (TPage)this;
    }
    
    public virtual TPage Select(Func<TPage, IElement> elementSelector, string value)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.SelectOption(element.Selector, value);
        return (TPage)this;
    }
    
    public virtual TPage Focus(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.Focus(element.Selector);
        return (TPage)this;
    }
    
    public virtual TPage Hover(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.Hover(element.Selector);
        return (TPage)this;
    }
    
    public virtual TPage Clear(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.Clear(element.Selector);
        return (TPage)this;
    }
    
    // Navigation methods
    public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>
    {
        var targetPage = ServiceProvider.GetRequiredService<TTarget>();
        return targetPage;
    }
    
    // Framework-specific access - direct driver access
    protected TDriver Driver { get; private set; }
    
    // Verification access
    public IVerificationContext Verify => new VerificationContext(Driver, Options, Logger);
    
    // Generic verification methods for common scenarios
    public virtual TPage Verify<TValue>(Func<IElement, TValue> elementSelector, TValue expectedValue, string description = null)
    {
        var element = GetElementFromSelector(elementSelector);
        var actualValue = GetElementValue<TValue>(element);
        
        if (!EqualityComparer<TValue>.Default.Equals(actualValue, expectedValue))
        {
            var message = description ?? $"Expected '{expectedValue}', but got '{actualValue}'";
            throw new ElementValidationException(message);
        }
        
        return (TPage)this;
    }
    
    // Verify with default inner text comparison
    public virtual TPage Verify(Func<IElement, string> elementSelector, string expectedText, string description = null)
    {
        return Verify(elementSelector, expectedText, description);
    }
    
    // Verify with specific property comparison
    public virtual TPage Verify(Func<IElement, string> elementSelector, string expectedValue, string propertyName, string description = null)
    {
        var element = GetElementFromSelector(elementSelector);
        var actualValue = GetElementPropertyValue(element, propertyName);
        
        if (actualValue != expectedValue)
        {
            var message = description ?? $"Expected property '{propertyName}' to be '{expectedValue}', but got '{actualValue}'";
            throw new ElementValidationException(message);
        }
        
        return (TPage)this;
    }
    
    // Internal fluent verification API
    public virtual TPage Verify(Func<IElement, IElementVerificationBuilder> verificationBuilder)
    {
        var element = GetCurrentElement();
        var builder = verificationBuilder(element);
        builder.Execute();
        return (TPage)this;
    }
    
    // Common page methods that return the correct type for fluent API
    public virtual TPage VerifyPageTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        var pageTitle = Driver.GetPageTitle();
        if (!pageTitle.Equals(title, StringComparison.OrdinalIgnoreCase))
        {
            throw new ElementValidationException($"Expected page title '{title}', but got '{pageTitle}'.");
        }

        return (TPage)this;
    }
    
    public virtual TPage WaitForElement(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.WaitForElement(element.Selector);
        return (TPage)this;
    }
    
    public virtual TPage WaitForElementToBeVisible(Func<TPage, IElement> elementSelector)
    {
        var element = GetElementFromSelector(elementSelector);
        Driver.WaitForElementToBeVisible(element.Selector);
        return (TPage)this;
    }
}
```

### 3. Framework-Specific Driver Interface

```csharp
public interface IUIDriver : IDisposable
{
    Uri CurrentUrl { get; }
    
    // Core interactions
    void Click(string selector);
    void Type(string selector, string text);
    void SelectOption(string selector, string value);
    string GetText(string selector);
    bool IsVisible(string selector);
    bool IsEnabled(string selector);
    void WaitForElement(string selector);
    void WaitForElementToBeVisible(string selector);
    void WaitForElementToBeHidden(string selector);
    void Focus(string selector);
    void Hover(string selector);
    void Clear(string selector);
    
    // Navigation
    void NavigateToUrl(Uri url);
    
    // Framework-specific access
    TFramework GetFrameworkDriver<TFramework>() where TFramework : class;
}
```

### 4. Plugin System

```csharp
public interface IUITestingFrameworkPlugin
{
    string Name { get; }
    string Version { get; }
    Type DriverType { get; }
    
    void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options);
    IUIDriver CreateDriver(IServiceProvider serviceProvider, FluentUIScaffoldOptions options);
}

public abstract class FrameworkOptions
{
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DefaultRetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public bool CaptureScreenshotsOnFailure { get; set; } = true;
    public string ScreenshotPath { get; set; } = "./screenshots";
}

// Framework-specific options
public class PlaywrightOptions : FrameworkOptions
{
    public bool Headless { get; set; } = true;
    public int SlowMo { get; set; } = 0;
    public int ViewportWidth { get; set; } = 1280;
    public int ViewportHeight { get; set; } = 720;
}

public class SeleniumOptions : FrameworkOptions
{
    public string BrowserName { get; set; } = "chrome";
    public Dictionary<string, object> Capabilities { get; set; } = new();
}
```

## Configuration and Usage

### Basic Configuration

```csharp
// Configure FluentUIScaffold with Playwright
var fluentUI = FluentUIScaffoldBuilder.Web<WebApp>(options =>
{
    options.BaseUrl = new Uri("https://localhost:5001");
    options.LogLevel = LogLevel.Information;
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
}, frameworkOptions =>
{
    var playwrightOptions = new PlaywrightOptions
    {
        DefaultTimeout = TimeSpan.FromSeconds(30),
        DefaultRetryInterval = TimeSpan.FromMilliseconds(500),
        CaptureScreenshotsOnFailure = true,
        Headless = false,
        SlowMo = 1000
    };
    frameworkOptions.UsePlaywright(playwrightOptions);
});
```

### Page Component Implementation

```csharp
public class HomePage : BasePageComponent<PlaywrightDriver, HomePage>
{
    private IElement _counterButton;
    private IElement _counterText;
    private IElement _weatherDataElement;
    private IElement _navTodosButton;
    
    public HomePage(IServiceProvider serviceProvider) 
        : base(serviceProvider, new Uri("/"))
    {
    }
    
    protected override void ConfigureElements()
    {
        _counterButton = Element(".counter-button")
            .WithDescription("Counter Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
            
        _counterText = Element(".counter-text")
            .WithDescription("Counter Text")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _weatherDataElement = Element("[data-testid='weather-data']")
            .WithDescription("Weather Data Element")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _navTodosButton = Element("[data-testid='nav-todos']")
            .WithDescription("Navigation to Todos")
            .WithWaitStrategy(WaitStrategy.Clickable);
    }
    
    // Example of custom business logic methods
    public HomePage AddToCounter(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Click(e => _counterButton);
        }
        return this;
    }
    
    public HomePage VerifyCounterValue(string expectedValue)
    {
        return Verify(e => _counterText, expectedValue);
    }
    
    public HomePage WaitForWeatherData()
    {
        return WaitForElement(e => _weatherDataElement);
    }
    
    public TodosPage NavigateToTodos()
    {
        Click(e => _navTodosButton);
        return NavigateTo<TodosPage>();
    }
}
```

### Fluent API Usage

```csharp
[TestMethod]
public async Task Can_Add_Todo_And_Verify_Counter()
{
    fluentUI
        .NavigateTo<HomePage>()
        .VerifyPageTitle("FluentUIScaffold Sample App")
        .WaitForElement(e => e.WeatherDataElement)
        .Click(e => e.CounterButton)
        .Verify(e => e.CounterText, "1")
        .NavigateTo<TodosPage>()
        .Type(e => e.TodoInput, "Test Todo")
        .Click(e => e.AddTodoButton)
        .Verify(e => e.TodoCount, "1");
}
```

## Framework Plugin Implementation

### Playwright Plugin

```csharp
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Playwright";
    public string Version => "2.0.0";
    public Type DriverType => typeof(PlaywrightDriver);
    
    public void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options)
    {
        services.AddSingleton<PlaywrightOptions>(provider =>
        {
            var frameworkOptions = provider.GetService<FrameworkOptions>() as PlaywrightOptions;
            return frameworkOptions ?? new PlaywrightOptions();
        });
        
        services.AddSingleton<IUIDriver, PlaywrightDriver>();
        services.AddSingleton<PlaywrightDriver>();
    }
    
    public IUIDriver CreateDriver(IServiceProvider serviceProvider, FluentUIScaffoldOptions options)
    {
        var playwrightOptions = serviceProvider.GetRequiredService<PlaywrightOptions>();
        return new PlaywrightDriver(playwrightOptions);
    }
}
```

### Selenium Plugin

```csharp
public class SeleniumPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Selenium";
    public string Version => "2.0.0";
    public Type DriverType => typeof(SeleniumDriver);
    
    public void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options)
    {
        services.AddSingleton<SeleniumOptions>(provider =>
        {
            var frameworkOptions = provider.GetService<FrameworkOptions>() as SeleniumOptions;
            return frameworkOptions ?? new SeleniumOptions();
        });
        
        services.AddSingleton<IUIDriver, SeleniumDriver>();
        services.AddSingleton<SeleniumDriver>();
    }
    
    public IUIDriver CreateDriver(IServiceProvider serviceProvider, FluentUIScaffoldOptions options)
    {
        var seleniumOptions = serviceProvider.GetRequiredService<SeleniumOptions>();
        return new SeleniumDriver(seleniumOptions);
    }
}
```

## Verification System

```csharp
public interface IVerificationContext
{
    // Element verifications
    IVerificationContext ElementIsVisible(string selector);
    IVerificationContext ElementIsHidden(string selector);
    IVerificationContext ElementIsEnabled(string selector);
    IVerificationContext ElementIsDisabled(string selector);
    IVerificationContext ElementContainsText(string selector, string text);
    IVerificationContext ElementHasAttribute(string selector, string attribute, string value);
    
    // Page verifications
    IVerificationContext PageTitleContains(string text);
    IVerificationContext UrlMatches(string pattern);
    
    // Custom verifications
    IVerificationContext That(Func<bool> condition, string description);
    IVerificationContext That<T>(Func<T> actual, Func<T, bool> condition, string description);
}

public class VerificationContext : IVerificationContext
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly ILogger _logger;
    
    public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
    {
        _driver = driver;
        _options = options;
        _logger = logger;
    }
    
    public IVerificationContext ElementIsVisible(string selector)
    {
        _logger.LogInformation($"Verifying element '{selector}' is visible");
        if (!_driver.IsVisible(selector))
        {
            throw new VerificationException($"Element '{selector}' is not visible");
        }
        return this;
    }
    
    // Implement other verification methods...
}

// Internal fluent verification API
public interface IElementVerificationBuilder
{
    IElementVerificationBuilder HasText(string expectedText);
    IElementVerificationBuilder HasValue(string expectedValue);
    IElementVerificationBuilder IsVisible();
    IElementVerificationBuilder IsEnabled();
    IElementVerificationBuilder ContainsText(string expectedText);
    IElementVerificationBuilder IsDisabled();
    IElementVerificationBuilder HasAttribute(string attribute, string expectedValue);
    void Execute();
}

public class ElementVerificationBuilder : IElementVerificationBuilder
{
    private readonly IElement _element;
    private readonly List<Action> _verifications = new();
    
    public ElementVerificationBuilder(IElement element)
    {
        _element = element;
    }
    
    public IElementVerificationBuilder HasText(string expectedText)
    {
        _verifications.Add(() => {
            var actualText = _element.GetText();
            if (actualText != expectedText)
            {
                throw new ElementValidationException($"Expected text '{expectedText}', but got '{actualText}'");
            }
        });
        return this;
    }
    
    public IElementVerificationBuilder HasValue(string expectedValue)
    {
        _verifications.Add(() => {
            var actualValue = _element.GetValue();
            if (actualValue != expectedValue)
            {
                throw new ElementValidationException($"Expected value '{expectedValue}', but got '{actualValue}'");
            }
        });
        return this;
    }
    
    public IElementVerificationBuilder IsVisible()
    {
        _verifications.Add(() => {
            if (!_element.IsVisible())
            {
                throw new ElementValidationException("Element is not visible");
            }
        });
        return this;
    }
    
    public IElementVerificationBuilder IsEnabled()
    {
        _verifications.Add(() => {
            if (!_element.IsEnabled())
            {
                throw new ElementValidationException("Element is not enabled");
            }
        });
        return this;
    }
    
    public IElementVerificationBuilder IsDisabled()
    {
        _verifications.Add(() => {
            if (_element.IsEnabled())
            {
                throw new ElementValidationException("Element is enabled but should be disabled");
            }
        });
        return this;
    }
    
    public IElementVerificationBuilder ContainsText(string expectedText)
    {
        _verifications.Add(() => {
            var actualText = _element.GetText();
            if (!actualText.Contains(expectedText))
            {
                throw new ElementValidationException($"Element text '{actualText}' does not contain '{expectedText}'");
            }
        });
        return this;
    }
    
    public IElementVerificationBuilder HasAttribute(string attribute, string expectedValue)
    {
        _verifications.Add(() => {
            var actualValue = _element.GetAttribute(attribute);
            if (actualValue != expectedValue)
            {
                throw new ElementValidationException($"Expected attribute '{attribute}' to be '{expectedValue}', but got '{actualValue}'");
            }
        });
        return this;
    }
    
    public void Execute()
    {
        foreach (var verification in _verifications)
        {
            verification();
        }
    }
}
```

## Element Configuration System

```csharp
public interface IElement
{
    string Selector { get; }
    string Description { get; }
    TimeSpan Timeout { get; }
    WaitStrategy WaitStrategy { get; }
    TimeSpan RetryInterval { get; }
}

public class ElementBuilder
{
    private readonly string _selector;
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    
    public ElementBuilder(string selector, IUIDriver driver, FluentUIScaffoldOptions options)
    {
        _selector = selector;
        _driver = driver;
        _options = options;
    }
    
    public ElementBuilder WithDescription(string description)
    {
        // Implementation
        return this;
    }
    
    public ElementBuilder WithTimeout(TimeSpan timeout)
    {
        // Implementation
        return this;
    }
    
    public ElementBuilder WithWaitStrategy(WaitStrategy waitStrategy)
    {
        // Implementation
        return this;
    }
    
    public ElementBuilder WithRetryInterval(TimeSpan retryInterval)
    {
        // Implementation
        return this;
    }
    
    public IElement Build()
    {
        return new Element(_selector, _driver, _options);
    }
}
```

## Error Handling and Logging

```csharp
public class FluentUIScaffoldException : Exception
{
    public string ScreenshotPath { get; set; }
    public string DOMState { get; set; }
    public string CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; set; }
    
    public FluentUIScaffoldException(string message, Exception innerException = null) 
        : base(message, innerException)
    {
        Context = new Dictionary<string, object>();
    }
}

public class VerificationException : FluentUIScaffoldException
{
    public VerificationException(string message) : base(message) { }
}

public class ElementNotFoundException : FluentUIScaffoldException
{
    public ElementNotFoundException(string selector) 
        : base($"Element with selector '{selector}' was not found") { }
}
```

## Migration from V1 to V2.0

### Key Changes

1. **Constructor Changes**: Page components now take `IServiceProvider` instead of individual dependencies
2. **Framework Exposure**: Base page components now expose the underlying framework driver
3. **Dependency Injection**: All components are resolved through IoC container
4. **Plugin System**: Framework-specific code is now in separate plugins
5. **Configuration**: Framework-specific options are separated from core options

### Migration Example

**V1 Code:**
```csharp
public class HomePage : BasePageComponent<WebApp>
{
    public HomePage(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
        : base(driver, options, logger)
    {
        ConfigureElements();
    }
}
```

**V2.0 Code:**
```csharp
public class HomePage : BasePageComponent<PlaywrightDriver>
{
    public HomePage(IServiceProvider serviceProvider) 
        : base(serviceProvider, new Uri("/"))
    {
    }
}
```

## Benefits of V2.0

1. **True Framework Agnosticism**: Core framework is completely independent
2. **Better Testability**: All dependencies are injected
3. **Direct Framework Access**: Developers can access framework-specific features directly through the `Driver` property
4. **Plugin Architecture**: Easy to add new framework support
5. **Cleaner Separation**: Clear boundaries between core and framework-specific code
6. **Better Maintainability**: Less coupling between components
7. **Extensibility**: Easy to add new features without affecting existing code
8. **Simplified API**: Direct access to framework drivers without additional method calls
9. **Fluent API Context**: Second generic type parameter ensures proper return types for method chaining
10. **Type Safety**: Compile-time guarantees for fluent API method chains

## Future Extensions

1. **Mobile Support**: Android/iOS testing frameworks
2. **Desktop Support**: WinAppDriver, etc.
3. **Visual Testing**: Screenshot comparison, visual regression testing
4. **Performance Testing**: Integration with performance monitoring
5. **Accessibility Testing**: WCAG compliance testing
6. **Multi-Browser Testing**: Parallel execution across browsers
7. **Cloud Testing**: Integration with cloud-based testing platforms

## Refined API Patterns

### 1. Base Element Actions

The V2.0 specification provides consistent base element actions that work across all frameworks:

```csharp
// Element interactions
.Click(e => e.Button)                    // Click an element
.Type(e => e.Input, "text")              // Type text into an element
.Select(e => e.Dropdown, "option")       // Select an option from dropdown
.Focus(e => e.Input)                     // Focus on an element
.Hover(e => e.Tooltip)                   // Hover over an element
.Clear(e => e.Input)                     // Clear element content

// Element waiting
.WaitForElement(e => e.LoadingSpinner)   // Wait for element to exist
.WaitForElementToBeVisible(e => e.Content) // Wait for element to be visible
```

### 2. Verification Patterns

Multiple verification patterns are available to reduce repetition and improve maintainability:

```csharp
// Simple value comparison (defaults to inner text)
.Verify(e => e.SubTotalElement, "$299.99")

// Explicit property comparison
.Verify(e => e.StatusElement, "enabled", "className")

// Internal fluent verification API
.Verify(e => e.ShippingCost.HasText("$15.99"))
.Verify(e => e.TotalAmount.IsVisible().HasValue("$339.98"))
.Verify(e => e.CheckoutButton.IsEnabled())

// Advanced verification with framework access
.Verify(validator => validator.FindElement(e => e.TotalAmount.Selector).HasValue("$339.98"))
```

### 3. Navigation Patterns

Two navigation patterns are supported:

```csharp
// Base navigation method - direct navigation using IoC container
fluentUI.NavigateTo<TodosPage>()

// Custom navigation methods - encapsulate UI actions
homePage.NavigateToTodos()  // Clicks navigation button then navigates
```

### 4. Fluent API Pattern

The second generic type parameter `TPage` is crucial for maintaining fluent API context throughout method chains. This ensures that:

```csharp
// Without second generic type - returns base type
_uiScaffold.NavigateTo<HomePage>().VerifyPageTitle("My Home") // Returns BasePageComponent<TDriver>

// With second generic type - returns correct type
_uiScaffold.NavigateTo<HomePage>().VerifyPageTitle("My Home") // Returns HomePage
```

### Benefits of Fluent API Context

1. **Method Chaining**: All methods return the correct page type for continued chaining
2. **IntelliSense Support**: IDE provides proper autocomplete for page-specific methods
3. **Type Safety**: Compile-time guarantees that methods return the expected type
4. **Readability**: Clear indication of which page context you're working in
5. **Maintainability**: Easier to refactor and extend page-specific functionality

### Example Usage

```csharp
// Fluent API with proper context using base actions
fluentUI
    .NavigateTo<HomePage>()                    // Returns HomePage
    .VerifyPageTitle("FluentUIScaffold")      // Returns HomePage
    .Click(e => e.CounterButton)               // Returns HomePage
    .Verify(e => e.CounterText, "1")          // Returns HomePage
    .NavigateTo<TodosPage>()                   // Returns TodosPage
    .Type(e => e.TodoInput, "Test Todo")      // Returns TodosPage
    .Click(e => e.AddTodoButton)               // Returns TodosPage
    .Verify(e => e.TodoCount, "1");           // Returns TodosPage
```

### 5. Benefits of Refined API

1. **Consistency**: All element interactions use the same pattern
2. **Framework Agnostic**: Base actions work across different testing frameworks
3. **Type Safety**: Compile-time checking of element references
4. **Maintainability**: Less custom code to maintain
5. **Flexibility**: Easy to combine base actions for complex scenarios
6. **Readability**: Clear and intuitive API
7. **Extensibility**: Easy to add new base actions without breaking existing code
8. **Performance**: Direct element access without additional abstraction layers
9. **Debugging**: Clear stack traces and error messages
10. **Documentation**: Self-documenting API that's easy to understand

This specification provides a solid foundation for a truly framework-agnostic E2E testing library that focuses on control flow and structure while maintaining flexibility for framework-specific implementations. 