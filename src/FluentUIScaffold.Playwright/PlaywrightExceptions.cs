// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using FluentUIScaffold.Core.Exceptions;

namespace FluentUIScaffold.Playwright;

/// <summary>
/// Exception thrown when a Playwright-specific error occurs.
/// </summary>
public class PlaywrightException : FluentUIScaffoldException
{
    /// <summary>
    /// Gets the selector that was being used when the error occurred.
    /// </summary>
    public string? Selector { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="selector">The selector that was being used.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlaywrightException(string message, string? selector = null, Exception? innerException = null) 
        : base(message, innerException ?? new Exception())
    {
        Selector = selector;
    }
}

/// <summary>
/// Exception thrown when a Playwright timeout occurs.
/// </summary>
public class PlaywrightTimeoutException : PlaywrightException
{
    /// <summary>
    /// Gets the timeout duration that was exceeded.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="selector">The selector that was being used.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    public PlaywrightTimeoutException(string message, string? selector = null, TimeSpan timeout = default) 
        : base(message, selector, null)
    {
        Timeout = timeout;
    }
}

/// <summary>
/// Exception thrown when a Playwright element is not found.
/// </summary>
public class PlaywrightElementNotFoundException : PlaywrightException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightElementNotFoundException"/> class.
    /// </summary>
    /// <param name="selector">The selector that was not found.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlaywrightElementNotFoundException(string selector, Exception? innerException = null) 
        : base($"Element with selector '{selector}' was not found.", selector, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a Playwright element is not visible.
/// </summary>
public class PlaywrightElementNotVisibleException : PlaywrightException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightElementNotVisibleException"/> class.
    /// </summary>
    /// <param name="selector">The selector that was not visible.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlaywrightElementNotVisibleException(string selector, Exception? innerException = null) 
        : base($"Element with selector '{selector}' is not visible.", selector, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a Playwright element is not enabled.
/// </summary>
public class PlaywrightElementNotEnabledException : PlaywrightException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightElementNotEnabledException"/> class.
    /// </summary>
    /// <param name="selector">The selector that was not enabled.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlaywrightElementNotEnabledException(string selector, Exception? innerException = null) 
        : base($"Element with selector '{selector}' is not enabled.", selector, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a Playwright navigation fails.
/// </summary>
public class PlaywrightNavigationException : PlaywrightException
{
    /// <summary>
    /// Gets the URL that was being navigated to.
    /// </summary>
    public string? Url { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightNavigationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="url">The URL that was being navigated to.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlaywrightNavigationException(string message, string? url = null, Exception? innerException = null) 
        : base(message, null, innerException)
    {
        Url = url;
    }
}

/// <summary>
/// Exception thrown when a Playwright browser fails to start.
/// </summary>
public class PlaywrightBrowserStartupException : PlaywrightException
{
    /// <summary>
    /// Gets the browser type that failed to start.
    /// </summary>
    public string? BrowserType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightBrowserStartupException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="browserType">The browser type that failed to start.</param>
    /// <param name="innerException">The inner exception.</param>
    public PlaywrightBrowserStartupException(string message, string? browserType = null, Exception? innerException = null) 
        : base(message, null, innerException)
    {
        BrowserType = browserType;
    }
} 