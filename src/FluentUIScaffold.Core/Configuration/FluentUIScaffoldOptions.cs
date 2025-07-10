// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration;

/// <summary>
/// Configuration options for FluentUIScaffold.
/// </summary>
public class FluentUIScaffoldOptions
{
    /// <summary>
    /// Gets or sets the base URL for the application under test.
    /// </summary>
    public Uri? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the default timeout for operations.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the default retry interval for operations.
    /// </summary>
    public TimeSpan DefaultRetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the default wait strategy.
    /// </summary>
    public WaitStrategy DefaultWaitStrategy { get; set; } = WaitStrategy.Smart;

    /// <summary>
    /// Gets or sets the page validation strategy.
    /// </summary>
    public PageValidationStrategy PageValidationStrategy { get; set; } = PageValidationStrategy.Configurable;

    /// <summary>
    /// Gets or sets the log level for the framework.
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Gets or sets a value indicating whether to capture screenshots on failure.
    /// </summary>
    public bool CaptureScreenshotsOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to capture the DOM state on failure.
    /// </summary>
    public bool CaptureDOMStateOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the path where screenshots will be saved.
    /// </summary>
    public string ScreenshotPath { get; set; } = "./screenshots";

    /// <summary>
    /// Gets framework-specific options.
    /// </summary>
    public Dictionary<string, object> FrameworkSpecificOptions { get; } = new Dictionary<string, object>();
} 