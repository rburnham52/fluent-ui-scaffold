# Getting Started with FluentUIScaffold

## Overview

This guide will help you get started with FluentUIScaffold, a framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation.

**Status**: The framework is actively developed with Example 1 (User Registration and Login Flow) fully implemented and tested. See the [roadmap](roadmap/README.md) for current development status.

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 6.0 or later** - [Download .NET](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** or **VS Code** - [Download Visual Studio](https://visualstudio.microsoft.com/) or [Download VS Code](https://code.visualstudio.com/)
- **Git** - [Download Git](https://git-scm.com/)

## Installation

### Option 1: Clone the Repository

```bash
# Clone the repository
git clone https://github.com/your-org/fluent-ui-scaffold.git
cd fluent-ui-scaffold

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Option 2: Add to Existing Project

```bash
# Add the NuGet packages (when published)
dotnet add package FluentUIScaffold.Core
dotnet add package FluentUIScaffold.Playwright
```

## Quick Start

### 1. Create Your First Test

Create a new test project and add the following code:

```csharp
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using Microsoft.Extensions.Logging;

[TestClass]
public class MyFirstTest
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;

    [TestInitialize]
    public async Task Setup()
    {
        var options = new FluentUIScaffoldOptions
        {
            BaseUrl = new Uri("https://your-app.com"),
            DefaultWaitTimeout = TimeSpan.FromSeconds(30),
            LogLevel = LogLevel.Information,
            HeadlessMode = true // Run in headless mode for CI/CD
        };

        _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
        await _fluentUI.InitializeAsync(options);
    }

    [TestMethod]
    public async Task Can_Navigate_To_Home_Page()
    {
        // Arrange
        var homePage = _fluentUI.NavigateTo<HomePage>();

        // Act & Assert
        homePage.Verify.ElementIsVisible("#welcome-message");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
}
```

### 2. Web Server Launching (Optional)

For applications that need to be launched before testing, you can configure automatic web server launching:

```csharp
[TestInitialize]
public async Task Setup()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = new Uri("https://your-app.com"),
        DefaultWaitTimeout = TimeSpan.FromSeconds(30),
        LogLevel = LogLevel.Information,
        HeadlessMode = true,
        // Web server launching configuration
        EnableWebServerLaunch = true,
        WebServerProjectPath = "path/to/your/web/app",
        ReuseExistingServer = false // Set to true to reuse an already running server
    };

    _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
    await _fluentUI.InitializeAsync(options); // This will launch the web server if enabled
}

The framework will automatically:
- Launch the web server using `dotnet run` in the specified project path
- Wait for the server to be accessible at the configured base URL
- Clean up the server process when tests complete

### 4. Debug Mode

For easier debugging during development, debug mode automatically:
- Disables headless mode to show the browser window
- Sets SlowMo to 1000ms to slow down interactions for better visibility
- Provides detailed logging of browser actions
- **Automatically enables when a debugger is attached** (no configuration needed!)

```csharp
[TestInitialize]
public async Task Setup()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = new Uri("https://your-app.com"),
        DefaultWaitTimeout = TimeSpan.FromSeconds(30),
        LogLevel = LogLevel.Information,
        // DebugMode automatically enables when debugger is attached
        // You can also explicitly set it: DebugMode = true
        // When DebugMode is true, HeadlessMode is automatically set to false
        // and SlowMo is set to 1000ms
    };

    _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
    await _fluentUI.InitializeAsync();
}
```

**Debug Mode vs Normal Mode:**

| Setting | Normal Mode | Debug Mode |
|---------|-------------|------------|
| Headless | `HeadlessMode` property (default: true) | Always `false` |
| SlowMo | `FrameworkOptions["SlowMo"]` or `0` | Always `1000ms` |
| Browser Window | Hidden (if headless) | Visible |
| Interaction Speed | Normal | Slowed down for visibility |

**Best Practices:**
- Debug mode automatically activates when you run tests in debug mode (F5 in Visual Studio, or when a debugger is attached)
- For CI/CD environments where no debugger is attached, it remains disabled by default
- You can explicitly set `DebugMode = true` for manual control
- You can also set `HeadlessMode = false` and `FrameworkOptions["SlowMo"] = 1000` manually for more control

For easier debugging during development, debug mode automatically:
- Disables headless mode to show the browser window
- Sets SlowMo to 1000ms to slow down interactions for better visibility
- Provides detailed logging of browser actions
- **Automatically enables when a debugger is attached** (no configuration needed!)

```csharp
[TestInitialize]
public async Task Setup()
{
    var options = new FluentUIScaffoldOptions
    {
        BaseUrl = new Uri("https://your-app.com"),
        DefaultWaitTimeout = TimeSpan.FromSeconds(30),
        LogLevel = LogLevel.Information,
        // DebugMode automatically enables when debugger is attached
        // You can also explicitly set it: DebugMode = true
        // When DebugMode is true, HeadlessMode is automatically set to false
        // and SlowMo is set to 1000ms
    };

    _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
    await _fluentUI.InitializeAsync();
}
```

**Debug Mode vs Normal Mode:**

| Setting | Normal Mode | Debug Mode |
|---------|-------------|------------|
| Headless | `HeadlessMode` property (default: true) | Always `false` |
| SlowMo | `FrameworkOptions["SlowMo"]` or `0` | Always `1000ms` |
| Browser Window | Hidden (if headless) | Visible |
| Interaction Speed | Normal | Slowed down for visibility |

**Best Practices:**
- Debug mode automatically activates when you run tests in debug mode (F5 in Visual Studio, or when a debugger is attached)
- For CI/CD environments where no debugger is attached, it remains disabled by default
- You can explicitly set `DebugMode = true` for manual control
- You can also set `HeadlessMode = false` and `FrameworkOptions["SlowMo"] = 1000` manually for more control

### 5. Create Your First Page Object
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Pages;

public class HomePage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/home";
    
    private IElement _welcomeMessage;
    private IElement _loginButton;
    
    protected override void ConfigureElements()
    {
        _welcomeMessage = Element("#welcome-message")
            .WithDescription("Welcome Message")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _loginButton = Element("#login-button")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
    }
    
    public HomePage VerifyWelcomeMessage()
    {
        _welcomeMessage.WaitFor();
        return this;
    }
    
    public LoginPage ClickLogin()
    {
        _loginButton.Click();
        return NavigateTo<LoginPage>();
    }
}
```

### 3. Run Your Tests

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "Can_Navigate_To_Home_Page"
```

## Sample Application

The project includes a comprehensive sample application that demonstrates the framework's capabilities:

### Running the Sample

```bash
# Navigate to sample app
cd samples/SampleApp

# Install frontend dependencies
cd ClientApp && npm install && cd ..

# Run the application
dotnet run

# Run tests (in another terminal)
cd ../SampleApp.Tests
dotnet test
```

### Sample Features

The sample application includes:

- **Modern Web Application**: ASP.NET Core backend with Svelte frontend
- **User Registration and Login**: Complete form interactions and validation
- **Comprehensive Testing**: 43 passing tests covering all scenarios
- **Framework Features**: Navigation, form interactions, verification, error handling

### Example Test

```csharp
[TestMethod]
public async Task Can_Register_New_User_With_Valid_Data()
{
    // Arrange
    var homePage = _fluentUI!.NavigateTo<HomePage>();
    homePage.NavigateToRegisterSection();

    // Act
    homePage
        .Type(e => e.EmailInput, "test@example.com")
        .Type(e => e.PasswordInput, "SecurePass123!")
        .Type(e => e.FirstNameInput, "Test")
        .Type(e => e.LastNameInput, "User")
        .Click(e => e.RegisterButton);

    // Assert
    homePage.Verify.ElementContainsText("#success-message", "Registration successful!");
}
```

## Configuration

### Basic Configuration

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("https://your-app.com"),
    DefaultTimeout = TimeSpan.FromSeconds(30),
    DefaultRetryInterval = TimeSpan.FromMilliseconds(500),
    WaitStrategy = WaitStrategy.Smart,
    LogLevel = LogLevel.Information,
    CaptureScreenshotsOnFailure = true,
    CaptureDOMStateOnFailure = true,
    ScreenshotPath = "./screenshots"
};
```

### Framework-Specific Configuration

```csharp
// Playwright-specific options
options.FrameworkSpecificOptions["Headless"] = false;
options.FrameworkSpecificOptions["SlowMo"] = 1000;
options.FrameworkSpecificOptions["ViewportWidth"] = 1280;
options.FrameworkSpecificOptions["ViewportHeight"] = 720;
```

## Element Configuration

### Basic Element Setup

```csharp
// Simple element
var button = Element("#submit-button");

// Element with description
var input = Element("#email-input")
    .WithDescription("Email Input Field");

// Element with timeout
var dropdown = Element("#country-select")
    .WithTimeout(TimeSpan.FromSeconds(10));

// Element with wait strategy
var loadingSpinner = Element(".loading-spinner")
    .WithWaitStrategy(WaitStrategy.Hidden);
```

### Advanced Element Configuration

```csharp
var complexElement = Element("[data-testid='user-card']")
    .WithDescription("User Card Component")
    .WithTimeout(TimeSpan.FromSeconds(15))
    .WithWaitStrategy(WaitStrategy.Visible)
    .WithRetryInterval(TimeSpan.FromMilliseconds(200));
```

## Wait Strategies

### Available Wait Strategies

```csharp
// No waiting
var element = Element("#button").WithWaitStrategy(WaitStrategy.None);

// Wait for visibility
var visibleElement = Element("#modal").WithWaitStrategy(WaitStrategy.Visible);

// Wait for clickability
var clickableElement = Element("#submit").WithWaitStrategy(WaitStrategy.Clickable);

// Wait for text
var textElement = Element("#message").WithWaitStrategy(WaitStrategy.TextPresent);

// Smart waiting (framework-specific)
var smartElement = Element("#dynamic-content").WithWaitStrategy(WaitStrategy.Smart);
```

### Custom Wait Conditions

```csharp
// Wait for custom condition
var element = Element("#status")
    .WithWaitStrategy(WaitStrategy.Custom)
    .WithCustomWaitCondition(e => e.GetText() == "Complete");
```

## Page Object Pattern

### Creating Page Objects

```csharp
public class LoginPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/login";
    
    private IElement _emailInput;
    private IElement _passwordInput;
    private IElement _loginButton;
    private IElement _errorMessage;
    
    protected override void ConfigureElements()
    {
        _emailInput = Element("#email")
            .WithDescription("Email Input")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _passwordInput = Element("#password")
            .WithDescription("Password Input")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _loginButton = Element("#login-btn")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
            
        _errorMessage = Element(".error-message")
            .WithDescription("Error Message")
            .WithWaitStrategy(WaitStrategy.Visible);
    }
    
    public LoginPage EnterEmail(string email)
    {
        _emailInput.Type(email);
        return this;
    }
    
    public LoginPage EnterPassword(string password)
    {
        _passwordInput.Type(password);
        return this;
    }
    
    public HomePage ClickLogin()
    {
        _loginButton.Click();
        return NavigateTo<HomePage>();
    }
    
    public LoginPage VerifyErrorMessage(string expectedMessage)
    {
        Verify.ElementContainsText(".error-message", expectedMessage);
        return this;
    }
}
```

### Page Navigation

```csharp
// Navigate to specific page
var homePage = fluentUI.NavigateTo<HomePage>();

// Navigate with parameters
var userProfilePage = fluentUI.NavigateTo<UserProfilePage>();

// Check current page
if (homePage.IsCurrentPage())
{
    // Current page logic
}

// Validate current page
homePage.ValidateCurrentPage();
```

### Advanced Navigation

For more complex navigation scenarios, you can use custom navigation methods:

```csharp
// Navigate to specific user profile
var profilePage = fluentUI.NavigateTo<UserProfilePage>();
profilePage.NavigateToUserProfile(123);

// Navigate with query parameters
var searchPage = fluentUI.NavigateTo<SearchPage>();
searchPage.NavigateWithQuery("test query", "category");

// Direct URL navigation
fluentUI.NavigateToUrl(new Uri("https://your-app.com/specific-path"));
```

For detailed navigation patterns, see the [Page Object Pattern](page-object-pattern.md) documentation.

## Verification

### Element Verification

```csharp
// Verify element is visible
page.Verify.ElementIsVisible("#submit-button");

// Verify element contains text
page.Verify.ElementContainsText("#message", "Success");

// Verify element has attribute
page.Verify.ElementHasAttribute("#link", "href", "https://example.com");

// Verify element is enabled
page.Verify.ElementIsEnabled("#input-field");
```

### Page Verification

```csharp
// Verify current page type
page.Verify.CurrentPageIs<HomePage>();

// Verify URL matches pattern
page.Verify.UrlMatches("/home");

// Verify title contains text
page.Verify.TitleContains("Welcome");
```

### Custom Verification

```csharp
// Custom condition
page.Verify.That(() => page.GetElementCount() > 0, "Should have elements");

// Custom condition with actual value
page.Verify.That(
    () => page.GetElementText("#counter"),
    text => int.Parse(text) > 5,
    "Counter should be greater than 5"
);
```

## Playwright Integration

### Using Playwright-Specific Features

```csharp
// Get Playwright driver
var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();

// Take screenshot
await playwrightDriver.TakeScreenshotAsync("screenshot.png");

// Intercept network requests
playwrightDriver.InterceptNetworkRequests("/api/*", response =>
{
    // Handle intercepted response
});

// Set viewport size
playwrightDriver.SetViewportSize(1920, 1080);

// Set user agent
playwrightDriver.SetUserAgent("Custom User Agent");
```

### Advanced Playwright Features

```csharp
// Generate PDF
await playwrightDriver.GeneratePdfAsync("page.pdf");

// Set geolocation
playwrightDriver.SetGeolocation(40.7128, -74.0060);

// Set permissions
playwrightDriver.SetPermissions(new[] { "geolocation", "notifications" });
```

## Testing Best Practices

### Test Structure

```csharp
[TestClass]
public class UserManagementTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;

    [TestInitialize]
    public async Task Setup()
    {
        var options = new FluentUIScaffoldOptions
        {
            BaseUrl = new Uri("https://your-app.com"),
            DefaultWaitTimeout = TimeSpan.FromSeconds(30),
            LogLevel = LogLevel.Information,
            HeadlessMode = true,
            CaptureScreenshotsOnFailure = true,
            // Optional: Enable web server launching
            EnableWebServerLaunch = true,
            WebServerProjectPath = "path/to/your/web/app"
        };

        _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
        await _fluentUI.InitializeAsync(options);
    }

    [TestMethod]
    public async Task Can_Create_New_User()
    {
        // Arrange
        var user = new User { Name = "John Doe", Email = "john@example.com" };
        var userPage = _fluentUI.NavigateTo<UserManagementPage>();

        // Act
        var resultPage = userPage
            .ClickCreateUser()
            .EnterUserName(user.Name)
            .EnterUserEmail(user.Email)
            .ClickSave();

        // Assert
        resultPage.Verify
            .ElementContainsText("#success-message", "User created successfully")
            .CurrentPageIs<UserListPage>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
}
```

### Error Handling

```csharp
[TestMethod]
public async Task Handles_Network_Errors()
{
    try
    {
        var page = _fluentUI.NavigateTo<HomePage>();
        page.Verify.ElementIsVisible("#content");
    }
    catch (ElementTimeoutException ex)
    {
        // Handle timeout
        Assert.Fail($"Element not found: {ex.Selector}");
    }
    catch (FluentUIScaffoldException ex)
    {
        // Handle other framework errors
        Assert.Fail($"Framework error: {ex.Message}");
    }
}
```

## Troubleshooting

### Common Issues

#### Element Not Found

```csharp
// Check if selector is correct
var element = Element("#correct-selector");

// Use more specific selectors
var element = Element("[data-testid='unique-id']");

// Wait for element to be present
element.WithWaitStrategy(WaitStrategy.Visible);
```

#### Test Flakiness

```csharp
// Increase timeout for slow elements
element.WithTimeout(TimeSpan.FromSeconds(30));

// Use smart wait strategy
element.WithWaitStrategy(WaitStrategy.Smart);

// Add retry logic
element.WithRetryInterval(TimeSpan.FromMilliseconds(500));
```

#### Framework-Specific Issues

```csharp
// Check Playwright driver
var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();

// Enable debugging
options.LogLevel = LogLevel.Debug;

// Capture screenshots on failure
options.CaptureScreenshotsOnFailure = true;
```

### Debugging

```csharp
// Enable detailed logging
options.LogLevel = LogLevel.Debug;

// Capture DOM state on failure
options.CaptureDOMStateOnFailure = true;

// Set custom screenshot path
options.ScreenshotPath = "./debug-screenshots";
```

## Next Steps

- Read the [API Reference](api-reference.md) for complete documentation
- Explore the [Sample Application](sample-application.md) for real-world examples
- Learn about [Page Object Pattern](page-object-pattern.md) best practices
- Check out [Testing Best Practices](testing-best-practices.md) for advanced techniques

## Support

- **Issues**: [GitHub Issues](https://github.com/your-org/fluent-ui-scaffold/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/fluent-ui-scaffold/discussions)
- **Documentation**: [API Reference](api-reference.md) 