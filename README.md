# FluentUIScaffold E2E Testing Framework

[![.NET](https://github.com/your-org/fluent-ui-scaffold/actions/workflows/dotnet.yml/badge.svg)](https://github.com/your-org/fluent-ui-scaffold/actions/workflows/dotnet.yml)
[![NuGet](https://img.shields.io/nuget/v/FluentUIScaffold.Core.svg)](https://www.nuget.org/packages/FluentUIScaffold.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.md)

A framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. FluentUIScaffold abstracts underlying testing frameworks (Playwright) while providing a consistent developer experience.

## Features

- **Fluent API**: Intuitive, chainable API with deferred execution and custom awaitables
- **Multi-Target Support**: Support .NET 6, 7, 8, 9
- **Page Object Pattern**: `Page<TSelf>` with deferred action chains and `GetAwaiter()` support
- **Plugin System**: Extensible plugin architecture via `IUITestingPlugin`
- **Hosting Strategies**: Pluggable hosting for .NET, Node, External, and Aspire apps
- **Async-First Design**: Modern async lifecycle with `AppScaffold<TApp>` and per-test browser sessions
- **DI-Powered Actions**: Queue DI-injected lambdas with `Enqueue<T>()` for direct framework access

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
public static class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .Web<WebApp>(opts => opts.BaseUrl = new Uri("https://your-app.com"))
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null) await _app.DisposeAsync();
    }

    public static AppScaffold<WebApp> App => _app!;
}

// Write your tests
[TestClass]
public class HomePageTests
{
    [TestInitialize]
    public async Task Setup() => await TestAssemblyHooks.App.CreateSessionAsync();

    [TestCleanup]
    public async Task Cleanup() => await TestAssemblyHooks.App.DisposeSessionAsync();

    [TestMethod]
    public async Task Can_Navigate_And_Interact()
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .VerifyWelcomeVisible()
            .ClickCounter();
    }
}
```

## Sample Application

The `samples/` directory contains a comprehensive example application that demonstrates:

- **Modern Web Application**: ASP.NET Core backend with Svelte frontend
- **Page Objects**: Deferred action chain pattern with `Enqueue<T>()`
- **Session Lifecycle**: Per-test browser session management
- **Aspire Integration**: Distributed application testing with Aspire

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

- **AppScaffold\<TApp\>**: Unified async-first application orchestrator with session management
- **FluentUIScaffoldBuilder**: Fluent configuration builder
- **Page\<TSelf\>**: Base class for page objects; builds deferred execution chains with `GetAwaiter()`
- **IUITestingPlugin**: Plugin interface with `CreateSessionAsync(IServiceProvider)`
- **IBrowserSession**: Per-test browser session created via `CreateSessionAsync()`/`DisposeSessionAsync()`
- **IHostingStrategy**: Pluggable hosting abstraction

### Hosting Strategies

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `DotNetHostingStrategy` | Manages .NET app via `dotnet run` | Standard .NET apps |
| `NodeHostingStrategy` | Manages Node.js app via `npm run` | Node.js/SPA apps |
| `ExternalHostingStrategy` | Health check only | CI/staging environments |
| `AspireHostingStrategy` | Wraps Aspire testing builder | Aspire distributed apps |

### Framework Support

- **Playwright**: Full support via `.UsePlaywright()` convenience method

## Configuration

### Basic Configuration

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
        opts.HeadlessMode = true;
    })
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

Pages are deferred execution chain builders. Actions are queued with `Enqueue<T>()` and executed when the chain is awaited.

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public HomePage ClickCounter()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
        });
    }

    public HomePage VerifyWelcomeVisible()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
        });
    }
}
```

### Parameterized Routes

```csharp
[Route("/users/{userId}")]
public class UserPage : Page<UserPage>
{
    protected UserPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    // ...
}

// Navigate with parameters:
await app.NavigateTo<UserPage>(new { userId = "123" });
```

## Testing

### Session Lifecycle

Each test gets its own browser session. Use `CreateSessionAsync()` in `TestInitialize` and `DisposeSessionAsync()` in `TestCleanup`:

```csharp
[TestClass]
public class MyTests
{
    [TestInitialize]
    public async Task Setup() => await TestAssemblyHooks.App.CreateSessionAsync();

    [TestCleanup]
    public async Task Cleanup() => await TestAssemblyHooks.App.DisposeSessionAsync();

    [TestMethod]
    public async Task MyTest()
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .VerifyWelcomeVisible()
            .ClickCounter();
    }
}
```

### Page Navigation

```csharp
// Navigate to a page (uses [Route] attribute for URL)
await app.NavigateTo<HomePage>()
    .ClickCounter();

// Get a page reference without navigating
await app.On<HomePage>()
    .VerifyWelcomeVisible();

// Navigate from one page to another (freezes current page, shares action list)
await app.NavigateTo<HomePage>()
    .ClickLoginLink()
    .NavigateTo<LoginPage>()
    .EnterUsername("testuser");
```

## Development Status

### Phase 1: Foundation & Core Architecture (MVP) - Complete

- Project Structure Setup
- Core Interfaces & Abstractions
- Fluent Entry Point
- Playwright Plugin Implementation
- Base Page Component Implementation
- Page Navigation
- Sample App Integration

### Phase 2: Advanced Features - Complete

- Hosting Strategies
- Error Handling and Debugging
- Logging Integration

### Phase 3: API Unification - Complete

- IHostingStrategy abstraction
- Unified AppScaffold\<TApp\>
- Simplified Page\<TSelf\> base class
- Async-first design

### Phase 4: Foundation Redesign - In Progress

- Deferred execution chain builder pattern for Page\<TSelf\>
- `Enqueue<T>()` with DI-injected lambdas
- Per-test browser sessions via `IBrowserSession`
- `IUITestingPlugin` with `CreateSessionAsync()`
- Custom awaitable (`GetAwaiter()`) on page chains

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
