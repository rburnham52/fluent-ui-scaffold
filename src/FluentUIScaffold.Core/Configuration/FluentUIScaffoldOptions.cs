// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Configuration options for FluentUIScaffold.
    /// </summary>
    public class FluentUIScaffoldOptions
    {
        /// <summary>
        /// Gets or sets the base URL for the web application.
        /// </summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the default wait timeout for element operations.
        /// </summary>
        public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets whether to run the browser in headless mode.
        /// When null, the framework will determine headless mode automatically based on debug mode and CI environment.
        /// </summary>
        public bool? HeadlessMode { get; set; } = null;

        /// <summary>
        /// Gets or sets the SlowMo value for browser operations in milliseconds.
        /// When null, the framework will determine SlowMo automatically based on debug mode.
        /// </summary>
        public int? SlowMo { get; set; } = null;

        /// <summary>
        /// Gets or sets the explicitly requested driver type.
        /// When set, the plugin selection will prefer plugins that can handle this driver type.
        /// </summary>
        public Type? RequestedDriverType { get; set; }

        /// <summary>
        /// Gets or sets the server configuration for managed server lifecycle.
        /// When set, the framework will use the new server manager to start and manage the server.
        /// </summary>
        public LaunchPlan? ServerConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the service provider for dependency injection.
        /// Used for accessing server manager and other services.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; }

        /// <summary>
        /// Gets the list of registered plugins.
        /// </summary>
        public List<IUITestingFrameworkPlugin> Plugins { get; } = new List<IUITestingFrameworkPlugin>();
    }
}
