// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Exceptions;

/// <summary>
/// Exception thrown when an element operation times out.
/// </summary>
public class ElementTimeoutException : FluentUIScaffoldException
{
    /// <summary>
    /// Gets the selector of the element that timed out.
    /// </summary>
    public string Selector { get; }

    /// <summary>
    /// Gets the timeout duration that was exceeded.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="selector">The selector of the element that timed out.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    public ElementTimeoutException(string message, string selector, TimeSpan timeout) : base(message)
    {
        Selector = selector ?? string.Empty;
        Timeout = timeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="selector">The selector of the element that timed out.</param>
    /// <param name="timeout">The timeout duration that was exceeded.</param>
    /// <param name="innerException">The inner exception.</param>
    public ElementTimeoutException(string message, string selector, TimeSpan timeout, Exception innerException) 
        : base(message, innerException)
    {
        Selector = selector ?? string.Empty;
        Timeout = timeout;
    }
} 