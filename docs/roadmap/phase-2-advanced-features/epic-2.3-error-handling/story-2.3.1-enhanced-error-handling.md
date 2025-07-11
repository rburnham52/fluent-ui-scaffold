# Story 2.3.1: Enhanced Error Handling

## Story Information
- **Epic**: Epic 2.3 - Error Handling and Debugging
- **Priority**: Medium
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.1.2
- **File**: `phase-2-advanced-features/epic-2.3-error-handling/story-2.3.1-enhanced-error-handling.md`

## User Story

**As a** test developer  
**I want** comprehensive error handling that provides detailed context and recovery options  
**So that** I can quickly diagnose and resolve test failures

## Acceptance Criteria

- [ ] Enhanced exception hierarchy is implemented with detailed context
- [ ] Error handling includes automatic screenshot capture on failure
- [ ] Error handling includes DOM state capture on failure
- [ ] Error handling provides detailed error messages with context
- [ ] Error handling supports custom error recovery strategies
- [ ] Error handling includes retry mechanisms for transient failures
- [ ] Error handling provides debugging information and stack traces
- [ ] Error handling supports framework-specific error details
- [ ] Error handling includes performance impact monitoring
- [ ] Comprehensive unit tests are written and passing
- [ ] Integration tests demonstrate error handling scenarios

## Technical Tasks

### 1. Implement Enhanced Exception Hierarchy

```csharp
public abstract class FluentUIScaffoldException : Exception
{
    public string ScreenshotPath { get; set; }
    public string DOMState { get; set; }
    public string CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; set; }
    public DateTime Timestamp { get; set; }
    public string TestName { get; set; }
    public string TestMethod { get; set; }
    
    protected FluentUIScaffoldException(string message) : base(message)
    {
        Context = new Dictionary<string, object>();
        Timestamp = DateTime.UtcNow;
    }
    
    protected FluentUIScaffoldException(string message, Exception innerException) : base(message, innerException)
    {
        Context = new Dictionary<string, object>();
        Timestamp = DateTime.UtcNow;
    }
}

public class ElementNotFoundException : FluentUIScaffoldException
{
    public string Selector { get; }
    public TimeSpan Timeout { get; }
    
    public ElementNotFoundException(string selector, TimeSpan timeout) 
        : base($"Element not found: {selector} after {timeout.TotalSeconds}s timeout")
    {
        Selector = selector;
        Timeout = timeout;
    }
}

public class ElementTimeoutException : FluentUIScaffoldException
{
    public string Selector { get; }
    public TimeSpan Timeout { get; }
    public string ExpectedCondition { get; }
    
    public ElementTimeoutException(string selector, TimeSpan timeout, string expectedCondition) 
        : base($"Element timeout: {selector} - {expectedCondition} after {timeout.TotalSeconds}s timeout")
    {
        Selector = selector;
        Timeout = timeout;
        ExpectedCondition = expectedCondition;
    }
}

public class ElementInteractionException : FluentUIScaffoldException
{
    public string Selector { get; }
    public string Action { get; }
    public string Value { get; }
    
    public ElementInteractionException(string selector, string action, string value, Exception innerException) 
        : base($"Failed to {action} on element {selector} with value '{value}'", innerException)
    {
        Selector = selector;
        Action = action;
        Value = value;
    }
}

public class NavigationException : FluentUIScaffoldException
{
    public string FromUrl { get; }
    public string ToUrl { get; }
    public string ExpectedUrl { get; }
    
    public NavigationException(string fromUrl, string toUrl, string expectedUrl) 
        : base($"Navigation failed: from {fromUrl} to {toUrl}, expected {expectedUrl}")
    {
        FromUrl = fromUrl;
        ToUrl = toUrl;
        ExpectedUrl = expectedUrl;
    }
}

public class VerificationException : FluentUIScaffoldException
{
    public string Verification { get; }
    public string ExpectedValue { get; }
    public string ActualValue { get; }
    
    public VerificationException(string verification, string expectedValue, string actualValue) 
        : base($"Verification failed: {verification} - expected '{expectedValue}', got '{actualValue}'")
    {
        Verification = verification;
        ExpectedValue = expectedValue;
        ActualValue = actualValue;
    }
}

public class FrameworkException : FluentUIScaffoldException
{
    public string FrameworkName { get; }
    public string FrameworkVersion { get; }
    public string FrameworkError { get; }
    
    public FrameworkException(string frameworkName, string frameworkVersion, string frameworkError, Exception innerException) 
        : base($"Framework error in {frameworkName} {frameworkVersion}: {frameworkError}", innerException)
    {
        FrameworkName = frameworkName;
        FrameworkVersion = frameworkVersion;
        FrameworkError = frameworkError;
    }
}
```

### 2. Implement Error Context Builder

```csharp
public interface IErrorContextBuilder
{
    IErrorContextBuilder WithScreenshot(string path);
    IErrorContextBuilder WithDOMState(string domState);
    IErrorContextBuilder WithCurrentUrl(string url);
    IErrorContextBuilder WithContext(string key, object value);
    IErrorContextBuilder WithTestInfo(string testName, string testMethod);
    IErrorContextBuilder WithPerformanceMetrics(Dictionary<string, object> metrics);
    IErrorContextBuilder WithFrameworkInfo(string frameworkName, string frameworkVersion);
    FluentUIScaffoldException BuildException(Exception innerException = null);
}

public class ErrorContextBuilder : IErrorContextBuilder
{
    private readonly string _message;
    private string _screenshotPath;
    private string _domState;
    private string _currentUrl;
    private readonly Dictionary<string, object> _context = new();
    private string _testName;
    private string _testMethod;
    private Dictionary<string, object> _performanceMetrics;
    private string _frameworkName;
    private string _frameworkVersion;
    
    public ErrorContextBuilder(string message)
    {
        _message = message;
    }
    
    public IErrorContextBuilder WithScreenshot(string path)
    {
        _screenshotPath = path;
        return this;
    }
    
    public IErrorContextBuilder WithDOMState(string domState)
    {
        _domState = domState;
        return this;
    }
    
    public IErrorContextBuilder WithCurrentUrl(string url)
    {
        _currentUrl = url;
        return this;
    }
    
    public IErrorContextBuilder WithContext(string key, object value)
    {
        _context[key] = value;
        return this;
    }
    
    public IErrorContextBuilder WithTestInfo(string testName, string testMethod)
    {
        _testName = testName;
        _testMethod = testMethod;
        return this;
    }
    
    public IErrorContextBuilder WithPerformanceMetrics(Dictionary<string, object> metrics)
    {
        _performanceMetrics = metrics;
        return this;
    }
    
    public IErrorContextBuilder WithFrameworkInfo(string frameworkName, string frameworkVersion)
    {
        _frameworkName = frameworkName;
        _frameworkVersion = frameworkVersion;
        return this;
    }
    
    public FluentUIScaffoldException BuildException(Exception innerException = null)
    {
        var exception = new FluentUIScaffoldException(_message, innerException)
        {
            ScreenshotPath = _screenshotPath,
            DOMState = _domState,
            CurrentUrl = _currentUrl,
            TestName = _testName,
            TestMethod = _testMethod
        };
        
        foreach (var kvp in _context)
        {
            exception.Context[kvp.Key] = kvp.Value;
        }
        
        if (_performanceMetrics != null)
        {
            exception.Context["PerformanceMetrics"] = _performanceMetrics;
        }
        
        if (!string.IsNullOrEmpty(_frameworkName))
        {
            exception.Context["FrameworkName"] = _frameworkName;
            exception.Context["FrameworkVersion"] = _frameworkVersion;
        }
        
        return exception;
    }
}
```

### 3. Implement Error Recovery Strategies

```csharp
public interface IErrorRecoveryStrategy
{
    bool CanHandle(Exception exception);
    Task<bool> TryRecover(Exception exception, IUIDriver driver);
    TimeSpan GetRecoveryTimeout();
    int GetMaxRetries();
}

public class ElementNotFoundRecoveryStrategy : IErrorRecoveryStrategy
{
    public bool CanHandle(Exception exception)
    {
        return exception is ElementNotFoundException;
    }
    
    public async Task<bool> TryRecover(Exception exception, IUIDriver driver)
    {
        var elementException = (ElementNotFoundException)exception;
        
        // Try refreshing the page
        try
        {
            await driver.RefreshAsync();
            await Task.Delay(2000); // Wait for page to load
            
            // Check if element is now available
            return await driver.IsVisibleAsync(elementException.Selector);
        }
        catch
        {
            return false;
        }
    }
    
    public TimeSpan GetRecoveryTimeout() => TimeSpan.FromSeconds(10);
    public int GetMaxRetries() => 2;
}

public class NavigationRecoveryStrategy : IErrorRecoveryStrategy
{
    public bool CanHandle(Exception exception)
    {
        return exception is NavigationException;
    }
    
    public async Task<bool> TryRecover(Exception exception, IUIDriver driver)
    {
        var navigationException = (NavigationException)exception;
        
        // Try navigating again
        try
        {
            await driver.NavigateToUrlAsync(navigationException.ExpectedUrl);
            await Task.Delay(3000); // Wait for navigation
            
            return driver.CurrentUrl == navigationException.ExpectedUrl;
        }
        catch
        {
            return false;
        }
    }
    
    public TimeSpan GetRecoveryTimeout() => TimeSpan.FromSeconds(15);
    public int GetMaxRetries() => 1;
}

public class VerificationRecoveryStrategy : IErrorRecoveryStrategy
{
    public bool CanHandle(Exception exception)
    {
        return exception is VerificationException;
    }
    
    public async Task<bool> TryRecover(Exception exception, IUIDriver driver)
    {
        // For verification failures, wait a bit and try again
        await Task.Delay(1000);
        return true; // Always return true to retry verification
    }
    
    public TimeSpan GetRecoveryTimeout() => TimeSpan.FromSeconds(5);
    public int GetMaxRetries() => 3;
}
```

### 4. Implement Error Handler

```csharp
public interface IErrorHandler
{
    Task<Exception> HandleException(Exception exception, IUIDriver driver, ILogger logger);
    void RegisterRecoveryStrategy(IErrorRecoveryStrategy strategy);
    void EnableAutomaticScreenshots(bool enable);
    void EnableDOMCapture(bool enable);
    void SetScreenshotPath(string path);
}

public class ErrorHandler : IErrorHandler
{
    private readonly List<IErrorRecoveryStrategy> _recoveryStrategies = new();
    private readonly ILogger _logger;
    private bool _enableScreenshots = true;
    private bool _enableDOMCapture = true;
    private string _screenshotPath = "./screenshots";
    
    public ErrorHandler(ILogger logger)
    {
        _logger = logger;
        
        // Register default recovery strategies
        RegisterRecoveryStrategy(new ElementNotFoundRecoveryStrategy());
        RegisterRecoveryStrategy(new NavigationRecoveryStrategy());
        RegisterRecoveryStrategy(new VerificationRecoveryStrategy());
    }
    
    public async Task<Exception> HandleException(Exception exception, IUIDriver driver, ILogger logger)
    {
        _logger.LogError(exception, "Handling exception in FluentUIScaffold");
        
        // Capture diagnostic information
        var enhancedException = await CaptureDiagnosticInfo(exception, driver);
        
        // Try recovery strategies
        foreach (var strategy in _recoveryStrategies)
        {
            if (strategy.CanHandle(exception))
            {
                _logger.LogInformation($"Attempting recovery with strategy: {strategy.GetType().Name}");
                
                for (int i = 0; i < strategy.GetMaxRetries(); i++)
                {
                    try
                    {
                        var recovered = await strategy.TryRecover(exception, driver);
                        if (recovered)
                        {
                            _logger.LogInformation($"Recovery successful after {i + 1} attempts");
                            return null; // Recovery successful
                        }
                    }
                    catch (Exception recoveryException)
                    {
                        _logger.LogWarning(recoveryException, $"Recovery attempt {i + 1} failed");
                    }
                    
                    if (i < strategy.GetMaxRetries() - 1)
                    {
                        await Task.Delay(strategy.GetRecoveryTimeout());
                    }
                }
                
                _logger.LogWarning($"All recovery attempts failed for {strategy.GetType().Name}");
            }
        }
        
        return enhancedException;
    }
    
    public void RegisterRecoveryStrategy(IErrorRecoveryStrategy strategy)
    {
        _recoveryStrategies.Add(strategy);
    }
    
    public void EnableAutomaticScreenshots(bool enable)
    {
        _enableScreenshots = enable;
    }
    
    public void EnableDOMCapture(bool enable)
    {
        _enableDOMCapture = enable;
    }
    
    public void SetScreenshotPath(string path)
    {
        _screenshotPath = path;
    }
    
    private async Task<FluentUIScaffoldException> CaptureDiagnosticInfo(Exception exception, IUIDriver driver)
    {
        var builder = new ErrorContextBuilder(exception.Message);
        
        try
        {
            // Capture current URL
            builder.WithCurrentUrl(driver.CurrentUrl);
            
            // Capture screenshot if enabled
            if (_enableScreenshots)
            {
                var screenshotPath = await CaptureScreenshot(driver);
                builder.WithScreenshot(screenshotPath);
            }
            
            // Capture DOM state if enabled
            if (_enableDOMCapture)
            {
                var domState = await CaptureDOMState(driver);
                builder.WithDOMState(domState);
            }
            
            // Add framework information
            var frameworkInfo = driver.GetFrameworkInfo();
            if (frameworkInfo != null)
            {
                builder.WithFrameworkInfo(frameworkInfo.Name, frameworkInfo.Version);
            }
            
            // Add performance metrics
            var metrics = driver.GetPerformanceMetrics();
            if (metrics != null)
            {
                builder.WithPerformanceMetrics(metrics);
            }
        }
        catch (Exception captureException)
        {
            _logger.LogWarning(captureException, "Failed to capture diagnostic information");
        }
        
        return builder.BuildException(exception);
    }
    
    private async Task<string> CaptureScreenshot(IUIDriver driver)
    {
        try
        {
            var fileName = $"screenshot_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            var filePath = Path.Combine(_screenshotPath, fileName);
            
            Directory.CreateDirectory(_screenshotPath);
            await driver.TakeScreenshotAsync(filePath);
            
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture screenshot");
            return null;
        }
    }
    
    private async Task<string> CaptureDOMState(IUIDriver driver)
    {
        try
        {
            return await driver.GetPageSourceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to capture DOM state");
            return null;
        }
    }
}
```

### 5. Add Unit Tests

```csharp
[TestFixture]
public class ErrorHandlingTests
{
    [Test]
    public void ElementNotFoundException_WithContext_ContainsSelector()
    {
        // Arrange & Act
        var exception = new ElementNotFoundException("#test", TimeSpan.FromSeconds(10));
        
        // Assert
        Assert.That(exception.Selector, Is.EqualTo("#test"));
        Assert.That(exception.Timeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
    }
    
    [Test]
    public void ErrorContextBuilder_WithScreenshot_BuildsExceptionWithPath()
    {
        // Arrange
        var builder = new ErrorContextBuilder("Test error");
        
        // Act
        var exception = builder.WithScreenshot("/path/to/screenshot.png").BuildException();
        
        // Assert
        Assert.That(exception.ScreenshotPath, Is.EqualTo("/path/to/screenshot.png"));
    }
    
    [Test]
    public async Task ErrorHandler_WithRecoveryStrategy_AttemptsRecovery()
    {
        // Arrange
        var logger = Mock.Of<ILogger>();
        var handler = new ErrorHandler(logger);
        var driver = Mock.Of<IUIDriver>();
        var exception = new ElementNotFoundException("#test", TimeSpan.FromSeconds(10));
        
        // Act
        var result = await handler.HandleException(exception, driver, logger);
        
        // Assert
        Assert.That(result, Is.Not.Null);
    }
}
```

## Definition of Done

- [ ] Enhanced exception hierarchy is implemented with detailed context
- [ ] Error handling includes automatic screenshot capture on failure
- [ ] Error handling includes DOM state capture on failure
- [ ] Error handling provides detailed error messages with context
- [ ] Error handling supports custom error recovery strategies
- [ ] Error handling includes retry mechanisms for transient failures
- [ ] Error handling provides debugging information and stack traces
- [ ] Error handling supports framework-specific error details
- [ ] Error handling includes performance impact monitoring
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate error handling scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the core interfaces and driver implementations
- Error handling should be comprehensive but not overly complex
- Consider performance implications of diagnostic capture
- Plan for future mobile error handling support
- Ensure thread safety for concurrent error handling 