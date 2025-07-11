# Story 1.3.3: Page Navigation and Validation

## Story Information
- **Epic**: Epic 1.3 - Page Object Pattern Implementation
- **Priority**: High
- **Estimated Time**: 2-3 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD
- **Dependencies**: Story 1.3.1, Story 1.3.2
- **File**: `phase-1-foundation/epic-1.3-page-objects/story-1.3.3-page-navigation.md`

## User Story

**As a** test developer  
**I want** robust page navigation and validation capabilities  
**So that** I can reliably navigate between pages and verify I'm on the correct page

## Acceptance Criteria

- [ ] Page navigation system supports URL-based navigation
- [ ] Page navigation system supports action-based navigation (clicks, form submissions)
- [ ] URL pattern matching supports various patterns (exact, regex, parameterized)
- [ ] Page validation includes URL validation and element presence checks
- [ ] Navigation supports both synchronous and asynchronous operations
- [ ] Navigation includes proper error handling and retry logic
- [ ] Navigation provides detailed logging for debugging
- [ ] Navigation supports custom validation strategies
- [ ] Navigation handles redirects and dynamic URL changes
- [ ] Navigation supports browser back/forward navigation
- [ ] Comprehensive unit tests are written and passing
- [ ] Integration tests demonstrate real navigation scenarios

## Technical Tasks

### 1. Implement URL Pattern Matching System

```csharp
public interface IUrlPatternMatcher
{
    bool Matches(string url, string pattern);
    Dictionary<string, string> ExtractParameters(string url, string pattern);
}

public class UrlPatternMatcher : IUrlPatternMatcher
{
    public bool Matches(string url, string pattern)
    {
        if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(pattern))
            return false;
            
        // Support exact matching
        if (!pattern.Contains("{") && !pattern.Contains("*"))
            return url.Equals(pattern, StringComparison.OrdinalIgnoreCase);
            
        // Support parameterized patterns like "/user/{id:int}/profile"
        var regexPattern = ConvertToRegex(pattern);
        return Regex.IsMatch(url, regexPattern, RegexOptions.IgnoreCase);
    }
    
    public Dictionary<string, string> ExtractParameters(string url, string pattern)
    {
        var parameters = new Dictionary<string, string>();
        var regexPattern = ConvertToRegex(pattern);
        var match = Regex.Match(url, regexPattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            var parameterNames = ExtractParameterNames(pattern);
            for (int i = 0; i < parameterNames.Count; i++)
            {
                if (i + 1 < match.Groups.Count)
                {
                    parameters[parameterNames[i]] = match.Groups[i + 1].Value;
                }
            }
        }
        
        return parameters;
    }
    
    private string ConvertToRegex(string pattern)
    {
        // Convert {param:type} to regex groups
        var regex = pattern
            .Replace("{", "(?<")
            .Replace("}", ">[^/]+)")
            .Replace(":int", "\\d+")
            .Replace(":string", "[^/]+")
            .Replace(":guid", "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
            
        return $"^{regex}$";
    }
    
    private List<string> ExtractParameterNames(string pattern)
    {
        var matches = Regex.Matches(pattern, @"\{([^:}]+)(?::[^}]+)?\}");
        return matches.Cast<Match>().Select(m => m.Groups[1].Value).ToList();
    }
}
```

### 2. Implement Page Navigation Service

```csharp
public interface IPageNavigationService
{
    TTarget NavigateTo<TTarget>(string url) where TTarget : BasePageComponent;
    TTarget NavigateTo<TTarget>(Action navigationAction) where TTarget : BasePageComponent;
    Task<TTarget> NavigateToAsync<TTarget>(string url) where TTarget : BasePageComponent;
    Task<TTarget> NavigateToAsync<TTarget>(Func<Task> navigationAction) where TTarget : BasePageComponent;
    bool IsCurrentPage<TTarget>() where TTarget : BasePageComponent;
    void ValidateCurrentPage<TTarget>() where TTarget : BasePageComponent;
}

public class PageNavigationService : IPageNavigationService
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly ILogger _logger;
    private readonly IUrlPatternMatcher _urlMatcher;
    private readonly IPageComponentFactory _pageFactory;
    
    public PageNavigationService(
        IUIDriver driver, 
        FluentUIScaffoldOptions options, 
        ILogger logger,
        IUrlPatternMatcher urlMatcher,
        IPageComponentFactory pageFactory)
    {
        _driver = driver;
        _options = options;
        _logger = logger;
        _urlMatcher = urlMatcher;
        _pageFactory = pageFactory;
    }
    
    public TTarget NavigateTo<TTarget>(string url) where TTarget : BasePageComponent
    {
        _logger.LogInformation($"Navigating to {url}");
        
        try
        {
            _driver.NavigateToUrl(url);
            WaitForPageLoad();
            
            var page = _pageFactory.CreatePageComponent<TTarget>();
            page.ValidateCurrentPage();
            
            _logger.LogInformation($"Successfully navigated to {typeof(TTarget).Name}");
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to navigate to {url}");
            throw new NavigationException($"Failed to navigate to {url}", ex);
        }
    }
    
    public TTarget NavigateTo<TTarget>(Action navigationAction) where TTarget : BasePageComponent
    {
        _logger.LogInformation($"Performing navigation action to {typeof(TTarget).Name}");
        
        try
        {
            navigationAction();
            WaitForPageLoad();
            
            var page = _pageFactory.CreatePageComponent<TTarget>();
            page.ValidateCurrentPage();
            
            _logger.LogInformation($"Successfully navigated to {typeof(TTarget).Name}");
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to navigate to {typeof(TTarget).Name}");
            throw new NavigationException($"Failed to navigate to {typeof(TTarget).Name}", ex);
        }
    }
    
    public async Task<TTarget> NavigateToAsync<TTarget>(string url) where TTarget : BasePageComponent
    {
        _logger.LogInformation($"Navigating to {url} (async)");
        
        try
        {
            await _driver.NavigateToUrlAsync(url);
            await WaitForPageLoadAsync();
            
            var page = await _pageFactory.CreatePageComponentAsync<TTarget>();
            page.ValidateCurrentPage();
            
            _logger.LogInformation($"Successfully navigated to {typeof(TTarget).Name}");
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to navigate to {url}");
            throw new NavigationException($"Failed to navigate to {url}", ex);
        }
    }
    
    public async Task<TTarget> NavigateToAsync<TTarget>(Func<Task> navigationAction) where TTarget : BasePageComponent
    {
        _logger.LogInformation($"Performing navigation action to {typeof(TTarget).Name} (async)");
        
        try
        {
            await navigationAction();
            await WaitForPageLoadAsync();
            
            var page = await _pageFactory.CreatePageComponentAsync<TTarget>();
            page.ValidateCurrentPage();
            
            _logger.LogInformation($"Successfully navigated to {typeof(TTarget).Name}");
            return page;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to navigate to {typeof(TTarget).Name}");
            throw new NavigationException($"Failed to navigate to {typeof(TTarget).Name}", ex);
        }
    }
    
    public bool IsCurrentPage<TTarget>() where TTarget : BasePageComponent
    {
        var page = _pageFactory.CreatePageComponent<TTarget>();
        return page.IsCurrentPage();
    }
    
    public void ValidateCurrentPage<TTarget>() where TTarget : BasePageComponent
    {
        var page = _pageFactory.CreatePageComponent<TTarget>();
        page.ValidateCurrentPage();
    }
    
    private void WaitForPageLoad()
    {
        // Wait for page to be ready
        _driver.WaitForElement("body");
        
        // Wait for any loading indicators to disappear
        var loadingSelectors = _options.PageValidationStrategy.LoadingSelectors;
        foreach (var selector in loadingSelectors)
        {
            try
            {
                _driver.WaitForElementToBeHidden(selector, TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                // Loading indicator didn't disappear, but continue
                _logger.LogWarning($"Loading indicator {selector} did not disappear within timeout");
            }
        }
    }
    
    private async Task WaitForPageLoadAsync()
    {
        // Wait for page to be ready
        await _driver.WaitForElementAsync("body");
        
        // Wait for any loading indicators to disappear
        var loadingSelectors = _options.PageValidationStrategy.LoadingSelectors;
        foreach (var selector in loadingSelectors)
        {
            try
            {
                await _driver.WaitForElementToBeHiddenAsync(selector, TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                // Loading indicator didn't disappear, but continue
                _logger.LogWarning($"Loading indicator {selector} did not disappear within timeout");
            }
        }
    }
}
```

### 3. Implement Page Component Factory

```csharp
public interface IPageComponentFactory
{
    TTarget CreatePageComponent<TTarget>() where TTarget : BasePageComponent;
    Task<TTarget> CreatePageComponentAsync<TTarget>() where TTarget : BasePageComponent;
}

public class PageComponentFactory : IPageComponentFactory
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly ILogger _logger;
    private readonly IElementFactory _elementFactory;
    private readonly IServiceProvider _serviceProvider;
    
    public PageComponentFactory(
        IUIDriver driver,
        FluentUIScaffoldOptions options,
        ILogger logger,
        IElementFactory elementFactory,
        IServiceProvider serviceProvider)
    {
        _driver = driver;
        _options = options;
        _logger = logger;
        _elementFactory = elementFactory;
        _serviceProvider = serviceProvider;
    }
    
    public TTarget CreatePageComponent<TTarget>() where TTarget : BasePageComponent
    {
        try
        {
            // Try to create using dependency injection first
            var page = _serviceProvider.GetService<TTarget>();
            if (page != null)
                return page;
                
            // Fallback to reflection-based creation
            var constructor = typeof(TTarget).GetConstructor(new[] 
            { 
                typeof(IUIDriver), 
                typeof(FluentUIScaffoldOptions), 
                typeof(ILogger),
                typeof(IElementFactory)
            });
            
            if (constructor != null)
            {
                return (TTarget)constructor.Invoke(new object[] 
                { 
                    _driver, 
                    _options, 
                    _logger,
                    _elementFactory
                });
            }
            
            throw new InvalidOperationException($"Cannot create page component {typeof(TTarget).Name}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create page component {typeof(TTarget).Name}");
            throw new PageComponentCreationException($"Failed to create page component {typeof(TTarget).Name}", ex);
        }
    }
    
    public async Task<TTarget> CreatePageComponentAsync<TTarget>() where TTarget : BasePageComponent
    {
        return await Task.Run(() => CreatePageComponent<TTarget>());
    }
}
```

### 4. Implement Page Validation Strategy

```csharp
public class PageValidationStrategy
{
    public bool ValidateUrl { get; set; } = true;
    public bool ValidateElements { get; set; } = true;
    public string[] RequiredElements { get; set; } = Array.Empty<string>();
    public string[] LoadingSelectors { get; set; } = Array.Empty<string>();
    public TimeSpan ValidationTimeout { get; set; } = TimeSpan.FromSeconds(10);
    public bool RetryOnFailure { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
}

public class PageValidator
{
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    private readonly IUrlPatternMatcher _urlMatcher;
    
    public PageValidator(IUIDriver driver, ILogger logger, IUrlPatternMatcher urlMatcher)
    {
        _driver = driver;
        _logger = logger;
        _urlMatcher = urlMatcher;
    }
    
    public void ValidatePage(BasePageComponent page, PageValidationStrategy strategy)
    {
        _logger.LogInformation($"Validating page {page.GetType().Name}");
        
        if (strategy.ValidateUrl)
        {
            ValidateUrl(page, strategy);
        }
        
        if (strategy.ValidateElements)
        {
            ValidateElements(page, strategy);
        }
    }
    
    private void ValidateUrl(BasePageComponent page, PageValidationStrategy strategy)
    {
        var currentUrl = _driver.CurrentUrl;
        var urlPattern = page.UrlPattern;
        
        if (!_urlMatcher.Matches(currentUrl, urlPattern))
        {
            throw new InvalidPageException(
                $"Expected URL to match pattern '{urlPattern}', but current URL is '{currentUrl}'");
        }
        
        _logger.LogDebug($"URL validation passed: {currentUrl} matches {urlPattern}");
    }
    
    private void ValidateElements(BasePageComponent page, PageValidationStrategy strategy)
    {
        foreach (var elementSelector in strategy.RequiredElements)
        {
            try
            {
                _driver.WaitForElement(elementSelector, strategy.ValidationTimeout);
                _logger.LogDebug($"Element validation passed: {elementSelector}");
            }
            catch (TimeoutException ex)
            {
                throw new PageValidationException(
                    $"Required element '{elementSelector}' not found on page {page.GetType().Name}", ex);
            }
        }
    }
}
```

### 5. Add Browser Navigation Support

```csharp
public interface IBrowserNavigation
{
    void GoBack();
    void GoForward();
    void Refresh();
    Task GoBackAsync();
    Task GoForwardAsync();
    Task RefreshAsync();
}

public class BrowserNavigation : IBrowserNavigation
{
    private readonly IUIDriver _driver;
    private readonly ILogger _logger;
    
    public BrowserNavigation(IUIDriver driver, ILogger logger)
    {
        _driver = driver;
        _logger = logger;
    }
    
    public void GoBack()
    {
        _logger.LogInformation("Navigating back");
        _driver.GoBack();
        WaitForPageLoad();
    }
    
    public void GoForward()
    {
        _logger.LogInformation("Navigating forward");
        _driver.GoForward();
        WaitForPageLoad();
    }
    
    public void Refresh()
    {
        _logger.LogInformation("Refreshing page");
        _driver.Refresh();
        WaitForPageLoad();
    }
    
    public async Task GoBackAsync()
    {
        _logger.LogInformation("Navigating back (async)");
        await _driver.GoBackAsync();
        await WaitForPageLoadAsync();
    }
    
    public async Task GoForwardAsync()
    {
        _logger.LogInformation("Navigating forward (async)");
        await _driver.GoForwardAsync();
        await WaitForPageLoadAsync();
    }
    
    public async Task RefreshAsync()
    {
        _logger.LogInformation("Refreshing page (async)");
        await _driver.RefreshAsync();
        await WaitForPageLoadAsync();
    }
    
    private void WaitForPageLoad()
    {
        _driver.WaitForElement("body");
    }
    
    private async Task WaitForPageLoadAsync()
    {
        await _driver.WaitForElementAsync("body");
    }
}
```

### 6. Add Unit Tests

```csharp
[TestFixture]
public class PageNavigationServiceTests
{
    [Test]
    public void NavigateTo_WithValidUrl_ReturnsPageComponent()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var urlMatcher = Mock.Of<IUrlPatternMatcher>();
        var pageFactory = Mock.Of<IPageComponentFactory>();
        var service = new PageNavigationService(driver, options, logger, urlMatcher, pageFactory);
        
        var testPage = new TestPageComponent(driver, options, logger);
        Mock.Get(pageFactory).Setup(f => f.CreatePageComponent<TestPageComponent>()).Returns(testPage);
        
        // Act
        var result = service.NavigateTo<TestPageComponent>("/test/page");
        
        // Assert
        Assert.That(result, Is.Not.Null);
        Mock.Get(driver).Verify(d => d.NavigateToUrl("/test/page"), Times.Once);
    }
    
    [Test]
    public void NavigateTo_WithInvalidUrl_ThrowsException()
    {
        // Arrange
        var driver = Mock.Of<IUIDriver>();
        Mock.Get(driver).Setup(d => d.NavigateToUrl(It.IsAny<string>())).Throws(new Exception("Navigation failed"));
        var options = new FluentUIScaffoldOptions();
        var logger = Mock.Of<ILogger>();
        var urlMatcher = Mock.Of<IUrlPatternMatcher>();
        var pageFactory = Mock.Of<IPageComponentFactory>();
        var service = new PageNavigationService(driver, options, logger, urlMatcher, pageFactory);
        
        // Act & Assert
        Assert.Throws<NavigationException>(() => service.NavigateTo<TestPageComponent>("/invalid/page"));
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

### 7. Add Integration Tests

```csharp
[TestFixture]
public class PageNavigationIntegrationTests
{
    [Test]
    public async Task NavigateTo_WithRealBrowser_NavigatesSuccessfully()
    {
        // Arrange
        var options = new FluentUIScaffoldOptions 
        { 
            BaseUrl = "https://localhost:5001",
            DefaultTimeout = TimeSpan.FromSeconds(30)
        };
        var scaffold = FluentUIScaffold<WebApp>(options);
        
        // Act
        var loginPage = await scaffold.NavigateToAsync<LoginPage>();
        var homePage = await loginPage.LoginAsync("testuser", "password");
        
        // Assert
        Assert.That(homePage, Is.Not.Null);
        Assert.That(homePage.IsCurrentPage(), Is.True);
    }
}
```

## Definition of Done

- [ ] URL pattern matching system is implemented
- [ ] Page navigation service is implemented
- [ ] Page component factory is implemented
- [ ] Page validation strategy is implemented
- [ ] Browser navigation support is implemented
- [ ] Both synchronous and asynchronous navigation is supported
- [ ] Error handling and retry logic is comprehensive
- [ ] Logging is detailed and useful for debugging
- [ ] Custom validation strategies are supported
- [ ] Redirect handling is implemented
- [ ] Browser back/forward navigation is supported
- [ ] Unit tests are written and passing (>90% coverage)
- [ ] Integration tests demonstrate real scenarios
- [ ] Documentation is updated with usage examples
- [ ] Code follows .NET coding conventions
- [ ] No breaking changes introduced
- [ ] Performance is acceptable
- [ ] Story status is updated to "Completed"

## Notes

- This story builds on the base page component implementation
- Navigation should be robust and handle various edge cases
- Consider performance implications of page validation
- Plan for future mobile navigation support
- Ensure thread safety for concurrent navigation 