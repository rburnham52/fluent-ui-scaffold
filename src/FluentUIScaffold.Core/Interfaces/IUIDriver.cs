// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Core interface for UI driver implementations that abstract underlying testing frameworks.
/// </summary>
public interface IUIDriver : IDisposable
{
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
    /// Gets an attribute value from an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="attributeName">The attribute name to read.</param>
    /// <returns>The attribute value or empty string if not found.</returns>
    string GetAttribute(string selector, string attributeName);

    /// <summary>
    /// Gets the current input value for an element (e.g., input, textarea) identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The input value.</returns>
    string GetValue(string selector);

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
    /// Focuses on an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void Focus(string selector);

    /// <summary>
    /// Hovers over an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void Hover(string selector);

    /// <summary>
    /// Clears the content of an element identified by the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    void Clear(string selector);

    /// <summary>
    /// Gets the page title.
    /// </summary>
    /// <returns>The page title.</returns>
    string GetPageTitle();

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

    /// <summary>
    /// Executes JavaScript in the browser page context and returns a typed result.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="script">The JavaScript expression to evaluate.</param>
    /// <returns>The result of the script evaluation, deserialized to type T.</returns>
    Task<T> ExecuteScriptAsync<T>(string script);

    /// <summary>
    /// Executes JavaScript in the browser page context with no return value.
    /// </summary>
    /// <param name="script">The JavaScript expression to evaluate.</param>
    Task ExecuteScriptAsync(string script);

    /// <summary>
    /// Saves a screenshot of the current page to the specified file path.
    /// </summary>
    /// <param name="filePath">The file path where the screenshot will be saved.</param>
    /// <returns>The screenshot as a byte array.</returns>
    Task<byte[]> TakeScreenshotAsync(string filePath);
}
