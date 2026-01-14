# FluentUIScaffold E2E Testing Framework - API Specification

## Overview

FluentUIScaffold is a framework-agnostic E2E testing library that provides a fluent API for building maintainable and reusable UI test automation. It abstracts underlying testing frameworks (Playwright, Selenium) while providing a consistent developer experience.

## Core Architecture

### Entry Points

```csharp
// Build and configure the application scaffold
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
        opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

// Start the application (async-first design)
await app.StartAsync();

// Use the app for testing
app.NavigateTo<HomePage>();

// Clean up when done
await app.DisposeAsync();
```

### Configuration Options

```csharp
public class FluentUIScaffoldOptions
{
    public Uri? BaseUrl { get; set; }
    public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool? HeadlessMode { get; set; } = null; // null = automatic (debugger/CI)
    public int? SlowMo { get; set; } = null;        // null = automatic (debugger/CI)
    public Type? RequestedDriverType { get; set; }
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
// Explicit registration (recommended)
var builder = new FluentUIScaffoldBuilder();
builder.UsePlugin(new PlaywrightPlugin());
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
```

## Base Classes and Interfaces

### Page Base Class

```csharp
public abstract class Page<TSelf> : IPage<TSelf>, IAsyncDisposable
    where TSelf : Page<TSelf>
{
    protected Page(IServiceProvider serviceProvider, Uri urlPattern);

    // DI-resolved properties
    public IServiceProvider ServiceProvider { get; }
    public IUIDriver Driver { get; }
    public Uri UrlPattern { get; }
    protected ILogger Logger { get; }
    protected FluentUIScaffoldOptions Options { get; }

    // Element Building
    protected ElementBuilder Element(string selector);

    // Abstract Configuration
    protected abstract void ConfigureElements();

    // Fluent Navigation (returns TSelf)
    public virtual TSelf Navigate();
    public TSelf NavigateAndWait(Uri url);

    // Cross-Page Navigation
    public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;

    // Fluent Interactions (all return TSelf)
    public TSelf Click(Func<TSelf, IElement> elementSelector);
    public TSelf Type(Func<TSelf, IElement> elementSelector, string text);
    public TSelf Select(Func<TSelf, IElement> elementSelector, string option);
    public TSelf WaitForElement(Func<TSelf, IElement> elementSelector);
    public TSelf WaitForVisible(Func<TSelf, IElement> elementSelector);
    public TSelf WaitForHidden(Func<TSelf, IElement> elementSelector);

    // Verification
    public IVerificationContext<TSelf> Verify { get; }

    // Validation
    public virtual bool IsCurrentPage();
    public virtual void ValidateCurrentPage();
}
```

### Page Interface

```csharp
public interface IPage<TSelf> where TSelf : IPage<TSelf>
{
    IServiceProvider ServiceProvider { get; }
    IUIDriver Driver { get; }
    Uri UrlPattern { get; }

    TSelf Navigate();
    TSelf NavigateAndWait(Uri url);
    TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;
    IVerificationContext<TSelf> Verify { get; }
}
```

### Driver Abstraction

```csharp
public interface IUIDriver : IDisposable
{
    Uri? CurrentUrl { get; }
    void Click(string selector);
    void Type(string selector, string text);
    void SelectOption(string selector, string value);
    string GetText(string selector);
    string GetAttribute(string selector, string attributeName);
    string GetValue(string selector);
    bool IsVisible(string selector);
    bool IsEnabled(string selector);
    void WaitForElement(string selector);
    void WaitForElementToBeVisible(string selector);
    void WaitForElementToBeHidden(string selector);
    void Focus(string selector);
    void Hover(string selector);
    void Clear(string selector);
    string GetPageTitle();
    void NavigateToUrl(Uri url);
    TTarget NavigateTo<TTarget>() where TTarget : class;
    TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
}
```

## Element Configuration

### Fluent Element Configuration

```csharp
public class RosterDetailsPage : Page<RosterDetailsPage>
{
    public IElement RosterGrid { get; private set; } = null!;
    public IElement AddShiftButton { get; private set; } = null!;
    public IElement EmployeeDropdown { get; private set; } = null!;

    public RosterDetailsPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        RosterGrid = Element("#roster-grid")
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        AddShiftButton = Element(".add-shift-btn")
            .WithDescription("Add Shift Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();

        EmployeeDropdown = Element("[data-testid='employee-dropdown']")
            .WithRetryInterval(TimeSpan.FromMilliseconds(200))
            .Build();
    }
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
public interface IVerificationContext<TSelf>
{
    // Element verifications (fluent with lambda selectors)
    IVerificationContext<TSelf> Visible(Func<TSelf, IElement> elementSelector);
    IVerificationContext<TSelf> Hidden(Func<TSelf, IElement> elementSelector);
    IVerificationContext<TSelf> Enabled(Func<TSelf, IElement> elementSelector);
    IVerificationContext<TSelf> Disabled(Func<TSelf, IElement> elementSelector);
    IVerificationContext<TSelf> TextContains(Func<TSelf, IElement> elementSelector, string text);
    IVerificationContext<TSelf> HasAttribute(Func<TSelf, IElement> elementSelector, string attribute, string value);

    // Page verifications
    IVerificationContext<TSelf> CurrentPageIs<TPage>() where TPage : Page<TPage>;
    IVerificationContext<TSelf> UrlMatches(string pattern);
    IVerificationContext<TSelf> TitleContains(string text);

    // Custom verifications
    IVerificationContext<TSelf> That(Func<bool> condition, string description);
    IVerificationContext<TSelf> That<T>(Func<T> actual, Func<T, bool> condition, string description);
}
```

## Example Page Implementation

```csharp
public class RosterDetailsPage : Page<RosterDetailsPage>
{
    public override Uri UrlPattern => new Uri("/roster/details/{rosterId:int}", UriKind.Relative);

    // Elements configured in ConfigureElements()
    public IElement RosterGrid { get; private set; } = null!;
    public IElement AddShiftButton { get; private set; } = null!;
    public IElement EmployeeDropdown { get; private set; } = null!;
    public IElement StartTimeInput { get; private set; } = null!;
    public IElement EndTimeInput { get; private set; } = null!;
    public IElement SaveButton { get; private set; } = null!;

    public RosterDetailsPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        RosterGrid = Element("#roster-grid").Build();
        AddShiftButton = Element(".add-shift-btn").Build();
        EmployeeDropdown = Element("[data-testid='employee-dropdown']").Build();
        StartTimeInput = Element("#start-time").Build();
        EndTimeInput = Element("#end-time").Build();
        SaveButton = Element(".save-btn").Build();
    }

    // Custom action methods (return this for fluent chaining)
    public RosterDetailsPage OpenRoster(int rosterId)
    {
        Logger.LogInformation($"Opening roster {rosterId}");
        Driver.NavigateToUrl(new Uri($"/roster/details/{rosterId}", UriKind.Relative));
        ValidateCurrentPage();
        return this;
    }

    public RosterDetailsPage AddShift(RosterShift shift)
    {
        Logger.LogInformation($"Adding shift for employee {shift.EmployeeId}");

        return this
            .Click(p => p.AddShiftButton)
            .Select(p => p.EmployeeDropdown, shift.EmployeeId.ToString())
            .Type(p => p.StartTimeInput, shift.StartTime.ToString("HH:mm"))
            .Type(p => p.EndTimeInput, shift.EndTime.ToString("HH:mm"))
            .Click(p => p.SaveButton);
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
            .That(() => Driver.IsVisible(shiftSelector), $"Shift row visible for employee {shift.EmployeeId}")
            .That(() => Driver.GetText($"{shiftSelector} .start-time").Contains(shift.StartTime.ToString("HH:mm")), "Start time matches");

        return this;
    }
}
```

## Usage Example

```csharp
[TestClass]
public class RosterTests
{
    private static AppScaffold<WebApp>? _app;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .Web<WebApp>(opts =>
            {
                opts.BaseUrl = new Uri("https://localhost:5001");
                opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
            })
            .WithAutoPageDiscovery()
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }

    [TestMethod]
    public void Can_Add_Shift_To_Roster()
    {
        var rosterId = 123;
        var shift = new RosterShift
        {
            StartTime = DateTime.Now.AddHours(1),
            EndTime = DateTime.Now.AddHours(2),
            EmployeeId = 456
        };

        _app!.NavigateTo<RosterDetailsPage>()
            .OpenRoster(rosterId)
            .AddShift(shift)
            .VerifyShiftAdded(shift)
            .NavigateTo<HomePage>()
            .Verify.Visible(p => p.RosterSchedule);
    }
}
```

## Framework-Specific Features

```csharp
// Accessing Playwright-specific features
var page = app.Framework<IPage>();
await page.ScreenshotAsync(new PageScreenshotOptions { Path = "screenshot.png" });
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
```

## Component Transitions

```csharp
public class HomePage : Page<HomePage>
{
    public IElement RosterLink { get; private set; } = null!;
    public IElement DeleteRosterButton { get; private set; } = null!;

    public HomePage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        RosterLink = Element(".roster-link").Build();
        DeleteRosterButton = Element(".delete-roster").Build();
    }

    public RosterDetailsPage OpenRosterDetails(int rosterId)
    {
        Driver.Click($".roster-link[data-roster-id='{rosterId}']");
        return NavigateTo<RosterDetailsPage>();
    }

    public ConfirmationDialog DeleteRoster(int rosterId)
    {
        Driver.Click($".delete-roster[data-roster-id='{rosterId}']");
        return NavigateTo<ConfirmationDialog>();
    }
}

public class ConfirmationDialog : Page<ConfirmationDialog>
{
    public IElement ConfirmButton { get; private set; } = null!;
    public IElement CancelButton { get; private set; } = null!;

    public ConfirmationDialog(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        ConfirmButton = Element(".confirm-button").Build();
        CancelButton = Element(".cancel-button").Build();
    }

    public HomePage ConfirmDelete()
    {
        return Click(p => p.ConfirmButton)
            .NavigateTo<HomePage>();
    }

    public HomePage CancelDelete()
    {
        return Click(p => p.CancelButton)
            .NavigateTo<HomePage>();
    }
}
```

This specification provides a comprehensive foundation for the FluentUIScaffold E2E testing framework, supporting all features while maintaining flexibility for future enhancements.
