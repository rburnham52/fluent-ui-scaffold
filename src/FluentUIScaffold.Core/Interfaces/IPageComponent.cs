// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Interface for page components that represent web pages in the application.
/// </summary>
/// <typeparam name="TApp">The type representing the application context.</typeparam>
public interface IPageComponent<TApp> {
    /// <summary>
    /// Gets the URL pattern that identifies this page.
    /// </summary>
    Uri? UrlPattern { get; }

    /// <summary>
    /// Gets a value indicating whether the page should be validated during navigation.
    /// </summary>
    bool ShouldValidateOnNavigation { get; }

    /// <summary>
    /// Checks if the current page matches this page component.
    /// </summary>
    /// <returns>True if the current page matches this component; otherwise, false.</returns>
    bool IsCurrentPage();

    /// <summary>
    /// Validates that the current page matches this page component.
    /// </summary>
    /// <exception cref="InvalidPageException">Thrown when the current page does not match this component.</exception>
    void ValidateCurrentPage();

    /// <summary>
    /// Navigates to a target page component.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target page component.</typeparam>
    /// <returns>The target page component instance.</returns>
    TTarget NavigateTo<TTarget>() where TTarget : class;

    /// <summary>
    /// Gets the verification context for this page component.
    /// </summary>
    IVerificationContext<TApp> Verify { get; }
}
