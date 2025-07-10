# Story 1.2.2: Element Configuration System

## Story Information
- **Epic**: 1.2 Fluent API Foundation
- **Priority**: Critical
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD

## User Story
**As a** test developer  
**I want** a fluent way to configure page elements with wait strategies and timeouts  
**So that** my tests are robust and handle dynamic content

## Acceptance Criteria
- [ ] `IElement` interface implemented with fluent configuration
- [ ] Support `WaitStrategy` enum (None, Visible, Hidden, Clickable, Enabled, Disabled, TextPresent, Smart)
- [ ] Implement `ElementBuilder` with fluent API
- [ ] Support custom timeouts and retry intervals
- [ ] Provide descriptive element names for better error messages
- [ ] All public APIs have comprehensive unit tests
- [ ] Element configuration is intuitive and chainable
- [ ] Documentation with examples is complete

## Technical Tasks

### Task 1.2.2.1: Implement IElement Interface
- [ ] Create `IElement` interface with core methods
- [ ] Implement element interaction methods
- [ ] Add element state checking methods
- [ ] Implement wait functionality
- [ ] Add element description support

```csharp
public interface IElement
{
    string Selector { get; }
    string Description { get; }
    TimeSpan Timeout { get; }
    WaitStrategy WaitStrategy { get; }
    TimeSpan RetryInterval { get; }
    
    // Core interactions
    void Click();
    void Type(string text);
    void Select(string value);
    string GetText();
    bool IsVisible();
    bool IsEnabled();
    bool IsDisplayed();
    
    // Wait operations
    void WaitFor();
    void WaitForVisible();
    void WaitForHidden();
    void WaitForClickable();
    void WaitForEnabled();
    void WaitForDisabled();
    
    // State checking
    bool Exists();
    bool IsSelected();
    string GetAttribute(string attributeName);
    string GetCssValue(string propertyName);
}
```

### Task 1.2.2.2: Implement ElementBuilder Class
- [ ] Create `ElementBuilder` class with fluent API
- [ ] Implement configuration methods
- [ ] Add validation for configuration options
- [ ] Support element description
- [ ] Implement build pattern

```csharp
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
    
    public ElementBuilder WithTimeout(TimeSpan timeout)
    public ElementBuilder WithWaitStrategy(WaitStrategy strategy)
    public ElementBuilder WithDescription(string description)
    public ElementBuilder WithRetryInterval(TimeSpan interval)
    public ElementBuilder WithCustomWait(Func<bool> waitCondition)
    public ElementBuilder WithAttribute(string name, string value)
    
    public IElement Build()
}
```

### Task 1.2.2.3: Implement WaitStrategy Enum and Configuration
- [ ] Create `WaitStrategy` enum with all strategies
- [ ] Implement `WaitStrategyConfig` class
- [ ] Add smart wait strategy implementation
- [ ] Support custom wait conditions
- [ ] Implement wait strategy validation

```csharp
public enum WaitStrategy
{
    None,
    Visible,
    Hidden,
    Clickable,
    Enabled,
    Disabled,
    TextPresent,
    Smart
}

public class WaitStrategyConfig
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public string ExpectedText { get; set; }
    public bool IgnoreExceptions { get; set; }
    public Func<bool> CustomCondition { get; set; }
}
```

### Task 1.2.2.4: Implement Element Factory and Registry
- [ ] Create `ElementFactory` class
- [ ] Implement element caching mechanism
- [ ] Add element validation
- [ ] Support element templates
- [ ] Implement element registry

```csharp
public class ElementFactory
{
    private readonly Dictionary<string, IElement> _elementCache = new();
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    
    public IElement CreateElement(string selector)
    public IElement CreateElement(string selector, Action<ElementBuilder> configure)
    public IElement GetOrCreateElement(string key, string selector)
    public void RegisterElement(string key, IElement element)
    public void ClearCache()
}
```

### Task 1.2.2.5: Add Element Validation and Error Handling
- [ ] Implement element existence validation
- [ ] Add meaningful error messages
- [ ] Implement element state validation
- [ ] Add timeout handling
- [ ] Implement retry logic

```csharp
public class ElementValidationException : FluentUIScaffoldException
{
    public ElementValidationException(string message, string selector) : base(message)
    {
        Selector = selector;
    }
    
    public string Selector { get; }
}

public class ElementTimeoutException : FluentUIScaffoldException
{
    public ElementTimeoutException(string message, string selector, TimeSpan timeout) : base(message)
    {
        Selector = selector;
        Timeout = timeout;
    }
    
    public string Selector { get; }
    public TimeSpan Timeout { get; }
}
```

### Task 1.2.2.6: Implement Element Collections and Lists
- [ ] Create `IElementCollection` interface
- [ ] Implement element list operations
- [ ] Add collection filtering methods
- [ ] Support collection iteration
- [ ] Implement collection validation

```csharp
public interface IElementCollection : IEnumerable<IElement>
{
    int Count { get; }
    IElement this[int index] { get; }
    
    IElementCollection Filter(Func<IElement, bool> predicate);
    IElementCollection FilterByText(string text);
    IElementCollection FilterByAttribute(string attribute, string value);
    
    IElement First();
    IElement FirstOrDefault();
    IElement Last();
    IElement LastOrDefault();
}
```

## Dependencies
- Story 1.1.1 (Project Structure Setup)
- Story 1.1.2 (Core Interfaces & Abstractions)
- Story 1.2.1 (Fluent Entry Point)

## Definition of Done
- [ ] IElement interface is implemented and tested
- [ ] ElementBuilder provides fluent configuration
- [ ] WaitStrategy enum supports all required strategies
- [ ] All public APIs have unit tests with >90% coverage
- [ ] Element validation provides clear error messages
- [ ] Documentation with examples is complete
- [ ] Integration tests demonstrate element usage

## Notes
- Follow fluent API design patterns
- Ensure element configuration is intuitive
- Consider performance implications of element caching
- Plan for async operations in future iterations
- Make error messages developer-friendly

## Related Documentation
- [Fluent API Design Patterns](https://github.com/rburnham52/fluent-test-scaffold)
- [Builder Pattern in C#](https://docs.microsoft.com/en-us/dotnet/standard/modern-web-apps-azure-architecture/architectural-principles#the-builder-pattern)
- [Element Locator Strategies](https://playwright.dev/docs/locators) 