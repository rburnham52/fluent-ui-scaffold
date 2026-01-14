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
    public IReadOnlyList<Type> SupportedDriverTypes { get; }

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
    public Uri CurrentUrl { get; }

    // Core interactions
    public void Click(string selector);
    public void Type(string selector, string text);
    public void SelectOption(string selector, string value);
    public string GetText(string selector);
    public string GetValue(string selector);
    public string GetAttribute(string selector, string attributeName);
    public bool IsVisible(string selector);
    public bool IsEnabled(string selector);
    public void WaitForElement(string selector);
    public void WaitForElementToBeVisible(string selector);
    public void WaitForElementToBeHidden(string selector);

    // Navigation
    public void NavigateToUrl(Uri url);
    public TTarget NavigateTo<TTarget>() where TTarget : class;

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
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
        opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
        opts.HeadlessMode = false;
        opts.SlowMo = 1000;
    })
    .Build<WebApp>();

await app.StartAsync();

// Get Playwright driver
var playwrightDriver = app.Framework<PlaywrightDriver>();
```

### Basic Interactions

```csharp
[TestMethod]
public void Can_Interact_With_Playwright()
{
    // Arrange
    var page = TestAssemblyHooks.App.NavigateTo<HomePage>();

    // Act
    page.EnterEmail("test@example.com")
        .EnterPassword("password")
        .ClickLogin();

    // Assert
    page.Verify.Visible(p => p.WelcomeMessage);
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
public void Can_Intercept_Network_Requests()
{
    // Arrange
    var playwrightDriver = TestAssemblyHooks.App.Framework<PlaywrightDriver>();

    // Act
    playwrightDriver.InterceptNetworkRequests("/api/*", response =>
    {
        // Modify response
        response.SetBody("{\"status\": \"success\"}");
    });

    var page = TestAssemblyHooks.App.NavigateTo<ApiTestPage>();
    page.TriggerApiCall();

    // Assert
    page.Verify.TextContains(p => p.ResultMessage, "success");
}
```

### Screenshots and PDFs

```csharp
[TestMethod]
public async Task Can_Take_Screenshots_And_Generate_PDFs()
{
    // Arrange
    var playwrightDriver = TestAssemblyHooks.App.Framework<PlaywrightDriver>();
    var page = TestAssemblyHooks.App.NavigateTo<HomePage>();

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
public void Can_Configure_Browser_Settings()
{
    // Arrange - Configuration done at builder level
    var app = new FluentUIScaffoldBuilder()
        .UsePlugin(new PlaywrightPlugin())
        .Web<WebApp>(opts =>
        {
            opts.BaseUrl = new Uri("https://your-app.com");
            opts.HeadlessMode = false;
            opts.SlowMo = 500;
        })
        .Build<WebApp>();

    // Act
    var page = app.NavigateTo<GeolocationPage>();

    // Assert
    page.Verify.Visible(p => p.LocationInfo);
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
public void Can_Use_Playwright_Wait_Strategies()
{
    // Arrange
    var page = TestAssemblyHooks.App.NavigateTo<DynamicPage>();

    // Act - Using fluent page methods with wait strategies
    page.WaitForVisible(p => p.DynamicContent)
        .WaitForVisible(p => p.ActionButton)
        .Click(p => p.ActionButton);

    // Assert
    page.Verify.Visible(p => p.DynamicContent);
}
```

## Browser Management

### Browser Types

```csharp
[TestMethod]
public void Can_Use_Different_Browsers()
{
    // Arrange - Browser type configured at plugin/driver level
    var page = TestAssemblyHooks.App.NavigateTo<HomePage>();

    // Act & Assert
    page.Verify.Visible(p => p.Content);
}
```

### Browser Context

```csharp
[TestMethod]
public void Can_Manage_Browser_Context()
{
    // Arrange
    var page = TestAssemblyHooks.App.NavigateTo<LocalizedPage>();

    // Assert
    page.Verify.TextContains(p => p.LocaleDisplay, "en-US");
}
```

## Mobile Emulation

### Mobile Device Emulation

```csharp
[TestMethod]
public void Can_Emulate_Mobile_Devices()
{
    // Arrange - Mobile emulation configured at driver level
    var page = TestAssemblyHooks.App.NavigateTo<MobilePage>();

    // Assert
    page.Verify.Visible(p => p.MobileMenu);
}
```

## Network and Performance

### Network Conditions

```csharp
[TestMethod]
public void Can_Simulate_Network_Conditions()
{
    // Arrange
    var page = TestAssemblyHooks.App.NavigateTo<PerformancePage>();

    // Assert
    page.Verify.Visible(p => p.Content);
}
```

### Performance Monitoring

```csharp
[TestMethod]
public async Task Can_Monitor_Performance()
{
    // Arrange
    var playwrightDriver = TestAssemblyHooks.App.Framework<PlaywrightDriver>();

    // Act
    var page = TestAssemblyHooks.App.NavigateTo<PerformancePage>();

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
    var page = TestAssemblyHooks.App.NavigateTo<ErrorPage>();

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

### Headless and SlowMo Defaults

The framework provides convenient debug mode that automatically configures Playwright for easier debugging:

```csharp
[TestMethod]
public void Can_Use_Debug_Mode()
{
    // Arrange - When a debugger is attached, PlaywrightDriver defaults to non-headless and slight SlowMo
    var app = new FluentUIScaffoldBuilder()
        .UsePlugin(new PlaywrightPlugin())
        .Web<WebApp>(opts =>
        {
            opts.BaseUrl = new Uri("https://your-app.com");
            // HeadlessMode and SlowMo auto-detect when debugger is attached
        })
        .Build<WebApp>();

    // Act
    var page = app.NavigateTo<DebugPage>();

    // Assert
    page.Verify.Visible(p => p.DebugInfo);
}
```

Defaults while debugging: headless disabled and slight SlowMo (e.g., 250ms). In CI/non-debug, headless with 0ms SlowMo unless overridden via options.
- **Visible Browser**: Automatically disables headless mode to show the browser window
- **SlowMo**: Sets SlowMo to slow down interactions for better visibility
- **Enhanced Logging**: Provides detailed logging of browser actions
- **Automatic Detection**: Automatically enables when a debugger is attached (no configuration needed!)
- **CI/CD Safe**: Remains disabled in CI/CD environments where no debugger is attached
- **Development Friendly**: Perfect for debugging test failures and understanding test flow

**Manual Configuration Alternative:**
```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
        opts.HeadlessMode = false;
        opts.SlowMo = 1000;
    })
    .Build<WebApp>();
```

### Tracing

```csharp
[TestMethod]
public async Task Can_Use_Tracing()
{
    // Arrange
    var playwrightDriver = TestAssemblyHooks.App.Framework<PlaywrightDriver>();

    // Start tracing
    await playwrightDriver.StartTracingAsync("trace.zip");

    // Act
    var page = TestAssemblyHooks.App.NavigateTo<TracePage>();
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
// Good - explicit configuration via builder
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
        opts.HeadlessMode = false;
        opts.SlowMo = 1000;
        opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
    })
    .Build<WebApp>();

// Bad - relying on defaults without explicit configuration
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Build<WebApp>();
```

### 2. Error Handling

```csharp
[TestMethod]
public async Task Robust_Error_Handling()
{
    try
    {
        var page = TestAssemblyHooks.App.NavigateTo<TestPage>();
        page.PerformAction();
    }
    catch (PlaywrightTimeoutException ex)
    {
        // Log timeout details
        Logger.LogWarning($"Element {ex.Selector} timed out after {ex.Timeout}");

        // Take screenshot for debugging
        var playwrightDriver = TestAssemblyHooks.App.Framework<PlaywrightDriver>();
        await playwrightDriver.TakeScreenshotAsync("timeout-screenshot.png");

        throw;
    }
}
```

### 3. Performance Optimization

```csharp
[TestMethod]
public void Optimized_Performance()
{
    // Arrange
    var app = new FluentUIScaffoldBuilder()
        .UsePlugin(new PlaywrightPlugin())
        .Web<WebApp>(opts =>
        {
            opts.BaseUrl = new Uri("https://your-app.com");
            opts.DefaultWaitTimeout = TimeSpan.FromSeconds(10);
            opts.HeadlessMode = true; // Faster in CI
        })
        .Build<WebApp>();

    // Act
    var page = app.NavigateTo<PerformancePage>();

    // Assert
    page.Verify.Visible(p => p.Content);
}
```

### 4. Network Interception

```csharp
[TestMethod]
public void Effective_Network_Interception()
{
    // Arrange
    var playwrightDriver = TestAssemblyHooks.App.Framework<PlaywrightDriver>();

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
    var page = TestAssemblyHooks.App.NavigateTo<ApiPage>();
    page.LoadUsers();

    // Assert
    page.Verify.TextContains(p => p.UserList, "John Doe");
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
    private static AppScaffold<WebApp>? _app;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .Web<WebApp>(opts =>
            {
                opts.BaseUrl = new Uri("https://your-app.com");
                opts.HeadlessMode = true;
            })
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [TestMethod]
    public async Task Can_Use_Playwright_Features()
    {
        // Arrange
        var playwrightDriver = _app!.Framework<PlaywrightDriver>();
        var page = _app.NavigateTo<TestPage>();

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
        page.Verify.Visible(p => p.Content);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }
}
```

## Hosting Strategies with Playwright

FluentUIScaffold supports multiple hosting strategies that work seamlessly with Playwright:

### Aspire Hosting

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.SampleApp_AppHost>(
        appHost => { /* configure */ },
        "sampleapp")
    .Web<WebApp>(opts => { opts.UsePlaywright(); })
    .Build<WebApp>();

await app.StartAsync();
```

### External Server

For CI environments or staging servers:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://staging.your-app.com");
        opts.HeadlessMode = true;
    })
    .Build<WebApp>();

await app.StartAsync();
```

## Conclusion

The Playwright integration in FluentUIScaffold provides comprehensive access to Playwright's powerful features while maintaining the framework's fluent API. Key benefits include:

- **Full Playwright Support**: Access to all Playwright features
- **Fluent API**: Consistent interface across different frameworks
- **Advanced Features**: Network interception, screenshots, PDF generation
- **Mobile Emulation**: Test responsive designs effectively
- **Performance Monitoring**: Built-in performance metrics
- **Debugging Tools**: Comprehensive debugging capabilities
- **Hosting Strategies**: Pluggable hosting for .NET, Node, External, and Aspire apps

For more information, see the [API Reference](api-reference.md) and [Getting Started](getting-started.md) guides.
