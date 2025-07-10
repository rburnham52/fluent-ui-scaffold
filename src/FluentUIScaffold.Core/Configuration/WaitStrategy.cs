// Copyright (c) FluentUIScaffold. All rights reserved.

namespace FluentUIScaffold.Core.Configuration;

/// <summary>
/// Specifies the wait strategy for element interactions.
/// </summary>
public enum WaitStrategy
{
    /// <summary>
    /// No waiting - immediate execution.
    /// </summary>
    None,

    /// <summary>
    /// Wait for element to become visible.
    /// </summary>
    Visible,

    /// <summary>
    /// Wait for element to become hidden.
    /// </summary>
    Hidden,

    /// <summary>
    /// Wait for element to become clickable.
    /// </summary>
    Clickable,

    /// <summary>
    /// Wait for element to become enabled.
    /// </summary>
    Enabled,

    /// <summary>
    /// Wait for element to become disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// Wait for specific text to be present in element.
    /// </summary>
    TextPresent,

    /// <summary>
    /// Framework-specific intelligent waiting strategy.
    /// </summary>
    Smart
}
