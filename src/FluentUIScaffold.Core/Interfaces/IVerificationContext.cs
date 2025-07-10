// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Interface for verification context that provides fluent verification methods.
/// </summary>
/// <typeparam name="TApp">The type representing the application context.</typeparam>
public interface IVerificationContext<TApp>
{
    /// <summary>
    /// Verifies that an element is visible.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> ElementIsVisible(string selector);

    /// <summary>
    /// Verifies that an element is hidden.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> ElementIsHidden(string selector);

    /// <summary>
    /// Verifies that an element is enabled.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> ElementIsEnabled(string selector);

    /// <summary>
    /// Verifies that an element is disabled.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> ElementIsDisabled(string selector);

    /// <summary>
    /// Verifies that an element contains the specified text.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="text">The text that the element should contain.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> ElementContainsText(string selector, string text);

    /// <summary>
    /// Verifies that an element has the specified attribute with the specified value.
    /// </summary>
    /// <param name="selector">The CSS selector or other identifier for the element.</param>
    /// <param name="attribute">The name of the attribute to check.</param>
    /// <param name="value">The expected value of the attribute.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> ElementHasAttribute(string selector, string attribute, string value);

    /// <summary>
    /// Verifies that the current page is of the specified type.
    /// </summary>
    /// <typeparam name="TPage">The type of the expected page.</typeparam>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> CurrentPageIs<TPage>() where TPage : class;

    /// <summary>
    /// Verifies that the current URL matches the specified pattern.
    /// </summary>
    /// <param name="pattern">The URL pattern to match against.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> UrlMatches(string pattern);

    /// <summary>
    /// Verifies that the page title contains the specified text.
    /// </summary>
    /// <param name="text">The text that the title should contain.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> TitleContains(string text);

    /// <summary>
    /// Verifies a custom condition.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <param name="description">A description of the verification.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> That(Func<bool> condition, string description);

    /// <summary>
    /// Verifies a custom condition with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value to verify.</typeparam>
    /// <param name="actual">A function that returns the actual value.</param>
    /// <param name="condition">The condition to verify against the actual value.</param>
    /// <param name="description">A description of the verification.</param>
    /// <returns>The verification context for method chaining.</returns>
    IVerificationContext<TApp> That<T>(Func<T> actual, Func<T, bool> condition, string description);
}
