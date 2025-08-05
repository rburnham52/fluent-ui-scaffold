# Playwright Integration

## Overview

FluentUIScaffold provides comprehensive integration with Microsoft Playwright, offering access to all Playwright features while maintaining the framework's fluent API. This integration allows you to leverage Playwright's powerful capabilities while using FluentUIScaffold's abstraction layer.

## Core Components

### PlaywrightPlugin

The main plugin that integrates Playwright with FluentUIScaffold:

```csharp
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Playwright";
    public string Version => "1.0.0";
    public Type[] SupportedDriverTypes => new[] { typeof(PlaywrightDriver) };
    
    public bool CanHandle(Type driverType) => driverType == typeof(PlaywrightDriver);
    public IUIDriver CreateDriver(FluentUIScaffoldOptions options) => new PlaywrightDriver(options);
    public void ConfigureServices(IServiceCollection services) { /* ... */ }
}
```

### PlaywrightDriver

The main driver that implements the `IUIDriver` interface using Playwright:

```csharp
public class PlaywrightDriver : IUIDriver
{
    public string CurrentUrl { get; }
    
    // Core interactions
    public void Click(string selector);
    public void Type(string selector, string text);
    public void Select(string selector, string value);
    public string GetText(string selector);
    public bool IsVisible(string selector);
    public bool IsEnabled(string selector);
    public void WaitForElement(string selector);
    public void WaitForElementToBeVisible(string selector);
    public void WaitForElementToBeHidden(string selector);
    
    // Navigation
    public void NavigateToUrl(string url);
    public TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent;
    
    // Framework-specific access
    public TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
    
    // Lifecycle
    public void Dispose();
}
```

## Basic Usage

### Setting Up Playwright

```csharp
// Configure FluentUIScaffold with Playwright
var fluentUI = FluentUIScaffoldBuilder.Web(options =>
{
    options.BaseUrl = new Uri("https://your-app.com");
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    
    // Playwright-specific options
    options.FrameworkSpecificOptions["Headless"] = false;
    options.FrameworkSpecificOptions["SlowMo"] = 1000;
    options.FrameworkSpecificOptions["ViewportWidth"] = 1280;
    options.FrameworkSpecificOptions["ViewportHeight"] = 720;
});

// Get Playwright driver
var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
```

### Basic Interactions

```csharp
[TestMethod]
public async Task Can_Interact_With_Playwright()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var page = fluentUI.NavigateTo<HomePage>();
    
    // Act
    page.EnterEmail("test@example.com")
        .EnterPassword("password")
        .ClickLogin();
    
    // Assert
    page.Verify.ElementIsVisible("#welcome-message");
}
```

## Advanced Features

### PlaywrightAdvancedFeatures

Access to advanced Playwright capabilities:

```csharp
public class PlaywrightAdvancedFeatures
{
    public void InterceptNetworkRequests(string urlPattern, Action<IResponse> handler);
    public async Task<byte[]> TakeScreenshotAsync(string path = null);
    public async Task<byte[]> GeneratePdfAsync(string path = null);
    public void SetViewportSize(int width, int height);
    public void SetUserAgent(string userAgent);
    public void SetGeolocation(double latitude, double longitude);
    public void SetPermissions(string[] permissions);
}
```

### Network Interception

```csharp
[TestMethod]
public async Task Can_Intercept_Network_Requests()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
    
    // Act
    playwrightDriver.InterceptNetworkRequests("/api/*", response =>
    {
        // Modify response
        response.SetBody("{\"status\": \"success\"}");
    });
    
    var page = fluentUI.NavigateTo<ApiTestPage>();
    page.TriggerApiCall();
    
    // Assert
    page.Verify.ElementContainsText("#result", "success");
}
```

### Screenshots and PDFs

```csharp
[TestMethod]
public async Task Can_Take_Screenshots_And_Generate_PDFs()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
    var page = fluentUI.NavigateTo<HomePage>();
    
    // Act
    // Take screenshot
    var screenshotBytes = await playwrightDriver.TakeScreenshotAsync("home-page.png");
    
    // Generate PDF
    var pdfBytes = await playwrightDriver.GeneratePdfAsync("home-page.pdf");
    
    // Assert
    Assert.IsNotNull(screenshotBytes);
    Assert.IsNotNull(pdfBytes);
}
```

### Browser Configuration

```csharp
[TestMethod]
public async Task Can_Configure_Browser_Settings()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Set viewport size
        options.FrameworkSpecificOptions["ViewportWidth"] = 1920;
        options.FrameworkSpecificOptions["ViewportHeight"] = 1080;
        
        // Set user agent
        options.FrameworkSpecificOptions["UserAgent"] = "Custom User Agent";
        
        // Set geolocation
        options.FrameworkSpecificOptions["Geolocation"] = new { Latitude = 40.7128, Longitude = -74.0060 };
        
        // Set permissions
        options.FrameworkSpecificOptions["Permissions"] = new[] { "geolocation", "notifications" };
    });
    
    var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
    
    // Act
    var page = fluentUI.NavigateTo<GeolocationPage>();
    
    // Assert
    page.Verify.ElementIsVisible("#location-info");
}
```

## Wait Strategies

### PlaywrightWaitStrategy

Playwright-specific wait strategy implementation:

```csharp
public class PlaywrightWaitStrategy
{
    public void WaitForElement(string selector, WaitStrategy strategy, TimeSpan timeout);
    public void WaitForElementToBeVisible(string selector, TimeSpan timeout);
    public void WaitForElementToBeHidden(string selector, TimeSpan timeout);
    public void WaitForElementToBeClickable(string selector, TimeSpan timeout);
    public void WaitForTextToBePresent(string selector, string text, TimeSpan timeout);
}
```

### Using Wait Strategies

```csharp
[TestMethod]
public async Task Can_Use_Playwright_Wait_Strategies()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var page = fluentUI.NavigateTo<DynamicPage>();
    
    // Act
    // Wait for element to be visible
    page.WaitForElement("#dynamic-content", WaitStrategy.Visible);
    
    // Wait for element to be clickable
    page.WaitForElement("#button", WaitStrategy.Clickable);
    
    // Wait for text to be present
    page.WaitForElement("#message", WaitStrategy.TextPresent);
    
    // Assert
    page.Verify.ElementIsVisible("#dynamic-content");
}
```

## Browser Management

### Browser Types

```csharp
[TestMethod]
public async Task Can_Use_Different_Browsers()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Use Chromium
        options.FrameworkSpecificOptions["BrowserType"] = "Chromium";
        
        // Or use Firefox
        // options.FrameworkSpecificOptions["BrowserType"] = "Firefox";
        
        // Or use WebKit
        // options.FrameworkSpecificOptions["BrowserType"] = "WebKit";
    });
    
    // Act
    var page = fluentUI.NavigateTo<HomePage>();
    
    // Assert
    page.Verify.ElementIsVisible("#content");
}
```

### Browser Context

```csharp
[TestMethod]
public async Task Can_Manage_Browser_Context()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Set viewport size
        options.FrameworkSpecificOptions["ViewportWidth"] = 1280;
        options.FrameworkSpecificOptions["ViewportHeight"] = 720;
        
        // Set user agent
        options.FrameworkSpecificOptions["UserAgent"] = "Custom User Agent";
        
        // Set locale
        options.FrameworkSpecificOptions["Locale"] = "en-US";
        
        // Set timezone
        options.FrameworkSpecificOptions["Timezone"] = "America/New_York";
    });
    
    // Act
    var page = fluentUI.NavigateTo<LocalizedPage>();
    
    // Assert
    page.Verify.ElementContainsText("#locale", "en-US");
}
```

## Mobile Emulation

### Mobile Device Emulation

```csharp
[TestMethod]
public async Task Can_Emulate_Mobile_Devices()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Emulate iPhone
        options.FrameworkSpecificOptions["Device"] = "iPhone 12";
        
        // Or emulate Android
        // options.FrameworkSpecificOptions["Device"] = "Galaxy S21";
    });
    
    // Act
    var page = fluentUI.NavigateTo<MobilePage>();
    
    // Assert
    page.Verify.ElementIsVisible("#mobile-menu");
}
```

### Custom Mobile Configuration

```csharp
[TestMethod]
public async Task Can_Configure_Custom_Mobile_Settings()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Custom viewport
        options.FrameworkSpecificOptions["ViewportWidth"] = 375;
        options.FrameworkSpecificOptions["ViewportHeight"] = 667;
        
        // Custom user agent
        options.FrameworkSpecificOptions["UserAgent"] = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1";
        
        // Touch support
        options.FrameworkSpecificOptions["HasTouch"] = true;
    });
    
    // Act
    var page = fluentUI.NavigateTo<TouchPage>();
    
    // Assert
    page.Verify.ElementIsVisible("#touch-area");
}
```

## Network and Performance

### Network Conditions

```csharp
[TestMethod]
public async Task Can_Simulate_Network_Conditions()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Slow 3G
        options.FrameworkSpecificOptions["NetworkConditions"] = new
        {
            Offline = false,
            DownloadSpeed = 780 * 1024, // 780 Kbps
            UploadSpeed = 330 * 1024,   // 330 Kbps
            Latency = 200               // 200ms
        };
    });
    
    // Act
    var page = fluentUI.NavigateTo<PerformancePage>();
    
    // Assert
    page.Verify.ElementIsVisible("#content");
}
```

### Performance Monitoring

```csharp
[TestMethod]
public async Task Can_Monitor_Performance()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
    
    // Act
    var page = fluentUI.NavigateTo<PerformancePage>();
    
    // Get performance metrics
    var metrics = await playwrightDriver.GetPerformanceMetricsAsync();
    
    // Assert
    Assert.IsTrue(metrics.LoadTime < 3000); // Less than 3 seconds
    Assert.IsTrue(metrics.DOMContentLoaded < 1000); // Less than 1 second
}
```

## Error Handling

### Playwright-Specific Exceptions

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

### Error Handling Examples

```csharp
[TestMethod]
public async Task Can_Handle_Playwright_Errors()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var page = fluentUI.NavigateTo<ErrorPage>();
    
    try
    {
        // Act
        page.ClickNonExistentElement();
    }
    catch (PlaywrightTimeoutException ex)
    {
        // Handle timeout
        Assert.AreEqual("#non-existent", ex.Selector);
        Assert.IsTrue(ex.Timeout.TotalSeconds > 0);
    }
    catch (PlaywrightException ex)
    {
        // Handle other Playwright errors
        Assert.IsNotNull(ex.Selector);
    }
}
```

## Debugging Features

### Debug Mode

The framework provides a convenient debug mode that automatically configures Playwright for easier debugging:

```csharp
[TestMethod]
public async Task Can_Use_Debug_Mode()
{
    // Arrange
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = new Uri("https://your-app.com"),
        // DebugMode automatically enables when debugger is attached
        // You can also explicitly set it: DebugMode = true
        // When DebugMode is true:
        // - HeadlessMode is automatically set to false
        // - SlowMo is automatically set to 1000ms
    };

    var fluentUI = new FluentUIScaffoldApp<WebApp>(options);
    await fluentUI.InitializeAsync();
    
    // Act
    var page = fluentUI.NavigateTo<DebugPage>();
    
    // Assert
    page.Verify.ElementIsVisible("#debug-info");
}
```

**Debug Mode Features:**
- **Visible Browser**: Automatically disables headless mode to show the browser window
- **SlowMo**: Sets SlowMo to 1000ms to slow down interactions for better visibility
- **Enhanced Logging**: Provides detailed logging of browser actions
- **Automatic Detection**: Automatically enables when a debugger is attached (no configuration needed!)
- **CI/CD Safe**: Remains disabled in CI/CD environments where no debugger is attached
- **Development Friendly**: Perfect for debugging test failures and understanding test flow

**Manual Configuration Alternative:**
```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("https://your-app.com"),
    HeadlessMode = false, // Manual control
    FrameworkOptions = 
    {
        ["SlowMo"] = 1000, // Manual SlowMo setting
        ["Devtools"] = true // Additional debug features
    }
};
```

### Tracing

```csharp
[TestMethod]
public async Task Can_Use_Tracing()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
    
    // Start tracing
    await playwrightDriver.StartTracingAsync("trace.zip");
    
    // Act
    var page = fluentUI.NavigateTo<TracePage>();
    page.PerformComplexAction();
    
    // Stop tracing
    await playwrightDriver.StopTracingAsync();
    
    // Assert
    Assert.IsTrue(File.Exists("trace.zip"));
}
```

## Best Practices

### 1. Browser Configuration

```csharp
// Good - explicit configuration
var fluentUI = FluentUIScaffoldBuilder.Web(options =>
{
    options.FrameworkSpecificOptions["Headless"] = false;
    options.FrameworkSpecificOptions["SlowMo"] = 1000;
    options.FrameworkSpecificOptions["ViewportWidth"] = 1280;
    options.FrameworkSpecificOptions["ViewportHeight"] = 720;
});

// Bad - relying on defaults
var fluentUI = FluentUIScaffoldBuilder.Web();
```

### 2. Error Handling

```csharp
[TestMethod]
public async Task Robust_Error_Handling()
{
    try
    {
        var page = _fluentUI.NavigateTo<TestPage>();
        page.PerformAction();
    }
    catch (PlaywrightTimeoutException ex)
    {
        // Log timeout details
        Logger.LogWarning($"Element {ex.Selector} timed out after {ex.Timeout}");
        
        // Take screenshot for debugging
        var playwrightDriver = _fluentUI.Framework<PlaywrightDriver>();
        await playwrightDriver.TakeScreenshotAsync("timeout-screenshot.png");
        
        throw;
    }
}
```

### 3. Performance Optimization

```csharp
[TestMethod]
public async Task Optimized_Performance()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        // Disable images for faster loading
        options.FrameworkSpecificOptions["BlockImages"] = true;
        
        // Set reasonable timeouts
        options.DefaultTimeout = TimeSpan.FromSeconds(10);
    });
    
    // Act
    var page = fluentUI.NavigateTo<PerformancePage>();
    
    // Assert
    page.Verify.ElementIsVisible("#content");
}
```

### 4. Network Interception

```csharp
[TestMethod]
public async Task Effective_Network_Interception()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web();
    var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
    
    // Intercept API calls
    playwrightDriver.InterceptNetworkRequests("/api/*", response =>
    {
        // Mock API responses
        if (response.Url.Contains("/users"))
        {
            response.SetBody("[{\"id\": 1, \"name\": \"John Doe\"}]");
        }
    });
    
    // Act
    var page = fluentUI.NavigateTo<ApiPage>();
    page.LoadUsers();
    
    // Assert
    page.Verify.ElementContainsText("#user-list", "John Doe");
}
```

## Testing Playwright Integration

### Unit Testing

```csharp
[TestClass]
public class PlaywrightIntegrationTests
{
    private Mock<IPlaywright> _mockPlaywright;
    private Mock<IBrowser> _mockBrowser;
    private Mock<IBrowserContext> _mockContext;
    private Mock<IPage> _mockPage;
    private PlaywrightDriver _driver;
    
    [TestInitialize]
    public void Setup()
    {
        _mockPlaywright = new Mock<IPlaywright>();
        _mockBrowser = new Mock<IBrowser>();
        _mockContext = new Mock<IBrowserContext>();
        _mockPage = new Mock<IPage>();
        
        // Setup mocks
        _mockBrowser.Setup(b => b.NewContextAsync(It.IsAny<BrowserNewContextOptions>()))
                   .ReturnsAsync(_mockContext.Object);
        _mockContext.Setup(c => c.NewPageAsync())
                   .ReturnsAsync(_mockPage.Object);
        
        var options = new FluentUIScaffoldOptions();
        _driver = new PlaywrightDriver(options);
    }
    
    [TestMethod]
    public void Click_Should_Use_Playwright_Click()
    {
        // Arrange
        _mockPage.Setup(p => p.ClickAsync(It.IsAny<string>(), It.IsAny<PageClickOptions>()))
                .Returns(Task.CompletedTask);
        
        // Act
        _driver.Click("#button");
        
        // Assert
        _mockPage.Verify(p => p.ClickAsync("#button", It.IsAny<PageClickOptions>()), Times.Once);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class PlaywrightIntegrationTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = new Uri("https://your-app.com");
            options.FrameworkSpecificOptions["Headless"] = true;
        });
    }
    
    [TestMethod]
    public async Task Can_Use_Playwright_Features()
    {
        // Arrange
        var playwrightDriver = _fluentUI.Framework<PlaywrightDriver>();
        var page = _fluentUI.NavigateTo<TestPage>();
        
        // Act
        // Take screenshot
        var screenshot = await playwrightDriver.TakeScreenshotAsync("test.png");
        
        // Intercept network
        playwrightDriver.InterceptNetworkRequests("/api/*", response =>
        {
            response.SetBody("{\"status\": \"success\"}");
        });
        
        // Assert
        Assert.IsNotNull(screenshot);
        page.Verify.ElementIsVisible("#content");
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
}
```

## Conclusion

The Playwright integration in FluentUIScaffold provides comprehensive access to Playwright's powerful features while maintaining the framework's fluent API. Key benefits include:

- **Full Playwright Support**: Access to all Playwright features
- **Fluent API**: Consistent interface across different frameworks
- **Advanced Features**: Network interception, screenshots, PDF generation
- **Mobile Emulation**: Test responsive designs effectively
- **Performance Monitoring**: Built-in performance metrics
- **Debugging Tools**: Comprehensive debugging capabilities

For more information, see the [API Reference](api-reference.md) and [Getting Started](getting-started.md) guides. 