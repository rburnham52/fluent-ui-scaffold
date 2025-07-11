# Story 2.1.2: Element Verification Methods

## Story Information
- **Epic**: Epic 2.1 - Verification System
- **Priority**: High
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 2.1.1
- **File**: `phase-2-advanced-features/epic-2.1-verification/story-2.1.2-element-verification.md`

## User Story

**As a** test developer  
**I want** comprehensive element verification methods that cover all common validation scenarios  
**So that** I can verify element state, content, and behavior with confidence

## Acceptance Criteria

- [ ] Element visibility verification methods are implemented
- [ ] Element text and content verification methods are implemented
- [ ] Element attribute verification methods are implemented
- [ ] Element state verification methods (enabled/disabled, selected) are implemented
- [ ] Element count and collection verification methods are implemented
- [ ] Element position and size verification methods are implemented
- [ ] Element interaction verification methods are implemented
- [ ] Element CSS property verification methods are implemented
- [ ] Element accessibility verification methods are implemented
- [ ] All verification methods support async operations
- [ ] All verification methods include proper error handling
- [ ] Comprehensive unit tests are written and passing

## Technical Tasks

### 1. Implement Element Visibility Verification

```csharp
public interface IElementVerification
{
    IVerificationContext<TApp> IsVisible();
    IVerificationContext<TApp> IsHidden();
    IVerificationContext<TApp> IsDisplayed();
    IVerificationContext<TApp> IsNotDisplayed();
    IVerificationContext<TApp> IsInViewport();
    IVerificationContext<TApp> IsNotInViewport();
    Task<IVerificationContext<TApp>> IsVisibleAsync();
    Task<IVerificationContext<TApp>> IsHiddenAsync();
}

public class ElementVerification<TApp> : IElementVerification
{
    private readonly IUIDriver _driver;
    private readonly string _selector;
    private readonly IVerificationContext<TApp> _context;
    
    public ElementVerification(IUIDriver driver, string selector, IVerificationContext<TApp> context)
    {
        _driver = driver;
        _selector = selector;
        _context = context;
    }
    
    public IVerificationContext<TApp> IsVisible()
    {
        return _context.ElementIsVisible(_selector);
    }
    
    public IVerificationContext<TApp> IsHidden()
    {
        return _context.ElementIsHidden(_selector);
    }
    
    public IVerificationContext<TApp> IsDisplayed()
    {
        return _context.That(() => _driver.IsDisplayed(_selector), $"Element '{_selector}' should be displayed");
    }
    
    public IVerificationContext<TApp> IsNotDisplayed()
    {
        return _context.That(() => !_driver.IsDisplayed(_selector), $"Element '{_selector}' should not be displayed");
    }
    
    public IVerificationContext<TApp> IsInViewport()
    {
        return _context.That(() => _driver.IsInViewport(_selector), $"Element '{_selector}' should be in viewport");
    }
    
    public IVerificationContext<TApp> IsNotInViewport()
    {
        return _context.That(() => !_driver.IsInViewport(_selector), $"Element '{_selector}' should not be in viewport");
    }
    
    public async Task<IVerificationContext<TApp>> IsVisibleAsync()
    {
        return await _context.ElementIsVisibleAsync(_selector);
    }
    
    public async Task<IVerificationContext<TApp>> IsHiddenAsync()
    {
        return await _context.ElementIsHiddenAsync(_selector);
    }
}
```

### 2. Implement Element Text and Content Verification

```csharp
public interface IElementTextVerification
{
    IVerificationContext<TApp> ContainsText(string text);
    IVerificationContext<TApp> HasText(string text);
    IVerificationContext<TApp> StartsWithText(string text);
    IVerificationContext<TApp> EndsWithText(string text);
    IVerificationContext<TApp> MatchesPattern(string pattern);
    IVerificationContext<TApp> IsEmpty();
    IVerificationContext<TApp> IsNotEmpty();
    IVerificationContext<TApp> HasLength(int length);
    IVerificationContext<TApp> HasLengthAtLeast(int minimumLength);
    IVerificationContext<TApp> HasLengthAtMost(int maximumLength);
}

public class ElementTextVerification<TApp> : IElementTextVerification
{
    private readonly IUIDriver _driver;
    private readonly string _selector;
    private readonly IVerificationContext<TApp> _context;
    
    public ElementTextVerification(IUIDriver driver, string selector, IVerificationContext<TApp> context)
    {
        _driver = driver;
        _selector = selector;
        _context = context;
    }
    
    public IVerificationContext<TApp> ContainsText(string text)
    {
        return _context.ElementContainsText(_selector, text);
    }
    
    public IVerificationContext<TApp> HasText(string text)
    {
        return _context.ElementHasText(_selector, text);
    }
    
    public IVerificationContext<TApp> StartsWithText(string text)
    {
        return _context.That(
            () => _driver.GetText(_selector).StartsWith(text, StringComparison.OrdinalIgnoreCase),
            $"Element '{_selector}' should start with text '{text}'"
        );
    }
    
    public IVerificationContext<TApp> EndsWithText(string text)
    {
        return _context.That(
            () => _driver.GetText(_selector).EndsWith(text, StringComparison.OrdinalIgnoreCase),
            $"Element '{_selector}' should end with text '{text}'"
        );
    }
    
    public IVerificationContext<TApp> MatchesPattern(string pattern)
    {
        return _context.That(
            () => Regex.IsMatch(_driver.GetText(_selector), pattern),
            $"Element '{_selector}' text should match pattern '{pattern}'"
        );
    }
    
    public IVerificationContext<TApp> IsEmpty()
    {
        return _context.That(
            () => string.IsNullOrWhiteSpace(_driver.GetText(_selector)),
            $"Element '{_selector}' should be empty"
        );
    }
    
    public IVerificationContext<TApp> IsNotEmpty()
    {
        return _context.That(
            () => !string.IsNullOrWhiteSpace(_driver.GetText(_selector)),
            $"Element '{_selector}' should not be empty"
        );
    }
    
    public IVerificationContext<TApp> HasLength(int length)
    {
        return _context.That(
            () => _driver.GetText(_selector).Length == length,
            $"Element '{_selector}' should have text length {length}"
        );
    }
    
    public IVerificationContext<TApp> HasLengthAtLeast(int minimumLength)
    {
        return _context.That(
            () => _driver.GetText(_selector).Length >= minimumLength,
            $"Element '{_selector}' should have text length at least {minimumLength}"
        );
    }
    
    public IVerificationContext<TApp> HasLengthAtMost(int maximumLength)
    {
        return _context.That(
            () => _driver.GetText(_selector).Length <= maximumLength,
            $"Element '{_selector}' should have text length at most {maximumLength}"
        );
    }
}
```

### 3. Implement Element Attribute Verification

```csharp
public interface IElementAttributeVerification
{
    IVerificationContext<TApp> HasAttribute(string attribute);
    IVerificationContext<TApp> HasAttributeValue(string attribute, string value);
    IVerificationContext<TApp> HasAttributeContaining(string attribute, string value);
    IVerificationContext<TApp> HasAttributeMatching(string attribute, string pattern);
    IVerificationContext<TApp> DoesNotHaveAttribute(string attribute);
    IVerificationContext<TApp> HasClass(string className);
    IVerificationContext<TApp> HasClasses(params string[] classNames);
    IVerificationContext<TApp> DoesNotHaveClass(string className);
    IVerificationContext<TApp> HasId(string id);
    IVerificationContext<TApp> HasName(string name);
    IVerificationContext<TApp> HasType(string type);
    IVerificationContext<TApp> HasValue(string value);
    IVerificationContext<TApp> HasPlaceholder(string placeholder);
    IVerificationContext<TApp> HasTitle(string title);
    IVerificationContext<TApp> HasAlt(string alt);
    IVerificationContext<TApp> HasSrc(string src);
    IVerificationContext<TApp> HasHref(string href);
}

public class ElementAttributeVerification<TApp> : IElementAttributeVerification
{
    private readonly IUIDriver _driver;
    private readonly string _selector;
    private readonly IVerificationContext<TApp> _context;
    
    public ElementAttributeVerification(IUIDriver driver, string selector, IVerificationContext<TApp> context)
    {
        _driver = driver;
        _selector = selector;
        _context = context;
    }
    
    public IVerificationContext<TApp> HasAttribute(string attribute)
    {
        return _context.That(
            () => !string.IsNullOrEmpty(_driver.GetAttribute(_selector, attribute)),
            $"Element '{_selector}' should have attribute '{attribute}'"
        );
    }
    
    public IVerificationContext<TApp> HasAttributeValue(string attribute, string value)
    {
        return _context.ElementHasAttribute(_selector, attribute, value);
    }
    
    public IVerificationContext<TApp> HasAttributeContaining(string attribute, string value)
    {
        return _context.That(
            () => _driver.GetAttribute(_selector, attribute)?.Contains(value, StringComparison.OrdinalIgnoreCase) == true,
            $"Element '{_selector}' should have attribute '{attribute}' containing '{value}'"
        );
    }
    
    public IVerificationContext<TApp> HasAttributeMatching(string attribute, string pattern)
    {
        return _context.That(
            () => Regex.IsMatch(_driver.GetAttribute(_selector, attribute) ?? "", pattern),
            $"Element '{_selector}' should have attribute '{attribute}' matching pattern '{pattern}'"
        );
    }
    
    public IVerificationContext<TApp> DoesNotHaveAttribute(string attribute)
    {
        return _context.That(
            () => string.IsNullOrEmpty(_driver.GetAttribute(_selector, attribute)),
            $"Element '{_selector}' should not have attribute '{attribute}'"
        );
    }
    
    public IVerificationContext<TApp> HasClass(string className)
    {
        return _context.That(
            () => _driver.GetAttribute(_selector, "class")?.Split(' ').Contains(className) == true,
            $"Element '{_selector}' should have class '{className}'"
        );
    }
    
    public IVerificationContext<TApp> HasClasses(params string[] classNames)
    {
        return _context.That(
            () => {
                var classes = _driver.GetAttribute(_selector, "class")?.Split(' ') ?? Array.Empty<string>();
                return classNames.All(cn => classes.Contains(cn));
            },
            $"Element '{_selector}' should have classes: {string.Join(", ", classNames)}"
        );
    }
    
    public IVerificationContext<TApp> DoesNotHaveClass(string className)
    {
        return _context.That(
            () => !(_driver.GetAttribute(_selector, "class")?.Split(' ').Contains(className) == true),
            $"Element '{_selector}' should not have class '{className}'"
        );
    }
    
    public IVerificationContext<TApp> HasId(string id)
    {
        return _context.HasAttributeValue(_selector, "id", id);
    }
    
    public IVerificationContext<TApp> HasName(string name)
    {
        return _context.HasAttributeValue(_selector, "name", name);
    }
    
    public IVerificationContext<TApp> HasType(string type)
    {
        return _context.HasAttributeValue(_selector, "type", type);
    }
    
    public IVerificationContext<TApp> HasValue(string value)
    {
        return _context.ElementHasValue(_selector, value);
    }
    
    public IVerificationContext<TApp> HasPlaceholder(string placeholder)
    {
        return _context.HasAttributeValue(_selector, "placeholder", placeholder);
    }
    
    public IVerificationContext<TApp> HasTitle(string title)
    {
        return _context.HasAttributeValue(_selector, "title", title);
    }
    
    public IVerificationContext<TApp> HasAlt(string alt)
    {
        return _context.HasAttributeValue(_selector, "alt", alt);
    }
    
    public IVerificationContext<TApp> HasSrc(string src)
    {
        return _context.HasAttributeValue(_selector, "src", src);
    }
    
    public IVerificationContext<TApp> HasHref(string href)
    {
        return _context.HasAttributeValue(_selector, "href", href);
    }
}
```

### 4. Implement Element State Verification

```csharp
public interface IElementStateVerification
{
    IVerificationContext<TApp> IsEnabled();
    IVerificationContext<TApp> IsDisabled();
    IVerificationContext<TApp> IsSelected();
    IVerificationContext<TApp> IsNotSelected();
    IVerificationContext<TApp> IsChecked();
    IVerificationContext<TApp> IsNotChecked();
    IVerificationContext<TApp> IsRequired();
    IVerificationContext<TApp> IsNotRequired();
    IVerificationContext<TApp> IsReadOnly();
    IVerificationContext<TApp> IsNotReadOnly();
    IVerificationContext<TApp> IsFocused();
    IVerificationContext<TApp> IsNotFocused();
    IVerificationContext<TApp> IsClickable();
    IVerificationContext<TApp> IsNotClickable();
}

public class ElementStateVerification<TApp> : IElementStateVerification
{
    private readonly IUIDriver _driver;
    private readonly string _selector;
    private readonly IVerificationContext<TApp> _context;
    
    public ElementStateVerification(IUIDriver driver, string selector, IVerificationContext<TApp> context)
    {
        _driver = driver;
        _selector = selector;
        _context = context;
    }
    
    public IVerificationContext<TApp> IsEnabled()
    {
        return _context.ElementIsEnabled(_selector);
    }
    
    public IVerificationContext<TApp> IsDisabled()
    {
        return _context.ElementIsDisabled(_selector);
    }
    
    public IVerificationContext<TApp> IsSelected()
    {
        return _context.ElementIsSelected(_selector);
    }
    
    public IVerificationContext<TApp> IsNotSelected()
    {
        return _context.ElementIsNotSelected(_selector);
    }
    
    public IVerificationContext<TApp> IsChecked()
    {
        return _context.That(
            () => _driver.IsChecked(_selector),
            $"Element '{_selector}' should be checked"
        );
    }
    
    public IVerificationContext<TApp> IsNotChecked()
    {
        return _context.That(
            () => !_driver.IsChecked(_selector),
            $"Element '{_selector}' should not be checked"
        );
    }
    
    public IVerificationContext<TApp> IsRequired()
    {
        return _context.That(
            () => _driver.HasAttribute(_selector, "required"),
            $"Element '{_selector}' should be required"
        );
    }
    
    public IVerificationContext<TApp> IsNotRequired()
    {
        return _context.That(
            () => !_driver.HasAttribute(_selector, "required"),
            $"Element '{_selector}' should not be required"
        );
    }
    
    public IVerificationContext<TApp> IsReadOnly()
    {
        return _context.That(
            () => _driver.HasAttribute(_selector, "readonly"),
            $"Element '{_selector}' should be read-only"
        );
    }
    
    public IVerificationContext<TApp> IsNotReadOnly()
    {
        return _context.That(
            () => !_driver.HasAttribute(_selector, "readonly"),
            $"Element '{_selector}' should not be read-only"
        );
    }
    
    public IVerificationContext<TApp> IsFocused()
    {
        return _context.That(
            () => _driver.IsFocused(_selector),
            $"Element '{_selector}' should be focused"
        );
    }
    
    public IVerificationContext<TApp> IsNotFocused()
    {
        return _context.That(
            () => !_driver.IsFocused(_selector),
            $"Element '{_selector}' should not be focused"
        );
    }
    
    public IVerificationContext<TApp> IsClickable()
    {
        return _context.That(
            () => _driver.IsClickable(_selector),
            $"Element '{_selector}' should be clickable"
        );
    }
    
    public IVerificationContext<TApp> IsNotClickable()
    {
        return _context.That(
            () => !_driver.IsClickable(_selector),
            $"Element '{_selector}' should not be clickable"
        );
    }
}
```

### 5. Add Unit Tests

```csharp
[TestFixture]
public class ElementVerificationTests
{
    [Test]
    public void IsVisible_WithVisibleElement_ReturnsContext()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.IsVisible("#test")).Returns(true);
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var context = new VerificationContext<WebApp>(driver, options, logger);
        var verification = new ElementVerification<WebApp>(driver, "#test", context);
        
        // Act
        var result = verification.IsVisible();
        
        // Assert
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public void ContainsText_WithMatchingText_ReturnsContext()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.GetText("#test")).Returns("Hello World");
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var context = new VerificationContext<WebApp>(driver, options, logger);
        var verification = new ElementTextVerification<WebApp>(driver, "#test", context);
        
        // Act
        var result = verification.ContainsText("World");
        
        // Assert
        Assert.That(result, Is.Not.Null);
    }
}
```

## Definition of Done

- [ ] Element visibility verification methods are implemented
- [ ] Element text and content verification methods are implemented
- [ ] Element attribute verification methods are implemented
- [ ] Element state verification methods are implemented
- [ ] Element count and collection verification methods are implemented
- [ ] Element position and size verification methods are implemented
- [ ] Element interaction verification methods are implemented
- [ ] Element CSS property verification methods are implemented
- [ ] Element accessibility verification methods are implemented
- [ ] All verification methods support async operations
- [ ] All verification methods include proper error handling
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate real scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the verification context implementation
- All verification methods should be fluent and chainable
- Consider performance implications of multiple verifications
- Plan for future mobile verification support
- Ensure thread safety for concurrent verifications 