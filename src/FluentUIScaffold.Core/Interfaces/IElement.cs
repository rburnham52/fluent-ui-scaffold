// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using FluentUIScaffold.Core.Configuration;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Represents a UI element with fluent configuration and interaction capabilities.
/// </summary>
public interface IElement
{
    /// <summary>
    /// Gets the CSS selector or locator for this element.
    /// </summary>
    string Selector { get; }

    /// <summary>
    /// Gets the human-readable description of this element.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the timeout duration for element operations.
    /// </summary>
    TimeSpan Timeout { get; }

    /// <summary>
    /// Gets the wait strategy for this element.
    /// </summary>
    WaitStrategy WaitStrategy { get; }

    /// <summary>
    /// Gets the retry interval for wait operations.
    /// </summary>
    TimeSpan RetryInterval { get; }

    // Core interactions
    /// <summary>
    /// Clicks the element.
    /// </summary>
    void Click();

    /// <summary>
    /// Types text into the element.
    /// </summary>
    /// <param name="text">The text to type.</param>
    void Type(string text);

    /// <summary>
    /// Selects a value from a dropdown or select element.
    /// </summary>
    /// <param name="value">The value to select.</param>
    void SelectOption(string value);

    /// <summary>
    /// Gets the text content of the element.
    /// </summary>
    /// <returns>The text content.</returns>
    string GetText();

    /// <summary>
    /// Checks if the element is visible.
    /// </summary>
    /// <returns>True if the element is visible; otherwise, false.</returns>
    bool IsVisible();

    /// <summary>
    /// Checks if the element is enabled.
    /// </summary>
    /// <returns>True if the element is enabled; otherwise, false.</returns>
    bool IsEnabled();

    /// <summary>
    /// Checks if the element is displayed.
    /// </summary>
    /// <returns>True if the element is displayed; otherwise, false.</returns>
    bool IsDisplayed();

    // Wait operations
    /// <summary>
    /// Waits for the element according to the configured wait strategy.
    /// </summary>
    void WaitFor();

    /// <summary>
    /// Waits for the element to become visible.
    /// </summary>
    void WaitForVisible();

    /// <summary>
    /// Waits for the element to become hidden.
    /// </summary>
    void WaitForHidden();

    /// <summary>
    /// Waits for the element to become clickable.
    /// </summary>
    void WaitForClickable();

    /// <summary>
    /// Waits for the element to become enabled.
    /// </summary>
    void WaitForEnabled();

    /// <summary>
    /// Waits for the element to become disabled.
    /// </summary>
    void WaitForDisabled();

    // State checking
    /// <summary>
    /// Checks if the element exists in the DOM.
    /// </summary>
    /// <returns>True if the element exists; otherwise, false.</returns>
    bool Exists();

    /// <summary>
    /// Checks if the element is selected (for checkboxes, radio buttons, etc.).
    /// </summary>
    /// <returns>True if the element is selected; otherwise, false.</returns>
    bool IsSelected();

    /// <summary>
    /// Gets the value of an attribute on the element.
    /// </summary>
    /// <param name="attributeName">The name of the attribute.</param>
    /// <returns>The attribute value.</returns>
    string GetAttribute(string attributeName);

    /// <summary>
    /// Gets the computed CSS value of a property on the element.
    /// </summary>
    /// <param name="propertyName">The name of the CSS property.</param>
    /// <returns>The CSS property value.</returns>
    string GetCssValue(string propertyName);
}
