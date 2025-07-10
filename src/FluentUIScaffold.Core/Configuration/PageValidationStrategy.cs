// Copyright (c) FluentUIScaffold. All rights reserved.

namespace FluentUIScaffold.Core.Configuration;

/// <summary>
/// Specifies the strategy for validating page navigation.
/// </summary>
public enum PageValidationStrategy
{
    /// <summary>
    /// No page validation is performed.
    /// </summary>
    None,

    /// <summary>
    /// Page validation is configurable per page component.
    /// </summary>
    Configurable,

    /// <summary>
    /// Page validation is always performed for all page components.
    /// </summary>
    Always
} 