// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

namespace FluentUIScaffold.Core.Exceptions;

public class FluentUIScaffoldException : Exception {
    public string? ScreenshotPath { get; set; }
    public string? DOMState { get; set; }
    public Uri? CurrentUrl { get; set; }
    public Dictionary<string, object> Context { get; } = new Dictionary<string, object>();

    public FluentUIScaffoldException() { }
    public FluentUIScaffoldException(string message) : base(message) { }
    public FluentUIScaffoldException(string message, Exception inner) : base(message, inner) { }
}

public class InvalidPageException : FluentUIScaffoldException {
    public InvalidPageException() { }
    public InvalidPageException(string message) : base(message) { }
    public InvalidPageException(string message, Exception inner) : base(message, inner) { }
}

public class ElementNotFoundException : FluentUIScaffoldException {
    public ElementNotFoundException() { }
    public ElementNotFoundException(string message) : base(message) { }
    public ElementNotFoundException(string message, Exception inner) : base(message, inner) { }
}

public class TimeoutException : FluentUIScaffoldException {
    public TimeoutException() { }
    public TimeoutException(string message) : base(message) { }
    public TimeoutException(string message, Exception inner) : base(message, inner) { }
}
