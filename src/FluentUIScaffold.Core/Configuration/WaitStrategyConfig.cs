// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

namespace FluentUIScaffold.Core.Configuration;

/// <summary>
/// Configuration for wait strategies used in element interactions.
/// </summary>
public class WaitStrategyConfig {
    /// <summary>
    /// Gets or sets the timeout duration for wait operations.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the interval between retry attempts during wait operations.
    /// </summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Gets or sets the expected text to wait for when using TextPresent strategy.
    /// </summary>
    public string? ExpectedText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to ignore exceptions during wait operations.
    /// </summary>
    public bool IgnoreExceptions { get; set; }
}
