// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

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
        /// When null, the framework will resolve this at Build() time:
        /// debugger attached = false (visible), otherwise true (headless).
        /// </summary>
        public bool? HeadlessMode { get; set; } = null;

        /// <summary>
        /// Gets or sets the SlowMo value for browser operations in milliseconds.
        /// When null, the framework will determine SlowMo automatically based on headless mode.
        /// </summary>
        public int? SlowMo { get; set; } = null;

        /// <summary>
        /// Gets or sets the explicitly requested driver type.
        /// When set, the plugin selection will prefer plugins that can handle this driver type.
        /// </summary>
        public Type? RequestedDriverType { get; set; }

        /// <summary>
        /// Gets or sets whether to automatically discover and register page classes from loaded assemblies.
        /// When true, the framework will scan assemblies for classes inheriting from Page&lt;TSelf&gt;.
        /// </summary>
        public bool AutoDiscoverPages { get; set; }

        /// <summary>
        /// Custom environment variables applied to hosted applications.
        /// Applied after framework defaults (EnvironmentName, SpaProxy), so user values win.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The logical environment name (e.g., "Testing", "Development").
        /// Default: "Testing" — the framework assumes test execution.
        /// </summary>
        public string EnvironmentName { get; set; } = "Testing";

        /// <summary>
        /// Whether to enable the ASP.NET SPA dev server proxy.
        /// Default: false — tests use prebuilt static assets.
        /// </summary>
        public bool SpaProxyEnabled { get; set; } = false;
    }
}
