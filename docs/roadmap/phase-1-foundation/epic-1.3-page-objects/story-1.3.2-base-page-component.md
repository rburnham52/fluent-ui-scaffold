# Story 1.3.2: Base Page Component Implementation

## Story Information
- **Epic**: Epic 1.3 - Page Object Pattern Implementation
- **Priority**: Critical
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.1.1, Story 1.1.2, Story 1.2.1, Story 1.2.2
- **File**: `phase-1-foundation/epic-1.3-page-objects/story-1.3.2-base-page-component.md`

## User Story

**As a** test developer  
**I want** a base page component class that provides common functionality for all page objects  
**So that** I can create consistent, maintainable page objects with shared behavior

## Acceptance Criteria

- [ ] BasePageComponent<TApp> class is implemented with framework-agnostic design
- [ ] Class provides common element interaction methods (Click, Type, Select, GetText, IsVisible)
- [ ] Class supports page navigation with URL pattern validation
- [ ] Class provides framework-specific access through generic methods
- [ ] Class includes verification context access
- [ ] Class supports element configuration and caching
- [ ] Class provides logging integration
- [ ] Class handles page validation and error handling
- [ ] Class supports async/await patterns
- [ ] Comprehensive unit tests are written and passing
- [ ] Documentation is updated with usage examples

## Technical Tasks

### 1. Implement BasePageComponent<TApp> Class

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    protected IUIDriver Driver { get; }
    protected FluentUIScaffoldOptions Options { get; }
    protected ILogger Logger { get; }
    
    public abstract string UrlPattern { get; }
    public virtual bool ShouldValidateOnNavigation => true;
    
    protected BasePageComponent(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ConfigureElements();
    }
    
    protected abstract void ConfigureElements();
    
    // Framework-agnostic element interaction methods
    protected virtual void Click(string selector) => Driver.Click(selector);
    protected virtual void Type(string selector, string text) => Driver.Type(selector, text);
    protected virtual void Select(string selector, string value) => Driver.Select(selector, value);
    protected virtual string GetText(string selector) => Driver.GetText(selector);
    protected virtual bool IsVisible(string selector) => Driver.IsVisible(selector);
    protected virtual bool IsEnabled(string selector) => Driver.IsEnabled(selector);
    protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);
    
    // Navigation methods
    public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TApp>
        => Driver.NavigateTo<TTarget>();
    
    // Page validation
    public virtual bool IsCurrentPage() => Driver.CurrentUrl.Matches(UrlPattern);
    public virtual void ValidateCurrentPage()
    {
        if (!IsCurrentPage())
            throw new InvalidPageException($"Expected to be on page {GetType().Name}, but URL is {Driver.CurrentUrl}");
    }
    
    // Framework-specific access
    public TDriver Framework<TDriver>() where TDriver : class
        => Driver.GetFrameworkDriver<TDriver>();
    
    // Verification access
    public IVerificationContext<TApp> Verify => new VerificationContext<TApp>(Driver, Options, Logger);
}
```

### 2. Implement IPageComponent<TApp> Interface

```csharp
public interface IPageComponent<TApp>
{
    string UrlPattern { get; }
    bool ShouldValidateOnNavigation { get; }
    bool IsCurrentPage();
    void ValidateCurrentPage();
    TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TApp>;
    IVerificationContext<TApp> Verify { get; }
}
```

### 3. Add Element Configuration Support

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    protected Dictionary<string, IElement> Elements { get; } = new();
    
    protected IElement Element(string selector)
        => new ElementBuilder(selector, Driver, Options);
    
    protected void ConfigureElement(string name, string selector, Action<ElementBuilder> configure = null)
    {
        var builder = Element(selector);
        configure?.Invoke(builder);
        Elements[name] = builder.Build();
    }
    
    protected IElement GetElement(string name)
    {
        if (!Elements.ContainsKey(name))
            throw new ElementNotFoundException($"Element '{name}' not configured");
        return Elements[name];
    }
}
```

### 4. Add Async Support

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    // Async element interaction methods
    protected virtual async Task ClickAsync(string selector) => await Driver.ClickAsync(selector);
    protected virtual async Task TypeAsync(string selector, string text) => await Driver.TypeAsync(selector, text);
    protected virtual async Task SelectAsync(string selector, string value) => await Driver.SelectAsync(selector, value);
    protected virtual async Task<string> GetTextAsync(string selector) => await Driver.GetTextAsync(selector);
    protected virtual async Task<bool> IsVisibleAsync(string selector) => await Driver.IsVisibleAsync(selector);
    protected virtual async Task WaitForElementAsync(string selector) => await Driver.WaitForElementAsync(selector);
    
    // Async navigation
    public virtual async Task<TTarget> NavigateToAsync<TTarget>() where TTarget : BasePageComponent<TApp>
        => await Driver.NavigateToAsync<TTarget>();
}
```

### 5. Add Error Handling and Logging

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    protected virtual void LogAction(string action, string target, Dictionary<string, object> context = null)
    {
        Logger.LogInformation($"Performing {action} on {target}");
        if (context != null)
        {
            foreach (var kvp in context)
            {
                Logger.LogDebug($"  {kvp.Key}: {kvp.Value}");
            }
        }
    }
    
    protected virtual void LogError(Exception exception, string action, string target)
    {
        Logger.LogError(exception, $"Error performing {action} on {target}");
        throw new FluentUIScaffoldException($"Failed to perform {action} on {target}", exception);
    }
}
```

### 6. Add Page Transition Support

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    public virtual TTarget TransitionTo<TTarget>() where TTarget : BasePageComponent<TApp>
    {
        Logger.LogInformation($"Transitioning from {GetType().Name} to {typeof(TTarget).Name}");
        return Driver.CreatePageComponent<TTarget>();
    }
    
    public virtual async Task<TTarget> TransitionToAsync<TTarget>() where TTarget : BasePageComponent<TApp>
    {
        Logger.LogInformation($"Transitioning from {GetType().Name} to {typeof(TTarget).Name}");
        return await Driver.CreatePageComponentAsync<TTarget>();
    }
}
```

### 7. Add Element Factory Integration

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    protected IElementFactory ElementFactory { get; }
    
    protected BasePageComponent(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger, IElementFactory elementFactory)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ElementFactory = elementFactory ?? throw new ArgumentNullException(nameof(elementFactory));
        ConfigureElements();
    }
    
    protected IElement CreateElement(string selector, Action<ElementBuilder> configure = null)
    {
        return ElementFactory.CreateElement(selector, configure);
    }
}
```

### 8. Add Unit Tests

```csharp
[TestFixture]
public class BasePageComponentTests
{
    [Test]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        
        // Act
        var page = new TestPageComponent(driver, options, logger);
        
        // Assert
        Assert.That(page, Is.Not.Null);
    }
    
    [Test]
    public void IsCurrentPage_WithMatchingUrl_ReturnsTrue()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.CurrentUrl).Returns("/test/page");
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var page = new TestPageComponent(driver, options, logger);
        
        // Act
        var result = page.IsCurrentPage();
        
        // Assert
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void ValidateCurrentPage_WithInvalidUrl_ThrowsException()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.CurrentUrl).Returns("/wrong/page");
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var page = new TestPageComponent(driver, options, logger);
        
        // Act & Assert
        Assert.Throws<InvalidPageException>(() => page.ValidateCurrentPage());
    }
    
    private class TestPageComponent : BasePageComponent<WebApp>
    {
        public override string UrlPattern => "/test/page";
        
        public TestPageComponent(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger) 
            : base(driver, options, logger) { }
        
        protected override void ConfigureElements() { }
    }
}
```

### 9. Add Integration Tests

```csharp
[TestFixture]
public class BasePageComponentIntegrationTests
{
    [Test]
    public async Task NavigateTo_WithValidPage_TransitionsSuccessfully()
    {
        // Arrange
        var options = new FluentUIScaffoldOptions { BaseUrl = "https://localhost:5001" };
        var scaffold = FluentUIScaffold<WebApp>(options);
        
        // Act
        var page = await scaffold.NavigateToAsync<LoginPage>();
        
        // Assert
        Assert.That(page, Is.Not.Null);
        Assert.That(page.IsCurrentPage(), Is.True);
    }
}
```

## Definition of Done

- [ ] BasePageComponent<TApp> class is fully implemented
- [ ] IPageComponent<TApp> interface is implemented
- [ ] All element interaction methods are implemented (sync and async)
- [ ] Page navigation and validation is working
- [ ] Framework-specific access is implemented
- [ ] Verification context integration is complete
- [ ] Element configuration and caching is implemented
- [ ] Logging integration is complete
- [ ] Error handling is comprehensive
- [ ] Async/await patterns are supported
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests are written and passing
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Sample app page objects are updated to use BasePageComponent
- [ ] Sample app tests are verified to work with BasePageComponent
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the core interfaces and fluent API foundation
- The base page component should be framework-agnostic
- Consider performance implications of element caching
- Ensure thread safety for concurrent access
- Plan for future mobile support in the design
- Sample app integration ensures real-world validation of the implementation 