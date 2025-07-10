// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Core interface for UI driver implementations that abstract underlying testing frameworks.
/// </summary>
public interface IUIDriver : IDisposable {
    /// <summary>
    /// Gets the current URL of the browser.
    /// </summary>
    Uri? CurrentUrl { get; }

    /// <summary>
    /// Clicks an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void Click(string selector);

    /// <summary>
    /// Types text into an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="text">The text to type into the element.</param>
    void Type(string selector, string text);

    /// <summary>
    /// Selects an option from a dropdown element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the dropdown element.</param>
    /// <param name="value">The value of the option to select.</param>
    void SelectOption(string selector, string value);

    /// <summary>
    /// Gets the text content of an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The text content of the element.</returns>
    string GetText(string selector);

    /// <summary>
    /// Checks if an element identified by the specified selector is visible.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>True if the element is visible; otherwise, false.</returns>
    bool IsVisible(string selector);

    /// <summary>
    /// Checks if an element identified by the specified selector is enabled.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>True if the element is enabled; otherwise, false.</returns>
    bool IsEnabled(string selector);

    /// <summary>
    /// Waits for an element identified by the specified selector to be present in the DOM.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void WaitForElement(string selector);

    /// <summary>
    /// Waits for an element identified by the specified selector to become visible.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void WaitForElementToBeVisible(string selector);

    /// <summary>
    /// Waits for an element identified by the specified selector to become hidden.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void WaitForElementToBeHidden(string selector);

    /// <summary>
    /// Navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    void NavigateToUrl(Uri url);

    /// <summary>
    /// Navigates to a page component of the specified type.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target page component.</typeparam>
    /// <returns>The target page component instance.</returns>
    TTarget NavigateTo<TTarget>() where TTarget : class;

    /// <summary>
    /// Gets the underlying framework-specific driver instance.
    /// </summary>
    /// <typeparam name="TDriver">The type of the framework-specific driver.</typeparam>
    /// <returns>The framework-specific driver instance.</returns>
    TDriver GetFrameworkDriver<TDriver>() where TDriver : class;
}
