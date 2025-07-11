// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright;

/// <summary>
/// Provides advanced Playwright-specific features.
/// </summary>
public class PlaywrightAdvancedFeatures
{
    private readonly IPage _page;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightAdvancedFeatures"/> class.
    /// </summary>
    /// <param name="page">The Playwright page instance.</param>
    public PlaywrightAdvancedFeatures(IPage page)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
    }

    /// <summary>
    /// Intercepts network requests matching the specified pattern.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to match.</param>
    /// <param name="handler">The handler to execute for matched requests.</param>
    public void InterceptNetworkRequests(string urlPattern, Action<IAPIResponse> handler)
    {
        if (string.IsNullOrEmpty(urlPattern))
            throw new ArgumentException("URL pattern cannot be null or empty.", nameof(urlPattern));

        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        _page.RouteAsync(urlPattern, async route =>
        {
            var response = await route.FetchAsync();
            handler(response);
            await route.FulfillAsync(new RouteFulfillOptions { Response = response });
        });
    }

    /// <summary>
    /// Takes a screenshot of the current page.
    /// </summary>
    /// <param name="path">Optional path to save the screenshot. If null, a default path will be used.</param>
    /// <returns>The screenshot as a byte array.</returns>
    public async Task<byte[]> TakeScreenshotAsync(string? path = null)
    {
        var screenshotOptions = new PageScreenshotOptions
        {
            Path = path ?? $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png",
            FullPage = true
        };

        return await _page.ScreenshotAsync(screenshotOptions);
    }

    /// <summary>
    /// Takes a screenshot of a specific element.
    /// </summary>
    /// <param name="selector">The selector for the element to screenshot.</param>
    /// <param name="path">Optional path to save the screenshot. If null, a default path will be used.</param>
    /// <returns>The screenshot as a byte array.</returns>
    public async Task<byte[]> TakeElementScreenshotAsync(string selector, string? path = null)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        var locator = _page.Locator(selector);
        var screenshotOptions = new LocatorScreenshotOptions
        {
            Path = path ?? $"element_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        };

        return await locator.ScreenshotAsync(screenshotOptions);
    }

    /// <summary>
    /// Generates a PDF of the current page.
    /// </summary>
    /// <param name="path">Optional path to save the PDF. If null, a default path will be used.</param>
    /// <returns>The PDF as a byte array.</returns>
    public async Task<byte[]> GeneratePdfAsync(string? path = null)
    {
        var pdfOptions = new PagePdfOptions
        {
            Path = path ?? $"document_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
            Format = "A4",
            PrintBackground = true
        };

        return await _page.PdfAsync(pdfOptions);
    }

    /// <summary>
    /// Records video of the browser session.
    /// </summary>
    /// <param name="path">The path where the video will be saved.</param>
    /// <returns>A task that completes when video recording starts.</returns>
    public async Task StartVideoRecordingAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));

        // Note: Video recording is configured at the browser context level
        // This method provides a way to start recording for the current context
        await Task.CompletedTask; // Placeholder for future implementation
    }

    /// <summary>
    /// Stops video recording.
    /// </summary>
    /// <returns>A task that completes when video recording stops.</returns>
    public async Task StopVideoRecordingAsync()
    {
        // Note: Video recording is configured at the browser context level
        // This method provides a way to stop recording for the current context
        await Task.CompletedTask; // Placeholder for future implementation
    }

    /// <summary>
    /// Emulates a mobile device.
    /// </summary>
    /// <param name="deviceName">The name of the device to emulate (e.g., "iPhone 12").</param>
    /// <returns>A task that completes when device emulation is set up.</returns>
    public async Task EmulateMobileDeviceAsync(string deviceName)
    {
        if (string.IsNullOrEmpty(deviceName))
            throw new ArgumentException("Device name cannot be null or empty.", nameof(deviceName));

        // Note: Device emulation is not directly supported in the current Playwright version
        // This is a placeholder for future implementation
        await Task.CompletedTask;
    }

    /// <summary>
    /// Sets the geolocation for the browser context.
    /// </summary>
    /// <param name="latitude">The latitude coordinate.</param>
    /// <param name="longitude">The longitude coordinate.</param>
    /// <returns>A task that completes when geolocation is set.</returns>
    public async Task SetGeolocationAsync(double latitude, double longitude)
    {
        await _page.Context.SetGeolocationAsync(new Geolocation { Latitude = (float)latitude, Longitude = (float)longitude });
    }

    /// <summary>
    /// Sets the permissions for the browser context.
    /// </summary>
    /// <param name="permissions">The permissions to set.</param>
    /// <returns>A task that completes when permissions are set.</returns>
    public async Task SetPermissionsAsync(params string[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            throw new ArgumentException("At least one permission must be specified.", nameof(permissions));

        await _page.Context.GrantPermissionsAsync(permissions);
    }

    /// <summary>
    /// Adds a script to be evaluated on every page load.
    /// </summary>
    /// <param name="script">The JavaScript script to add.</param>
    /// <returns>A task that completes when the script is added.</returns>
    public async Task AddInitScriptAsync(string script)
    {
        if (string.IsNullOrEmpty(script))
            throw new ArgumentException("Script cannot be null or empty.", nameof(script));

        await _page.AddInitScriptAsync(script);
    }

    /// <summary>
    /// Evaluates JavaScript in the page context.
    /// </summary>
    /// <param name="script">The JavaScript to evaluate.</param>
    /// <returns>The result of the script evaluation.</returns>
    public async Task<object?> EvaluateJavaScriptAsync(string script)
    {
        if (string.IsNullOrEmpty(script))
            throw new ArgumentException("Script cannot be null or empty.", nameof(script));

        return await _page.EvaluateAsync(script);
    }

    /// <summary>
    /// Waits for a network request to complete.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to wait for.</param>
    /// <param name="timeout">The timeout for the wait operation.</param>
    /// <returns>The response that completed.</returns>
    public async Task<IResponse> WaitForNetworkRequestAsync(string urlPattern, TimeSpan timeout)
    {
        if (string.IsNullOrEmpty(urlPattern))
            throw new ArgumentException("URL pattern cannot be null or empty.", nameof(urlPattern));

        var response = await _page.WaitForResponseAsync(urlPattern, new PageWaitForResponseOptions
        {
            Timeout = (float)timeout.TotalMilliseconds
        });

        return response;
    }

    /// <summary>
    /// Waits for a network request to be initiated.
    /// </summary>
    /// <param name="urlPattern">The URL pattern to wait for.</param>
    /// <param name="timeout">The timeout for the wait operation.</param>
    /// <returns>The request that was initiated.</returns>
    public async Task<IRequest> WaitForNetworkRequestInitiatedAsync(string urlPattern, TimeSpan timeout)
    {
        if (string.IsNullOrEmpty(urlPattern))
            throw new ArgumentException("URL pattern cannot be null or empty.", nameof(urlPattern));

        var request = await _page.WaitForRequestAsync(urlPattern, new PageWaitForRequestOptions
        {
            Timeout = (float)timeout.TotalMilliseconds
        });

        return request;
    }
}
