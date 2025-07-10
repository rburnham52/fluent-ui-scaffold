// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FluentUIScaffold.Core.Exceptions;

public class FluentUIScaffoldException : Exception
{
    public string? ScreenshotPath { get; set; }
    public string? DOMState { get; set; }
    public Uri? CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; } = new Dictionary<string, object>();

    public FluentUIScaffoldException() { }
    public FluentUIScaffoldException(string message) : base(message) { }
    public FluentUIScaffoldException(string message, Exception inner) : base(message, inner) { }
    protected FluentUIScaffoldException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}

public class InvalidPageException : FluentUIScaffoldException
{
    public InvalidPageException() { }
    public InvalidPageException(string message) : base(message) { }
    public InvalidPageException(string message, Exception inner) : base(message, inner) { }
#pragma warning disable SYSLIB0051 // Type or member is obsolete
    protected InvalidPageException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#pragma warning restore SYSLIB0051
}

public class ElementNotFoundException : FluentUIScaffoldException
{
    public ElementNotFoundException() { }
    public ElementNotFoundException(string message) : base(message) { }
    public ElementNotFoundException(string message, Exception inner) : base(message, inner) { }
#pragma warning disable SYSLIB0051 // Type or member is obsolete
    protected ElementNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#pragma warning restore SYSLIB0051
}

public class TimeoutException : FluentUIScaffoldException
{
    public TimeoutException() { }
    public TimeoutException(string message) : base(message) { }
    public TimeoutException(string message, Exception inner) : base(message, inner) { }
#pragma warning disable SYSLIB0051 // Type or member is obsolete
    protected TimeoutException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#pragma warning restore SYSLIB0051
}
