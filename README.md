# FluentUIScaffold E2E Testing Framework

A framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. FluentUIScaffold abstracts underlying testing frameworks (Playwright, Selenium) while providing a consistent developer experience.

## 🚀 Features

- **Framework Agnostic**: Abstract underlying testing frameworks (Playwright, Selenium) while providing consistent developer experience
- **Fluent API**: Intuitive, chainable API similar to [fluent-test-scaffold](https://github.com/rburnham52/fluent-test-scaffold)
- **Multi-Target Support**: Support .NET 6, 7, 8, 9
- **Page Object Pattern**: Comprehensive page object implementation with navigation and validation
- **Element Configuration**: Flexible element definition with wait strategies and timeouts
- **Plugin System**: Extensible plugin architecture for different testing frameworks
- **Comprehensive Testing**: TDD approach with comprehensive unit tests for all public APIs

## 📦 Quick Start

### Installation

```bash
# Add the NuGet package (when published)
dotnet add package FluentUIScaffold.Core
dotnet add package FluentUIScaffold.Playwright
```

### Basic Usage

```csharp
// Configure FluentUIScaffold with Playwright
var fluentUI = FluentUIScaffoldBuilder.Web(options =>
{
    options.BaseUrl = new Uri("https://your-app.com");
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    options.LogLevel = LogLevel.Information;
});

// Navigate and interact
var homePage = fluentUI
    .NavigateToUrl(new Uri("https://your-app.com"))
    .Framework<HomePage>();

homePage
    .ClickButton()
    .VerifyElementIsVisible()
    .NavigateTo<OtherPage>();
```

## 🏗️ Architecture

### Core Components

- **FluentUIScaffoldApp<TApp>**: Main entry point for the testing framework
- **BasePageComponent<TApp>**: Base class for all page objects
- **IElement**: Interface for element interactions
- **ElementBuilder**: Fluent API for element configuration
- **Plugin System**: Extensible architecture for different testing frameworks

### Framework Support

- **Playwright**: Full support with advanced features
- **Selenium**: Planned for future releases
- **Mobile**: Planned for future releases

## 📚 Documentation

- **[API Specification](docs/fluent-ui-scaffold-spec.md)**: Comprehensive API documentation
- **[Developer Quick Start](docs/roadmap/developer-quick-start.md)**: Getting started guide
- **[Roadmap](docs/roadmap/README.md)**: Implementation roadmap and story tracking
- **[Sample Application](samples/README.md)**: Complete example with Svelte app and tests

## 🎯 Sample Application

The `samples/` directory contains a comprehensive example application that demonstrates:

- **Modern Web Application**: ASP.NET Core backend with Svelte frontend
- **Complex UI Components**: Todo list, user profile forms, interactive elements
- **Comprehensive Testing**: Page objects, fluent API usage, various testing scenarios
- **Framework Features**: Element configuration, wait strategies, navigation, verification

### Running the Sample

```bash
# Navigate to sample app
cd samples/SampleApp

# Install dependencies
cd ClientApp && npm install && cd ..

# Run the application
dotnet run

# Run tests (in another terminal)
cd ../SampleApp.Tests
dotnet test
```

## 🔧 Configuration

### Basic Configuration

```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("https://your-app.com"),
    DefaultTimeout = TimeSpan.FromSeconds(30),
    DefaultRetryInterval = TimeSpan.FromMilliseconds(500),
    WaitStrategy = WaitStrategy.Smart,
    LogLevel = LogLevel.Information,
    CaptureScreenshotsOnFailure = true
};
```

### Page Object Pattern

```csharp
public class HomePage : BasePageComponent<WebApp>
{
    private IElement _button;

    protected override void ConfigureElements()
    {
        _button = Element("[data-testid='my-button']")
            .WithDescription("My Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
    }

    public HomePage ClickButton()
    {
        _button.Click();
        return this;
    }
}
```

## 🧪 Testing

### Writing Tests

```csharp
[TestMethod]
public async Task Can_Interact_With_Button()
{
    // Arrange
    var homePage = _fluentUI
        .NavigateToUrl(new Uri("https://your-app.com"))
        .Framework<HomePage>();

    // Act
    homePage.ClickButton();

    // Assert
    homePage.VerifyElementIsVisible();
}
```

### Wait Strategies

- `None`: No waiting
- `Visible`: Wait for element to be visible
- `Hidden`: Wait for element to be hidden
- `Clickable`: Wait for element to be clickable
- `Enabled`: Wait for element to be enabled
- `Disabled`: Wait for element to be disabled
- `TextPresent`: Wait for specific text to be present
- `Smart`: Framework-specific intelligent waiting

## 🚧 Development Status

### Phase 1: Foundation & Core Architecture (MVP) - 67% Complete

- ✅ Project Structure Setup
- ✅ Core Interfaces & Abstractions
- ✅ Fluent Entry Point
- ✅ Element Configuration System
- ✅ Playwright Plugin Implementation
- 🔄 Base Page Component Implementation
- ⏳ Page Navigation and Validation

### Phase 2: Advanced Features & Verification - 0% Complete

- ⏳ Verification System
- ⏳ Advanced Wait Strategies
- ⏳ Error Handling and Debugging
- ⏳ Logging Integration

### Phase 3: Documentation & Examples - 0% Complete

- ⏳ API Documentation
- ⏳ Tutorials and Best Practices
- ⏳ Sample Applications and Integration Tests

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 6+ SDK
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the solution
5. Run `dotnet test` to run tests

### Story Tracking

See [Story Tracking](docs/roadmap/story-tracking.md) for current development status and available stories to work on.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 🙏 Acknowledgments

- Inspired by [fluent-test-scaffold](https://github.com/rburnham52/fluent-test-scaffold)
- Built with [Playwright](https://playwright.dev/) for reliable browser automation
- Uses [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) for comprehensive logging

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/your-org/fluent-ui-scaffold/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-org/fluent-ui-scaffold/discussions)
- **Documentation**: [API Specification](docs/fluent-ui-scaffold-spec.md)

---

**FluentUIScaffold** - Making E2E testing fluent and maintainable. 🚀 