# Story 1.3.1: Implement Generic Verification

## Overview

Implement the generic verification methods required for Example 1 (User Registration and Login Flow), including the `Verify` method with default inner text comparison and support for specific property comparison.

## Background

Example 1 requires verification of form submission results and success/error messages. The V2.0 specification shows multiple verification patterns:

```csharp
.Verify(e => e.SuccessMessage, "Registration successful!")  // Simple value comparison (defaults to inner text)
.Verify(e => e.StatusElement, "enabled", "className")       // Compare specific property
```

This story focuses on implementing the generic verification methods in the `BasePageComponent<TDriver, TPage>` class.

## Acceptance Criteria

- [x] Implement `Verify(Func<IElement, string> elementSelector, string expectedText)`
- [x] Support default inner text comparison
- [x] Support specific property comparison
- [x] Add verification context system
- [x] Create working example with success message verification

## Technical Requirements

### 1. Generic Verification Implementation

Implement the `Verify` method in `BasePageComponent<TDriver, TPage>`:

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
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
    
    // Helper methods for element value retrieval
    protected virtual TValue GetElementValue<TValue>(IElement element)
    {
        if (typeof(TValue) == typeof(string))
        {
            return (TValue)(object)Driver.GetText(element.Selector);
        }
        
        throw new NotSupportedException($"Type {typeof(TValue)} is not supported for element value retrieval");
    }
    
    protected virtual string GetElementPropertyValue(IElement element, string propertyName)
    {
        switch (propertyName.ToLower())
        {
            case "innertext":
            case "text":
                return Driver.GetText(element.Selector);
            case "classname":
            case "class":
                return Driver.GetAttribute(element.Selector, "class");
            case "value":
                return Driver.GetAttribute(element.Selector, "value");
            case "enabled":
                return Driver.IsEnabled(element.Selector).ToString().ToLower();
            case "visible":
                return Driver.IsVisible(element.Selector).ToString().ToLower();
            default:
                return Driver.GetAttribute(element.Selector, propertyName);
        }
    }
}
```

### 2. Verification Context System

Implement a verification context system for more complex verifications:

```csharp
public interface IVerificationContext
{
    IVerificationContext HasText(string expectedText);
    IVerificationContext HasValue(string expectedValue);
    IVerificationContext IsVisible();
    IVerificationContext IsEnabled();
    IVerificationContext ContainsText(string expectedText);
    IVerificationContext MatchesPattern(string pattern);
}

public class VerificationContext : IVerificationContext
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly ILogger _logger;
    private readonly IElement _element;
    
    public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger, IElement element)
    {
        _driver = driver;
        _options = options;
        _logger = logger;
        _element = element;
    }
    
    public IVerificationContext HasText(string expectedText)
    {
        var actualText = _driver.GetText(_element.Selector);
        if (actualText != expectedText)
        {
            throw new ElementValidationException($"Expected text '{expectedText}', but got '{actualText}'");
        }
        return this;
    }
    
    public IVerificationContext HasValue(string expectedValue)
    {
        var actualValue = _driver.GetAttribute(_element.Selector, "value");
        if (actualValue != expectedValue)
        {
            throw new ElementValidationException($"Expected value '{expectedValue}', but got '{actualValue}'");
        }
        return this;
    }
    
    public IVerificationContext IsVisible()
    {
        if (!_driver.IsVisible(_element.Selector))
        {
            throw new ElementValidationException($"Element '{_element.Selector}' is not visible");
        }
        return this;
    }
    
    public IVerificationContext IsEnabled()
    {
        if (!_driver.IsEnabled(_element.Selector))
        {
            throw new ElementValidationException($"Element '{_element.Selector}' is not enabled");
        }
        return this;
    }
    
    public IVerificationContext ContainsText(string expectedText)
    {
        var actualText = _driver.GetText(_element.Selector);
        if (!actualText.Contains(expectedText))
        {
            throw new ElementValidationException($"Expected text to contain '{expectedText}', but got '{actualText}'");
        }
        return this;
    }
    
    public IVerificationContext MatchesPattern(string pattern)
    {
        var actualText = _driver.GetText(_element.Selector);
        if (!Regex.IsMatch(actualText, pattern))
        {
            throw new ElementValidationException($"Expected text to match pattern '{pattern}', but got '{actualText}'");
        }
        return this;
    }
}
```

### 3. Updated Page Classes

Update the RegistrationPage and LoginPage to include verification methods:

```csharp
public class RegistrationPage : BasePageComponent<PlaywrightDriver, RegistrationPage>
{
    // ... existing element definitions ...
    
    // Custom verification methods
    public RegistrationPage VerifyRegistrationSuccess()
    {
        return this.Verify(e => e.SuccessMessage, "Registration successful!");
    }
    
    public RegistrationPage VerifyRegistrationError(string expectedError)
    {
        return this.Verify(e => e.ErrorMessage, expectedError);
    }
    
    public RegistrationPage VerifyFormIsValid()
    {
        return this.Verify(e => e.Form, "valid", "className");
    }
    
    public RegistrationPage VerifyEmailFieldIsFocused()
    {
        return this.Verify(e => e.EmailInput, "true", "focused");
    }
}
```

### 4. Working Example

Create a working example that demonstrates the verification methods:

```csharp
[TestMethod]
public async Task Can_Verify_Registration_Form_Submission()
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
        .Click(e => e.RegisterButton)
        .Verify(e => e.SuccessMessage, "Registration successful!")
        .Verify(e => e.ErrorMessage, "hidden", "className");
}
```

## Implementation Tasks

### Phase 1: Core Verification Implementation
1. [x] Implement `Verify<TValue>` method with generic support
2. [x] Implement `Verify` method with default inner text comparison
3. [x] Implement `Verify` method with specific property comparison
4. [x] Add helper methods for element value retrieval

### Phase 2: Verification Context System
1. [x] Create `IVerificationContext` interface
2. [x] Create `VerificationContext` class
3. [x] Implement fluent verification methods
4. [x] Add verification context to BasePageComponent

### Phase 3: Page Integration
1. [x] Update RegistrationPage with verification methods
2. [x] Update LoginPage with verification methods
3. [x] Add custom verification methods for specific scenarios
4. [x] Test verification methods with sample app

### Phase 4: Testing and Examples
1. [x] Create comprehensive tests for verification methods
2. [x] Test different property comparisons
3. [x] Test error scenarios and validation
4. [x] Create working examples for all verification patterns

## Dependencies

- **Story 1.1.1**: Refactor to V2.0 BasePageComponent Pattern (must be completed first)
- **Story 1.2.1**: Implement Base Element Actions (must be completed first)
- **Story 1.2.2**: Create Registration and Login Pages (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [x] Generic verification methods are implemented and working
- [x] Verification context system is implemented
- [x] RegistrationPage and LoginPage have verification methods
- [x] Working examples demonstrate all verification patterns
- [x] Comprehensive tests are passing
- [x] Error scenarios are properly handled
- [x] All acceptance criteria are met

## Notes

- Verification methods should be framework-agnostic
- The fluent API should maintain type safety throughout method chains
- Error messages should be descriptive and helpful
- Verification should support both simple and complex scenarios

## Completion Status

**COMPLETED** - All acceptance criteria, implementation tasks, and definition of done items have been successfully implemented and tested. The generic verification system is fully functional with comprehensive test coverage demonstrating all verification patterns working correctly with the sample app. 