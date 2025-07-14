# Story 1.2.1: Implement Base Element Actions

## Overview

Implement the base element actions required for Example 1 (User Registration and Login Flow), including `Click`, `Type`, `Select`, `Focus`, `Hover`, and `Clear` methods that work with the fluent API.

## Background

Example 1 requires form interactions for user registration and login. The V2.0 specification shows base element actions that provide consistent, framework-agnostic element interactions:

```csharp
.Click(e => e.LoadDataButton)           // Click an element
.Type(e => e.SearchInput, "laptop")     // Type text into an element
.Select(e => e.BrandFilter, "Dell")     // Select an option from dropdown
.Focus(e => e.EmailInput)               // Focus on an element
.Hover(e => e.TooltipTrigger)           // Hover over an element
.Clear(e => e.SearchInput)              // Clear element content
```

This story focuses on implementing these base element actions in the `BasePageComponent<TDriver, TPage>` class.

## Acceptance Criteria

- [x] Implement `Click(Func<TPage, IElement> elementSelector)`
- [x] Implement `Type(Func<TPage, IElement> elementSelector, string text)`
- [x] Implement `Select(Func<TPage, IElement> elementSelector, string value)`
- [x] Implement `Focus(Func<TPage, IElement> elementSelector)`
- [x] Implement `Hover(Func<TPage, IElement> elementSelector)`
- [x] Implement `Clear(Func<TPage, IElement> elementSelector)`
- [x] Add element configuration system for pages
- [x] Create working example with form field interactions

## Technical Requirements

### 1. Base Element Actions Implementation

Implement all base element actions in `BasePageComponent<TDriver, TPage>`:

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
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
    
    // Helper method to get element from selector
    protected virtual IElement GetElementFromSelector(Func<TPage, IElement> elementSelector)
    {
        return elementSelector((TPage)this);
    }
}
```

### 2. Element Configuration System

Implement an element configuration system that allows pages to define their elements:

```csharp
public interface IElement
{
    string Selector { get; }
    string Name { get; }
    ElementType Type { get; }
}

public class Element : IElement
{
    public string Selector { get; }
    public string Name { get; }
    public ElementType Type { get; }
    
    public Element(string selector, string name = null, ElementType type = ElementType.Generic)
    {
        Selector = selector;
        Name = name ?? selector;
        Type = type;
    }
}

public enum ElementType
{
    Generic,
    Input,
    Button,
    Select,
    Link,
    Div,
    Span
}
```

### 3. Updated Page Classes

Update the RegistrationPage and LoginPage to use the new element configuration:

```csharp
public class RegistrationPage : BasePageComponent<PlaywrightDriver, RegistrationPage>
{
    public IElement EmailInput { get; private set; }
    public IElement PasswordInput { get; private set; }
    public IElement FirstNameInput { get; private set; }
    public IElement LastNameInput { get; private set; }
    public IElement RegisterButton { get; private set; }
    public IElement SuccessMessage { get; private set; }
    
    public RegistrationPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        EmailInput = new Element("#email", "Email Input", ElementType.Input);
        PasswordInput = new Element("#password", "Password Input", ElementType.Input);
        FirstNameInput = new Element("#firstName", "First Name Input", ElementType.Input);
        LastNameInput = new Element("#lastName", "Last Name Input", ElementType.Input);
        RegisterButton = new Element("#registerButton", "Register Button", ElementType.Button);
        SuccessMessage = new Element("#successMessage", "Success Message", ElementType.Div);
    }
}
```

### 4. Working Example

Create a working example that demonstrates the base element actions:

```csharp
[TestMethod]
public async Task Can_Complete_Registration_Form()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
    // Act
    var registrationPage = fluentUI.NavigateTo<RegistrationPage>();
    
    registrationPage
        .Type(e => e.EmailInput, "john.doe@example.com")
        .Type(e => e.PasswordInput, "SecurePass123!")
        .Type(e => e.FirstNameInput, "John")
        .Type(e => e.LastNameInput, "Doe")
        .Click(e => e.RegisterButton);
    
    // Assert
    registrationPage.Verify(e => e.SuccessMessage, "Registration successful!");
}
```

### 5. Additional Element Actions

Implement additional element actions that might be useful:

```csharp
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

public virtual TPage WaitForElementToBeHidden(Func<TPage, IElement> elementSelector)
{
    var element = GetElementFromSelector(elementSelector);
    Driver.WaitForElementToBeHidden(element.Selector);
    return (TPage)this;
}
```

## Implementation Tasks

### Phase 1: Core Element Actions
1. [x] Implement `Click` method with fluent API
2. [x] Implement `Type` method with fluent API
3. [x] Implement `Select` method with fluent API
4. [x] Implement `Focus` method with fluent API
5. [x] Implement `Hover` method with fluent API
6. [x] Implement `Clear` method with fluent API

### Phase 2: Element Configuration System
1. [x] Create `IElement` interface
2. [x] Create `Element` class with properties
3. [x] Add `ElementType` enum
4. [x] Implement `GetElementFromSelector` helper method

### Phase 3: Page Integration
1. [x] Update RegistrationPage to use new element configuration
2. [x] Update LoginPage to use new element configuration
3. [x] Add element configuration to existing pages
4. [x] Test element configuration system

### Phase 4: Testing and Examples
1. [x] Create comprehensive tests for all element actions
2. [x] Create working example with registration form
3. [x] Create working example with login form
4. [x] Test fluent API chaining

## Dependencies

- **Story 1.1.1**: Refactor to V2.0 BasePageComponent Pattern (must be completed first)
- **Story 1.1.2**: Implement Basic Navigation Methods (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [x] All base element actions are implemented and working
- [x] Element configuration system is implemented
- [x] RegistrationPage and LoginPage use new element configuration
- [x] Working examples demonstrate all element actions
- [x] Comprehensive tests are passing
- [x] Fluent API chaining works correctly
- [x] All acceptance criteria are met

## Notes

- Base element actions should be framework-agnostic
- The fluent API should maintain type safety throughout method chains
- Element configuration should be flexible and extensible
- All element actions should return the page instance for fluent chaining 