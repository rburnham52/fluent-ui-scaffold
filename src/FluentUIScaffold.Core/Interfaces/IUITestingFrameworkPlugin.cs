// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Interface for UI testing framework plugins that provide driver implementations.
/// </summary>
public interface IUITestingFrameworkPlugin {
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the list of supported driver types that this plugin can handle.
    /// </summary>
    IReadOnlyList<Type> SupportedDriverTypes { get; }

    /// <summary>
    /// Determines whether this plugin can handle the specified driver type.
    /// </summary>
    /// <param name="driverType">The type of driver to check.</param>
    /// <returns>True if this plugin can handle the driver type; otherwise, false.</returns>
    bool CanHandle(Type driverType);

    /// <summary>
    /// Creates a UI driver instance using the specified options.
    /// </summary>
    /// <param name="options">The configuration options for the driver.</param>
    /// <returns>A new UI driver instance.</returns>
    IUIDriver CreateDriver(FluentUIScaffoldOptions options);

    /// <summary>
    /// Configures the dependency injection services for this plugin.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    void ConfigureServices(IServiceCollection services);
}
