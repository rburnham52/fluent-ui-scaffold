# Story 2.1.1: Verification Context Implementation

## Story Information
- **Epic**: Epic 2.1 - Verification System
- **Priority**: High
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.3.1, Story 1.3.2
- **File**: `phase-2-advanced-features/epic-2.1-verification/story-2.1.1-verification-context.md`

## User Story

**As a** test developer  
**I want** a fluent verification context that provides comprehensive element and page validation capabilities  
**So that** I can write readable, maintainable assertions in my test automation

## Acceptance Criteria

- [ ] IVerificationContext<TApp> interface is implemented with fluent API
- [ ] Verification context supports element visibility, text, attributes, and state checks
- [ ] Verification context supports page-level validations (URL, title, content)
- [ ] Verification context supports custom verification functions
- [ ] Verification context provides detailed error messages with context
- [ ] Verification context supports both synchronous and asynchronous operations
- [ ] Verification context includes retry logic for flaky elements
- [ ] Verification context provides logging for debugging
- [ ] Verification context supports chaining multiple verifications
- [ ] Verification context handles timeouts and error conditions gracefully
- [ ] Comprehensive unit tests are written and passing
- [ ] Integration tests demonstrate real verification scenarios

## Technical Tasks

### 1. Implement IVerificationContext<TApp> Interface

```csharp
public interface IVerificationContext<TApp>
{
    // Element verifications
    IVerificationContext<TApp> ElementIsVisible(string selector);
    IVerificationContext<TApp> ElementIsHidden(string selector);
    IVerificationContext<TApp> ElementIsEnabled(string selector);
    IVerificationContext<TApp> ElementIsDisabled(string selector);
    IVerificationContext<TApp> ElementContainsText(string selector, string text);
    IVerificationContext<TApp> ElementHasText(string selector, string text);
    IVerificationContext<TApp> ElementHasAttribute(string selector, string attribute, string value);
    IVerificationContext<TApp> ElementHasValue(string selector, string value);
    IVerificationContext<TApp> ElementIsSelected(string selector);
    IVerificationContext<TApp> ElementIsNotSelected(string selector);
    IVerificationContext<TApp> ElementCount(string selector, int expectedCount);
    IVerificationContext<TApp> ElementCountAtLeast(string selector, int minimumCount);
    
    // Page verifications
    IVerificationContext<TApp> CurrentPageIs<TPage>() where TPage : BasePageComponent<TApp>;
    IVerificationContext<TApp> UrlMatches(string pattern);
    IVerificationContext<TApp> UrlContains(string text);
    IVerificationContext<TApp> TitleContains(string text);
    IVerificationContext<TApp> TitleIs(string text);
    IVerificationContext<TApp> PageContainsText(string text);
    IVerificationContext<TApp> PageDoesNotContainText(string text);
    
    // Custom verifications
    IVerificationContext<TApp> That(Func<bool> condition, string description);
    IVerificationContext<TApp> That<T>(Func<T> actual, Func<T, bool> condition, string description);
    IVerificationContext<TApp> That<T>(Func<T> actual, T expected, string description);
    
    // Async versions
    Task<IVerificationContext<TApp>> ElementIsVisibleAsync(string selector);
    Task<IVerificationContext<TApp>> ElementContainsTextAsync(string selector, string text);
    Task<IVerificationContext<TApp>> ThatAsync(Func<Task<bool>> condition, string description);
}
```

### 2. Implement VerificationContext<TApp> Class

```csharp
public class VerificationContext<TApp> : IVerificationContext<TApp>
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly ILogger _logger;
    private readonly List<VerificationResult> _results;
    
    public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _results = new List<VerificationResult>();
    }
    
    public IVerificationContext<TApp> ElementIsVisible(string selector)
    {
        return ExecuteVerification(
            () => _driver.IsVisible(selector),
            $"Element '{selector}' should be visible",
            $"Element '{selector}' is not visible"
        );
    }
    
    public IVerificationContext<TApp> ElementIsHidden(string selector)
    {
        return ExecuteVerification(
            () => !_driver.IsVisible(selector),
            $"Element '{selector}' should be hidden",
            $"Element '{selector}' is visible but should be hidden"
        );
    }
    
    public IVerificationContext<TApp> ElementIsEnabled(string selector)
    {
        return ExecuteVerification(
            () => _driver.IsEnabled(selector),
            $"Element '{selector}' should be enabled",
            $"Element '{selector}' is disabled"
        );
    }
    
    public IVerificationContext<TApp> ElementIsDisabled(string selector)
    {
        return ExecuteVerification(
            () => !_driver.IsEnabled(selector),
            $"Element '{selector}' should be disabled",
            $"Element '{selector}' is enabled but should be disabled"
        );
    }
    
    public IVerificationContext<TApp> ElementContainsText(string selector, string text)
    {
        return ExecuteVerification(
            () => _driver.GetText(selector).Contains(text, StringComparison.OrdinalIgnoreCase),
            $"Element '{selector}' should contain text '{text}'",
            $"Element '{selector}' does not contain text '{text}'"
        );
    }
    
    public IVerificationContext<TApp> ElementHasText(string selector, string text)
    {
        return ExecuteVerification(
            () => _driver.GetText(selector).Equals(text, StringComparison.OrdinalIgnoreCase),
            $"Element '{selector}' should have text '{text}'",
            $"Element '{selector}' has different text than expected '{text}'"
        );
    }
    
    public IVerificationContext<TApp> ElementHasAttribute(string selector, string attribute, string value)
    {
        return ExecuteVerification(
            () => _driver.GetAttribute(selector, attribute) == value,
            $"Element '{selector}' should have attribute '{attribute}' with value '{value}'",
            $"Element '{selector}' does not have attribute '{attribute}' with value '{value}'"
        );
    }
    
    public IVerificationContext<TApp> ElementHasValue(string selector, string value)
    {
        return ExecuteVerification(
            () => _driver.GetValue(selector) == value,
            $"Element '{selector}' should have value '{value}'",
            $"Element '{selector}' does not have value '{value}'"
        );
    }
    
    public IVerificationContext<TApp> ElementIsSelected(string selector)
    {
        return ExecuteVerification(
            () => _driver.IsSelected(selector),
            $"Element '{selector}' should be selected",
            $"Element '{selector}' is not selected"
        );
    }
    
    public IVerificationContext<TApp> ElementIsNotSelected(string selector)
    {
        return ExecuteVerification(
            () => !_driver.IsSelected(selector),
            $"Element '{selector}' should not be selected",
            $"Element '{selector}' is selected but should not be"
        );
    }
    
    public IVerificationContext<TApp> ElementCount(string selector, int expectedCount)
    {
        return ExecuteVerification(
            () => _driver.GetElementCount(selector) == expectedCount,
            $"Should find exactly {expectedCount} elements matching '{selector}'",
            $"Found {_driver.GetElementCount(selector)} elements matching '{selector}', expected {expectedCount}"
        );
    }
    
    public IVerificationContext<TApp> ElementCountAtLeast(string selector, int minimumCount)
    {
        return ExecuteVerification(
            () => _driver.GetElementCount(selector) >= minimumCount,
            $"Should find at least {minimumCount} elements matching '{selector}'",
            $"Found {_driver.GetElementCount(selector)} elements matching '{selector}', expected at least {minimumCount}"
        );
    }
    
    public IVerificationContext<TApp> CurrentPageIs<TPage>() where TPage : BasePageComponent<TApp>
    {
        return ExecuteVerification(
            () => {
                var page = CreatePageComponent<TPage>();
                return page.IsCurrentPage();
            },
            $"Current page should be {typeof(TPage).Name}",
            $"Current page is not {typeof(TPage).Name}"
        );
    }
    
    public IVerificationContext<TApp> UrlMatches(string pattern)
    {
        return ExecuteVerification(
            () => {
                var currentUrl = _driver.CurrentUrl;
                return Regex.IsMatch(currentUrl, pattern, RegexOptions.IgnoreCase);
            },
            $"URL should match pattern '{pattern}'",
            $"URL '{_driver.CurrentUrl}' does not match pattern '{pattern}'"
        );
    }
    
    public IVerificationContext<TApp> UrlContains(string text)
    {
        return ExecuteVerification(
            () => _driver.CurrentUrl.Contains(text, StringComparison.OrdinalIgnoreCase),
            $"URL should contain '{text}'",
            $"URL '{_driver.CurrentUrl}' does not contain '{text}'"
        );
    }
    
    public IVerificationContext<TApp> TitleContains(string text)
    {
        return ExecuteVerification(
            () => _driver.GetTitle().Contains(text, StringComparison.OrdinalIgnoreCase),
            $"Page title should contain '{text}'",
            $"Page title '{_driver.GetTitle()}' does not contain '{text}'"
        );
    }
    
    public IVerificationContext<TApp> TitleIs(string text)
    {
        return ExecuteVerification(
            () => _driver.GetTitle().Equals(text, StringComparison.OrdinalIgnoreCase),
            $"Page title should be '{text}'",
            $"Page title is '{_driver.GetTitle()}', expected '{text}'"
        );
    }
    
    public IVerificationContext<TApp> PageContainsText(string text)
    {
        return ExecuteVerification(
            () => _driver.GetPageText().Contains(text, StringComparison.OrdinalIgnoreCase),
            $"Page should contain text '{text}'",
            $"Page does not contain text '{text}'"
        );
    }
    
    public IVerificationContext<TApp> PageDoesNotContainText(string text)
    {
        return ExecuteVerification(
            () => !_driver.GetPageText().Contains(text, StringComparison.OrdinalIgnoreCase),
            $"Page should not contain text '{text}'",
            $"Page contains text '{text}' but should not"
        );
    }
    
    public IVerificationContext<TApp> That(Func<bool> condition, string description)
    {
        return ExecuteVerification(
            condition,
            description,
            $"Custom verification failed: {description}"
        );
    }
    
    public IVerificationContext<TApp> That<T>(Func<T> actual, Func<T, bool> condition, string description)
    {
        return ExecuteVerification(
            () => {
                var value = actual();
                return condition(value);
            },
            description,
            $"Custom verification failed: {description}"
        );
    }
    
    public IVerificationContext<TApp> That<T>(Func<T> actual, T expected, string description)
    {
        return ExecuteVerification(
            () => {
                var value = actual();
                return EqualityComparer<T>.Default.Equals(value, expected);
            },
            description,
            $"Expected {expected}, but got {actual()}"
        );
    }
    
    // Async versions
    public async Task<IVerificationContext<TApp>> ElementIsVisibleAsync(string selector)
    {
        return await ExecuteVerificationAsync(
            async () => await _driver.IsVisibleAsync(selector),
            $"Element '{selector}' should be visible",
            $"Element '{selector}' is not visible"
        );
    }
    
    public async Task<IVerificationContext<TApp>> ElementContainsTextAsync(string selector, string text)
    {
        return await ExecuteVerificationAsync(
            async () => (await _driver.GetTextAsync(selector)).Contains(text, StringComparison.OrdinalIgnoreCase),
            $"Element '{selector}' should contain text '{text}'",
            $"Element '{selector}' does not contain text '{text}'"
        );
    }
    
    public async Task<IVerificationContext<TApp>> ThatAsync(Func<Task<bool>> condition, string description)
    {
        return await ExecuteVerificationAsync(
            condition,
            description,
            $"Custom verification failed: {description}"
        );
    }
    
    private IVerificationContext<TApp> ExecuteVerification(Func<bool> condition, string successMessage, string failureMessage)
    {
        try
        {
            _logger.LogDebug($"Executing verification: {successMessage}");
            
            var result = condition();
            
            if (result)
            {
                _logger.LogDebug($"Verification passed: {successMessage}");
                _results.Add(new VerificationResult(true, successMessage));
            }
            else
            {
                _logger.LogError($"Verification failed: {failureMessage}");
                _results.Add(new VerificationResult(false, failureMessage));
                throw new VerificationException(failureMessage);
            }
        }
        catch (Exception ex) when (ex is not VerificationException)
        {
            _logger.LogError(ex, $"Verification error: {failureMessage}");
            _results.Add(new VerificationResult(false, failureMessage, ex));
            throw new VerificationException(failureMessage, ex);
        }
        
        return this;
    }
    
    private async Task<IVerificationContext<TApp>> ExecuteVerificationAsync(Func<Task<bool>> condition, string successMessage, string failureMessage)
    {
        try
        {
            _logger.LogDebug($"Executing async verification: {successMessage}");
            
            var result = await condition();
            
            if (result)
            {
                _logger.LogDebug($"Async verification passed: {successMessage}");
                _results.Add(new VerificationResult(true, successMessage));
            }
            else
            {
                _logger.LogError($"Async verification failed: {failureMessage}");
                _results.Add(new VerificationResult(false, failureMessage));
                throw new VerificationException(failureMessage);
            }
        }
        catch (Exception ex) when (ex is not VerificationException)
        {
            _logger.LogError(ex, $"Async verification error: {failureMessage}");
            _results.Add(new VerificationResult(false, failureMessage, ex));
            throw new VerificationException(failureMessage, ex);
        }
        
        return this;
    }
    
    private TPage CreatePageComponent<TPage>() where TPage : BasePageComponent<TApp>
    {
        // This would typically use a page factory
        var constructor = typeof(TPage).GetConstructor(new[] 
        { 
            typeof(IUIDriver), 
            typeof(FluentUIScaffoldOptions), 
            typeof(ILogger) 
        });
        
        if (constructor == null)
            throw new InvalidOperationException($"Cannot create page component {typeof(TPage).Name}");
            
        return (TPage)constructor.Invoke(new object[] { _driver, _options, _logger });
    }
}
```

### 3. Implement VerificationResult Class

```csharp
public class VerificationResult
{
    public bool Success { get; }
    public string Message { get; }
    public Exception Exception { get; }
    public DateTime Timestamp { get; }
    
    public VerificationResult(bool success, string message, Exception exception = null)
    {
        Success = success;
        Message = message;
        Exception = exception;
        Timestamp = DateTime.UtcNow;
    }
}
```

### 4. Implement VerificationException Class

```csharp
public class VerificationException : FluentUIScaffoldException
{
    public string ScreenshotPath { get; set; }
    public string DOMState { get; set; }
    public string CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; set; }
    
    public VerificationException(string message) : base(message) { }
    public VerificationException(string message, Exception innerException) : base(message, innerException) { }
}
```

### 5. Add Retry Logic Support

```csharp
public class VerificationContext<TApp> : IVerificationContext<TApp>
{
    private readonly RetryPolicy _retryPolicy;
    
    public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _results = new List<VerificationResult>();
        
        _retryPolicy = Policy
            .Handle<VerificationException>()
            .WaitAndRetry(
                _options.VerificationRetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(_options.VerificationRetryDelay * retryAttempt),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Verification attempt {retryCount} failed, retrying in {timeSpan.TotalMilliseconds}ms");
                }
            );
    }
    
    private IVerificationContext<TApp> ExecuteVerificationWithRetry(Func<bool> condition, string successMessage, string failureMessage)
    {
        return _retryPolicy.Execute(() => ExecuteVerification(condition, successMessage, failureMessage));
    }
}
```

### 6. Add Unit Tests

```csharp
[TestFixture]
public class VerificationContextTests
{
    [Test]
    public void ElementIsVisible_WithVisibleElement_ReturnsContext()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.IsVisible("#test")).Returns(true);
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var context = new VerificationContext<WebApp>(driver, options, logger);
        
        // Act
        var result = context.ElementIsVisible("#test");
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<IVerificationContext<WebApp>>());
    }
    
    [Test]
    public void ElementIsVisible_WithHiddenElement_ThrowsException()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.IsVisible("#test")).Returns(false);
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var context = new VerificationContext<WebApp>(driver, options, logger);
        
        // Act & Assert
        Assert.Throws<VerificationException>(() => context.ElementIsVisible("#test"));
    }
    
    [Test]
    public void ElementContainsText_WithMatchingText_ReturnsContext()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.GetText("#test")).Returns("Hello World");
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var context = new VerificationContext<WebApp>(driver, options, logger);
        
        // Act
        var result = context.ElementContainsText("#test", "World");
        
        // Assert
        Assert.That(result, Is.Not.Null);
    }
    
    [Test]
    public void That_WithCustomCondition_ReturnsContext()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var context = new VerificationContext<WebApp>(driver, options, logger);
        
        // Act
        var result = context.That(() => true, "Custom verification");
        
        // Assert
        Assert.That(result, Is.Not.Null);
    }
}
```

### 7. Add Integration Tests

```csharp
[TestFixture]
public class VerificationContextIntegrationTests
{
    [Test]
    public async Task ElementVerifications_WithRealBrowser_WorkCorrectly()
    {
        // Arrange
        var options = new FluentUIScaffoldOptions { BaseUrl = "https://localhost:5001" };
        var scaffold = FluentUIScaffold<WebApp>(options);
        var page = await scaffold.NavigateToAsync<LoginPage>();
        
        // Act & Assert
        page.Verify
            .ElementIsVisible("#username")
            .ElementIsVisible("#password")
            .ElementIsVisible("#login-button")
            .ElementContainsText("h1", "Login")
            .That(() => page.Driver.CurrentUrl.Contains("/login"), "Should be on login page");
    }
}
```

## Definition of Done

- [ ] IVerificationContext<TApp> interface is fully implemented
- [ ] VerificationContext<TApp> class is implemented with all methods
- [ ] Element verification methods are working (visibility, text, attributes, state)
- [ ] Page verification methods are working (URL, title, content)
- [ ] Custom verification methods are working
- [ ] Async verification methods are implemented
- [ ] Retry logic is implemented and configurable
- [ ] Error handling is comprehensive with detailed messages
- [ ] Logging is detailed and useful for debugging
- [ ] Verification results are tracked and accessible
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate real scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the base page component and driver implementations
- Verification should be fluent and chainable
- Consider performance implications of multiple verifications
- Plan for future mobile verification support
- Ensure thread safety for concurrent verifications 