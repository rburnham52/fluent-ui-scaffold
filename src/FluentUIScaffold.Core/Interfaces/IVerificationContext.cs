// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Interfaces;

// Legacy non-generic IVerificationContext removed.

/// <summary>
/// Chainable, page-aware verification context that supports richer assertions and fluent return to page via And.
/// </summary>
/// <typeparam name="TPage">The page type this verification context is associated with</typeparam>
public interface IVerificationContext<TPage>
{
    /// <summary>
    /// Verifies that the current URL is exactly the specified URL.
    /// </summary>
    IVerificationContext<TPage> UrlIs(string url);

    /// <summary>
    /// Verifies that the current URL contains the specified segment.
    /// </summary>
    IVerificationContext<TPage> UrlContains(string segment);

    /// <summary>
    /// Verifies that the page title is exactly the specified value.
    /// </summary>
    IVerificationContext<TPage> TitleIs(string title);

    /// <summary>
    /// Verifies that the page title contains the specified text.
    /// </summary>
    IVerificationContext<TPage> TitleContains(string text);

    /// <summary>
    /// Verifies that the element's text contains the specified text.
    /// </summary>
    IVerificationContext<TPage> TextContains(Func<TPage, IElement> elementSelector, string contains);

    /// <summary>
    /// Verifies that the specified element is visible.
    /// </summary>
    IVerificationContext<TPage> Visible(Func<TPage, IElement> elementSelector);

    /// <summary>
    /// Verifies that the specified element is not visible.
    /// </summary>
    IVerificationContext<TPage> NotVisible(Func<TPage, IElement> elementSelector);

    /// <summary>
    /// Returns the page for continued chaining.
    /// </summary>
    TPage And { get; }
}
