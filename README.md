# FluentUIScaffold E2E Testing Framework

[![.NET](https://github.com/your-org/fluent-ui-scaffold/actions/workflows/dotnet.yml/badge.svg)](https://github.com/your-org/fluent-ui-scaffold/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/FluentUIScaffold.Core.svg)](https://www.nuget.org/packages/FluentUIScaffold.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. FluentUIScaffold abstracts underlying testing frameworks (Playwright) while providing a consistent developer experience.

## Features

- **Framework Agnostic**: Abstract underlying testing frameworks (Playwright) while providing consistent developer experience
- **Fluent API**: Intuitive, chainable API similar to [fluent-test-scaffold](https://github.com/rburnham52/fluent-test-scaffold)
- **Multi-Target Support**: Support .NET 6, 7, 8, 9
- **Page Object Pattern**: Comprehensive `Page<TSelf>` implementation with navigation and verification
- **Element Configuration**: Flexible element definition with wait strategies and timeouts
- **Plugin System**: Extensible plugin architecture for different testing frameworks
- **Hosting Strategies**: Pluggable hosting for .NET, Node, External, and Aspire apps
- **Async-First Design**: Modern async lifecycle with `AppScaffold<TApp>`
- **Comprehensive Testing**: TDD approach with comprehensive unit tests for all public APIs

## Quick Start

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
using FluentUIScaffold.Playwright;

// Set up assembly-level initialization
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
                opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
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

// Write your tests
[TestMethod]
public void Can_Navigate_And_Interact()
{
    var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();

    homePage
        .WaitForVisible(p => p.CounterButton)
        .Click(p => p.CounterButton)
        .Verify.TextContains(p => p.CounterValue, "1");
}
```

## Documentation

- **[API Reference](docs/api-reference.md)**: Complete API documentation
- **[Getting Started](docs/getting-started.md)**: Step-by-step setup guide
- **[Page Object Pattern](docs/page-object-pattern.md)**: How to create page objects
- **[Element Configuration](docs/element-configuration.md)**: Element setup and wait strategies
- **[Playwright Integration](docs/playwright-integration.md)**: Playwright-specific features
- **[Sample Application](samples/README.md)**: Complete example with tests
- **[Testing Strategy](docs/testing-strategy.md)**: Guidelines and best practices for writing tests

## Sample Application

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

## Architecture

### Core Components

- **AppScaffold<TApp>**: Unified async-first application orchestrator
- **FluentUIScaffoldBuilder**: Fluent configuration builder
- **Page<TSelf>**: Base class for all page objects
- **IElement**: Interface for element interactions
- **ElementBuilder**: Fluent API for element configuration
- **IHostingStrategy**: Pluggable hosting abstraction

### Hosting Strategies

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `DotNetHostingStrategy` | Manages .NET app via `dotnet run` | Standard .NET apps |
| `NodeHostingStrategy` | Manages Node.js app via `npm run` | Node.js/SPA apps |
| `ExternalHostingStrategy` | Health check only | CI/staging environments |
| `AspireHostingStrategy` | Wraps Aspire testing builder | Aspire distributed apps |

### Framework Support

- **Playwright**: Full support with advanced features
- **Selenium**: Planned for future releases
- **Mobile**: Planned for future releases

## Configuration

### Basic Configuration

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
        opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
        opts.HeadlessMode = true;
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

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

### Page Object Pattern

```csharp
public class HomePage : Page<HomePage>
{
    public IElement Button { get; private set; } = null!;

    public HomePage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        Button = Element("[data-testid='my-button']")
            .WithDescription("My Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();
    }

    public HomePage ClickButton()
    {
        return Click(p => p.Button);
    }
}
```

## Testing

### Writing Tests

```csharp
[TestMethod]
public void Can_Interact_With_Button()
{
    // Arrange
    var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();

    // Act
    homePage.ClickButton();

    // Assert
    homePage.Verify.Visible(p => p.Button);
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

## Development Status

### Phase 1: Foundation & Core Architecture (MVP) - 100% Complete

- Project Structure Setup
- Core Interfaces & Abstractions
- Fluent Entry Point
- Element Configuration System
- Playwright Plugin Implementation
- Base Page Component Implementation
- Page Navigation and Validation
- Sample App Integration

### Phase 2: Advanced Features & Verification - 100% Complete

- Verification System
- Advanced Wait Strategies
- Error Handling and Debugging
- Logging Integration

### Phase 3: API Unification - 100% Complete

- IHostingStrategy abstraction
- Unified AppScaffold<TApp>
- Simplified Page<TSelf> base class
- Async-first design

See [Roadmap](docs/roadmap/README.md) for current development plans and story tracking.

## Project Structure

- **src\FluentUIScaffold.Core**: Core framework-independent library
- **src\FluentUIScaffold.Playwright**: Playwright plugin
- **src\FluentUIScaffold.AspireHosting**: Aspire hosting extensions
- **samples\SampleApp**: Sample ASP.NET Core app with Svelte frontend
- **samples\SampleApp.AppHost**: Aspire AppHost for distributed testing
- **samples\SampleApp.AspireTests**: Sample Aspire-hosted tests
- **samples\SampleApp.Tests**: Sample standard .NET tests
- **tests\FluentUIScaffold.Core.Tests**: Core framework tests
- **tests\FluentUIScaffold.Playwright.Tests**: Playwright plugin tests

## Contributing

We welcome contributions! Please see our [CONTRIBUTING.md](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 6+ SDK
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the solution
5. Run `dotnet test` to run tests

### Story Tracking

See [Story Tracking](docs/roadmap/story-tracking.md) for current development status and available stories to work on.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Acknowledgments

- Inspired by [fluent-test-scaffold](https://github.com/rburnham52/fluent-test-scaffold)
- Built with [Playwright](https://playwright.dev/) for reliable browser automation
- Uses Microsoft.Extensions.Logging for comprehensive logging

## Support

- Issues: GitHub Issues
- Discussions: GitHub Discussions
- Documentation: API Reference

---

**FluentUIScaffold** - Making E2E testing fluent and maintainable.
