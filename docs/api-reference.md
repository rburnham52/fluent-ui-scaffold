# FluentUIScaffold API Reference

## Overview

This document provides a comprehensive reference for the FluentUIScaffold API. The framework uses a deferred execution chain pattern where page actions are queued and executed sequentially when the chain is awaited.

## Table of Contents

- [Core Framework](#core-framework)
- [Configuration](#configuration)
- [Page Object Pattern](#page-object-pattern)
- [Plugin System](#plugin-system)
- [Browser Sessions](#browser-sessions)
- [Hosting Strategies](#hosting-strategies)
- [Exceptions](#exceptions)

## Core Framework

### FluentUIScaffoldBuilder

The main entry point for creating FluentUIScaffold instances. Instance-based builder with fluent API.

```csharp
public class FluentUIScaffoldBuilder
{
    // Plugin registration
    public FluentUIScaffoldBuilder UsePlugin(IUITestingPlugin plugin);

    // Web application configuration
    public FluentUIScaffoldBuilder Web<TWebApp>(Action<FluentUIScaffoldOptions> configureOptions);

    // Hosting strategies
    public FluentUIScaffoldBuilder UseDotNetHosting(Action<DotNetHostingOptions> configure);
    public FluentUIScaffoldBuilder UseNodeHosting(Action<NodeHostingOptions> configure);
    public FluentUIScaffoldBuilder UseExternalServer(Uri baseUrl, params string[] healthCheckEndpoints);

    // Environment configuration
    public FluentUIScaffoldBuilder WithEnvironmentName(string environmentName);
    public FluentUIScaffoldBuilder WithSpaProxy(bool enabled);
    public FluentUIScaffoldBuilder WithHeadlessMode(bool? headless);
    public FluentUIScaffoldBuilder WithEnvironmentVariable(string key, string value);

    // Service and startup configuration
    public FluentUIScaffoldBuilder ConfigureServices(Action<IServiceCollection> configure);
    public FluentUIScaffoldBuilder AddStartupAction(Func<IServiceProvider, Task> action);

    // Build
    public AppScaffold<TWebApp> Build<TWebApp>();
}
```

#### Usage

```csharp
// Web application with Playwright
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
    })
    .Build<WebApp>();

await app.StartAsync();

// Aspire-hosted application
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.MyAppHost>(
        appHost => { /* configure */ },
        "myapp")
    .UsePlaywright()
    .Build<WebApp>();

await app.StartAsync();
```

### AppScaffold\<TWebApp>

The central hub for test infrastructure. Manages hosting, plugin lifecycle, and per-test browser session creation.

```csharp
public class AppScaffold<TWebApp> : IAsyncDisposable
{
    public IServiceProvider ServiceProvider { get; }

    // Lifecycle
    public Task StartAsync(CancellationToken cancellationToken = default);
    public ValueTask DisposeAsync();

    // Session management (per-test)
    public Task<IBrowserSession> CreateSessionAsync();
    public Task DisposeSessionAsync();

    // Page navigation
    public TPage NavigateTo<TPage>() where TPage : Page<TPage>;
    public TPage NavigateTo<TPage>(object routeParams) where TPage : Page<TPage>;
    public TPage On<TPage>() where TPage : Page<TPage>;

    // Service resolution
    public T GetService<T>() where T : notnull;
}
```

#### Methods

| Method | Description | Returns |
|--------|-------------|---------|
| `StartAsync` | Starts hosting strategies and initializes the plugin (launches browser) | `Task` |
| `DisposeAsync` | Disposes the application, sessions, plugin, and hosting | `ValueTask` |
| `CreateSessionAsync` | Creates an isolated browser session for the current test | `Task<IBrowserSession>` |
| `DisposeSessionAsync` | Disposes the current test's browser session | `Task` |
| `NavigateTo<TPage>` | Creates a page and enqueues navigation to its route | `TPage` |
| `NavigateTo<TPage>(routeParams)` | Creates a page and enqueues navigation with route parameters | `TPage` |
| `On<TPage>` | Creates a page without navigating (for current page) | `TPage` |
| `GetService<T>` | Resolves a service from the root DI container | `T` |

#### Usage

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(opts => opts.BaseUrl = new Uri("https://your-app.com"))
    .Build<WebApp>();

await app.StartAsync();

// Per-test session lifecycle
await app.CreateSessionAsync();

// Navigate to page and interact (must be awaited)
await app.NavigateTo<HomePage>()
    .ClickLogin();

// Clean up session after test
await app.DisposeSessionAsync();

// Clean up app after all tests
await app.DisposeAsync();
```

## Configuration

### FluentUIScaffoldOptions

Configuration options for the FluentUIScaffold framework.

```csharp
public class FluentUIScaffoldOptions
{
    public Uri? BaseUrl { get; set; }
    public bool? HeadlessMode { get; set; } = null;
    public int? SlowMo { get; set; } = null;
    public string EnvironmentName { get; set; } = "Testing";
    public bool SpaProxyEnabled { get; set; } = false;
    public Dictionary<string, string> EnvironmentVariables { get; }
}
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `Uri?` | `null` | Base URL for the application under test |
| `HeadlessMode` | `bool?` | `null` | Explicit headless control (null = auto: visible when debugger attached, headless otherwise) |
| `SlowMo` | `int?` | `null` | Browser slow-motion delay in milliseconds (null = automatic) |
| `EnvironmentName` | `string` | `"Testing"` | Logical environment name passed to hosted apps |
| `SpaProxyEnabled` | `bool` | `false` | Whether to enable the ASP.NET SPA dev server proxy |
| `EnvironmentVariables` | `Dictionary<string, string>` | empty | Custom environment variables for hosted applications |

### RouteAttribute

Specifies the route path for a page. Combined with `BaseUrl` to form the full URL.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RouteAttribute : Attribute
{
    public string Path { get; }
    public RouteAttribute(string path);
}
```

#### Usage

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage> { /* ... */ }
// Full URL: BaseUrl + "/login"

[Route("/users/{userId}")]
public class UserPage : Page<UserPage> { /* ... */ }
// Navigate with parameters:
app.NavigateTo<UserPage>(new { userId = "123" });
// Full URL: BaseUrl + "/users/123"
```

## Page Object Pattern

### Page\<TSelf>

Base class for all page objects. Acts as a deferred execution chain builder with `GetAwaiter()` support. Page methods queue actions via `Enqueue` that execute sequentially when the chain is awaited.

```csharp
public abstract class Page<TSelf> where TSelf : Page<TSelf>
{
    // Constructor
    protected Page(IServiceProvider serviceProvider);

    // Properties
    protected IServiceProvider ServiceProvider { get; }
    protected TSelf Self { get; }

    // Deferred execution
    public TaskAwaiter GetAwaiter();

    // Action enqueueing
    protected TSelf Enqueue(Func<Task> action);
    protected TSelf Enqueue<T>(Func<T, Task> action) where T : notnull;

    // Cross-page navigation
    public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;
}
```

#### Key Concepts

**Deferred Execution**: Page methods do not execute immediately. Instead, they enqueue actions into an internal list. The entire chain executes when awaited.

**DI-Injected Lambdas**: `Enqueue<T>` resolves a service of type `T` from the session's service provider at execution time, not at enqueue time.

**Frozen Pages**: After calling `NavigateTo<TTarget>()`, the source page is frozen and cannot accept new actions. Use the returned target page to continue the chain.

#### Usage

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    public HomePage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public HomePage ClickLogin() => Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("[data-testid='login-button']");
    });

    public HomePage EnterSearch(string text) => Enqueue<IPage>(async page =>
    {
        await page.FillAsync("#search-input", text);
    });

    public LoginPage GoToLogin() => NavigateTo<LoginPage>();
}

// In tests (must await the chain):
await app.NavigateTo<HomePage>()
    .ClickLogin();

// Cross-page navigation:
await app.NavigateTo<HomePage>()
    .EnterSearch("test")
    .GoToLogin()
    .EnterUsername("admin")
    .SubmitForm();
```

## Plugin System

### IUITestingPlugin

Plugin contract for UI testing frameworks. Owns the browser singleton and creates per-test sessions.

```csharp
public interface IUITestingPlugin : IAsyncDisposable
{
    void ConfigureServices(IServiceCollection services);
    Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken cancellationToken = default);
    Task<IBrowserSession> CreateSessionAsync(IServiceProvider rootProvider);
}
```

#### Methods

| Method | Description |
|--------|-------------|
| `ConfigureServices` | Registers shared services into the DI container. Called once during builder configuration. |
| `InitializeAsync` | Initializes the plugin (e.g., launches browser). Called once during `AppScaffold.StartAsync()`. |
| `CreateSessionAsync` | Creates an isolated browser session for a single test. Each session owns its own browser context and page. |

### PlaywrightPlugin

Playwright implementation of `IUITestingPlugin`. Manages the Playwright browser singleton and creates per-test `PlaywrightBrowserSession` instances.

```csharp
public class PlaywrightPlugin : IUITestingPlugin
{
    public void ConfigureServices(IServiceCollection services);
    public Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken ct = default);
    public Task<IBrowserSession> CreateSessionAsync(IServiceProvider rootProvider);
    public ValueTask DisposeAsync();
}
```

#### Registration

```csharp
// Explicit plugin registration
var builder = new FluentUIScaffoldBuilder();
builder.UsePlugin(new PlaywrightPlugin());

// Convenience extension method
var builder = new FluentUIScaffoldBuilder();
builder.UsePlaywright();
```

## Browser Sessions

### IBrowserSession

Represents an isolated browser session for a single test. Each session owns its own browser context and page.

```csharp
public interface IBrowserSession : IAsyncDisposable
{
    Task NavigateToUrlAsync(Uri url);
    IServiceProvider ServiceProvider { get; }
}
```

The session's `ServiceProvider` is a `SessionServiceProvider` that resolves session-specific services first (e.g., `IPage`, `IBrowserContext`, `IBrowser`), then falls back to the root provider for shared services (e.g., `FluentUIScaffoldOptions`).

### PlaywrightBrowserSession

Playwright implementation of `IBrowserSession`. Owns an `IBrowserContext` and `IPage`.

```csharp
public class PlaywrightBrowserSession : IBrowserSession
{
    public IServiceProvider ServiceProvider { get; }
    public Task NavigateToUrlAsync(Uri url);
    public ValueTask DisposeAsync();
}
```

### SessionServiceProvider

Lightweight wrapper `IServiceProvider` that checks session-local services first, then falls back to the root provider. Available session services for Playwright:

| Service Type | Description |
|-------------|-------------|
| `IPage` | The Playwright page for this test session |
| `IBrowserContext` | The Playwright browser context for this test session |
| `IBrowser` | The shared Playwright browser instance |

## Hosting Strategies

### IHostingStrategy

Pluggable abstraction for managing application hosts.

```csharp
public interface IHostingStrategy : IAsyncDisposable
{
    string ConfigurationHash { get; }
    Uri? BaseUrl { get; }
    Task<HostingResult> StartAsync(ILogger logger, CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
    HostingStatus GetStatus();
}
```

### Available Strategies

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `DotNetHostingStrategy` | Manages .NET app via `dotnet run` | Standard .NET apps |
| `NodeHostingStrategy` | Manages Node.js app via `npm run` | Node.js/SPA apps |
| `ExternalHostingStrategy` | Health check only, no process management | CI/staging environments |
| `AspireHostingStrategy` | Wraps `DistributedApplicationTestingBuilder` | Aspire distributed apps |

## Exceptions

### FluentUIScaffoldException

Base exception for FluentUIScaffold.

```csharp
public class FluentUIScaffoldException : Exception
{
    public string? ScreenshotPath { get; set; }
    public string? DOMState { get; set; }
    public string? CurrentUrl { get; set; }
    public Dictionary<string, object>? Context { get; set; }
}
```

### FrozenPageException

Thrown when attempting to enqueue actions on a page that has been frozen by a `NavigateTo` call.

```csharp
public class FrozenPageException : InvalidOperationException
{
    public Type PageType { get; }
}
```

### FluentUIScaffoldValidationException

Exception for validation errors.

```csharp
public class FluentUIScaffoldValidationException : FluentUIScaffoldException
{
    public string PropertyName { get; }
}
```

### FluentUIScaffoldPluginException

Exception for plugin-related errors.

```csharp
public class FluentUIScaffoldPluginException : FluentUIScaffoldException { }
```
