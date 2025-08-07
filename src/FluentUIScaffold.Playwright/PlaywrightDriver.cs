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



    private void InitializeBrowser()
    {
        var browserType = GetBrowserType();

        // Determine headless mode and SlowMo with explicit control and sensible defaults
        bool isHeadless;
        int slowMo;

        // Determine headless mode: explicit setting takes precedence, then automatic logic
        if (_options.HeadlessMode.HasValue)
        {
            // Use explicit headless setting
            isHeadless = _options.HeadlessMode.Value;
            _logger?.LogInformation("Using explicit headless mode setting: {Headless}", isHeadless);
        }
        else
        {
            // Automatic determination based on debug mode and CI environment
            if (_options.EnableDebugMode)
            {
                // Debug mode: non-headless for easier debugging
                isHeadless = false;
                _logger?.LogInformation("Debug mode enabled: automatically setting headless mode to false");
            }
            else
            {
                // Normal mode: use headless mode in CI environments, visible in development
                isHeadless = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
                _logger?.LogInformation("Automatic headless mode determination: {Headless} (CI: {IsCI})",
                    isHeadless, !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")));
            }
        }

        // Determine SlowMo: explicit setting takes precedence, then automatic logic
        if (_options.SlowMo.HasValue)
        {
            // Use explicit SlowMo setting
            slowMo = _options.SlowMo.Value;
            _logger?.LogInformation("Using explicit SlowMo setting: {SlowMo}ms", slowMo);
        }
        else
        {
            // Automatic determination based on debug mode
            if (_options.EnableDebugMode)
            {
                // Debug mode: use SlowMo for easier debugging
                slowMo = 1000; // Default SlowMo for debugging
                _logger?.LogInformation("Debug mode enabled: automatically setting SlowMo to {SlowMo}ms", slowMo);
            }
            else
            {
                // Normal mode: no SlowMo for faster execution
                slowMo = 0;
                _logger?.LogInformation("Normal mode: setting SlowMo to 0ms for faster execution");
            }
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
                Width = 1920, // Default window width
                Height = 1080 // Default window height
            }
            // Use default UserAgent
        };

        _context = _browser.NewContextAsync(contextOptions).Result;
        _page = _context.NewPageAsync().Result;
    }

    private IBrowserType GetBrowserType()
    {
        // Default to Chromium browser
        return _playwright.Chromium;
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
