// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

namespace FluentUIScaffold.Core;

/// <summary>
/// Concrete implementation of the IElement interface.
/// </summary>
public class Element : IElement
{
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    private readonly Func<bool>? _customWaitCondition;
    private readonly Dictionary<string, string> _attributes;

    /// <summary>
    /// Initializes a new instance of the <see cref="Element"/> class.
    /// </summary>
    public Element(string selector, IUIDriver driver, FluentUIScaffoldOptions options, 
        TimeSpan timeout, WaitStrategy waitStrategy, string description, TimeSpan retryInterval,
        Func<bool>? customWaitCondition, Dictionary<string, string> attributes)
    {
        Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        Timeout = timeout;
        WaitStrategy = waitStrategy;
        Description = description;
        RetryInterval = retryInterval;
        _customWaitCondition = customWaitCondition;
        _attributes = attributes ?? new Dictionary<string, string>();
    }

    /// <inheritdoc/>
    public string Selector { get; }

    /// <inheritdoc/>
    public string Description { get; }

    /// <inheritdoc/>
    public TimeSpan Timeout { get; }

    /// <inheritdoc/>
    public WaitStrategy WaitStrategy { get; }

    /// <inheritdoc/>
    public TimeSpan RetryInterval { get; }

    /// <inheritdoc/>
    public void Click()
    {
        WaitFor();
        _driver.Click(Selector);
    }

    /// <inheritdoc/>
    public void Type(string text)
    {
        WaitFor();
        _driver.Type(Selector, text);
    }

    /// <inheritdoc/>
    public void Select(string value)
    {
        WaitFor();
        _driver.SelectOption(Selector, value);
    }

    /// <inheritdoc/>
    public string GetText()
    {
        WaitFor();
        return _driver.GetText(Selector);
    }

    /// <inheritdoc/>
    public bool IsVisible()
    {
        return _driver.IsVisible(Selector);
    }

    /// <inheritdoc/>
    public bool IsEnabled()
    {
        return _driver.IsEnabled(Selector);
    }

    /// <inheritdoc/>
    public bool IsDisplayed()
    {
        return IsVisible();
    }

    /// <inheritdoc/>
    public void WaitFor()
    {
        switch (WaitStrategy)
        {
            case WaitStrategy.None:
                // No waiting required
                break;
            case WaitStrategy.Visible:
                WaitForVisible();
                break;
            case WaitStrategy.Hidden:
                WaitForHidden();
                break;
            case WaitStrategy.Clickable:
                WaitForClickable();
                break;
            case WaitStrategy.Enabled:
                WaitForEnabled();
                break;
            case WaitStrategy.Disabled:
                WaitForDisabled();
                break;
            case WaitStrategy.TextPresent:
                // This would need additional configuration for expected text
                WaitForVisible();
                break;
            case WaitStrategy.Smart:
                // Framework-specific intelligent waiting
                WaitForVisible();
                break;
            default:
                throw new ArgumentException($"Unsupported wait strategy: {WaitStrategy}");
        }
    }

    /// <inheritdoc/>
    public void WaitForVisible()
    {
        _driver.WaitForElementToBeVisible(Selector);
    }

    /// <inheritdoc/>
    public void WaitForHidden()
    {
        _driver.WaitForElementToBeHidden(Selector);
    }

    /// <inheritdoc/>
    public void WaitForClickable()
    {
        // For now, wait for visible and enabled
        WaitForVisible();
        WaitForEnabled();
    }

    /// <inheritdoc/>
    public void WaitForEnabled()
    {
        // This would need to be implemented in the driver
        // For now, just wait for visible
        WaitForVisible();
    }

    /// <inheritdoc/>
    public void WaitForDisabled()
    {
        // This would need to be implemented in the driver
        // For now, just wait for visible
        WaitForVisible();
    }

    /// <inheritdoc/>
    public bool Exists()
    {
        try
        {
            // Try to get text to check if element exists
            _driver.GetText(Selector);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool IsSelected()
    {
        // This would need to be implemented in the driver
        // For now, return false as a placeholder
        return false;
    }

    /// <inheritdoc/>
    public string GetAttribute(string attributeName)
    {
        // This would need to be implemented in the driver
        // For now, return empty string as a placeholder
        return string.Empty;
    }

    /// <inheritdoc/>
    public string GetCssValue(string propertyName)
    {
        // This would need to be implemented in the driver
        // For now, return empty string as a placeholder
        return string.Empty;
    }
} 