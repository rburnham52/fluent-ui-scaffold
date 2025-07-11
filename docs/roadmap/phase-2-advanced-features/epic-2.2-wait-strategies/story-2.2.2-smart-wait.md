# Story 2.2.2: Smart Wait Implementation

## Story Information
- **Epic**: Epic 2.2 - Wait Strategies and Smart Waiting
- **Priority**: Medium
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 2.2.1
- **File**: `phase-2-advanced-features/epic-2.2-wait-strategies/story-2.2.2-smart-wait.md`

## User Story

**As a** test developer  
**I want** intelligent wait strategies that automatically adapt to different scenarios  
**So that** I can write more reliable tests without manually configuring wait times

## Acceptance Criteria

- [ ] Smart wait automatically detects page load states
- [ ] Smart wait adapts to network conditions and response times
- [ ] Smart wait learns from previous wait patterns
- [ ] Smart wait provides intelligent defaults for common scenarios
- [ ] Smart wait supports framework-specific optimizations
- [ ] Smart wait includes performance monitoring and analytics
- [ ] Smart wait provides detailed logging and debugging information
- [ ] Smart wait supports both synchronous and asynchronous operations
- [ ] Smart wait handles edge cases and error conditions gracefully
- [ ] Comprehensive unit tests are written and passing
- [ ] Integration tests demonstrate real smart waiting scenarios

## Technical Tasks

### 1. Implement Smart Wait Interface

```csharp
public interface ISmartWait
{
    ISmartWaitBuilder ForElement(string selector);
    ISmartWaitBuilder ForPageLoad();
    ISmartWaitBuilder ForNetworkIdle();
    ISmartWaitBuilder ForJavaScriptReady();
    ISmartWaitBuilder ForCustomCondition(Func<bool> condition);
    ISmartWaitBuilder WithAdaptiveTimeout();
    ISmartWaitBuilder WithLearningEnabled();
    ISmartWaitBuilder WithPerformanceMonitoring();
}

public interface ISmartWaitBuilder
{
    ISmartWaitBuilder WithMinimumTimeout(TimeSpan timeout);
    ISmartWaitBuilder WithMaximumTimeout(TimeSpan timeout);
    ISmartWaitBuilder WithAdaptiveBackoff();
    ISmartWaitBuilder WithContext(string context);
    ISmartWaitBuilder WithPriority(WaitPriority priority);
    void Wait();
    Task WaitAsync();
    T WaitFor<T>(Func<T> resultProvider);
    Task<T> WaitForAsync<T>(Func<Task<T>> resultProvider);
}

public enum WaitPriority
{
    Low,
    Normal,
    High,
    Critical
}
```

### 2. Implement Smart Wait Engine

```csharp
public class SmartWaitEngine
{
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    private readonly IWaitMetrics _metrics;
    private readonly IWaitLearningEngine _learningEngine;
    private readonly Dictionary<string, WaitPattern> _patterns;
    
    public SmartWaitEngine(IUIDriver driver, ILogger logger, IWaitMetrics metrics, IWaitLearningEngine learningEngine)
    {
        _driver = driver;
        _logger = logger;
        _metrics = metrics;
        _learningEngine = learningEngine;
        _patterns = new Dictionary<string, WaitPattern>();
    }
    
    public TimeSpan CalculateOptimalTimeout(string context, WaitPriority priority)
    {
        var baseTimeout = GetBaseTimeout(priority);
        var learnedTimeout = _learningEngine.GetLearnedTimeout(context);
        var networkFactor = GetNetworkLatencyFactor();
        var pageComplexityFactor = GetPageComplexityFactor();
        
        var optimalTimeout = Math.Max(baseTimeout, learnedTimeout) * networkFactor * pageComplexityFactor;
        
        _logger.LogDebug($"Calculated optimal timeout: {optimalTimeout.TotalMilliseconds}ms for context: {context}");
        
        return optimalTimeout;
    }
    
    public TimeSpan CalculateRetryInterval(string context, int attempt)
    {
        var baseInterval = TimeSpan.FromMilliseconds(500);
        var learnedInterval = _learningEngine.GetLearnedInterval(context);
        var backoffFactor = Math.Pow(1.5, attempt - 1);
        
        var optimalInterval = Math.Max(baseInterval, learnedInterval) * backoffFactor;
        
        return optimalInterval;
    }
    
    private TimeSpan GetBaseTimeout(WaitPriority priority)
    {
        return priority switch
        {
            WaitPriority.Low => TimeSpan.FromSeconds(10),
            WaitPriority.Normal => TimeSpan.FromSeconds(30),
            WaitPriority.High => TimeSpan.FromSeconds(60),
            WaitPriority.Critical => TimeSpan.FromSeconds(120),
            _ => TimeSpan.FromSeconds(30)
        };
    }
    
    private double GetNetworkLatencyFactor()
    {
        // This would be implemented based on actual network monitoring
        return 1.0;
    }
    
    private double GetPageComplexityFactor()
    {
        // This would be implemented based on page analysis
        return 1.0;
    }
}
```

### 3. Implement Wait Learning Engine

```csharp
public interface IWaitLearningEngine
{
    TimeSpan GetLearnedTimeout(string context);
    TimeSpan GetLearnedInterval(string context);
    void RecordWaitPattern(string context, TimeSpan duration, int attempts, bool success);
    void UpdatePatterns();
    Dictionary<string, WaitPattern> GetPatterns();
}

public class WaitLearningEngine : IWaitLearningEngine
{
    private readonly Dictionary<string, List<WaitRecord>> _records;
    private readonly Dictionary<string, WaitPattern> _patterns;
    private readonly ILogger _logger;
    
    public WaitLearningEngine(ILogger logger)
    {
        _records = new Dictionary<string, List<WaitRecord>>();
        _patterns = new Dictionary<string, WaitPattern>();
        _logger = logger;
    }
    
    public TimeSpan GetLearnedTimeout(string context)
    {
        if (_patterns.TryGetValue(context, out var pattern))
        {
            return pattern.AverageTimeout;
        }
        
        return TimeSpan.FromSeconds(30); // Default
    }
    
    public TimeSpan GetLearnedInterval(string context)
    {
        if (_patterns.TryGetValue(context, out var pattern))
        {
            return pattern.AverageInterval;
        }
        
        return TimeSpan.FromMilliseconds(500); // Default
    }
    
    public void RecordWaitPattern(string context, TimeSpan duration, int attempts, bool success)
    {
        if (!_records.ContainsKey(context))
        {
            _records[context] = new List<WaitRecord>();
        }
        
        _records[context].Add(new WaitRecord(duration, attempts, success));
        
        if (_records[context].Count >= 10) // Update patterns after 10 records
        {
            UpdatePatterns();
        }
    }
    
    public void UpdatePatterns()
    {
        foreach (var kvp in _records)
        {
            var context = kvp.Key;
            var records = kvp.Value;
            
            if (records.Count < 5) continue; // Need at least 5 records
            
            var successfulRecords = records.Where(r => r.Success).ToList();
            
            if (successfulRecords.Any())
            {
                var averageTimeout = TimeSpan.FromTicks((long)successfulRecords.Average(r => r.Duration.Ticks));
                var averageAttempts = (int)successfulRecords.Average(r => r.Attempts);
                var averageInterval = averageTimeout / averageAttempts;
                
                _patterns[context] = new WaitPattern
                {
                    Context = context,
                    AverageTimeout = averageTimeout,
                    AverageInterval = averageInterval,
                    SuccessRate = (double)successfulRecords.Count / records.Count,
                    SampleCount = records.Count
                };
                
                _logger.LogInformation($"Updated pattern for {context}: {averageTimeout.TotalMilliseconds}ms timeout, {averageInterval.TotalMilliseconds}ms interval, {_patterns[context].SuccessRate:P} success rate");
            }
        }
    }
    
    public Dictionary<string, WaitPattern> GetPatterns()
    {
        return new Dictionary<string, WaitPattern>(_patterns);
    }
}

public class WaitRecord
{
    public TimeSpan Duration { get; }
    public int Attempts { get; }
    public bool Success { get; }
    public DateTime Timestamp { get; }
    
    public WaitRecord(TimeSpan duration, int attempts, bool success)
    {
        Duration = duration;
        Attempts = attempts;
        Success = success;
        Timestamp = DateTime.UtcNow;
    }
}

public class WaitPattern
{
    public string Context { get; set; }
    public TimeSpan AverageTimeout { get; set; }
    public TimeSpan AverageInterval { get; set; }
    public double SuccessRate { get; set; }
    public int SampleCount { get; set; }
}
```

### 4. Implement Smart Wait Builder

```csharp
public class SmartWaitBuilder : ISmartWaitBuilder
{
    private readonly SmartWaitEngine _engine;
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    private readonly List<Func<bool>> _conditions;
    private TimeSpan _minimumTimeout = TimeSpan.FromSeconds(5);
    private TimeSpan _maximumTimeout = TimeSpan.FromSeconds(120);
    private bool _adaptiveBackoff = true;
    private string _context = "default";
    private WaitPriority _priority = WaitPriority.Normal;
    
    public SmartWaitBuilder(SmartWaitEngine engine, IUIDriver driver, ILogger logger)
    {
        _engine = engine;
        _driver = driver;
        _logger = logger;
        _conditions = new List<Func<bool>>();
    }
    
    public ISmartWaitBuilder WithMinimumTimeout(TimeSpan timeout)
    {
        _minimumTimeout = timeout;
        return this;
    }
    
    public ISmartWaitBuilder WithMaximumTimeout(TimeSpan timeout)
    {
        _maximumTimeout = timeout;
        return this;
    }
    
    public ISmartWaitBuilder WithAdaptiveBackoff()
    {
        _adaptiveBackoff = true;
        return this;
    }
    
    public ISmartWaitBuilder WithContext(string context)
    {
        _context = context;
        return this;
    }
    
    public ISmartWaitBuilder WithPriority(WaitPriority priority)
    {
        _priority = priority;
        return this;
    }
    
    public void Wait()
    {
        var timeout = _engine.CalculateOptimalTimeout(_context, _priority);
        timeout = TimeSpan.FromMilliseconds(Math.Max(_minimumTimeout.TotalMilliseconds, 
                                                   Math.Min(timeout.TotalMilliseconds, _maximumTimeout.TotalMilliseconds)));
        
        var startTime = DateTime.UtcNow;
        var attempt = 0;
        var success = false;
        
        _logger.LogInformation($"Starting smart wait with timeout {timeout.TotalSeconds}s for context: {_context}");
        
        while (DateTime.UtcNow - startTime < timeout)
        {
            attempt++;
            
            try
            {
                var allConditionsMet = _conditions.All(condition => condition());
                
                if (allConditionsMet)
                {
                    success = true;
                    _logger.LogInformation($"Smart wait condition met after {attempt} attempts");
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Exception during smart wait attempt {attempt}: {ex.Message}");
            }
            
            var interval = _engine.CalculateRetryInterval(_context, attempt);
            if (DateTime.UtcNow + interval - startTime < timeout)
            {
                Thread.Sleep(interval);
            }
        }
        
        var duration = DateTime.UtcNow - startTime;
        _engine._learningEngine.RecordWaitPattern(_context, duration, attempt, success);
        
        if (!success)
        {
            _logger.LogError($"Smart wait timeout after {attempt} attempts for context: {_context}");
            throw new TimeoutException($"Smart wait timeout for context: {_context}");
        }
    }
    
    public async Task WaitAsync()
    {
        var timeout = _engine.CalculateOptimalTimeout(_context, _priority);
        timeout = TimeSpan.FromMilliseconds(Math.Max(_minimumTimeout.TotalMilliseconds, 
                                                   Math.Min(timeout.TotalMilliseconds, _maximumTimeout.TotalMilliseconds)));
        
        var startTime = DateTime.UtcNow;
        var attempt = 0;
        var success = false;
        
        _logger.LogInformation($"Starting async smart wait with timeout {timeout.TotalSeconds}s for context: {_context}");
        
        while (DateTime.UtcNow - startTime < timeout)
        {
            attempt++;
            
            try
            {
                var allConditionsMet = _conditions.All(condition => condition());
                
                if (allConditionsMet)
                {
                    success = true;
                    _logger.LogInformation($"Async smart wait condition met after {attempt} attempts");
                    break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Exception during async smart wait attempt {attempt}: {ex.Message}");
            }
            
            var interval = _engine.CalculateRetryInterval(_context, attempt);
            if (DateTime.UtcNow + interval - startTime < timeout)
            {
                await Task.Delay(interval);
            }
        }
        
        var duration = DateTime.UtcNow - startTime;
        _engine._learningEngine.RecordWaitPattern(_context, duration, attempt, success);
        
        if (!success)
        {
            _logger.LogError($"Async smart wait timeout after {attempt} attempts for context: {_context}");
            throw new TimeoutException($"Async smart wait timeout for context: {_context}");
        }
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

### 5. Add Unit Tests

```csharp
[TestFixture]
public class SmartWaitTests
{
    [Test]
    public void CalculateOptimalTimeout_WithLearnedPattern_ReturnsLearnedTimeout()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var logger = Mock.Of<ILogger>();
        var metrics = Mock.Of<IWaitMetrics>();
        var learningEngine = Mock.Of<IWaitLearningEngine>();
        Mock.Get(learningEngine).Setup(l => l.GetLearnedTimeout("test-context")).Returns(TimeSpan.FromSeconds(45));
        
        var engine = new SmartWaitEngine(driver, logger, metrics, learningEngine);
        
        // Act
        var timeout = engine.CalculateOptimalTimeout("test-context", WaitPriority.Normal);
        
        // Assert
        Assert.That(timeout.TotalSeconds, Is.GreaterThan(30));
    }
    
    [Test]
    public void Wait_WithValidCondition_CompletesSuccessfully()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var logger = Mock.Of<ILogger>();
        var metrics = Mock.Of<IWaitMetrics>();
        var learningEngine = Mock.Of<IWaitLearningEngine>();
        var engine = new SmartWaitEngine(driver, logger, metrics, learningEngine);
        var builder = new SmartWaitBuilder(engine, driver, logger);
        
        var conditionMet = false;
        builder.WithContext("test-context");
        
        // Act
        Task.Run(() => {
            Thread.Sleep(100);
            conditionMet = true;
        });
        
        // Assert
        Assert.DoesNotThrow(() => builder.Wait());
    }
}
```

## Definition of Done

- [ ] Smart wait automatically detects page load states
- [ ] Smart wait adapts to network conditions and response times
- [ ] Smart wait learns from previous wait patterns
- [ ] Smart wait provides intelligent defaults for common scenarios
- [ ] Smart wait supports framework-specific optimizations
- [ ] Smart wait includes performance monitoring and analytics
- [ ] Smart wait provides detailed logging and debugging information
- [ ] Smart wait supports both synchronous and asynchronous operations
- [ ] Smart wait handles edge cases and error conditions gracefully
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate real smart waiting scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the advanced wait strategy implementation
- Smart wait should be intelligent but not overly complex
- Consider performance implications of learning algorithms
- Plan for future mobile smart wait support
- Ensure thread safety for concurrent smart wait operations 