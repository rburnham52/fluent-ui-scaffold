# Story 2.4.1: Logging Framework Integration

## Story Information
- **Epic**: Epic 2.4 - Logging Integration
- **Priority**: Medium
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.1.2
- **File**: `phase-2-advanced-features/epic-2.4-logging/story-2.4.1-logging-framework.md`

## User Story

**As a** test developer  
**I want** comprehensive logging that provides detailed insights into test execution  
**So that** I can debug issues and monitor test performance effectively

## Acceptance Criteria

- [ ] Logging framework integration supports multiple logging providers
- [ ] Logging includes detailed action tracking (clicks, typing, navigation)
- [ ] Logging includes performance metrics and timing information
- [ ] Logging includes element state and page information
- [ ] Logging supports structured logging with context
- [ ] Logging includes error tracking and stack traces
- [ ] Logging supports different log levels and filtering
- [ ] Logging includes framework-specific information
- [ ] Logging supports custom log sinks and outputs
- [ ] Logging includes test execution context and metadata
- [ ] Comprehensive unit tests are written and passing
- [ ] Integration tests demonstrate logging scenarios

## Technical Tasks

### 1. Implement Logging Interface

```csharp
public interface IFluentUIScaffoldLogger
{
    void LogAction(string action, string target, Dictionary<string, object> context = null);
    void LogNavigation(string from, string to, TimeSpan duration);
    void LogElementInteraction(string element, string action, string value, TimeSpan duration);
    void LogVerification(string verification, bool success, string details);
    void LogWait(string condition, TimeSpan duration, int attempts);
    void LogError(Exception exception, Dictionary<string, object> context = null);
    void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object> metrics = null);
    void LogFrameworkInfo(string framework, string version, Dictionary<string, object> capabilities = null);
    void LogTestContext(string testName, string testMethod, Dictionary<string, object> context = null);
    void LogDebug(string message, Dictionary<string, object> context = null);
    void LogInfo(string message, Dictionary<string, object> context = null);
    void LogWarning(string message, Dictionary<string, object> context = null);
    void LogError(string message, Dictionary<string, object> context = null);
}

public interface ILoggingProvider
{
    void Log(LogLevel level, string message, Dictionary<string, object> context = null);
    void LogException(Exception exception, Dictionary<string, object> context = null);
    void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object> metrics = null);
    void LogStructured(string category, string message, Dictionary<string, object> context = null);
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Critical
}
```

### 2. Implement Default Logging Provider

```csharp
public class DefaultLoggingProvider : ILoggingProvider
{
    private readonly ILogger _logger;
    private readonly LoggingOptions _options;
    
    public DefaultLoggingProvider(ILogger logger, LoggingOptions options)
    {
        _logger = logger;
        _options = options;
    }
    
    public void Log(LogLevel level, string message, Dictionary<string, object> context = null)
    {
        var logMessage = FormatMessage(message, context);
        
        switch (level)
        {
            case LogLevel.Debug:
                _logger.LogDebug(logMessage);
                break;
            case LogLevel.Information:
                _logger.LogInformation(logMessage);
                break;
            case LogLevel.Warning:
                _logger.LogWarning(logMessage);
                break;
            case LogLevel.Error:
                _logger.LogError(logMessage);
                break;
            case LogLevel.Critical:
                _logger.LogCritical(logMessage);
                break;
        }
    }
    
    public void LogException(Exception exception, Dictionary<string, object> context = null)
    {
        var message = FormatMessage("Exception occurred", context);
        _logger.LogError(exception, message);
    }
    
    public void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object> metrics = null)
    {
        var context = new Dictionary<string, object>
        {
            ["Operation"] = operation,
            ["Duration"] = duration.TotalMilliseconds,
            ["DurationMs"] = duration.TotalMilliseconds
        };
        
        if (metrics != null)
        {
            foreach (var kvp in metrics)
            {
                context[kvp.Key] = kvp.Value;
            }
        }
        
        var message = $"Performance: {operation} took {duration.TotalMilliseconds:F2}ms";
        Log(LogLevel.Information, message, context);
    }
    
    public void LogStructured(string category, string message, Dictionary<string, object> context = null)
    {
        var structuredContext = new Dictionary<string, object>
        {
            ["Category"] = category
        };
        
        if (context != null)
        {
            foreach (var kvp in context)
            {
                structuredContext[kvp.Key] = kvp.Value;
            }
        }
        
        Log(LogLevel.Information, message, structuredContext);
    }
    
    private string FormatMessage(string message, Dictionary<string, object> context)
    {
        if (context == null || !context.Any())
            return message;
        
        var contextString = string.Join(", ", context.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{message} | {contextString}";
    }
}
```

### 3. Implement FluentUIScaffold Logger

```csharp
public class FluentUIScaffoldLogger : IFluentUIScaffoldLogger
{
    private readonly ILoggingProvider _provider;
    private readonly LoggingOptions _options;
    private readonly Dictionary<string, object> _testContext = new();
    
    public FluentUIScaffoldLogger(ILoggingProvider provider, LoggingOptions options)
    {
        _provider = provider;
        _options = options;
    }
    
    public void LogAction(string action, string target, Dictionary<string, object> context = null)
    {
        var logContext = new Dictionary<string, object>
        {
            ["Action"] = action,
            ["Target"] = target,
            ["Timestamp"] = DateTime.UtcNow
        };
        
        if (context != null)
        {
            foreach (var kvp in context)
            {
                logContext[kvp.Key] = kvp.Value;
            }
        }
        
        MergeTestContext(logContext);
        _provider.LogStructured("Action", $"Performed {action} on {target}", logContext);
    }
    
    public void LogNavigation(string from, string to, TimeSpan duration)
    {
        var context = new Dictionary<string, object>
        {
            ["FromUrl"] = from,
            ["ToUrl"] = to,
            ["Duration"] = duration.TotalMilliseconds,
            ["DurationMs"] = duration.TotalMilliseconds
        };
        
        MergeTestContext(context);
        _provider.LogStructured("Navigation", $"Navigated from {from} to {to} in {duration.TotalMilliseconds:F2}ms", context);
    }
    
    public void LogElementInteraction(string element, string action, string value, TimeSpan duration)
    {
        var context = new Dictionary<string, object>
        {
            ["Element"] = element,
            ["Action"] = action,
            ["Value"] = value,
            ["Duration"] = duration.TotalMilliseconds,
            ["DurationMs"] = duration.TotalMilliseconds
        };
        
        MergeTestContext(context);
        _provider.LogStructured("ElementInteraction", $"Interacted with {element}: {action} '{value}' in {duration.TotalMilliseconds:F2}ms", context);
    }
    
    public void LogVerification(string verification, bool success, string details)
    {
        var context = new Dictionary<string, object>
        {
            ["Verification"] = verification,
            ["Success"] = success,
            ["Details"] = details,
            ["Timestamp"] = DateTime.UtcNow
        };
        
        MergeTestContext(context);
        var level = success ? LogLevel.Information : LogLevel.Warning;
        _provider.Log(level, $"Verification: {verification} - {(success ? "PASSED" : "FAILED")} - {details}", context);
    }
    
    public void LogWait(string condition, TimeSpan duration, int attempts)
    {
        var context = new Dictionary<string, object>
        {
            ["Condition"] = condition,
            ["Duration"] = duration.TotalMilliseconds,
            ["DurationMs"] = duration.TotalMilliseconds,
            ["Attempts"] = attempts
        };
        
        MergeTestContext(context);
        _provider.LogStructured("Wait", $"Waited for {condition} in {duration.TotalMilliseconds:F2}ms ({attempts} attempts)", context);
    }
    
    public void LogError(Exception exception, Dictionary<string, object> context = null)
    {
        var logContext = new Dictionary<string, object>
        {
            ["ExceptionType"] = exception.GetType().Name,
            ["ExceptionMessage"] = exception.Message,
            ["StackTrace"] = exception.StackTrace
        };
        
        if (context != null)
        {
            foreach (var kvp in context)
            {
                logContext[kvp.Key] = kvp.Value;
            }
        }
        
        MergeTestContext(logContext);
        _provider.LogException(exception, logContext);
    }
    
    public void LogPerformance(string operation, TimeSpan duration, Dictionary<string, object> metrics = null)
    {
        _provider.LogPerformance(operation, duration, metrics);
    }
    
    public void LogFrameworkInfo(string framework, string version, Dictionary<string, object> capabilities = null)
    {
        var context = new Dictionary<string, object>
        {
            ["Framework"] = framework,
            ["Version"] = version
        };
        
        if (capabilities != null)
        {
            foreach (var kvp in capabilities)
            {
                context[kvp.Key] = kvp.Value;
            }
        }
        
        MergeTestContext(context);
        _provider.LogStructured("Framework", $"Using {framework} {version}", context);
    }
    
    public void LogTestContext(string testName, string testMethod, Dictionary<string, object> context = null)
    {
        _testContext["TestName"] = testName;
        _testContext["TestMethod"] = testMethod;
        
        if (context != null)
        {
            foreach (var kvp in context)
            {
                _testContext[kvp.Key] = kvp.Value;
            }
        }
        
        var logContext = new Dictionary<string, object>(_testContext);
        _provider.LogStructured("TestContext", $"Test context: {testName}.{testMethod}", logContext);
    }
    
    public void LogDebug(string message, Dictionary<string, object> context = null)
    {
        MergeTestContext(context);
        _provider.Log(LogLevel.Debug, message, context);
    }
    
    public void LogInfo(string message, Dictionary<string, object> context = null)
    {
        MergeTestContext(context);
        _provider.Log(LogLevel.Information, message, context);
    }
    
    public void LogWarning(string message, Dictionary<string, object> context = null)
    {
        MergeTestContext(context);
        _provider.Log(LogLevel.Warning, message, context);
    }
    
    public void LogError(string message, Dictionary<string, object> context = null)
    {
        MergeTestContext(context);
        _provider.Log(LogLevel.Error, message, context);
    }
    
    private void MergeTestContext(Dictionary<string, object> context)
    {
        if (context == null) return;
        
        foreach (var kvp in _testContext)
        {
            if (!context.ContainsKey(kvp.Key))
            {
                context[kvp.Key] = kvp.Value;
            }
        }
    }
}
```

### 4. Implement Logging Options

```csharp
public class LoggingOptions
{
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;
    public bool EnableActionLogging { get; set; } = true;
    public bool EnableNavigationLogging { get; set; } = true;
    public bool EnableElementLogging { get; set; } = true;
    public bool EnableVerificationLogging { get; set; } = true;
    public bool EnableWaitLogging { get; set; } = true;
    public bool EnablePerformanceLogging { get; set; } = true;
    public bool EnableFrameworkLogging { get; set; } = true;
    public bool EnableTestContextLogging { get; set; } = true;
    public bool IncludeTimestamps { get; set; } = true;
    public bool IncludeStackTraces { get; set; } = true;
    public string LogFormat { get; set; } = "structured";
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}
```

### 5. Implement Logging Factory

```csharp
public interface ILoggingFactory
{
    IFluentUIScaffoldLogger CreateLogger(LoggingOptions options);
    ILoggingProvider CreateProvider(string providerType, LoggingOptions options);
    void RegisterProvider(string name, Type providerType);
}

public class LoggingFactory : ILoggingFactory
{
    private readonly Dictionary<string, Type> _providers = new();
    private readonly IServiceProvider _serviceProvider;
    
    public LoggingFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        RegisterDefaultProviders();
    }
    
    public IFluentUIScaffoldLogger CreateLogger(LoggingOptions options)
    {
        var provider = CreateProvider("default", options);
        return new FluentUIScaffoldLogger(provider, options);
    }
    
    public ILoggingProvider CreateProvider(string providerType, LoggingOptions options)
    {
        if (_providers.TryGetValue(providerType, out var type))
        {
            return (ILoggingProvider)Activator.CreateInstance(type, _serviceProvider.GetService<ILogger>(), options);
        }
        
        // Default to console provider
        return new DefaultLoggingProvider(_serviceProvider.GetService<ILogger>(), options);
    }
    
    public void RegisterProvider(string name, Type providerType)
    {
        if (typeof(ILoggingProvider).IsAssignableFrom(providerType))
        {
            _providers[name] = providerType;
        }
        else
        {
            throw new ArgumentException($"Type {providerType.Name} does not implement ILoggingProvider");
        }
    }
    
    private void RegisterDefaultProviders()
    {
        RegisterProvider("default", typeof(DefaultLoggingProvider));
        RegisterProvider("console", typeof(DefaultLoggingProvider));
    }
}
```

### 6. Add Unit Tests

```csharp
[TestFixture]
public class LoggingTests
{
    [Test]
    public void LogAction_WithValidParameters_LogsCorrectly()
    {
        // Arrange
        var logger = Mock.Of<ILogger>();
        var options = new LoggingOptions();
        var provider = new DefaultLoggingProvider(logger, options);
        var scaffoldLogger = new FluentUIScaffoldLogger(provider, options);
        
        // Act
        scaffoldLogger.LogAction("click", "#button");
        
        // Assert
        Mock.Get(logger).Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
    
    [Test]
    public void LogNavigation_WithValidParameters_LogsCorrectly()
    {
        // Arrange
        var logger = Mock.Of<ILogger>();
        var options = new LoggingOptions();
        var provider = new DefaultLoggingProvider(logger, options);
        var scaffoldLogger = new FluentUIScaffoldLogger(provider, options);
        
        // Act
        scaffoldLogger.LogNavigation("/page1", "/page2", TimeSpan.FromSeconds(2));
        
        // Assert
        Mock.Get(logger).Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
    }
    
    [Test]
    public void LogError_WithException_LogsCorrectly()
    {
        // Arrange
        var logger = Mock.Of<ILogger>();
        var options = new LoggingOptions();
        var provider = new DefaultLoggingProvider(logger, options);
        var scaffoldLogger = new FluentUIScaffoldLogger(provider, options);
        var exception = new Exception("Test exception");
        
        // Act
        scaffoldLogger.LogError(exception);
        
        // Assert
        Mock.Get(logger).Verify(l => l.LogError(exception, It.IsAny<string>()), Times.Once);
    }
}
```

## Definition of Done

- [ ] Logging framework integration supports multiple logging providers
- [ ] Logging includes detailed action tracking (clicks, typing, navigation)
- [ ] Logging includes performance metrics and timing information
- [ ] Logging includes element state and page information
- [ ] Logging supports structured logging with context
- [ ] Logging includes error tracking and stack traces
- [ ] Logging supports different log levels and filtering
- [ ] Logging includes framework-specific information
- [ ] Logging supports custom log sinks and outputs
- [ ] Logging includes test execution context and metadata
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate logging scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the core interfaces and driver implementations
- Logging should be comprehensive but not overly verbose
- Consider performance implications of detailed logging
- Plan for future mobile logging support
- Ensure thread safety for concurrent logging operations 