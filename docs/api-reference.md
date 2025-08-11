# FluentUIScaffold API Reference

## Overview

This document provides a comprehensive reference for the FluentUIScaffold API, including all implemented features and their usage.

## Table of Contents

- [Core Framework](#core-framework)
- [Configuration](#configuration)
- [Element System](#element-system)
- [Page Object Pattern](#page-object-pattern)
- [Playwright Integration](#playwright-integration)
- [Plugin System](#plugin-system)
- [Exceptions](#exceptions)

## Core Framework

### FluentUIScaffoldBuilder

The main entry point for creating FluentUIScaffold instances.

```csharp
public static class FluentUIScaffoldBuilder
{
    public static FluentUIScaffoldApp<WebApp> Web(Action<FluentUIScaffoldOptions>? configureOptions = null)
    public static FluentUIScaffoldApp<MobileApp> Mobile(Action<FluentUIScaffoldOptions>? configureOptions = null)
}
```

#### Usage

```csharp
// Web application
var fluentUI = FluentUIScaffoldBuilder.Web(options =>
{
    options.BaseUrl = new Uri("https://your-app.com");
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
});

// Mobile application (future)
var mobileUI = FluentUIScaffoldBuilder.Mobile(options =>
{
    options.BaseUrl = new Uri("https://your-app.com");
});
```

### FluentUIScaffoldApp<TApp>

The main application class that provides the fluent API for UI testing.

```csharp
public class FluentUIScaffoldApp<TApp> : IDisposable where TApp : class
{
    public FluentUIScaffoldApp<TApp> WithBaseUrl(Uri baseUrl)
    public FluentUIScaffoldApp<TApp> NavigateToUrl(Uri url)
    public TDriver Framework<TDriver>() where TDriver : class
    public void Dispose()
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `WithBaseUrl` | Sets the base URL for the application | `Uri baseUrl` | `FluentUIScaffoldApp<TApp>` |
| `NavigateToUrl` | Navigates to a specific URL | `Uri url` | `FluentUIScaffoldApp<TApp>` |
| `Framework<TDriver>` | Gets a framework-specific driver instance | `TDriver` type parameter | `TDriver` |
| `Dispose` | Disposes the application and its resources | None | `void` |

#### Usage

```csharp
var fluentUI = FluentUIScaffoldBuilder.Web()
    .WithBaseUrl(new Uri("https://your-app.com"))
    .NavigateToUrl(new Uri("https://your-app.com/home"));

var playwrightDriver = fluentUI.Framework<PlaywrightDriver>();
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
```

#### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `Uri?` | `null` | Base URL for the application |
| `DefaultWaitTimeout` | `TimeSpan` | `30s` | Default timeout for element operations |
| `RequestedDriverType` | `Type?` | `null` | Preferred driver type for plugin selection |
| `HeadlessMode` | `bool?` | `null` | Explicit headless control (null = automatic) |
| `SlowMo` | `int?` | `null` | Explicit SlowMo control in milliseconds (null = automatic) |
|  |  |  |  |

### FluentUIScaffoldOptionsBuilder

Builder for creating and configuring FluentUIScaffoldOptions instances with a fluent API.

```csharp
public class FluentUIScaffoldOptionsBuilder
{
    public FluentUIScaffoldOptionsBuilder WithBaseUrl(Uri baseUrl)
    public FluentUIScaffoldOptionsBuilder WithDefaultWaitTimeout(TimeSpan timeout)
    public FluentUIScaffoldOptionsBuilder WithHeadlessMode(bool? headless)
    public FluentUIScaffoldOptionsBuilder WithSlowMo(int? slowMo)
    public FluentUIScaffoldOptionsBuilder WithDriver<TDriver>() where TDriver : class
    public FluentUIScaffoldOptions Build()
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `WithBaseUrl` | Sets the base URL for the application | `Uri baseUrl` | `FluentUIScaffoldOptionsBuilder` |
| `WithDefaultWaitTimeout` | Sets the default timeout for element operations | `TimeSpan timeout` | `FluentUIScaffoldOptionsBuilder` |
| `WithDriver<TDriver>` | Requests a specific driver type for plugin selection | `TDriver` type parameter | `FluentUIScaffoldOptionsBuilder` |
| `WithHeadlessMode` | Sets explicit headless control | `bool? headless` | `FluentUIScaffoldOptionsBuilder` |
| `WithSlowMo` | Sets explicit SlowMo control in milliseconds | `int? slowMo` | `FluentUIScaffoldOptionsBuilder` |
|  |  |  |  |
|  |  |  |  |
| `Build` | Builds and returns the configured options | None | `FluentUIScaffoldOptions` |

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

### ServerConfiguration

Configuration for web server launching.

```csharp
public class ServerConfiguration
{
    public ServerType ServerType { get; set; }
    public Uri BaseUrl { get; set; }
    public string ProjectPath { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; }
    public List<string> Arguments { get; set; }
    public List<string> HealthCheckEndpoints { get; set; }
    
    // Factory methods for creating pre-configured servers
    public static DotNetServerConfigurationBuilder CreateDotNetServer(Uri baseUrl, string projectPath)
    public static DotNetServerConfigurationBuilder CreateAspireServer(Uri baseUrl, string projectPath)
    public static NodeJsServerConfigurationBuilder CreateNodeJsServer(Uri baseUrl, string projectPath)
}
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `ServerType` | `ServerType` | Type of server to launch |
| `BaseUrl` | `Uri` | Base URL for the server |
| `ProjectPath` | `string` | Path to the project file |
| `EnvironmentVariables` | `Dictionary<string, string>` | Environment variables for the server |
| `Arguments` | `List<string>` | Command line arguments |
| `HealthCheckEndpoints` | `List<string>` | Endpoints to check for server readiness |

#### Factory Methods

| Method | Description | Parameters |
|--------|-------------|------------|
| `CreateDotNetServer` | Creates ASP.NET Core server configuration | `Uri baseUrl, string projectPath` |
| `CreateAspireServer` | Creates Aspire App Host configuration | `Uri baseUrl, string projectPath` |
| `CreateNodeJsServer` | Creates Node.js server configuration | `Uri baseUrl, string projectPath` |

### ServerType

Enumeration of supported server types.

```csharp
public enum ServerType
{
    AspNetCore,
    Aspire,
    NodeJs,
    WebApplicationFactory
}
```

#### Values

| Value | Description |
|-------|-------------|
| `AspNetCore` | ASP.NET Core applications |
| `Aspire` | .NET Aspire App Host applications |
| `NodeJs` | Node.js applications |
| `WebApplicationFactory` | ASP.NET Core apps hosted in-process via WebApplicationFactory (falls back to ASP.NET launcher by default) |

### DotNetServerConfigurationBuilder

Builder for .NET server configurations.

```csharp
public class DotNetServerConfigurationBuilder : ServerConfigurationBuilder
{
    public DotNetServerConfigurationBuilder WithAspNetCoreEnvironment(string environment)
    public DotNetServerConfigurationBuilder WithDotNetEnvironment(string environment)
    public DotNetServerConfigurationBuilder WithAspireDashboardOtlpEndpoint(string endpoint)
    public DotNetServerConfigurationBuilder WithAspireResourceServiceEndpoint(string endpoint)
    public DotNetServerConfigurationBuilder WithAspNetCoreHostingStartupAssemblies(string assemblies)
    public DotNetServerConfigurationBuilder WithAspNetCoreUrls(string urls)
    public DotNetServerConfigurationBuilder WithAspNetCoreForwardedHeaders(bool enabled)
}
```

### NodeJsServerConfigurationBuilder

Builder for Node.js server configurations.

```csharp
public class NodeJsServerConfigurationBuilder : ServerConfigurationBuilder
{
    public NodeJsServerConfigurationBuilder WithNpmScript(string script)
    public NodeJsServerConfigurationBuilder WithNodeEnvironment(string environment)
    public NodeJsServerConfigurationBuilder WithPort(int port)
}
```

### PageValidationStrategy

Enumeration of page validation strategies.

```csharp
public enum PageValidationStrategy
{
    None,
    Configurable,
    Strict
}
```

#### Values

| Value | Description |
|-------|-------------|
| `None` | No page validation |
| `Configurable` | Configurable page validation |
| `Strict` | Strict page validation |

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
    void Select(string value);
    string GetText();
    bool IsVisible();
    bool IsEnabled();
    void WaitFor();
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `Click` | Clicks the element | None | `void` |
| `Type` | Types text into the element | `string text` | `void` |
| `Select` | Selects a value from the element | `string value` | `void` |
| `GetText` | Gets the text content of the element | None | `string` |
| `IsVisible` | Checks if the element is visible | None | `bool` |
| `IsEnabled` | Checks if the element is enabled | None | `bool` |
| `WaitFor` | Waits for the element according to its wait strategy | None | `void` |

### ElementBuilder

Fluent API for element configuration.

```csharp
public class ElementBuilder : IElement
{
    public ElementBuilder WithDescription(string description)
    public ElementBuilder WithTimeout(TimeSpan timeout)
    public ElementBuilder WithWaitStrategy(WaitStrategy strategy)
    public ElementBuilder WithRetryInterval(TimeSpan interval)
    public IElement Build()
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `WithDescription` | Sets the element description | `string description` | `ElementBuilder` |
| `WithTimeout` | Sets the element timeout | `TimeSpan timeout` | `ElementBuilder` |
| `WithWaitStrategy` | Sets the wait strategy | `WaitStrategy strategy` | `ElementBuilder` |
| `WithRetryInterval` | Sets the retry interval | `TimeSpan interval` | `ElementBuilder` |
| `Build` | Builds the element | None | `IElement` |

#### Usage

```csharp
var button = Element("#submit-button")
    .WithDescription("Submit Button")
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithWaitStrategy(WaitStrategy.Clickable)
    .Build();
```

### ElementFactory

Factory for creating elements with caching.

```csharp
public class ElementFactory
{
    public IElement CreateElement(string selector, Action<ElementBuilder>? configure = null)
    public IElement GetOrCreateElement(string selector, Action<ElementBuilder>? configure = null)
    public void ClearCache()
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `CreateElement` | Creates a new element | `string selector`, `Action<ElementBuilder>?` | `IElement` |
| `GetOrCreateElement` | Gets or creates an element with caching | `string selector`, `Action<ElementBuilder>?` | `IElement` |
| `ClearCache` | Clears the element cache | None | `void` |

### ElementCollection

Collection of elements with filtering capabilities.

```csharp
public class ElementCollection : IElementCollection
{
    public IElement this[int index] { get; }
    public int Count { get; }
    public IElement First();
    public IElement Last();
    public IElementCollection Where(Func<IElement, bool> predicate);
    public IElementCollection WithText(string text);
    public IElementCollection WithAttribute(string attribute, string value);
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `First` | Gets the first element | None | `IElement` |
| `Last` | Gets the last element | None | `IElement` |
| `Where` | Filters elements by predicate | `Func<IElement, bool>` | `IElementCollection` |
| `WithText` | Filters elements by text content | `string text` | `IElementCollection` |
| `WithAttribute` | Filters elements by attribute | `string attribute`, `string value` | `IElementCollection` |

## Page Object Pattern

### BasePageComponent<TDriver, TPage>

Base class for all page objects with dual generic types for fluent API context.

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    protected TDriver Driver { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected FluentUIScaffoldOptions Options { get; }
    protected ElementFactory ElementFactory { get; }
    
    public Uri UrlPattern { get; }
    public virtual bool ShouldValidateOnNavigation => false;
    
    protected abstract void ConfigureElements();
    
    // Framework-agnostic element interaction methods
    protected virtual void ClickElement(string selector) => Driver.Click(selector);
    protected virtual void TypeText(string selector, string text) => Driver.Type(selector, text);
    protected virtual void SelectOption(string selector, string value) => Driver.SelectOption(selector, value);
    protected virtual string GetElementText(string selector) => Driver.GetText(selector);
    protected virtual bool IsElementVisible(string selector) => Driver.IsVisible(selector);
    protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);
    
    // Fluent API element action methods
    public virtual TPage Click(Func<TPage, IElement> elementSelector);
    public virtual TPage Type(Func<TPage, IElement> elementSelector, string text);
    public virtual TPage Select(Func<TPage, IElement> elementSelector, string value);
    public virtual TPage Focus(Func<TPage, IElement> elementSelector);
    public virtual TPage Hover(Func<TPage, IElement> elementSelector);
    public virtual TPage Clear(Func<TPage, IElement> elementSelector);
    
    // Additional fluent element actions
    public virtual TPage WaitForElement(Func<TPage, IElement> elementSelector);
    public virtual TPage WaitForElementToBeVisible(Func<TPage, IElement> elementSelector);
    public virtual TPage WaitForElementToBeHidden(Func<TPage, IElement> elementSelector);
    
    // Generic verification methods
    public virtual TPage VerifyValue<TValue>(Func<TPage, IElement> elementSelector, TValue expectedValue, string description = null);
    public virtual TPage VerifyText(Func<TPage, IElement> elementSelector, string expectedText, string description = null);
    public virtual TPage VerifyProperty(Func<TPage, IElement> elementSelector, string expectedValue, string propertyName, string description = null);
    
    // Navigation methods
    public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>;
    
    // Framework-specific access
    protected TDriver FrameworkDriver => Driver;
    public TDriver TestDriver => Driver;
    
    // Verification access
    public IVerificationContext Verify { get; }
    
    // Helper methods
    protected ElementBuilder Element(string selector);
    protected virtual void NavigateToUrl(Uri url);
    
    // IPageComponent implementation
    public virtual bool IsCurrentPage();
    public virtual void ValidateCurrentPage();
}
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Driver` | `TDriver` | The UI driver instance |
| `ServiceProvider` | `IServiceProvider` | Dependency injection container |
| `Logger` | `ILogger` | Logger instance |
| `Options` | `FluentUIScaffoldOptions` | Framework options |
| `ElementFactory` | `ElementFactory` | Factory for creating elements |
| `UrlPattern` | `Uri` | URL pattern for page validation |
| `ShouldValidateOnNavigation` | `bool` | Whether to validate on navigation |
| `TestDriver` | `TDriver` | Public access to driver for testing |
| `Verify` | `IVerificationContext` | Verification context |

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `ConfigureElements` | Configures page elements | None | `void` |
| `Click` | Clicks an element using fluent API | `Func<TPage, IElement> elementSelector` | `TPage` |
| `Type` | Types text into an element using fluent API | `Func<TPage, IElement> elementSelector`, `string text` | `TPage` |
| `Select` | Selects a value from an element using fluent API | `Func<TPage, IElement> elementSelector`, `string value` | `TPage` |
| `Focus` | Focuses on an element using fluent API | `Func<TPage, IElement> elementSelector` | `TPage` |
| `Hover` | Hovers over an element using fluent API | `Func<TPage, IElement> elementSelector` | `TPage` |
| `Clear` | Clears an element using fluent API | `Func<TPage, IElement> elementSelector` | `TPage` |
| `WaitForElement` | Waits for an element using fluent API | `Func<TPage, IElement> elementSelector` | `TPage` |
| `VerifyText` | Verifies element text using fluent API | `Func<TPage, IElement> elementSelector`, `string expectedText` | `TPage` |
| `VerifyValue` | Verifies element value using fluent API | `Func<TPage, IElement> elementSelector`, `TValue expectedValue` | `TPage` |
| `NavigateTo<TTarget>` | Navigates to another page | `TTarget` type parameter | `TTarget` |
| `IsCurrentPage` | Checks if currently on this page | None | `bool` |
| `ValidateCurrentPage` | Validates current page | None | `void` |

#### Usage

```csharp
public class HomePage : BasePageComponent<PlaywrightDriver, HomePage>
{
    public override Uri UrlPattern => new Uri("/home");
    
    private IElement _button;
    private IElement _input;
    
    protected override void ConfigureElements()
    {
        _button = Element("#submit-button")
            .WithDescription("Submit Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
            
        _input = Element("#search-input")
            .WithDescription("Search Input")
            .WithTimeout(TimeSpan.FromSeconds(10));
    }
    
    public HomePage ClickSubmit()
    {
        Click(e => e._button);
        return this;
    }
    
    public HomePage EnterSearchText(string text)
    {
        Type(e => e._input, text);
        return this;
    }
    
    public HomePage VerifyWelcomeMessage(string expectedText)
    {
        VerifyText(e => e._welcomeMessage, expectedText);
        return this;
    }
}
```

### IPageComponent<TDriver, TPage>

Interface for page components with dual generic types.

```csharp
public interface IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    Uri UrlPattern { get; }
    bool ShouldValidateOnNavigation { get; }
    bool IsCurrentPage();
    void ValidateCurrentPage();
    TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>;
    IVerificationContext Verify { get; }
}
```

## Verification Context

### IVerificationContext

Interface for verification operations.

```csharp
public interface IVerificationContext
{
    void ElementContainsText(string selector, string expectedText);
    void ElementIsVisible(string selector);
    void ElementIsHidden(string selector);
    void ElementIsEnabled(string selector);
    void ElementIsDisabled(string selector);
    void ElementHasValue(string selector, string expectedValue);
    void ElementHasAttribute(string selector, string attributeName, string expectedValue);
    void That<TValue>(Func<TValue> actualValueProvider, Func<TValue, bool> condition, string message = null);
}
```

### VerificationContext

Implementation of the verification context.

```csharp
public class VerificationContext : IVerificationContext
{
    public void ElementContainsText(string selector, string expectedText);
    public void ElementIsVisible(string selector);
    public void ElementIsHidden(string selector);
    public void ElementIsEnabled(string selector);
    public void ElementIsDisabled(string selector);
    public void ElementHasValue(string selector, string expectedValue);
    public void ElementHasAttribute(string selector, string attributeName, string expectedValue);
    public void That<TValue>(Func<TValue> actualValueProvider, Func<TValue, bool> condition, string message = null);
}
```

#### Usage

```csharp
// Using the verification context
page.Verify.ElementContainsText("#welcome-message", "Welcome!");
page.Verify.ElementIsVisible("#submit-button");
page.Verify.ElementHasValue("#email-input", "test@example.com");

// Using fluent verification methods
page.VerifyText(e => e._welcomeMessage, "Welcome!");
page.VerifyValue(e => e._emailInput, "test@example.com");
```

## Playwright Integration

### PlaywrightPlugin

Plugin for Playwright integration.

```csharp
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Playwright";
    public string Version => "1.0.0";
    public Type[] SupportedDriverTypes => new[] { typeof(PlaywrightDriver) };
    
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
    public string CurrentUrl { get; }
    
    public void Click(string selector);
    public void Type(string selector, string text);
    public void Select(string selector, string value);
    public string GetText(string selector);
    public bool IsVisible(string selector);
    public bool IsEnabled(string selector);
    public void WaitForElement(string selector);
    public void WaitForElementToBeVisible(string selector);
    public void WaitForElementToBeHidden(string selector);
    public void NavigateToUrl(string url);
    public TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent;
    public TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
    public void Dispose();
}
```

### PlaywrightAdvancedFeatures

Advanced Playwright-specific features.

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

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `InterceptNetworkRequests` | Intercepts network requests | `string urlPattern`, `Action<IResponse>` | `void` |
| `TakeScreenshotAsync` | Takes a screenshot | `string path` | `Task<byte[]>` |
| `GeneratePdfAsync` | Generates a PDF | `string path` | `Task<byte[]>` |
| `SetViewportSize` | Sets viewport size | `int width`, `int height` | `void` |
| `SetUserAgent` | Sets user agent | `string userAgent` | `void` |
| `SetGeolocation` | Sets geolocation | `double latitude`, `double longitude` | `void` |
| `SetPermissions` | Sets browser permissions | `string[] permissions` | `void` |

### PlaywrightWaitStrategy

Playwright-specific wait strategy implementation.

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

## Plugin System

### IUITestingFrameworkPlugin

Interface for testing framework plugins.

```csharp
public interface IUITestingFrameworkPlugin
{
    string Name { get; }
    string Version { get; }
    Type[] SupportedDriverTypes { get; }
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
| `SupportedDriverTypes` | `Type[]` | Supported driver types |

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `CanHandle` | Checks if plugin can handle driver type | `Type driverType` | `bool` |
| `CreateDriver` | Creates a driver instance | `FluentUIScaffoldOptions options` | `IUIDriver` |
| `ConfigureServices` | Configures services for the plugin | `IServiceCollection services` | `void` |

### PluginManager

Manages plugin registration and discovery.

```csharp
public class PluginManager
{
    public void RegisterPlugin<TPlugin>() where TPlugin : IUITestingFrameworkPlugin;
    public void RegisterPlugin(IUITestingFrameworkPlugin plugin);
    public IUITestingFrameworkPlugin GetPlugin(Type driverType);
    public IEnumerable<IUITestingFrameworkPlugin> GetAllPlugins();
    public void ClearPlugins();
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `RegisterPlugin<TPlugin>` | Registers a plugin by type | `TPlugin` type parameter | `void` |
| `RegisterPlugin` | Registers a plugin instance | `IUITestingFrameworkPlugin plugin` | `void` |
| `GetPlugin` | Gets plugin for driver type | `Type driverType` | `IUITestingFrameworkPlugin` |
| `GetAllPlugins` | Gets all registered plugins | None | `IEnumerable<IUITestingFrameworkPlugin>` |
| `ClearPlugins` | Clears all plugins | None | `void` |

## Exceptions

### FluentUIScaffoldException

Base exception for FluentUIScaffold.

```csharp
public class FluentUIScaffoldException : Exception
{
    public string ScreenshotPath { get; set; }
    public string DOMState { get; set; }
    public string CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; set; }
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

### ElementValidationException

Exception for element validation errors.

```csharp
public class ElementValidationException : FluentUIScaffoldException
{
    public string Selector { get; }
    public string ExpectedValue { get; }
    public string ActualValue { get; }
}
```

### FluentUIScaffoldPluginException

Exception for plugin errors.

```csharp
public class FluentUIScaffoldPluginException : FluentUIScaffoldException
{
    public string PluginName { get; }
    public string PluginVersion { get; }
}
```

## Verification Context

### IVerificationContext<TApp>

Interface for verification context.

```csharp
public interface IVerificationContext<TApp>
{
    IVerificationContext<TApp> ElementIsVisible(string selector);
    IVerificationContext<TApp> ElementIsHidden(string selector);
    IVerificationContext<TApp> ElementIsEnabled(string selector);
    IVerificationContext<TApp> ElementIsDisabled(string selector);
    IVerificationContext<TApp> ElementContainsText(string selector, string text);
    IVerificationContext<TApp> ElementHasAttribute(string selector, string attribute, string value);
    IVerificationContext<TApp> CurrentPageIs<TPage>() where TPage : BasePageComponent<TApp>;
    IVerificationContext<TApp> UrlMatches(string pattern);
    IVerificationContext<TApp> TitleContains(string text);
    IVerificationContext<TApp> That(Func<bool> condition, string description);
    IVerificationContext<TApp> That<T>(Func<T> actual, Func<T, bool> condition, string description);
}
```

#### Methods

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `ElementIsVisible` | Verifies element is visible | `string selector` | `IVerificationContext<TApp>` |
| `ElementIsHidden` | Verifies element is hidden | `string selector` | `IVerificationContext<TApp>` |
| `ElementIsEnabled` | Verifies element is enabled | `string selector` | `IVerificationContext<TApp>` |
| `ElementIsDisabled` | Verifies element is disabled | `string selector` | `IVerificationContext<TApp>` |
| `ElementContainsText` | Verifies element contains text | `string selector`, `string text` | `IVerificationContext<TApp>` |
| `ElementHasAttribute` | Verifies element has attribute | `string selector`, `string attribute`, `string value` | `IVerificationContext<TApp>` |
| `CurrentPageIs<TPage>` | Verifies current page type | `TPage` type parameter | `IVerificationContext<TApp>` |
| `UrlMatches` | Verifies URL matches pattern | `string pattern` | `IVerificationContext<TApp>` |
| `TitleContains` | Verifies title contains text | `string text` | `IVerificationContext<TApp>` |
| `That` | Verifies custom condition | `Func<bool> condition`, `string description` | `IVerificationContext<TApp>` |
| `That<T>` | Verifies custom condition with actual value | `Func<T> actual`, `Func<T, bool> condition`, `string description` | `IVerificationContext<TApp>` |

#### Usage

```csharp
page.Verify
    .ElementIsVisible("#submit-button")
    .ElementContainsText("#message", "Success")
    .CurrentPageIs<HomePage>()
    .UrlMatches("/home")
    .That(() => page.GetElementCount() > 0, "Should have elements");
``` 