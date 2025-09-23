# Getting Started with FluentUIScaffold

## Overview

This guide will help you get started with FluentUIScaffold, a framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation.

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

Create a new test project and set up assembly-level initialization:

```csharp
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .Web<WebApp>(opts =>
            {
                opts.BaseUrl = new Uri("https://your-app.com");
                opts.DefaultWaitTimeout = TimeSpan.FromSeconds(60);
            })
            .WithAutoPageDiscovery()
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }

    public static AppScaffold<WebApp> App => _app!;
}
```

### 2. Write Your First Test

```csharp
[TestClass]
public class HomePageTests
{
    [TestMethod]
    public void Can_Navigate_To_Home_Page()
    {
        // Arrange & Act
        var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();

        // Assert
        homePage.Verify
            .Visible(p => p.WelcomeMessage)
            .TitleContains("Welcome");
    }
}
```

### 3. Create Your First Page Object

```csharp
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Interfaces;

public class HomePage : Page<HomePage>
{
    public IElement WelcomeMessage { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;

    public HomePage(IServiceProvider serviceProvider, Uri urlPattern)
        : base(serviceProvider, urlPattern)
    {
    }

    protected override void ConfigureElements()
    {
        WelcomeMessage = Element("#welcome-message")
            .WithDescription("Welcome Message")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        LoginButton = Element("#login-button")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();
    }

    public HomePage VerifyWelcomeMessage()
    {
        return WaitForVisible(p => p.WelcomeMessage);
    }

    public LoginPage ClickLogin()
    {
        Click(p => p.LoginButton);
        return NavigateTo<LoginPage>();
    }
}
```

### 4. Run Your Tests

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "Can_Navigate_To_Home_Page"
```

## Hosting Strategies

FluentUIScaffold supports multiple hosting strategies for different scenarios:

### Using Aspire Hosting (Recommended for Distributed Apps)

```csharp
[AssemblyInitialize]
public static async Task AssemblyInitialize(TestContext context)
{
    _app = new FluentUIScaffoldBuilder()
        .UseAspireHosting<Projects.SampleApp_AppHost>(
            appHost => { /* configure distributed app */ },
            "sampleapp")
        .Web<WebApp>(options => { options.UsePlaywright(); })
        .Build<WebApp>();

    await _app.StartAsync();
}
```

### Using External Server (Pre-started)

For CI environments or staging servers that are already running:

```csharp
_app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts => opts.BaseUrl = new Uri("https://staging.your-app.com"))
    .Build<WebApp>();

await _app.StartAsync();
```

## Headless and SlowMo Defaults

For easier debugging during development, debug mode automatically:
- Disables headless mode to show the browser window
- Sets SlowMo to slow down interactions for better visibility
- **Automatically enables when a debugger is attached** (no configuration needed!)

You can override these defaults:

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("https://your-app.com"))
    .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
    .WithHeadlessMode(false) // Force visible browser
    .WithSlowMo(250)         // Custom SlowMo delay
    .Build();
```

## Element Configuration

### Basic Element Setup

```csharp
// Simple element
var button = Element("#submit-button").Build();

// Element with description
var input = Element("#email-input")
    .WithDescription("Email Input Field")
    .Build();

// Element with timeout
var dropdown = Element("#country-select")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .Build();

// Element with wait strategy
var loadingSpinner = Element(".loading-spinner")
    .WithWaitStrategy(WaitStrategy.Hidden)
    .Build();
```

### Advanced Element Configuration

```csharp
var complexElement = Element("[data-testid='user-card']")
    .WithDescription("User Card Component")
    .WithTimeout(TimeSpan.FromSeconds(15))
    .WithWaitStrategy(WaitStrategy.Visible)
    .WithRetryInterval(TimeSpan.FromMilliseconds(200))
    .Build();
```

## Wait Strategies

### Available Wait Strategies

```csharp
// No waiting
var element = Element("#button").WithWaitStrategy(WaitStrategy.None).Build();

// Wait for visibility
var visibleElement = Element("#modal").WithWaitStrategy(WaitStrategy.Visible).Build();

// Wait for clickability
var clickableElement = Element("#submit").WithWaitStrategy(WaitStrategy.Clickable).Build();

// Wait for text
var textElement = Element("#message").WithWaitStrategy(WaitStrategy.TextPresent).Build();

// Smart waiting (framework-specific)
var smartElement = Element("#dynamic-content").WithWaitStrategy(WaitStrategy.Smart).Build();
```

## Page Object Pattern

### Creating Page Objects

```csharp
public class LoginPage : Page<LoginPage>
{
    public IElement EmailInput { get; private set; } = null!;
    public IElement PasswordInput { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;
    public IElement ErrorMessage { get; private set; } = null!;

    public LoginPage(IServiceProvider serviceProvider, Uri urlPattern)
        : base(serviceProvider, urlPattern)
    {
    }

    protected override void ConfigureElements()
    {
        EmailInput = Element("#email")
            .WithDescription("Email Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        PasswordInput = Element("#password")
            .WithDescription("Password Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        LoginButton = Element("#login-btn")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();

        ErrorMessage = Element(".error-message")
            .WithDescription("Error Message")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();
    }

    public LoginPage EnterEmail(string email)
    {
        return Type(p => p.EmailInput, email);
    }

    public LoginPage EnterPassword(string password)
    {
        return Type(p => p.PasswordInput, password);
    }

    public HomePage ClickLogin()
    {
        Click(p => p.LoginButton);
        return NavigateTo<HomePage>();
    }

    public LoginPage VerifyErrorMessage(string expectedMessage)
    {
        Verify.TextContains(p => p.ErrorMessage, expectedMessage);
        return this;
    }
}
```

### Page Navigation

```csharp
// Navigate to specific page
var homePage = app.NavigateTo<HomePage>();

// Get current page without navigating
var currentPage = app.On<HomePage>();

// Wait for page to be ready
app.WaitFor<HomePage>();

// Navigate with parameters via page methods
var userProfilePage = app.NavigateTo<UserProfilePage>();
userProfilePage.NavigateToUserProfile(123);
```

## Verification

### Using the Verify Property

```csharp
// Chain verifications
page.Verify
    .TitleContains("Dashboard")
    .UrlContains("/dashboard")
    .Visible(p => p.WelcomeMessage)
    .TextContains(p => p.UserGreeting, "Hello")
    .And  // Returns to page for continued interaction
    .Click(p => p.LogoutButton);
```

### Custom Verification

```csharp
// Custom condition
page.Verify.That(() => page.GetElementCount() > 0, "Should have elements");
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
- **Comprehensive Testing**: Full test coverage demonstrating framework features
- **Framework Features**: Navigation, form interactions, verification, error handling

### Example Test

```csharp
[TestMethod]
public void Can_Register_New_User_With_Valid_Data()
{
    // Arrange
    var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();
    homePage.NavigateToRegisterSection();

    // Act
    homePage
        .Type(p => p.EmailInput, "test@example.com")
        .Type(p => p.PasswordInput, "SecurePass123!")
        .Type(p => p.FirstNameInput, "Test")
        .Type(p => p.LastNameInput, "User")
        .Click(p => p.RegisterButton);

    // Assert
    homePage.Verify.TextContains(p => p.SuccessMessage, "Registration successful!");
}
```

## Playwright Integration

### Using Playwright-Specific Features

```csharp
// Get Playwright driver
var playwrightDriver = app.Framework<PlaywrightDriver>();

// Take screenshot
await playwrightDriver.TakeScreenshotAsync("screenshot.png");

// Set viewport size
playwrightDriver.SetViewportSize(1920, 1080);
```

## Testing Best Practices

### Test Structure

```csharp
[TestClass]
public class UserManagementTests
{
    [TestMethod]
    public void Can_Create_New_User()
    {
        // Arrange
        var user = new User { Name = "John Doe", Email = "john@example.com" };
        var userPage = TestAssemblyHooks.App.NavigateTo<UserManagementPage>();

        // Act
        var resultPage = userPage
            .ClickCreateUser()
            .EnterUserName(user.Name)
            .EnterUserEmail(user.Email)
            .ClickSave();

        // Assert
        resultPage.Verify
            .TextContains(p => p.SuccessMessage, "User created successfully");
    }
}
```

### Error Handling

```csharp
[TestMethod]
public void Handles_Network_Errors()
{
    try
    {
        var page = TestAssemblyHooks.App.NavigateTo<HomePage>();
        page.Verify.Visible(p => p.Content);
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
var element = Element("#correct-selector").Build();

// Use more specific selectors
var element = Element("[data-testid='unique-id']").Build();

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

## Next Steps

- Read the [API Reference](api-reference.md) for complete documentation
- Explore the [Sample Application](../samples/README.md) for real-world examples
- Learn about [Page Object Pattern](page-object-pattern.md) best practices

## Support

- **Issues**: [GitHub Issues](https://github.com/your-org/fluent-ui-scaffold/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/fluent-ui-scaffold/discussions)
- **Documentation**: [API Reference](api-reference.md)
