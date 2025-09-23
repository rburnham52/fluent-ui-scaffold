# Element Configuration

## Overview

Element configuration in FluentUIScaffold provides a flexible and powerful way to define how elements should be located, waited for, and interacted with. The framework uses a fluent API to configure elements with various options including wait strategies, timeouts, and descriptions.

## Core Concepts

### ElementBuilder

The `ElementBuilder` class provides a fluent API for configuring elements:

```csharp
public class ElementBuilder : IElement
{
    public ElementBuilder WithDescription(string description)
    public ElementBuilder WithTimeout(TimeSpan timeout)
    public ElementBuilder WithWaitStrategy(WaitStrategy strategy)
    public ElementBuilder WithRetryInterval(TimeSpan interval)
    public IElement Build()
}
```

### IElement Interface

The `IElement` interface defines the contract for element interactions:

```csharp
public interface IElement
{
    string Selector { get; }
    string Description { get; }
    TimeSpan Timeout { get; }
    WaitStrategy WaitStrategy { get; }
    
    void Click();
    void Type(string text);
    void Select(string value);
    string GetText();
    bool IsVisible();
    bool IsEnabled();
    void WaitFor();
}
```

## Basic Element Configuration

### Simple Element

```csharp
// Basic element with default configuration
var button = Element("#submit-button");
```

### Element with Description

```csharp
// Element with descriptive name for better logging
var emailInput = Element("#email")
    .WithDescription("Email Input Field");
```

### Element with Timeout

```csharp
// Element with custom timeout
var slowElement = Element("#slow-loading-element")
    .WithTimeout(TimeSpan.FromSeconds(30));
```

### Element with Wait Strategy

```csharp
// Element that waits for visibility
var modal = Element("#modal")
    .WithWaitStrategy(WaitStrategy.Visible);

// Element that waits for clickability
var button = Element("#button")
    .WithWaitStrategy(WaitStrategy.Clickable);
```

## Wait Strategies

### Available Wait Strategies

FluentUIScaffold provides several built-in wait strategies:

```csharp
public enum WaitStrategy
{
    None,           // No waiting
    Visible,        // Wait for element to be visible
    Hidden,         // Wait for element to be hidden
    Clickable,      // Wait for element to be clickable
    Enabled,        // Wait for element to be enabled
    Disabled,       // Wait for element to be disabled
    TextPresent,    // Wait for specific text to be present
    Smart           // Framework-specific intelligent waiting
}
```

### Using Wait Strategies

```csharp
// No waiting - immediate interaction
var immediateElement = Element("#instant-button")
    .WithWaitStrategy(WaitStrategy.None);

// Wait for visibility
var visibleElement = Element("#modal")
    .WithWaitStrategy(WaitStrategy.Visible);

// Wait for clickability
var clickableElement = Element("#submit-button")
    .WithWaitStrategy(WaitStrategy.Clickable);

// Wait for element to be enabled
var enabledElement = Element("#input-field")
    .WithWaitStrategy(WaitStrategy.Enabled);

// Wait for element to be disabled
var disabledElement = Element("#loading-spinner")
    .WithWaitStrategy(WaitStrategy.Disabled);

// Wait for text to be present
var textElement = Element("#message")
    .WithWaitStrategy(WaitStrategy.TextPresent);

// Smart waiting (framework-specific)
var smartElement = Element("#dynamic-content")
    .WithWaitStrategy(WaitStrategy.Smart);
```

### Custom Wait Conditions

```csharp
// Custom wait condition
var customElement = Element("#status")
    .WithWaitStrategy(WaitStrategy.Custom)
    .WithCustomWaitCondition(e => e.GetText() == "Complete");

// Wait for multiple conditions
var complexElement = Element("#complex-element")
    .WithWaitStrategy(WaitStrategy.Custom)
    .WithCustomWaitCondition(e => 
        e.IsVisible() && 
        e.IsEnabled() && 
        e.GetText().Contains("Ready"));
```

## Advanced Configuration

### Complex Element Configuration

```csharp
var complexElement = Element("[data-testid='user-card']")
    .WithDescription("User Card Component")
    .WithTimeout(TimeSpan.FromSeconds(15))
    .WithWaitStrategy(WaitStrategy.Visible)
    .WithRetryInterval(TimeSpan.FromMilliseconds(200));
```

### Framework-Specific Configuration

```csharp
// Playwright-specific configuration
var playwrightElement = Element("#playwright-element")
    .WithDescription("Playwright Element")
    .WithFrameworkSpecificOption("force", true)
    .WithFrameworkSpecificOption("trial", false);

// Selenium-specific configuration (future)
var seleniumElement = Element("#selenium-element")
    .WithDescription("Selenium Element")
    .WithFrameworkSpecificOption("scrollIntoView", true);
```

## Element Factory

### ElementFactory

The `ElementFactory` provides element creation with caching:

```csharp
public class ElementFactory
{
    public IElement CreateElement(string selector, Action<ElementBuilder>? configure = null)
    public IElement GetOrCreateElement(string selector, Action<ElementBuilder>? configure = null)
    public void ClearCache()
}
```

### Using ElementFactory

```csharp
// Create new element
var element = ElementFactory.CreateElement("#button");

// Create element with configuration
var configuredElement = ElementFactory.CreateElement("#input", builder =>
{
    builder.WithDescription("Email Input")
           .WithTimeout(TimeSpan.FromSeconds(10))
           .WithWaitStrategy(WaitStrategy.Visible);
});

// Get or create element (with caching)
var cachedElement = ElementFactory.GetOrCreateElement("#cached-button");

// Clear cache
ElementFactory.ClearCache();
```

## Element Collections

### IElementCollection Interface

```csharp
public interface IElementCollection : IEnumerable<IElement>
{
    IElement this[int index] { get; }
    int Count { get; }
    IElement First();
    IElement Last();
    IElementCollection Where(Func<IElement, bool> predicate);
    IElementCollection WithText(string text);
    IElementCollection WithAttribute(string attribute, string value);
}
```

### Working with Element Collections

```csharp
// Get all buttons
var buttons = ElementCollection("#buttons button");

// Get first button
var firstButton = buttons.First();

// Get last button
var lastButton = buttons.Last();

// Filter buttons by text
var submitButtons = buttons.WithText("Submit");

// Filter buttons by attribute
var enabledButtons = buttons.WithAttribute("disabled", "false");

// Custom filtering
var visibleButtons = buttons.Where(button => button.IsVisible());
```

## Element Interactions

### Basic Interactions

```csharp
var element = Element("#button");

// Click element
element.Click();

// Type text
element.Type("Hello World");

// Select value (for dropdowns)
element.Select("Option 1");

// Get text
string text = element.GetText();

// Check visibility
bool isVisible = element.IsVisible();

// Check if enabled
bool isEnabled = element.IsEnabled();

// Wait for element
element.WaitFor();
```

### Advanced Interactions

```csharp
// Clear element
element.Clear();

// Get attribute value
string href = element.GetAttribute("href");

// Get CSS property
string color = element.GetCssValue("color");

// Execute JavaScript
element.ExecuteScript("arguments[0].scrollIntoView();");

// Take screenshot
element.TakeScreenshot("element-screenshot.png");
```

## Element Selectors

### CSS Selectors

```csharp
// ID selector
var element = Element("#submit-button");

// Class selector
var element = Element(".btn-primary");

// Attribute selector
var element = Element("[data-testid='user-card']");

// Complex selector
var element = Element("div.container > button.btn[type='submit']");

// Pseudo-selector
var element = Element("input:focus");
```

### XPath Selectors

```csharp
// Basic XPath
var element = Element("//button[@id='submit']");

// XPath with text
var element = Element("//button[contains(text(), 'Submit')]");

// XPath with attribute
var element = Element("//input[@type='email']");

// XPath with position
var element = Element("//div[@class='item'][1]");
```

### Data Attributes

```csharp
// Using data-testid
var element = Element("[data-testid='submit-button']");

// Using custom data attributes
var element = Element("[data-user-id='123']");

// Using multiple data attributes
var element = Element("[data-role='admin'][data-status='active']");
```

## Best Practices

### 1. Descriptive Element Names

```csharp
// Good - descriptive
var emailInput = Element("#email")
    .WithDescription("Email Input Field");

// Bad - generic
var input = Element("#email");
```

### 2. Appropriate Wait Strategies

```csharp
// For buttons that need to be clickable
var button = Element("#submit")
    .WithWaitStrategy(WaitStrategy.Clickable);

// For loading spinners that should disappear
var spinner = Element(".loading-spinner")
    .WithWaitStrategy(WaitStrategy.Hidden);

// For dynamic content
var content = Element("#dynamic-content")
    .WithWaitStrategy(WaitStrategy.Smart);
```

### 3. Reasonable Timeouts

```csharp
// Short timeout for fast elements
var quickElement = Element("#instant-button")
    .WithTimeout(TimeSpan.FromSeconds(5));

// Longer timeout for slow elements
var slowElement = Element("#slow-loading-element")
    .WithTimeout(TimeSpan.FromSeconds(30));
```

### 4. Element Organization

```csharp
public class WellOrganizedPage : Page<WellOrganizedPage>
{
    // Group related elements
    public IElement EmailInput { get; private set; } = null!;
    public IElement PasswordInput { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;

    public WellOrganizedPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        ConfigureLoginElements();
    }

    private void ConfigureLoginElements()
    {
        EmailInput = Element("#email")
            .WithDescription("Email Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        PasswordInput = Element("#password")
            .WithDescription("Password Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        LoginButton = Element("#login-btn")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();
    }
}
```

### 5. Reusable Element Patterns

```csharp
// Create reusable element patterns
public static class ElementPatterns
{
    public static IElement Button(string id) =>
        Element($"#{id}")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .WithDescription($"Button: {id}");
    
    public static IElement Input(string id) =>
        Element($"#{id}")
            .WithWaitStrategy(WaitStrategy.Visible)
            .WithDescription($"Input: {id}");
    
    public static IElement Link(string text) =>
        Element($"a:contains('{text}')")
            .WithWaitStrategy(WaitStrategy.Visible)
            .WithDescription($"Link: {text}");
}

// Usage
var submitButton = ElementPatterns.Button("submit");
var emailInput = ElementPatterns.Input("email");
var homeLink = ElementPatterns.Link("Home");
```

## Performance Optimization

### Element Caching

```csharp
public class OptimizedPage : Page<OptimizedPage>
{
    public IElement Button { get; private set; } = null!;
    public IElement Input { get; private set; } = null!;

    public OptimizedPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        // Elements are automatically cached by ElementFactory
        Button = Element("#button").Build();
        Input = Element("#input").Build();
    }

    public OptimizedPage ClickButtonMultipleTimes(int times)
    {
        // Element is cached, no need to re-find
        for (int i = 0; i < times; i++)
        {
            Button.Click();
        }
        return this;
    }
}
```

### Lazy Loading

```csharp
public class LazyPage : Page<LazyPage>
{
    private IElement? _lazyElement;

    public LazyPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements() { }

    private IElement LazyElement => _lazyElement ??= Element("#lazy-loaded-element").Build();

    public LazyPage InteractWithLazyElement()
    {
        // Element is only found when first accessed
        LazyElement.Click();
        return this;
    }
}
```

### Batch Operations

```csharp
public class BatchPage : Page<BatchPage>
{
    public BatchPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements() { }

    public BatchPage FillMultipleInputs(Dictionary<string, string> data)
    {
        foreach (var item in data)
        {
            var element = Element($"#{item.Key}").Build();
            element.Type(item.Value);
        }
        return this;
    }

    public BatchPage ClickMultipleButtons(List<string> buttonIds)
    {
        foreach (var id in buttonIds)
        {
            var element = Element($"#{id}").Build();
            element.Click();
        }
        return this;
    }
}
```

## Error Handling

### Element Not Found

```csharp
public class RobustPage : Page<RobustPage>
{
    public RobustPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements() { }

    public RobustPage ClickButtonSafely()
    {
        try
        {
            var button = Element("#button").Build();
            button.Click();
        }
        catch (ElementTimeoutException ex)
        {
            // Log and use fallback logic
            Driver.ExecuteScript("document.querySelector('#button').click();");
        }
        return this;
    }
}
```

### Retry Logic

```csharp
public class RetryPage : Page<RetryPage>
{
    public RetryPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements() { }

    public IElement GetElementWithRetry(string selector, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var element = Element(selector).Build();
                element.WaitFor();
                return element;
            }
            catch (ElementTimeoutException)
            {
                if (i == maxRetries - 1) throw;
                Thread.Sleep(1000);
            }
        }

        throw new ElementTimeoutException($"Element {selector} not found after {maxRetries} attempts");
    }
}
```

## Testing Element Configuration

### Unit Testing

```csharp
[TestClass]
public class ElementConfigurationTests
{
    [TestMethod]
    public void Element_WithDescription_ShouldSetDescription()
    {
        // Arrange
        var element = Element("#button")
            .WithDescription("Test Button");
        
        // Assert
        Assert.AreEqual("Test Button", element.Description);
    }
    
    [TestMethod]
    public void Element_WithTimeout_ShouldSetTimeout()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(10);
        var element = Element("#button")
            .WithTimeout(timeout);
        
        // Assert
        Assert.AreEqual(timeout, element.Timeout);
    }
    
    [TestMethod]
    public void Element_WithWaitStrategy_ShouldSetWaitStrategy()
    {
        // Arrange
        var element = Element("#button")
            .WithWaitStrategy(WaitStrategy.Clickable);
        
        // Assert
        Assert.AreEqual(WaitStrategy.Clickable, element.WaitStrategy);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class ElementInteractionTests
{
    private static AppScaffold<WebApp>? _app;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .Web<WebApp>(opts =>
            {
                opts.BaseUrl = new Uri("https://your-app.com");
            })
            .WithAutoPageDiscovery()
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [TestMethod]
    public void Can_Interact_With_Configured_Element()
    {
        // Arrange
        var page = _app!.NavigateTo<TestPage>();

        // Act
        page.InteractWithConfiguredElement();

        // Assert
        page.Verify.Visible(p => p.ResultElement);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }
}
```

## Conclusion

Element configuration in FluentUIScaffold provides a powerful and flexible way to define element behavior. By following the best practices outlined in this guide, you can create robust and maintainable element configurations that handle various scenarios effectively.

Key takeaways:

- **Use descriptive names** for better logging and debugging
- **Choose appropriate wait strategies** for different element types
- **Set reasonable timeouts** based on element behavior
- **Organize elements logically** for better maintainability
- **Implement proper error handling** for robust tests
- **Optimize performance** with caching and lazy loading

For more information, see the [API Reference](api-reference.md) and [Page Object Pattern](page-object-pattern.md) guides. 