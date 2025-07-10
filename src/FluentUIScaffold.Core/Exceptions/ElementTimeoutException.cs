// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Runtime.Serialization;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ElementTimeoutException(string message) : base(message)
    {
        Selector = string.Empty;
        Timeout = TimeSpan.Zero;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTimeoutException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ElementTimeoutException(string message, Exception innerException) : base(message, innerException)
    {
        Selector = string.Empty;
        Timeout = TimeSpan.Zero;
    }

    protected ElementTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Selector = string.Empty;
        Timeout = TimeSpan.Zero;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementTimeoutException"/> class.
    /// </summary>
    public ElementTimeoutException() : base()
    {
        Selector = string.Empty;
        Timeout = TimeSpan.Zero;
    }
}
