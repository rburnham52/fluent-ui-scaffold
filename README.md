# FluentUIScaffold E2E Testing Framework

[![.NET](https://github.com/your-org/fluent-ui-scaffold/actions/workflows/dotnet.yml/badge.svg)](https://github.com/your-org/fluent-ui-scaffold/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/FluentUIScaffold.Core.svg)](https://www.nuget.org/packages/FluentUIScaffold.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. FluentUIScaffold abstracts underlying testing frameworks (Playwright, Selenium) while providing a consistent developer experience.

## ✨ Features

- **Framework Agnostic**: Abstract underlying testing frameworks (Playwright, Selenium) while providing consistent developer experience
- **Fluent API**: Intuitive, chainable API similar to [fluent-test-scaffold](https://github.com/rburnham52/fluent-test-scaffold)
- **Multi-Target Support**: Support .NET 6, 7, 8, 9
- **Page Object Pattern**: Comprehensive page object implementation with navigation and validation
- **Element Configuration**: Flexible element definition with wait strategies and timeouts
- **Plugin System**: Extensible plugin architecture for different testing frameworks
- **Comprehensive Testing**: TDD approach with comprehensive unit tests for all public APIs

## 🚀 Quick Start

### Prerequisites

- .NET 6.0 or later
- Visual Studio 2022 or VS Code
- Git

### Installation

```bash
# Clone the repository
git clone https://github.com/your-org/fluent-ui-scaffold.git
cd fluent-ui-scaffold

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Basic Usage

```csharp
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

// Configure FluentUIScaffold with auto-discovery
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("https://your-app.com"),
    DefaultWaitTimeout = TimeSpan.FromSeconds(30),
    EnableDebugMode = false
};

var fluentUI = new FluentUIScaffoldApp<WebApp>(options);

// Navigate and interact
fluentUI.NavigateTo<HomePage>()
    .WaitFor(e => e.CounterButton)
    .ClickCounter()
    .VerifyText(e => e.CounterValue, "1");
```

## 📚 Documentation

- **[API Reference](docs/api-reference.md)**: Complete API documentation
- **[Getting Started](docs/getting-started.md)**: Step-by-step setup guide
- **[Page Object Pattern](docs/page-object-pattern.md)**: How to create page objects
- **[Element Configuration](docs/element-configuration.md)**: Element setup and wait strategies
- **[Playwright Integration](docs/playwright-integration.md)**: Playwright-specific features
- **[Sample Application](docs/sample-application.md)**: Complete example with tests
- **[Configuration Guide](docs/configuration.md)**: Framework configuration options
- **[Testing Best Practices](docs/testing-best-practices.md)**: Guidelines for writing tests

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

## 🏗️ Architecture

### Core Components

- **FluentUIScaffoldApp<TApp>**: Main entry point for the testing framework
- **BasePageComponent<TApp>**: Base class for all page objects
- **IElement**: Interface for element interactions
- **ElementBuilder**: Fluent API for element configuration
- **Plugin System**: Extensible architecture for different testing frameworks

### Framework Support

- **Playwright**: ✅ Full support with advanced features
- **Selenium**: 🔄 Planned for future releases
- **Mobile**: 🔄 Planned for future releases

## 🔧 Configuration

### Basic Configuration

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("https://your-app.com"))
    .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
    .WithHeadlessMode(true)  // Explicit headless control
    .WithDebugMode(false)    // Debug mode with automatic headless/SlowMo
    .Build();
```

### Server Configuration

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("https://localhost:5001"))
    .WithServerConfiguration(ServerConfiguration.CreateAspNetCore(
        new Uri("https://localhost:5001"), 
        "./path/to/your/project.csproj")
        .WithAspNetCoreEnvironment("Development")
        .Build())
    .Build();
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

## 📊 Development Status

### Phase 1: Foundation & Core Architecture (MVP) - 100% Complete ✅

- ✅ **Project Structure Setup** - Complete
- ✅ **Core Interfaces & Abstractions** - Complete
- ✅ **Fluent Entry Point** - Complete
- ✅ **Element Configuration System** - Complete
- ✅ **Playwright Plugin Implementation** - Complete
- ✅ **Base Page Component Implementation** - Complete
- ✅ **Page Navigation and Validation** - Complete
- ✅ **Sample App Integration** - Complete

### Phase 2: Advanced Features & Verification - 100% Complete ✅

- ✅ **Verification System** - Complete
- ✅ **Advanced Wait Strategies** - Complete
- ✅ **Error Handling and Debugging** - Complete
- ✅ **Logging Integration** - Complete

### Phase 3: Documentation & Examples - 33% Complete

- ✅ **API Documentation** - Complete
- ✅ **Tutorials and Best Practices** - Complete
- ✅ **Sample Applications and Integration Tests** - Complete (Example 1)

### Example Implementations

#### ✅ Example 1: User Registration and Login Flow - COMPLETED
- **Status**: 100% Complete (6/6 stories)
- **Milestones**: All 3 milestones completed
- **Tests**: 43 passing tests
- **Features**: Navigation, form interactions, verification, comprehensive testing

#### 🔄 Example 2: Shopping Cart with Dynamic Pricing - NOT STARTED
- **Status**: 0% Complete (0/4 stories)
- **Next**: Advanced verification patterns, state management

#### 🔄 Example 3: Dashboard with Data Tables - NOT STARTED
- **Status**: 0% Complete (0/3 stories)
- **Next**: Data table interactions, advanced interactions

### Overall Progress: 46.2% Complete (6/13 stories)

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
- **Documentation**: [API Reference](docs/api-reference.md)

---

**FluentUIScaffold** - Making E2E testing fluent and maintainable. 🚀 