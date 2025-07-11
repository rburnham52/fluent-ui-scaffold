// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright;

/// <summary>
/// Playwright plugin implementation for FluentUIScaffold.
/// </summary>
public class PlaywrightPlugin : IUITestingFrameworkPlugin
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public string Name => "Playwright";

    /// <summary>
    /// Gets the version of the plugin.
    /// </summary>
    public string Version => "1.0.0";

    /// <summary>
    /// Gets the list of supported driver types that this plugin can handle.
    /// </summary>
    public IReadOnlyList<Type> SupportedDriverTypes => new[] { typeof(PlaywrightDriver) };

    /// <summary>
    /// Determines whether this plugin can handle the specified driver type.
    /// </summary>
    /// <param name="driverType">The type of driver to check.</param>
    /// <returns>True if this plugin can handle the driver type; otherwise, false.</returns>
    public bool CanHandle(Type driverType) => driverType == typeof(PlaywrightDriver);

    /// <summary>
    /// Creates a Playwright driver instance using the specified options.
    /// </summary>
    /// <param name="options">The configuration options for the driver.</param>
    /// <returns>A new Playwright driver instance.</returns>
    public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
    {
        return new PlaywrightDriver(options);
    }

    /// <summary>
    /// Configures the dependency injection services for this plugin.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<PlaywrightPlugin>();
        services.AddTransient<PlaywrightDriver>();
        services.AddSingleton<Microsoft.Playwright.IPlaywright>(provider => Microsoft.Playwright.Playwright.CreateAsync().Result);
    }
} 