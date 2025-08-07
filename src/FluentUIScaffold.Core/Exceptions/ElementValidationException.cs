// Copyright (c) FluentUIScaffold. All rights reserved.

using System;
using System.Runtime.Serialization;

namespace FluentUIScaffold.Core.Exceptions;

/// <summary>
/// Exception thrown when element validation fails.
/// </summary>
public class ElementValidationException : FluentUIScaffoldException
{
    /// <summary>
    /// Gets the selector of the element that failed validation.
    /// </summary>
    public string Selector { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="selector">The selector of the element that failed validation.</param>
    public ElementValidationException(string message, string selector) : base(message)
    {
        Selector = selector ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="selector">The selector of the element that failed validation.</param>
    /// <param name="innerException">The inner exception.</param>
    public ElementValidationException(string message, string selector, Exception innerException)
        : base(message, innerException)
    {
        Selector = selector ?? string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ElementValidationException(string message) : base(message)
    {
        Selector = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementValidationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ElementValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Selector = string.Empty;
    }

    protected ElementValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Selector = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementValidationException"/> class.
    /// </summary>
    public ElementValidationException() : base()
    {
        Selector = string.Empty;
    }
}
