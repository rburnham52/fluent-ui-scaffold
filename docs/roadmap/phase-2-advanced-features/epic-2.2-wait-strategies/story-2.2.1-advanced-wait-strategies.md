# Story 2.2.1: Advanced Wait Strategy Implementation

## Story Information
- **Epic**: Epic 2.2 - Wait Strategies and Smart Waiting
- **Priority**: Medium
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.3.1
- **File**: `phase-2-advanced-features/epic-2.2-wait-strategies/story-2.2.1-advanced-wait-strategies.md`

## User Story

**As a** test developer  
**I want** advanced wait strategies that handle complex waiting scenarios  
**So that** I can create robust tests that handle dynamic content and asynchronous operations

## Acceptance Criteria

- [ ] Advanced wait strategies are implemented (polling, exponential backoff, custom conditions)
- [ ] Wait strategies support multiple conditions (AND, OR, NOT)
- [ ] Wait strategies support custom timeout and retry configurations
- [ ] Wait strategies provide detailed logging and debugging information
- [ ] Wait strategies support both synchronous and asynchronous operations
- [ ] Wait strategies handle exceptions gracefully with proper error messages
- [ ] Wait strategies support framework-specific optimizations
- [ ] Wait strategies are configurable and extensible
- [ ] Wait strategies support performance monitoring and metrics
- [ ] Comprehensive unit tests are written and passing
- [ ] Integration tests demonstrate real waiting scenarios

## Technical Tasks

### 1. Implement Advanced Wait Strategy Interface

```csharp
public interface IAdvancedWaitStrategy
{
    IWaitStrategyBuilder ForElement(string selector);
    IWaitStrategyBuilder ForCondition(Func<bool> condition);
    IWaitStrategyBuilder ForCondition(Func<Task<bool>> condition);
    IWaitStrategyBuilder ForUrl(string urlPattern);
    IWaitStrategyBuilder ForTitle(string titlePattern);
    IWaitStrategyBuilder ForPageLoad();
    IWaitStrategyBuilder ForNetworkIdle();
    IWaitStrategyBuilder ForJavaScriptCondition(string script);
    IWaitStrategyBuilder ForCustomCondition<T>(Func<T, bool> condition, T context);
}

public interface IWaitStrategyBuilder
{
    IWaitStrategyBuilder WithTimeout(TimeSpan timeout);
    IWaitStrategyBuilder WithRetryInterval(TimeSpan interval);
    IWaitStrategyBuilder WithExponentialBackoff();
    IWaitStrategyBuilder WithLinearBackoff();
    IWaitStrategyBuilder WithCustomBackoff(Func<int, TimeSpan> backoffFunction);
    IWaitStrategyBuilder IgnoreExceptions(params Type[] exceptionTypes);
    IWaitStrategyBuilder WithMessage(string message);
    IWaitStrategyBuilder WithLogging(bool enableLogging);
    IWaitStrategyBuilder WithMetrics(bool enableMetrics);
    IWaitStrategyBuilder And(Func<bool> additionalCondition);
    IWaitStrategyBuilder Or(Func<bool> alternativeCondition);
    IWaitStrategyBuilder Not(Func<bool> negatedCondition);
    void Wait();
    Task WaitAsync();
    T WaitFor<T>(Func<T> resultProvider);
    Task<T> WaitForAsync<T>(Func<Task<T>> resultProvider);
}
```

### 2. Implement Wait Strategy Builder

```csharp
public class WaitStrategyBuilder : IWaitStrategyBuilder
{
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    private readonly List<Func<bool>> _conditions;
    private readonly List<Func<Task<bool>>> _asyncConditions;
    private TimeSpan _timeout = TimeSpan.FromSeconds(30);
    private TimeSpan _retryInterval = TimeSpan.FromMilliseconds(500);
    private Func<int, TimeSpan> _backoffFunction = null;
    private List<Type> _ignoredExceptions = new();
    private string _message = "Wait condition not met";
    private bool _enableLogging = true;
    private bool _enableMetrics = false;
    
    public WaitStrategyBuilder(IUIDriver driver, ILogger logger)
    {
        _driver = driver;
        _logger = logger;
        _conditions = new List<Func<bool>>();
        _asyncConditions = new List<Func<Task<bool>>>();
    }
    
    public IWaitStrategyBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }
    
    public IWaitStrategyBuilder WithRetryInterval(TimeSpan interval)
    {
        _retryInterval = interval;
        return this;
    }
    
    public IWaitStrategyBuilder WithExponentialBackoff()
    {
        _backoffFunction = attempt => TimeSpan.FromMilliseconds(_retryInterval.TotalMilliseconds * Math.Pow(2, attempt - 1));
        return this;
    }
    
    public IWaitStrategyBuilder WithLinearBackoff()
    {
        _backoffFunction = attempt => TimeSpan.FromMilliseconds(_retryInterval.TotalMilliseconds * attempt);
        return this;
    }
    
    public IWaitStrategyBuilder WithCustomBackoff(Func<int, TimeSpan> backoffFunction)
    {
        _backoffFunction = backoffFunction;
        return this;
    }
    
    public IWaitStrategyBuilder IgnoreExceptions(params Type[] exceptionTypes)
    {
        _ignoredExceptions.AddRange(exceptionTypes);
        return this;
    }
    
    public IWaitStrategyBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }
    
    public IWaitStrategyBuilder WithLogging(bool enableLogging)
    {
        _enableLogging = enableLogging;
        return this;
    }
    
    public IWaitStrategyBuilder WithMetrics(bool enableMetrics)
    {
        _enableMetrics = enableMetrics;
        return this;
    }
    
    public IWaitStrategyBuilder And(Func<bool> additionalCondition)
    {
        _conditions.Add(additionalCondition);
        return this;
    }
    
    public IWaitStrategyBuilder Or(Func<bool> alternativeCondition)
    {
        // For OR conditions, we need to modify the wait logic
        // This is a simplified implementation
        _conditions.Add(alternativeCondition);
        return this;
    }
    
    public IWaitStrategyBuilder Not(Func<bool> negatedCondition)
    {
        _conditions.Add(() => !negatedCondition());
        return this;
    }
    
    public void Wait()
    {
        var startTime = DateTime.UtcNow;
        var attempt = 0;
        
        if (_enableLogging)
            _logger.LogInformation($"Starting wait with timeout {_timeout.TotalSeconds}s");
        
        while (DateTime.UtcNow - startTime < _timeout)
        {
            attempt++;
            
            try
            {
                if (_enableLogging)
                    _logger.LogDebug($"Wait attempt {attempt}");
                
                var allConditionsMet = _conditions.All(condition => condition());
                var allAsyncConditionsMet = true;
                
                if (_asyncConditions.Any())
                {
                    var asyncTasks = _asyncConditions.Select(c => c());
                    await Task.WhenAll(asyncTasks);
                    allAsyncConditionsMet = asyncTasks.All(t => t.Result);
                }
                
                if (allConditionsMet && allAsyncConditionsMet)
                {
                    if (_enableLogging)
                        _logger.LogInformation($"Wait condition met after {attempt} attempts");
                    return;
                }
            }
            catch (Exception ex) when (_ignoredExceptions.Any(t => t.IsInstanceOfType(ex)))
            {
                if (_enableLogging)
                    _logger.LogDebug($"Ignored exception during wait: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                    _logger.LogError(ex, $"Unexpected exception during wait attempt {attempt}");
                throw;
            }
            
            var delay = _backoffFunction?.Invoke(attempt) ?? _retryInterval;
            if (DateTime.UtcNow + delay - startTime < _timeout)
            {
                Thread.Sleep(delay);
            }
        }
        
        if (_enableLogging)
            _logger.LogError($"Wait timeout after {attempt} attempts: {_message}");
        
        throw new TimeoutException(_message);
    }
    
    public async Task WaitAsync()
    {
        var startTime = DateTime.UtcNow;
        var attempt = 0;
        
        if (_enableLogging)
            _logger.LogInformation($"Starting async wait with timeout {_timeout.TotalSeconds}s");
        
        while (DateTime.UtcNow - startTime < _timeout)
        {
            attempt++;
            
            try
            {
                if (_enableLogging)
                    _logger.LogDebug($"Async wait attempt {attempt}");
                
                var allConditionsMet = _conditions.All(condition => condition());
                var allAsyncConditionsMet = true;
                
                if (_asyncConditions.Any())
                {
                    var asyncTasks = _asyncConditions.Select(c => c());
                    await Task.WhenAll(asyncTasks);
                    allAsyncConditionsMet = asyncTasks.All(t => t.Result);
                }
                
                if (allConditionsMet && allAsyncConditionsMet)
                {
                    if (_enableLogging)
                        _logger.LogInformation($"Async wait condition met after {attempt} attempts");
                    return;
                }
            }
            catch (Exception ex) when (_ignoredExceptions.Any(t => t.IsInstanceOfType(ex)))
            {
                if (_enableLogging)
                    _logger.LogDebug($"Ignored exception during async wait: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (_enableLogging)
                    _logger.LogError(ex, $"Unexpected exception during async wait attempt {attempt}");
                throw;
            }
            
            var delay = _backoffFunction?.Invoke(attempt) ?? _retryInterval;
            if (DateTime.UtcNow + delay - startTime < _timeout)
            {
                await Task.Delay(delay);
            }
        }
        
        if (_enableLogging)
            _logger.LogError($"Async wait timeout after {attempt} attempts: {_message}");
        
        throw new TimeoutException(_message);
    }
    
    public T WaitFor<T>(Func<T> resultProvider)
    {
        Wait();
        return resultProvider();
    }
    
    public async Task<T> WaitForAsync<T>(Func<Task<T>> resultProvider)
    {
        await WaitAsync();
        return await resultProvider();
    }
}
```

### 3. Implement Advanced Wait Strategy

```csharp
public class AdvancedWaitStrategy : IAdvancedWaitStrategy
{
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    
    public AdvancedWaitStrategy(IUIDriver driver, ILogger logger)
    {
        _driver = driver;
        _logger = logger;
    }
    
    public IWaitStrategyBuilder ForElement(string selector)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => _driver.IsVisible(selector));
        return builder;
    }
    
    public IWaitStrategyBuilder ForCondition(Func<bool> condition)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(condition);
        return builder;
    }
    
    public IWaitStrategyBuilder ForCondition(Func<Task<bool>> condition)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        // This would need to be handled differently in the builder
        return builder;
    }
    
    public IWaitStrategyBuilder ForUrl(string urlPattern)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => Regex.IsMatch(_driver.CurrentUrl, urlPattern));
        return builder;
    }
    
    public IWaitStrategyBuilder ForTitle(string titlePattern)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => Regex.IsMatch(_driver.GetTitle(), titlePattern));
        return builder;
    }
    
    public IWaitStrategyBuilder ForPageLoad()
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => _driver.IsPageLoaded());
        return builder;
    }
    
    public IWaitStrategyBuilder ForNetworkIdle()
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => _driver.IsNetworkIdle());
        return builder;
    }
    
    public IWaitStrategyBuilder ForJavaScriptCondition(string script)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => _driver.ExecuteJavaScript<bool>(script));
        return builder;
    }
    
    public IWaitStrategyBuilder ForCustomCondition<T>(Func<T, bool> condition, T context)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => condition(context));
        return builder;
    }
}
```

### 4. Implement Wait Strategy Factory

```csharp
public interface IWaitStrategyFactory
{
    IAdvancedWaitStrategy CreateAdvancedWaitStrategy();
    IWaitStrategyBuilder CreateBuilder();
    IWaitStrategyBuilder CreateBuilderForElement(string selector);
    IWaitStrategyBuilder CreateBuilderForCondition(Func<bool> condition);
}

public class WaitStrategyFactory : IWaitStrategyFactory
{
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    
    public WaitStrategyFactory(IUIDriver driver, ILogger logger)
    {
        _driver = driver;
        _logger = logger;
    }
    
    public IAdvancedWaitStrategy CreateAdvancedWaitStrategy()
    {
        return new AdvancedWaitStrategy(_driver, _logger);
    }
    
    public IWaitStrategyBuilder CreateBuilder()
    {
        return new WaitStrategyBuilder(_driver, _logger);
    }
    
    public IWaitStrategyBuilder CreateBuilderForElement(string selector)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(() => _driver.IsVisible(selector));
        return builder;
    }
    
    public IWaitStrategyBuilder CreateBuilderForCondition(Func<bool> condition)
    {
        var builder = new WaitStrategyBuilder(_driver, _logger);
        builder.And(condition);
        return builder;
    }
}
```

### 5. Add Performance Monitoring

```csharp
public interface IWaitMetrics
{
    void RecordWait(string condition, TimeSpan duration, int attempts);
    void RecordWaitFailure(string condition, TimeSpan duration, int attempts, Exception exception);
    WaitMetricsSummary GetSummary();
}

public class WaitMetrics : IWaitMetrics
{
    private readonly ConcurrentDictionary<string, List<WaitMetric>> _metrics = new();
    
    public void RecordWait(string condition, TimeSpan duration, int attempts)
    {
        var metric = new WaitMetric(condition, duration, attempts, true, null);
        _metrics.AddOrUpdate(condition, new List<WaitMetric> { metric }, (key, list) =>
        {
            list.Add(metric);
            return list;
        });
    }
    
    public void RecordWaitFailure(string condition, TimeSpan duration, int attempts, Exception exception)
    {
        var metric = new WaitMetric(condition, duration, attempts, false, exception);
        _metrics.AddOrUpdate(condition, new List<WaitMetric> { metric }, (key, list) =>
        {
            list.Add(metric);
            return list;
        });
    }
    
    public WaitMetricsSummary GetSummary()
    {
        var summary = new WaitMetricsSummary();
        
        foreach (var kvp in _metrics)
        {
            var condition = kvp.Key;
            var metrics = kvp.Value;
            
            var successCount = metrics.Count(m => m.Success);
            var failureCount = metrics.Count(m => !m.Success);
            var totalDuration = TimeSpan.FromTicks(metrics.Sum(m => m.Duration.Ticks));
            var averageDuration = totalDuration.TotalMilliseconds / metrics.Count;
            var maxAttempts = metrics.Max(m => m.Attempts);
            
            summary.Conditions[condition] = new ConditionMetrics
            {
                SuccessCount = successCount,
                FailureCount = failureCount,
                TotalDuration = totalDuration,
                AverageDuration = TimeSpan.FromMilliseconds(averageDuration),
                MaxAttempts = maxAttempts
            };
        }
        
        return summary;
    }
}

public class WaitMetric
{
    public string Condition { get; }
    public TimeSpan Duration { get; }
    public int Attempts { get; }
    public bool Success { get; }
    public Exception Exception { get; }
    
    public WaitMetric(string condition, TimeSpan duration, int attempts, bool success, Exception exception)
    {
        Condition = condition;
        Duration = duration;
        Attempts = attempts;
        Success = success;
        Exception = exception;
    }
}

public class WaitMetricsSummary
{
    public Dictionary<string, ConditionMetrics> Conditions { get; } = new();
}

public class ConditionMetrics
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public int MaxAttempts { get; set; }
}
```

### 6. Add Unit Tests

```csharp
[TestFixture]
public class AdvancedWaitStrategyTests
{
    [Test]
    public void ForElement_WithVisibleElement_ReturnsBuilder()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.IsVisible("#test")).Returns(true);
        var logger = Mock.Of<ILogger>();
        var strategy = new AdvancedWaitStrategy(driver, logger);
        
        // Act
        var builder = strategy.ForElement("#test");
        
        // Assert
        Assert.That(builder, Is.Not.Null);
    }
    
    [Test]
    public void Wait_WithValidCondition_CompletesSuccessfully()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var logger = Mock.Of<ILogger>();
        var builder = new WaitStrategyBuilder(driver, logger);
        var conditionMet = false;
        
        builder.ForCondition(() => conditionMet);
        
        // Act
        Task.Run(() => {
            Thread.Sleep(100);
            conditionMet = true;
        });
        
        // Assert
        Assert.DoesNotThrow(() => builder.Wait());
    }
    
    [Test]
    public void Wait_WithTimeout_ThrowsException()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var logger = Mock.Of<ILogger>();
        var builder = new WaitStrategyBuilder(driver, logger);
        
        builder.ForCondition(() => false)
               .WithTimeout(TimeSpan.FromMilliseconds(100));
        
        // Act & Assert
        Assert.Throws<TimeoutException>(() => builder.Wait());
    }
}
```

## Definition of Done

- [ ] Advanced wait strategies are implemented
- [ ] Wait strategies support multiple conditions (AND, OR, NOT)
- [ ] Wait strategies support custom timeout and retry configurations
- [ ] Wait strategies provide detailed logging and debugging information
- [ ] Wait strategies support both synchronous and asynchronous operations
- [ ] Wait strategies handle exceptions gracefully with proper error messages
- [ ] Wait strategies support framework-specific optimizations
- [ ] Wait strategies are configurable and extensible
- [ ] Wait strategies support performance monitoring and metrics
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate real waiting scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the existing wait strategy foundation
- Advanced wait strategies should be performant and reliable
- Consider framework-specific optimizations for different drivers
- Plan for future mobile wait strategy support
- Ensure thread safety for concurrent wait operations 