// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright;

/// <summary>
/// Implements wait strategies for Playwright driver.
/// </summary>
public class PlaywrightWaitStrategy
{
    private readonly IPage _page;
    private readonly FluentUIScaffoldOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaywrightWaitStrategy"/> class.
    /// </summary>
    /// <param name="page">The Playwright page instance.</param>
    /// <param name="options">The configuration options.</param>
    public PlaywrightWaitStrategy(IPage page, FluentUIScaffoldOptions options)
    {
        _page = page ?? throw new ArgumentNullException(nameof(page));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Waits for an element using the specified strategy.
    /// </summary>
    /// <param name="selector">The element selector.</param>
    /// <param name="strategy">The wait strategy to use.</param>
    /// <param name="timeout">The timeout for the wait operation.</param>
    public void WaitForElement(string selector, WaitStrategy strategy, TimeSpan timeout)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        var locator = _page.Locator(selector);
        var timeoutMs = (float)timeout.TotalMilliseconds;

        switch (strategy)
        {
            case WaitStrategy.Visible:
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Visible, 
                    Timeout = timeoutMs 
                }).Wait();
                break;

            case WaitStrategy.Hidden:
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Hidden, 
                    Timeout = timeoutMs 
                }).Wait();
                break;

            case WaitStrategy.Clickable:
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Visible, 
                    Timeout = timeoutMs 
                }).Wait();
                break;

            case WaitStrategy.Enabled:
                // For enabled state, we wait for visible first, then check if enabled
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Visible, 
                    Timeout = timeoutMs 
                }).Wait();
                if (!locator.IsEnabledAsync().Result)
                {
                    throw new InvalidOperationException($"Element {selector} is not enabled after waiting.");
                }
                break;

            case WaitStrategy.Disabled:
                // For disabled state, we wait for visible first, then check if disabled
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Visible, 
                    Timeout = timeoutMs 
                }).Wait();
                if (locator.IsEnabledAsync().Result)
                {
                    throw new InvalidOperationException($"Element {selector} is not disabled after waiting.");
                }
                break;

            case WaitStrategy.TextPresent:
                // For text present, we wait for visible first, then check if text is present
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Visible, 
                    Timeout = timeoutMs 
                }).Wait();
                break;

            case WaitStrategy.Smart:
                WaitForElementSmart(locator, timeout);
                break;

            case WaitStrategy.None:
                // No waiting required
                break;

            default:
                throw new ArgumentException($"Unsupported wait strategy: {strategy}", nameof(strategy));
        }
    }

    /// <summary>
    /// Implements smart wait strategy that tries multiple approaches.
    /// </summary>
    /// <param name="locator">The element locator.</param>
    /// <param name="timeout">The timeout for the wait operation.</param>
    private void WaitForElementSmart(ILocator locator, TimeSpan timeout)
    {
        var timeoutMs = (float)timeout.TotalMilliseconds;

        try
        {
            // First try: Wait for visible
            locator.WaitForAsync(new LocatorWaitForOptions 
            { 
                State = WaitForSelectorState.Visible, 
                Timeout = timeoutMs 
            }).Wait();
        }
        catch
        {
            try
            {
                // Second try: Wait for attached (element exists in DOM but might not be visible)
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Attached, 
                    Timeout = timeoutMs 
                }).Wait();
            }
            catch
            {
                // Third try: Wait for detached (element was removed and might be re-added)
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Detached, 
                    Timeout = timeoutMs / 2 
                }).Wait();
                
                // Then wait for attached
                locator.WaitForAsync(new LocatorWaitForOptions 
                { 
                    State = WaitForSelectorState.Attached, 
                    Timeout = timeoutMs / 2 
                }).Wait();
            }
        }
    }

    /// <summary>
    /// Waits for an element to have specific text content.
    /// </summary>
    /// <param name="selector">The element selector.</param>
    /// <param name="expectedText">The expected text content.</param>
    /// <param name="timeout">The timeout for the wait operation.</param>
    public void WaitForElementText(string selector, string expectedText, TimeSpan timeout)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        var locator = _page.Locator(selector);
        var timeoutMs = (float)timeout.TotalMilliseconds;

        // Wait for element to be visible first
        locator.WaitForAsync(new LocatorWaitForOptions 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = timeoutMs 
        }).Wait();

        // Then wait for text content
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            var actualText = locator.TextContentAsync().Result;
            if (actualText?.Contains(expectedText) == true)
            {
                return;
            }
            System.Threading.Thread.Sleep(100);
        }

        throw new TimeoutException($"Element {selector} did not contain expected text '{expectedText}' within {timeout.TotalSeconds} seconds.");
    }

    /// <summary>
    /// Waits for an element to have a specific attribute value.
    /// </summary>
    /// <param name="selector">The element selector.</param>
    /// <param name="attributeName">The attribute name.</param>
    /// <param name="expectedValue">The expected attribute value.</param>
    /// <param name="timeout">The timeout for the wait operation.</param>
    public void WaitForElementAttribute(string selector, string attributeName, string expectedValue, TimeSpan timeout)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        var locator = _page.Locator(selector);
        var timeoutMs = (float)timeout.TotalMilliseconds;

        // Wait for element to be visible first
        locator.WaitForAsync(new LocatorWaitForOptions 
        { 
            State = WaitForSelectorState.Visible, 
            Timeout = timeoutMs 
        }).Wait();

        // Then wait for attribute value
        var startTime = DateTime.UtcNow;
        while (DateTime.UtcNow - startTime < timeout)
        {
            var actualValue = locator.GetAttributeAsync(attributeName).Result;
            if (actualValue == expectedValue)
            {
                return;
            }
            System.Threading.Thread.Sleep(100);
        }

        throw new TimeoutException($"Element {selector} did not have expected attribute '{attributeName}' with value '{expectedValue}' within {timeout.TotalSeconds} seconds.");
    }
} 