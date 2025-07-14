// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Interface for verification context that provides fluent verification methods.
/// </summary>
public interface IVerificationContext
{
    /// <summary>
    /// Verifies that an element is visible.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext ElementIsVisible(string selector);

    /// <summary>
    /// Verifies that an element is hidden.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext ElementIsHidden(string selector);

    /// <summary>
    /// Verifies that an element is enabled.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext ElementIsEnabled(string selector);

    /// <summary>
    /// Verifies that an element is disabled.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext ElementIsDisabled(string selector);

    /// <summary>
    /// Verifies that an element contains the specified text.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="text">The text that the element should contain.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext ElementContainsText(string selector, string text);

    /// <summary>
    /// Verifies that an element has the specified attribute with the specified value.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="attribute">The name of the attribute to check.</param>
    /// <param name="value">The expected value of the attribute.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext ElementHasAttribute(string selector, string attribute, string value);

    /// <summary>
    /// Verifies that the current URL matches the specified pattern.
    /// </summary>
    /// <param name="pattern">The URL pattern to match against.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext UrlMatches(string pattern);

    /// <summary>
    /// Verifies that the page title contains the specified text.
    /// </summary>
    /// <param name="text">The text that the title should contain.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext TitleContains(string text);

    /// <summary>
    /// Verifies a custom condition.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <param name="description">A description of the verification.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext That(Func<bool> condition, string description);

    /// <summary>
    /// Verifies a custom condition with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value to verify.</typeparam>
    /// <param name="actual">A function that returns the actual value.</param>
    /// <param name="condition">The condition to verify against the actual value.</param>
    /// <param name="description">A description of the verification.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext That<T>(Func<T> actual, Func<T, bool> condition, string description);
}
