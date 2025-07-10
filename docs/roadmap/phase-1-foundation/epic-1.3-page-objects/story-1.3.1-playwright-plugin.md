# Story 1.3.1: Playwright Plugin Implementation

## Story Information
- **Epic**: 1.3 Page Object Pattern Implementation
- **Priority**: Critical
- **Estimated Time**: 3-4 weeks
- **Status**: 🔴 Not Started
- **Assigned To**: TBD

## User Story
**As a** test developer  
**I want** a Playwright plugin that implements the framework abstractions  
**So that** I can use Playwright's powerful features through the fluent API

## Acceptance Criteria
- [ ] `PlaywrightPlugin` implements `IUITestingFrameworkPlugin`
- [ ] `PlaywrightDriver` implements `IUIDriver`
- [ ] All core element interactions work correctly
- [ ] Wait strategies are properly implemented
- [ ] Navigation and page management works
- [ ] Framework-specific features are accessible
- [ ] All public APIs have comprehensive unit tests
- [ ] Integration tests demonstrate Playwright features
- [ ] Documentation with examples is complete

## Technical Tasks

### Task 1.3.1.1: Implement PlaywrightPlugin Class
- [ ] Create `PlaywrightPlugin` class
- [ ] Implement plugin metadata (Name, Version)
- [ ] Add supported driver types
- [ ] Implement driver creation method
- [ ] Configure Playwright services

```csharp
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Playwright";
    public string Version => "1.0.0";
    public Type[] SupportedDriverTypes => new[] { typeof(PlaywrightDriver) };
    
    public bool CanHandle(Type driverType) => driverType == typeof(PlaywrightDriver);
    
    public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
    {
        return new PlaywrightDriver(options);
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PlaywrightPlugin>();
        services.AddTransient<PlaywrightDriver>();
        services.AddSingleton<IPlaywright>(provider => Playwright.CreateAsync().Result);
    }
}
```

### Task 1.3.1.2: Implement PlaywrightDriver Class
- [ ] Create `PlaywrightDriver` class implementing `IUIDriver`
- [ ] Implement browser management
- [ ] Add page management
- [ ] Implement element interactions
- [ ] Add navigation methods

```csharp
public class PlaywrightDriver : IUIDriver, IDisposable
{
    private readonly FluentUIScaffoldOptions _options;
    private readonly IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;
    
    public string CurrentUrl => _page?.Url ?? string.Empty;
    
    public PlaywrightDriver(FluentUIScaffoldOptions options)
    {
        _options = options;
        _playwright = Playwright.CreateAsync().Result;
        InitializeBrowser();
    }
    
    // Core interactions
    public void Click(string selector)
    {
        _page.ClickAsync(selector).Wait();
    }
    
    public void Type(string selector, string text)
    {
        _page.FillAsync(selector, text).Wait();
    }
    
    public void Select(string selector, string value)
    {
        _page.SelectOptionAsync(selector, value).Wait();
    }
    
    public string GetText(string selector)
    {
        return _page.TextContentAsync(selector).Result;
    }
    
    public bool IsVisible(string selector)
    {
        return _page.IsVisibleAsync(selector).Result;
    }
    
    public bool IsEnabled(string selector)
    {
        return _page.IsEnabledAsync(selector).Result;
    }
    
    // Navigation
    public void NavigateToUrl(string url)
    {
        _page.GotoAsync(url).Wait();
    }
    
    public TDriver GetFrameworkDriver<TDriver>() where TDriver : class
    {
        if (typeof(TDriver) == typeof(IPage))
            return _page as TDriver;
        if (typeof(TDriver) == typeof(IBrowser))
            return _browser as TDriver;
        if (typeof(TDriver) == typeof(IBrowserContext))
            return _context as TDriver;
        
        throw new InvalidOperationException($"Unsupported framework driver type: {typeof(TDriver).Name}");
    }
    
    public void Dispose()
    {
        _page?.DisposeAsync();
        _context?.DisposeAsync();
        _browser?.DisposeAsync();
        _playwright?.Dispose();
    }
}
```

### Task 1.3.1.3: Implement Wait Strategies
- [ ] Implement smart wait strategy
- [ ] Add custom wait conditions
- [ ] Implement timeout handling
- [ ] Add retry logic
- [ ] Support element state waiting

```csharp
public class PlaywrightWaitStrategy
{
    private readonly IPage _page;
    private readonly FluentUIScaffoldOptions _options;
    
    public PlaywrightWaitStrategy(IPage page, FluentUIScaffoldOptions options)
    {
        _page = page;
        _options = options;
    }
    
    public void WaitForElement(string selector, WaitStrategy strategy, TimeSpan timeout)
    {
        var locator = _page.Locator(selector);
        
        switch (strategy)
        {
            case WaitStrategy.Visible:
                locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = (float)timeout.TotalMilliseconds }).Wait();
                break;
            case WaitStrategy.Hidden:
                locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Hidden, Timeout = (float)timeout.TotalMilliseconds }).Wait();
                break;
            case WaitStrategy.Clickable:
                locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = (float)timeout.TotalMilliseconds }).Wait();
                break;
            case WaitStrategy.Smart:
                WaitForElementSmart(locator, timeout);
                break;
        }
    }
    
    private void WaitForElementSmart(ILocator locator, TimeSpan timeout)
    {
        // Smart wait that tries multiple strategies
        try
        {
            locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = (float)timeout.TotalMilliseconds }).Wait();
        }
        catch
        {
            // Try alternative strategies
            locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached, Timeout = (float)timeout.TotalMilliseconds }).Wait();
        }
    }
}
```

### Task 1.3.1.4: Implement Browser Management
- [ ] Add browser type configuration
- [ ] Implement browser context management
- [ ] Add viewport configuration
- [ ] Support browser profiles
- [ ] Implement browser launch options

```csharp
public class PlaywrightBrowserManager
{
    private readonly FluentUIScaffoldOptions _options;
    private readonly IPlaywright _playwright;
    
    public IBrowser CreateBrowser(BrowserType browserType = BrowserType.Chromium)
    {
        var browserOptions = new BrowserTypeLaunchOptions
        {
            Headless = _options.FrameworkSpecificOptions.GetValueOrDefault("Headless", true),
            SlowMo = _options.FrameworkSpecificOptions.GetValueOrDefault("SlowMo", 0)
        };
        
        return browserType switch
        {
            BrowserType.Chromium => _playwright.Chromium.LaunchAsync(browserOptions).Result,
            BrowserType.Firefox => _playwright.Firefox.LaunchAsync(browserOptions).Result,
            BrowserType.Webkit => _playwright.Webkit.LaunchAsync(browserOptions).Result,
            _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
        };
    }
    
    public IBrowserContext CreateContext(IBrowser browser)
    {
        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = _options.FrameworkSpecificOptions.GetValueOrDefault("ViewportWidth", 1280),
                Height = _options.FrameworkSpecificOptions.GetValueOrDefault("ViewportHeight", 720)
            }
        };
        
        return browser.NewContextAsync(contextOptions).Result;
    }
}
```

### Task 1.3.1.5: Add Playwright-Specific Features
- [ ] Implement network interception
- [ ] Add screenshot capture
- [ ] Support video recording
- [ ] Implement PDF generation
- [ ] Add mobile emulation

```csharp
public class PlaywrightAdvancedFeatures
{
    private readonly IPage _page;
    
    public PlaywrightAdvancedFeatures(IPage page)
    {
        _page = page;
    }
    
    public void InterceptNetworkRequests(string urlPattern, Action<IResponse> handler)
    {
        _page.RouteAsync(urlPattern, async route =>
        {
            var response = await route.FetchAsync();
            handler(response);
            await route.FulfillAsync(new RouteFulfillOptions { Response = response });
        });
    }
    
    public async Task<byte[]> TakeScreenshotAsync(string path = null)
    {
        var screenshotOptions = new PageScreenshotOptions
        {
            Path = path ?? $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png",
            FullPage = true
        };
        
        return await _page.ScreenshotAsync(screenshotOptions);
    }
    
    public async Task<byte[]> GeneratePdfAsync(string path = null)
    {
        var pdfOptions = new PagePdfOptions
        {
            Path = path ?? $"document_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
        };
        
        return await _page.PdfAsync(pdfOptions);
    }
}
```

### Task 1.3.1.6: Implement Error Handling and Debugging
- [ ] Add Playwright-specific exceptions
- [ ] Implement error context capture
- [ ] Add debugging information
- [ ] Support error screenshots
- [ ] Implement error recovery

```csharp
public class PlaywrightException : FluentUIScaffoldException
{
    public PlaywrightException(string message, string selector, Exception innerException) 
        : base(message, innerException)
    {
        Selector = selector;
    }
    
    public string Selector { get; }
}

public class PlaywrightTimeoutException : PlaywrightException
{
    public PlaywrightTimeoutException(string message, string selector, TimeSpan timeout) 
        : base(message, selector, null)
    {
        Timeout = timeout;
    }
    
    public TimeSpan Timeout { get; }
}
```

## Dependencies
- Story 1.1.1 (Project Structure Setup)
- Story 1.1.2 (Core Interfaces & Abstractions)
- Story 1.2.1 (Fluent Entry Point)
- Story 1.2.2 (Element Configuration System)

## Definition of Done
- [ ] PlaywrightPlugin is implemented and tested
- [ ] PlaywrightDriver implements all IUIDriver methods
- [ ] All wait strategies work correctly
- [ ] Browser management is configurable
- [ ] All public APIs have unit tests with >90% coverage
- [ ] Integration tests demonstrate Playwright features
- [ ] Documentation with examples is complete
- [ ] Framework-specific features are accessible

## Notes
- Follow Playwright best practices
- Ensure proper resource disposal
- Consider performance implications
- Plan for async operations
- Make error messages developer-friendly

## Related Documentation
- [Playwright .NET Documentation](https://playwright.dev/dotnet/)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [Playwright API Reference](https://playwright.dev/dotnet/docs/api/class-playwright) 