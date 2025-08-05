// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright;

/// <summary>
/// Playwright driver implementation for FluentUIScaffold.
/// </summary>
public class PlaywrightDriver : IUIDriver, IDisposable
{
    private readonly FluentUIScaffoldOptions _options;
    private readonly IPlaywright _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private readonly ILogger<PlaywrightDriver>? _logger;
    private WebServerLauncher? _webServerLauncher;
    private bool _disposed;

    /// <summary>
    /// Gets the current URL of the browser.
    /// </summary>
    public Uri? CurrentUrl => _page?.Url != null ? new Uri(_page.Url) : new Uri("about:blank");

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightDriver"/> class.
    /// </summary>
    /// <param name="options">The configuration options for the driver.</param>
    public PlaywrightDriver(FluentUIScaffoldOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _playwright = Microsoft.Playwright.Playwright.CreateAsync().Result;
        InitializeBrowser();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightDriver"/> class with logger.
    /// </summary>
    /// <param name="options">The configuration options for the driver.</param>
    /// <param name="logger">The logger instance.</param>
    public PlaywrightDriver(FluentUIScaffoldOptions options, ILogger<PlaywrightDriver> logger)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
        _playwright = Microsoft.Playwright.Playwright.CreateAsync().Result;
        InitializeBrowser();
    }

    /// <summary>
    /// Launches a web server for testing if specified in the options.
    /// </summary>
    /// <param name="projectPath">The path to the ASP.NET Core project to launch.</param>
    /// <returns>A task that completes when the web server is ready.</returns>
    public async Task LaunchWebServerAsync(string projectPath)
    {
        if (string.IsNullOrEmpty(projectPath))
            throw new ArgumentException("Project path cannot be null or empty.", nameof(projectPath));

        if (_options.BaseUrl == null)
            throw new InvalidOperationException("BaseUrl must be configured to launch a web server.");

        // Use Playwright's built-in web server launching capabilities
        await LaunchWebServerWithPlaywrightAsync(projectPath);
    }

    /// <summary>
    /// Launches a web server using Playwright-style configuration.
    /// </summary>
    /// <param name="projectPath">The path to the ASP.NET Core project to launch.</param>
    /// <returns>A task that completes when the web server is ready.</returns>
    private async Task LaunchWebServerWithPlaywrightAsync(string projectPath)
    {
        try
        {
            _logger?.LogInformation("Launching web server using Playwright-style configuration");

            // Use our WebServerLauncher with Playwright-style configuration
            _webServerLauncher = new WebServerLauncher(_logger);
            await _webServerLauncher.LaunchWebServerAsync(projectPath, _options.BaseUrl, _options.DefaultWaitTimeout);

            _logger?.LogInformation("Web server launched successfully using Playwright-style configuration");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to launch web server using Playwright-style configuration");
            throw;
        }
    }

    private void InitializeBrowser()
    {
        var browserType = GetBrowserType();

        // Determine headless mode and SlowMo based on debug mode
        bool isHeadless;
        int slowMo;

        if (_options.DebugMode)
        {
            // Debug mode: non-headless with SlowMo for easier debugging
            isHeadless = false;
            slowMo = 1000; // Default SlowMo for debugging
            _logger?.LogInformation("Debug mode enabled: running in non-headless mode with SlowMo = {SlowMo}ms", slowMo);
        }
        else
        {
            // Normal mode: use configured headless mode and SlowMo
            isHeadless = _options.HeadlessMode || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            slowMo = _options.FrameworkOptions.TryGetValue("SlowMo", out var slowMoValue) ? (int)slowMoValue : 0;
        }

        var browserOptions = new BrowserTypeLaunchOptions
        {
            Headless = isHeadless,
            SlowMo = slowMo
        };

        _browser = browserType.LaunchAsync(browserOptions).Result;

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize
            {
                Width = _options.WindowWidth,
                Height = _options.WindowHeight
            },
            UserAgent = _options.UserAgent
        };

        _context = _browser.NewContextAsync(contextOptions).Result;
        _page = _context.NewPageAsync().Result;
    }

    private IBrowserType GetBrowserType()
    {
        var browserType = _options.FrameworkOptions.TryGetValue("BrowserType", out var browserTypeValue) ? browserTypeValue as string : "chromium";
        return browserType?.ToLowerInvariant() switch
        {
            "firefox" => _playwright.Firefox,
            "webkit" => _playwright.Webkit,
            _ => _playwright.Chromium
        };
    }

    /// <summary>
    /// Clicks an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    public void Click(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Clicking element with selector: {Selector}", selector);
        _page?.ClickAsync(selector).Wait();
    }

    /// <summary>
    /// Types text into an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="text">The text to type into the element.</param>
    public void Type(string selector, string text)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Typing text '{Text}' into element with selector: {Selector}", text, selector);
        _page?.FillAsync(selector, text).Wait();
    }

    /// <summary>
    /// Selects an option from a dropdown element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the dropdown element.</param>
    /// <param name="value">The value of the option to select.</param>
    public void SelectOption(string selector, string value)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Selecting option '{Value}' from element with selector: {Selector}", value, selector);
        _page?.SelectOptionAsync(selector, value).Wait();
    }

    /// <summary>
    /// Gets the text content of an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The text content of the element.</returns>
    public string GetText(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Getting text from element with selector: {Selector}", selector);
        var text = _page?.TextContentAsync(selector).Result;
        if (text == null)
            throw new InvalidOperationException($"Element with selector '{selector}' was not found or has no text content.");
        return text;
    }

    /// <summary>
    /// Checks if an element identified by the specified selector is visible.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>True if the element is visible; otherwise, false.</returns>
    public bool IsVisible(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Checking visibility of element with selector: {Selector}", selector);
        return _page?.IsVisibleAsync(selector).Result ?? false;
    }

    /// <summary>
    /// Checks if an element identified by the specified selector is enabled.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>True if the element is enabled; otherwise, false.</returns>
    public bool IsEnabled(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Checking if element with selector is enabled: {Selector}", selector);
        return _page?.IsEnabledAsync(selector).Result ?? false;
    }

    /// <summary>
    /// Waits for an element identified by the specified selector to be present in the DOM.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    public void WaitForElement(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Waiting for element with selector: {Selector}", selector);
        _page?.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Attached,
            Timeout = (float)_options.DefaultWaitTimeout.TotalMilliseconds
        }).Wait();
    }

    /// <summary>
    /// Waits for an element identified by the specified selector to become visible.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    public void WaitForElementToBeVisible(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Waiting for element to be visible with selector: {Selector}", selector);
        _page?.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = (float)_options.DefaultWaitTimeout.TotalMilliseconds
        }).Wait();
    }

    /// <summary>
    /// Waits for an element identified by the specified selector to become hidden.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    public void WaitForElementToBeHidden(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Waiting for element to be hidden with selector: {Selector}", selector);
        _page?.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Hidden,
            Timeout = (float)_options.DefaultWaitTimeout.TotalMilliseconds
        }).Wait();
    }

    /// <summary>
    /// Navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    public void NavigateToUrl(Uri url)
    {
        if (url == null)
            throw new ArgumentNullException(nameof(url));

        _logger?.LogDebug("Navigating to URL: {Url}", url);
        _page?.GotoAsync(url.ToString()).Wait();
    }

    /// <summary>
    /// Navigates to a page component of the specified type.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target page component.</typeparam>
    /// <returns>The target page component instance.</returns>
    public TTarget NavigateTo<TTarget>() where TTarget : class
    {
        // This will be implemented when page components are available
        throw new NotImplementedException("Page navigation will be implemented in a future story.");
    }

    /// <summary>
    /// Gets the underlying framework-specific driver instance.
    /// </summary>
    /// <typeparam name="TDriver">The type of the framework-specific driver.</typeparam>
    /// <returns>The framework-specific driver instance.</returns>
    public TDriver GetFrameworkDriver<TDriver>() where TDriver : class
    {
        if (typeof(TDriver) == typeof(IPage))
            return _page as TDriver ?? throw new InvalidOperationException("Page is not available");
        if (typeof(TDriver) == typeof(IBrowser))
            return _browser as TDriver ?? throw new InvalidOperationException("Browser is not available");
        if (typeof(TDriver) == typeof(IBrowserContext))
            return _context as TDriver ?? throw new InvalidOperationException("Browser context is not available");
        if (typeof(TDriver) == typeof(IPlaywright))
            return _playwright as TDriver ?? throw new InvalidOperationException("Playwright is not available");

        throw new InvalidOperationException($"Unsupported framework driver type: {typeof(TDriver).Name}");
    }

    /// <summary>
    /// Disposes the driver and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Disposes the driver and releases all resources.
    /// </summary>
    /// <param name="disposing">True if disposing; false if finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _page?.CloseAsync();
            _context?.CloseAsync();
            _browser?.CloseAsync();
            _playwright?.Dispose();
            _webServerLauncher?.Dispose();
            _disposed = true;
        }
    }

    public void Focus(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Focusing element with selector: {Selector}", selector);
        _page?.FocusAsync(selector).Wait();
    }

    public void Hover(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Hovering over element with selector: {Selector}", selector);
        _page?.HoverAsync(selector).Wait();
    }

    public void Clear(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        _logger?.LogDebug("Clearing element with selector: {Selector}", selector);
        _page?.FillAsync(selector, "").Wait();
    }
    public string GetPageTitle() => _page?.TitleAsync().Result ?? "No title available";
}
