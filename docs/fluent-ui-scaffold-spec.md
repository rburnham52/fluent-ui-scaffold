# FluentUIScaffold E2E Testing Framework - API Specification

## Overview

FluentUIScaffold is a framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. It abstracts underlying testing frameworks (Playwright, Selenium) while providing a consistent developer experience.

## Core Architecture

### Entry Points

```csharp
// Generic web application
FluentUIScaffold<WebApp> FluentUIScaffold = FluentUIScaffold<WebApp>(options);

// Explicit web application  
FluentUIScaffold<WebApp> FluentUIScaffold = FluentUIScaffold.Web<WebApp>(options);

// Future mobile support
FluentUIScaffold<MobileApp> FluentUIScaffold = FluentUIScaffold.Mobile<MobileApp>(options);
```

### Configuration Options

```csharp
public class FluentUIScaffoldOptions
{
    public string BaseUrl { get; set; }
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan DefaultRetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public WaitStrategy DefaultWaitStrategy { get; set; } = WaitStrategy.Smart;
    public PageValidationStrategy PageValidationStrategy { get; set; } = PageValidationStrategy.Configurable;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public bool CaptureScreenshotsOnFailure { get; set; } = true;
    public bool CaptureDOMStateOnFailure { get; set; } = true;
    public string ScreenshotPath { get; set; } = "./screenshots";
    public Dictionary<string, object> FrameworkSpecificOptions { get; set; } = new();
}
```

## Framework Plugin System

### Plugin Interface

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

### Plugin Registration

```csharp
// Automatic discovery via reflection
[assembly: UITestingFrameworkPlugin(typeof(PlaywrightPlugin))]
[assembly: UITestingFrameworkPlugin(typeof(SeleniumPlugin))]

// Manual registration
FluentUIScaffold<WebApp>(options => {
    options.RegisterPlugin<PlaywrightPlugin>();
    options.RegisterPlugin<SeleniumPlugin>();
});
```

### Built-in Plugins

```csharp
// Playwright Plugin
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Playwright";
    public string Version => "1.0.0";
    public Type[] SupportedDriverTypes => new[] { typeof(PlaywrightDriver) };
    
    public bool CanHandle(Type driverType) => driverType == typeof(PlaywrightDriver);
    public IUIDriver CreateDriver(FluentUIScaffoldOptions options) => new PlaywrightDriver(options);
    public void ConfigureServices(IServiceCollection services) { /* ... */ }
}

// Selenium Plugin  
public class SeleniumPlugin : IUITestingFrameworkPlugin
{
    public string Name => "Selenium";
    public string Version => "1.0.0";
    public Type[] SupportedDriverTypes => new[] { typeof(SeleniumDriver) };
    
    public bool CanHandle(Type driverType) => driverType == typeof(SeleniumDriver);
    public IUIDriver CreateDriver(FluentUIScaffoldOptions options) => new SeleniumDriver(options);
    public void ConfigureServices(IServiceCollection services) { /* ... */ }
}
```

## Base Classes and Interfaces

### Base Page Component

```csharp
public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
{
    protected IUIDriver Driver { get; }
    protected FluentUIScaffoldOptions Options { get; }
    protected ILogger Logger { get; }
    
    public abstract string UrlPattern { get; }
    public virtual bool ShouldValidateOnNavigation => true;
    
    protected BasePageComponent(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
    {
        Driver = driver;
        Options = options;
        Logger = logger;
        ConfigureElements();
    }
    
    protected abstract void ConfigureElements();
    
    // Framework-agnostic element interaction methods
    protected virtual void Click(string selector) => Driver.Click(selector);
    protected virtual void Type(string selector, string text) => Driver.Type(selector, text);
    protected virtual void Select(string selector, string value) => Driver.Select(selector, value);
    protected virtual string GetText(string selector) => Driver.GetText(selector);
    protected virtual bool IsVisible(string selector) => Driver.IsVisible(selector);
    protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);
    
    // Navigation methods
    public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TApp>
        => Driver.NavigateTo<TTarget>();
    
    // Page validation
    public virtual bool IsCurrentPage() => Driver.CurrentUrl.Matches(UrlPattern);
    public virtual void ValidateCurrentPage()
    {
        if (!IsCurrentPage())
            throw new InvalidPageException($"Expected to be on page {GetType().Name}, but URL is {Driver.CurrentUrl}");
    }
    
    // Framework-specific access
    public TDriver Framework<TDriver>() where TDriver : class
        => Driver.GetFrameworkDriver<TDriver>();
    
    // Verification access
    public IVerificationContext<TApp> Verify => new VerificationContext<TApp>(Driver, Options, Logger);
}
```

### Page Interface

```csharp
public interface IPageComponent<TApp>
{
    string UrlPattern { get; }
    bool ShouldValidateOnNavigation { get; }
    bool IsCurrentPage();
    void ValidateCurrentPage();
    TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TApp>;
    IVerificationContext<TApp> Verify { get; }
}
```

### Driver Abstraction

```csharp
public interface IUIDriver
{
    string CurrentUrl { get; }
    
    // Core interactions
    void Click(string selector);
    void Type(string selector, string text);
    void Select(string selector, string value);
    string GetText(string selector);
    bool IsVisible(string selector);
    bool IsEnabled(string selector);
    void WaitForElement(string selector);
    void WaitForElementToBeVisible(string selector);
    void WaitForElementToBeHidden(string selector);
    
    // Navigation
    void NavigateToUrl(string url);
    TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent;
    
    // Framework-specific access
    TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
    
    // Lifecycle
    void Dispose();
}
```

## Element Configuration

### Fluent Element Configuration

```csharp
public abstract class BasePageComponent<TApp>
{
    protected IElement RosterGrid { get; private set; }
    protected IElement AddShiftButton { get; private set; }
    protected IElement EmployeeDropdown { get; private set; }
    
    protected override void ConfigureElements()
    {
        RosterGrid = Element("#roster-grid")
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithWaitStrategy(WaitStrategy.Visible);
            
        AddShiftButton = Element(".add-shift-btn")
            .WithDescription("Add Shift Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
            
        EmployeeDropdown = Element("[data-testid='employee-dropdown']")
            .WithRetryInterval(TimeSpan.FromMilliseconds(200));
    }
    
    protected IElement Element(string selector)
        => new ElementBuilder(selector, Driver, Options);
}

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

## Wait Strategies

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
    Smart // Framework-specific intelligent waiting
}

public class WaitStrategyConfig
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public string ExpectedText { get; set; }
    public bool IgnoreExceptions { get; set; }
}
```

## Verification System

```csharp
public interface IVerificationContext<TApp>
{
    // Element verifications
    IVerificationContext<TApp> ElementIsVisible(string selector);
    IVerificationContext<TApp> ElementIsHidden(string selector);
    IVerificationContext<TApp> ElementIsEnabled(string selector);
    IVerificationContext<TApp> ElementIsDisabled(string selector);
    IVerificationContext<TApp> ElementContainsText(string selector, string text);
    IVerificationContext<TApp> ElementHasAttribute(string selector, string attribute, string value);
    
    // Page verifications
    IVerificationContext<TApp> CurrentPageIs<TPage>() where TPage : BasePageComponent<TApp>;
    IVerificationContext<TApp> UrlMatches(string pattern);
    IVerificationContext<TApp> TitleContains(string text);
    
    // Custom verifications
    IVerificationContext<TApp> That(Func<bool> condition, string description);
    IVerificationContext<TApp> That<T>(Func<T> actual, Func<T, bool> condition, string description);
}
```

## Example Page Implementation

```csharp
public class RosterDetailsPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/roster/details/{rosterId:int}";
    
    // Elements configured in ConfigureElements()
    protected IElement RosterGrid { get; private set; }
    protected IElement AddShiftButton { get; private set; }
    protected IElement EmployeeDropdown { get; private set; }
    protected IElement StartTimeInput { get; private set; }
    protected IElement EndTimeInput { get; private set; }
    protected IElement SaveButton { get; private set; }
    
    protected override void ConfigureElements()
    {
        RosterGrid = Element("#roster-grid");
        AddShiftButton = Element(".add-shift-btn");
        EmployeeDropdown = Element("[data-testid='employee-dropdown']");
        StartTimeInput = Element("#start-time");
        EndTimeInput = Element("#end-time");
        SaveButton = Element(".save-btn");
    }
    
    // Custom action methods
    public RosterDetailsPage OpenRoster(int rosterId)
    {
        Logger.LogInformation($"Opening roster {rosterId}");
        Driver.NavigateToUrl($"/roster/details/{rosterId}");
        ValidateCurrentPage();
        return this;
    }
    
    public RosterDetailsPage AddShift(RosterShift shift)
    {
        Logger.LogInformation($"Adding shift for employee {shift.EmployeeId}");
        
        AddShiftButton.Click();
        EmployeeDropdown.Select(shift.EmployeeId.ToString());
        StartTimeInput.Type(shift.StartTime.ToString("HH:mm"));
        EndTimeInput.Type(shift.EndTime.ToString("HH:mm"));
        SaveButton.Click();
        
        return this;
    }
    
    public HomePage NavigateToHome()
    {
        return NavigateTo<HomePage>();
    }
    
    // Custom verification methods
    public RosterDetailsPage VerifyShiftAdded(RosterShift shift)
    {
        var shiftSelector = $".shift-row[data-employee-id='{shift.EmployeeId}']";
        
        Verify
            .ElementIsVisible(shiftSelector)
            .ElementContainsText($"{shiftSelector} .start-time", shift.StartTime.ToString("HH:mm"))
            .ElementContainsText($"{shiftSelector} .end-time", shift.EndTime.ToString("HH:mm"));
            
        return this;
    }
}
```

## Usage Example

```csharp
[Test]
public void Can_Add_Shift_To_Roster()
{
    var rosterId = 123;
    var employeeId = 456;
    var shift = new RosterShift
    {
        StartTime = DateTime.Now.AddHours(1),
        EndTime = DateTime.Now.AddHours(2),
        EmployeeId = employeeId
    };
    
    FluentUIScaffold<WebApp>(options => {
        options.BaseUrl = "https://localhost:5001";
        options.DefaultTimeout = TimeSpan.FromSeconds(30);
        options.CaptureScreenshotsOnFailure = true;
    })
    .NavigateTo<RosterDetailsPage>()
    .OpenRoster(rosterId)
    .AddShift(shift)
    .VerifyShiftAdded(shift)
    .NavigateTo<HomePage>()
    .Verify.ElementIsVisible(".roster-schedule .accept-roster-shift");
}
```

## Framework-Specific Features

```csharp
// Accessing Playwright-specific features
FluentUIScaffold
    .NavigateTo<LoginPage>()
    .Framework<PlaywrightDriver>()
    .InterceptNetworkRequests("/api/auth", response => {
        // Playwright-specific network interception
    });

// Accessing Selenium-specific features  
FluentUIScaffold
    .NavigateTo<LoginPage>()
    .Framework<SeleniumDriver>()
    .SetBrowserProfile(profilePath);
```

## Error Handling and Debugging

```csharp
public class FluentUIScaffoldException : Exception
{
    public string ScreenshotPath { get; set; }
    public string DOMState { get; set; }
    public string CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; set; }
}

public class InvalidPageException : FluentUIScaffoldException { }
public class ElementNotFoundException : FluentUIScaffoldException { }
public class TimeoutException : FluentUIScaffoldException { }
```

## Logging Integration

```csharp
public interface IFluentUIScaffoldLogger
{
    void LogAction(string action, string target, Dictionary<string, object> context);
    void LogNavigation(string from, string to, TimeSpan duration);
    void LogVerification(string verification, bool success, string details);
    void LogError(Exception exception, Dictionary<string, object> context);
}

// Integration with popular logging frameworks
public class NLogFluentUIScaffoldLogger : IFluentUIScaffoldLogger { }
public class SerilogFluentUIScaffoldLogger : IFluentUIScaffoldLogger { }
public class XUnitFluentUIScaffoldLogger : IFluentUIScaffoldLogger { }
```

## Component Transitions

```csharp
public class HomePage : BasePageComponent<WebApp>
{
    public RosterDetailsPage OpenRosterDetails(int rosterId)
    {
        Click($".roster-link[data-roster-id='{rosterId}']");
        return NavigateTo<RosterDetailsPage>();
    }
    
    public ConfirmationDialog DeleteRoster(int rosterId)
    {
        Click($".delete-roster[data-roster-id='{rosterId}']");
        return TransitionTo<ConfirmationDialog>();
    }
}

public class ConfirmationDialog : BasePageComponent<WebApp>
{
    public HomePage ConfirmDelete()
    {
        Click(".confirm-button");
        return NavigateTo<HomePage>();
    }
    
    public HomePage CancelDelete()
    {
        Click(".cancel-button");
        return NavigateTo<HomePage>();
    }
}
```

This specification provides a comprehensive foundation for your FluentUIScaffold E2E testing framework, supporting all the features you requested while maintaining flexibility for future enhancements.