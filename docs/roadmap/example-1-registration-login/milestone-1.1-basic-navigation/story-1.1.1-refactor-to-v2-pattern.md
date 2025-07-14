# Story 1.1.1: Refactor to V2.0 BasePageComponent Pattern

## Overview

Refactor the existing FluentUIScaffold implementation to match the V2.0 specification exactly, implementing the `BasePageComponent<TDriver, TPage>` pattern and updating the framework structure.

## Background

The current implementation uses a different approach than the V2.0 specification. We need to refactor to match the V2.0 spec exactly, which includes:

- `BasePageComponent<TDriver, TPage>` with dual generic types
- Framework-agnostic base element actions
- Fluent API control flow
- Dependency injection first approach
- Explicit framework exposure

## Acceptance Criteria

- [ ] Implement `BasePageComponent<TDriver, TPage>` following V2.0 spec
- [ ] Refactor existing pages to use new pattern
- [ ] Update FluentUIScaffoldBuilder to match V2.0 spec
- [ ] All existing tests pass with new implementation
- [ ] Remove old implementation code

## Technical Requirements

### 1. BasePageComponent Implementation

Implement the `BasePageComponent<TDriver, TPage>` class as specified in V2.0:

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
}
```

### 2. FluentUIScaffoldBuilder Update

Update the builder to match V2.0 specification:

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

### 3. Refactor Existing Pages

Refactor the existing pages (HomePage, ProfilePage, TodosPage) to use the new pattern:

- Update class signatures to inherit from `BasePageComponent<PlaywrightDriver, TPage>`
- Implement `ConfigureElements()` method
- Update element definitions to use the new pattern
- Remove old implementation code

### 4. Update Tests

Update all existing tests to work with the new implementation:

- Update test setup to use new builder pattern
- Update page instantiation to use new pattern
- Ensure all existing test scenarios still pass
- Add new tests for V2.0 specific features

## Implementation Tasks

### Phase 1: Core Framework Updates
1. [ ] Create new `BasePageComponent<TDriver, TPage>` class
2. [ ] Update `FluentUIScaffoldBuilder` to match V2.0 spec
3. [ ] Update `FluentUIScaffoldApp<TApp>` to use new pattern
4. [ ] Update service registration and dependency injection

### Phase 2: Page Refactoring
1. [ ] Refactor `HomePage` to use new pattern
2. [ ] Refactor `ProfilePage` to use new pattern
3. [ ] Refactor `TodosPage` to use new pattern
4. [ ] Update element configuration system

### Phase 3: Test Updates
1. [ ] Update `HomePageTests` to work with new pattern
2. [ ] Update `ProfilePageTests` to work with new pattern
3. [ ] Update `TodosPageTests` to work with new pattern
4. [ ] Add new tests for V2.0 features

### Phase 4: Cleanup
1. [ ] Remove old implementation code
2. [ ] Update documentation to reflect V2.0 changes
3. [ ] Ensure all tests pass
4. [ ] Update sample app to use new pattern

## Dependencies

- None (this is the foundational story)

## Estimation

- **Time Estimate**: 3-4 weeks
- **Complexity**: High
- **Risk**: Medium (major refactoring)

## Definition of Done

- [ ] `BasePageComponent<TDriver, TPage>` is implemented and working
- [ ] All existing pages are refactored to use new pattern
- [ ] All existing tests pass with new implementation
- [ ] Old implementation code is removed
- [ ] Documentation is updated to reflect V2.0 changes
- [ ] Sample app works with new implementation
- [ ] Code review is completed
- [ ] All acceptance criteria are met

## Notes

- This is a major refactoring that affects the core framework
- Careful attention must be paid to maintaining backward compatibility where possible
- All existing functionality must be preserved during the refactoring
- Extensive testing is required to ensure no regressions 