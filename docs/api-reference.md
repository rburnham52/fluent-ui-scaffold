# FluentUIScaffold API Reference

## Overview

This document provides a comprehensive reference for the FluentUIScaffold API, including all implemented features and their usage.

## Table of Contents

- [Core Framework](#core-framework)
- [Configuration](#configuration)
- [Element System](#element-system)
- [Page Object Pattern](#page-object-pattern)
- [Hosting Strategies](#hosting-strategies)
- [Playwright Integration](#playwright-integration)
- [Plugin System](#plugin-system)
- [Exceptions](#exceptions)

## Core Framework

### FluentUIScaffoldBuilder

The main entry point for creating FluentUIScaffold instances. Instance-based builder with fluent API.

```csharp
public class FluentUIScaffoldBuilder
{
    public FluentUIScaffoldBuilder UsePlugin(IUITestingFrameworkPlugin plugin);
    public FluentUIScaffoldBuilder UsePlaywright();
    public FluentUIScaffoldBuilder UseAspireHosting<TEntryPoint>(Action<IDistributedApplicationTestingBuilder> configure, string? resourceName = null);
    public FluentUIScaffoldBuilder Web<TApp>(Action<FluentUIScaffoldOptions>? configure = null);
    public FluentUIScaffoldBuilder WithAutoPageDiscovery();
    public FluentUIScaffoldBuilder RegisterPage<TPage>() where TPage : class;
    public AppScaffold<TApp> Build<TApp>() where TApp : class;
}
```

#### Usage

```csharp
// Web application with Playwright
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://your-app.com");
        opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();

// Aspire-hosted application
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.MyAppHost>(
        appHost => { /* configure */ },
        "myapp")
    .Web<WebApp>(opts => { opts.UsePlaywright(); })
    .Build<WebApp>();

await app.StartAsync();
```

### AppScaffold<TWebApp>

The unified async-first application orchestrator for UI testing.

```csharp
public class AppScaffold<TWebApp> : IAsyncDisposable
{
    public IServiceProvider ServiceProvider { get; }

    // Lifecycle
    public Task StartAsync();
    public ValueTask DisposeAsync();

    // Service Resolution
    public T GetService<T>() where T : notnull;
    public TResult Framework<TResult>() where TResult : notnull;

    // Page Navigation
    public TPage NavigateTo<TPage>() where TPage : class;
    public TPage On<TPage>(bool validate = false) where TPage : class;
    public AppScaffold<TWebApp> WaitFor<TPage>() where TPage : class;
    public AppScaffold<TWebApp> WaitFor<TPage>(Func<TPage, IElement> elementSelector) where TPage : class;

    // URL Navigation
    public AppScaffold<TWebApp> NavigateToUrl(Uri url);
    public AppScaffold<TWebApp> WithBaseUrl(Uri baseUrl);
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `StartAsync` | Starts configured background services/hosts | None | `Task` |
| `DisposeAsync` | Disposes the application and resources | None | `ValueTask` |
| `GetService<T>` | Resolves a service from the DI container | `T` type parameter | `T` |
| `Framework<TResult>` | Gets framework-specific tools (e.g., IPage) | `TResult` type parameter | `TResult` |
| `NavigateTo<TPage>` | Navigates to a page and returns it | `TPage` type parameter | `TPage` |
| `On<TPage>` | Gets current page without navigating | `TPage`, `bool validate` | `TPage` |
| `WaitFor<TPage>` | Waits for a page to be available | `TPage` type parameter | `AppScaffold<TWebApp>` |
| `NavigateToUrl` | Navigates to a specific URL | `Uri url` | `AppScaffold<TWebApp>` |
| `WithBaseUrl` | Sets the base URL | `Uri baseUrl` | `AppScaffold<TWebApp>` |

#### Usage

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts => opts.BaseUrl = new Uri("https://your-app.com"))
    .Build<WebApp>();

await app.StartAsync();

// Navigate to page
var homePage = app.NavigateTo<HomePage>();

// Get current page
var currentPage = app.On<LoginPage>();

// Wait for page
app.WaitFor<DashboardPage>();

// Get framework driver
var playwrightDriver = app.Framework<PlaywrightDriver>();

await app.DisposeAsync();
```

## Configuration

### FluentUIScaffoldOptions

Configuration options for the FluentUIScaffold framework.

```csharp
public class FluentUIScaffoldOptions
{
    public Uri? BaseUrl { get; set; }
    public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool? HeadlessMode { get; set; } = null;  // null = automatic determination
    public int? SlowMo { get; set; } = null;         // null = automatic determination
    public Type? RequestedDriverType { get; set; }
}
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `Uri?` | `null` | Base URL for the application |
| `DefaultWaitTimeout` | `TimeSpan` | `30s` | Default timeout for element operations |
| `RequestedDriverType` | `Type?` | `null` | Preferred driver type for plugin selection |
| `HeadlessMode` | `bool?` | `null` | Explicit headless control (null = automatic) |
| `SlowMo` | `int?` | `null` | Explicit SlowMo control in milliseconds (null = automatic) |

### FluentUIScaffoldOptionsBuilder

Builder for creating and configuring FluentUIScaffoldOptions instances with a fluent API.

```csharp
public class FluentUIScaffoldOptionsBuilder
{
    public FluentUIScaffoldOptionsBuilder WithBaseUrl(Uri baseUrl);
    public FluentUIScaffoldOptionsBuilder WithDefaultWaitTimeout(TimeSpan timeout);
    public FluentUIScaffoldOptionsBuilder WithHeadlessMode(bool? headless);
    public FluentUIScaffoldOptionsBuilder WithSlowMo(int? slowMo);
    public FluentUIScaffoldOptionsBuilder WithDriver<TDriver>() where TDriver : class;
    public FluentUIScaffoldOptions Build();
}
```

### WaitStrategy

Enumeration of available wait strategies.

```csharp
public enum WaitStrategy
{
    None,
    Visible,
    Hidden,
    Clickable,
    Enabled,
    Disabled,
    TextPresent,
    Smart
}
```

#### Values

| Value | Description |
|-------|-------------|
| `None` | No waiting |
| `Visible` | Wait for element to be visible |
| `Hidden` | Wait for element to be hidden |
| `Clickable` | Wait for element to be clickable |
| `Enabled` | Wait for element to be enabled |
| `Disabled` | Wait for element to be disabled |
| `TextPresent` | Wait for specific text to be present |
| `Smart` | Framework-specific intelligent waiting |

## Element System

### IElement

Interface for element interactions.

```csharp
public interface IElement
{
    string Selector { get; }
    string Description { get; }
    TimeSpan Timeout { get; }
    WaitStrategy WaitStrategy { get; }

    void Click();
    void Type(string text);
    void SelectOption(string value);
    string GetText();
    string GetValue();
    string GetAttribute(string attributeName);
    bool IsVisible();
    bool IsEnabled();
    void WaitForVisible();
    void Clear();
    void Focus();
    void Hover();
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `Click` | Clicks the element | None | `void` |
| `Type` | Types text into the element | `string text` | `void` |
| `SelectOption` | Selects a value from dropdown | `string value` | `void` |
| `GetText` | Gets the text content | None | `string` |
| `GetValue` | Gets the element value | None | `string` |
| `GetAttribute` | Gets an attribute value | `string attributeName` | `string` |
| `IsVisible` | Checks if visible | None | `bool` |
| `IsEnabled` | Checks if enabled | None | `bool` |
| `WaitForVisible` | Waits for visibility | None | `void` |
| `Clear` | Clears the element | None | `void` |
| `Focus` | Focuses on element | None | `void` |
| `Hover` | Hovers over element | None | `void` |

### ElementBuilder

Fluent API for element configuration.

```csharp
public class ElementBuilder
{
    public ElementBuilder WithDescription(string description);
    public ElementBuilder WithTimeout(TimeSpan timeout);
    public ElementBuilder WithWaitStrategy(WaitStrategy strategy);
    public ElementBuilder WithRetryInterval(TimeSpan interval);
    public IElement Build();
}
```

#### Usage

```csharp
var button = Element("#submit-button")
    .WithDescription("Submit Button")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithWaitStrategy(WaitStrategy.Clickable)
    .Build();
```

## Page Object Pattern

### Page<TSelf>

Base class for all page objects with single self-referencing generic for fluent API.

```csharp
public abstract class Page<TSelf> : IAsyncDisposable
    where TSelf : Page<TSelf>
{
    // Properties
    public IServiceProvider ServiceProvider { get; }
    public IUIDriver Driver { get; }
    public Uri UrlPattern { get; }
    protected ILogger Logger { get; }
    protected FluentUIScaffoldOptions Options { get; }

    // Constructor
    protected Page(IServiceProvider serviceProvider, Uri urlPattern);

    // Element Building
    protected ElementBuilder Element(string selector);

    // Abstract Configuration
    protected abstract void ConfigureElements();

    // Navigation
    public virtual TSelf Navigate();
    public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;

    // Fluent Interactions (all return TSelf)
    public virtual TSelf Click(Func<TSelf, IElement> elementSelector);
    public virtual TSelf Type(Func<TSelf, IElement> elementSelector, string text);
    public virtual TSelf Select(Func<TSelf, IElement> elementSelector, string value);
    public virtual TSelf Clear(Func<TSelf, IElement> elementSelector);
    public virtual TSelf Focus(Func<TSelf, IElement> elementSelector);
    public virtual TSelf Hover(Func<TSelf, IElement> elementSelector);
    public virtual TSelf WaitForVisible(Func<TSelf, IElement> elementSelector);
    public virtual TSelf WaitForHidden(Func<TSelf, IElement> elementSelector);

    // Verification
    public IVerificationContext<TSelf> Verify { get; }

    // Page Validation
    public virtual bool IsCurrentPage();
    public virtual void ValidateCurrentPage();
}
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ServiceProvider` | `IServiceProvider` | Dependency injection container |
| `Driver` | `IUIDriver` | The UI driver instance |
| `UrlPattern` | `Uri` | URL pattern for page validation |
| `Logger` | `ILogger` | Logger instance |
| `Options` | `FluentUIScaffoldOptions` | Framework options |
| `Verify` | `IVerificationContext<TSelf>` | Verification context |

#### Fluent Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `Click` | Clicks an element | `Func<TSelf, IElement>` | `TSelf` |
| `Type` | Types text into element | `Func<TSelf, IElement>`, `string` | `TSelf` |
| `Select` | Selects a dropdown value | `Func<TSelf, IElement>`, `string` | `TSelf` |
| `Clear` | Clears an element | `Func<TSelf, IElement>` | `TSelf` |
| `Focus` | Focuses on element | `Func<TSelf, IElement>` | `TSelf` |
| `Hover` | Hovers over element | `Func<TSelf, IElement>` | `TSelf` |
| `WaitForVisible` | Waits for visibility | `Func<TSelf, IElement>` | `TSelf` |
| `WaitForHidden` | Waits for hidden | `Func<TSelf, IElement>` | `TSelf` |
| `Navigate` | Navigates to this page | None | `TSelf` |
| `NavigateTo<TTarget>` | Navigates to another page | `TTarget` type | `TTarget` |

#### Usage

```csharp
public class HomePage : Page<HomePage>
{
    public IElement SubmitButton { get; private set; } = null!;
    public IElement SearchInput { get; private set; } = null!;

    public HomePage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        SubmitButton = Element("#submit-button")
            .WithDescription("Submit Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();

        SearchInput = Element("#search-input")
            .WithDescription("Search Input")
            .WithTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }

    public HomePage ClickSubmit()
    {
        return Click(p => p.SubmitButton);
    }

    public HomePage EnterSearchText(string text)
    {
        return Type(p => p.SearchInput, text);
    }

    public HomePage VerifyWelcomeMessage(string expectedText)
    {
        Verify.TextContains(p => p.WelcomeMessage, expectedText);
        return this;
    }
}
```

## Verification Context

### IVerificationContext<TPage>

Interface for verification operations with fluent chaining.

```csharp
public interface IVerificationContext<TPage>
{
    TPage And { get; }

    IVerificationContext<TPage> Visible(Func<TPage, IElement> elementSelector);
    IVerificationContext<TPage> NotVisible(Func<TPage, IElement> elementSelector);
    IVerificationContext<TPage> Enabled(Func<TPage, IElement> elementSelector);
    IVerificationContext<TPage> Disabled(Func<TPage, IElement> elementSelector);
    IVerificationContext<TPage> TextContains(Func<TPage, IElement> elementSelector, string text);
    IVerificationContext<TPage> TextIs(Func<TPage, IElement> elementSelector, string text);
    IVerificationContext<TPage> ValueIs(Func<TPage, IElement> elementSelector, string value);

    IVerificationContext<TPage> UrlContains(string text);
    IVerificationContext<TPage> UrlIs(string url);
    IVerificationContext<TPage> TitleContains(string text);
    IVerificationContext<TPage> TitleIs(string title);

    IVerificationContext<TPage> That(Func<bool> condition, string description);
    IVerificationContext<TPage> That<T>(Func<T> actual, Func<T, bool> condition, string description);
}
```

#### Usage

```csharp
// Chain verifications
page.Verify
    .TitleContains("Dashboard")
    .UrlContains("/dashboard")
    .Visible(p => p.WelcomeMessage)
    .TextContains(p => p.UserGreeting, "Hello")
    .And  // Returns to page for continued interaction
    .Click(p => p.LogoutButton);

// Custom verification
page.Verify.That(
    () => page.GetItemCount(),
    count => count > 0,
    "Should have at least one item"
);
```

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

public record HostingResult(Uri BaseUrl, bool WasReused);
public record HostingStatus(bool IsRunning, Uri? BaseUrl, int? ProcessId);
```

### Available Strategies

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `DotNetHostingStrategy` | Manages .NET app via `dotnet run` | Standard .NET apps |
| `NodeHostingStrategy` | Manages Node.js app via `npm run` | Node.js/SPA apps |
| `ExternalHostingStrategy` | Health check only, no process management | CI/staging environments |
| `AspireHostingStrategy` | Wraps `DistributedApplicationTestingBuilder` | Aspire distributed apps |

## Playwright Integration

### PlaywrightPlugin

Plugin for Playwright integration.

```csharp
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Playwright";
    public string Version => "1.0.0";
    public IReadOnlyList<Type> SupportedDriverTypes { get; }

    public bool CanHandle(Type driverType);
    public IUIDriver CreateDriver(FluentUIScaffoldOptions options);
    public void ConfigureServices(IServiceCollection services);
}
```

### PlaywrightDriver

Playwright-specific driver implementation.

```csharp
public class PlaywrightDriver : IUIDriver
{
    public Uri CurrentUrl { get; }

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
    public void NavigateToUrl(Uri url);
    public TTarget NavigateTo<TTarget>() where TTarget : class;
    public TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
    public void Focus(string selector);
    public void Hover(string selector);
    public void Clear(string selector);
    public string GetPageTitle();

    // Browser interaction (async)
    public Task<T> ExecuteScriptAsync<T>(string script);
    public Task ExecuteScriptAsync(string script);
    public Task<byte[]> TakeScreenshotAsync(string filePath);

    public void Dispose();
}
```

#### Browser Interaction Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `ExecuteScriptAsync<T>` | Executes JavaScript and returns a typed result | `string script` | `Task<T>` |
| `ExecuteScriptAsync` | Executes JavaScript with no return value | `string script` | `Task` |
| `TakeScreenshotAsync` | Saves a screenshot to a file and returns the bytes | `string filePath` | `Task<byte[]>` |

#### Usage

```csharp
var driver = app.GetService<IUIDriver>();

// Execute JavaScript
await driver.ExecuteScriptAsync("localStorage.clear()");
var href = await driver.ExecuteScriptAsync<string>("window.location.href");
var count = await driver.ExecuteScriptAsync<int>("document.querySelectorAll('h1').length");

// Take screenshot
var bytes = await driver.TakeScreenshotAsync("debug-screenshot.png");
```

## Plugin System

### IUITestingFrameworkPlugin

Interface for testing framework plugins.

```csharp
public interface IUITestingFrameworkPlugin
{
    string Name { get; }
    string Version { get; }
    IReadOnlyList<Type> SupportedDriverTypes { get; }
    bool CanHandle(Type driverType);
    IUIDriver CreateDriver(FluentUIScaffoldOptions options);
    void ConfigureServices(IServiceCollection services);
}
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | Plugin name |
| `Version` | `string` | Plugin version |
| `SupportedDriverTypes` | `IReadOnlyList<Type>` | Supported driver types |

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `CanHandle` | Checks if plugin can handle driver type | `Type driverType` | `bool` |
| `CreateDriver` | Creates a driver instance | `FluentUIScaffoldOptions options` | `IUIDriver` |
| `ConfigureServices` | Configures services for the plugin | `IServiceCollection services` | `void` |

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

### FluentUIScaffoldValidationException

Exception for validation errors.

```csharp
public class FluentUIScaffoldValidationException : FluentUIScaffoldException
{
    public string PropertyName { get; }
}
```

### ElementTimeoutException

Exception for element timeout errors.

```csharp
public class ElementTimeoutException : FluentUIScaffoldException
{
    public string Selector { get; }
    public TimeSpan Timeout { get; }
    public WaitStrategy WaitStrategy { get; }
}
```

### VerificationException

Exception for verification failures.

```csharp
public class VerificationException : FluentUIScaffoldException
{
    public string Description { get; }
    public string? ExpectedValue { get; }
    public string? ActualValue { get; }
}
```
