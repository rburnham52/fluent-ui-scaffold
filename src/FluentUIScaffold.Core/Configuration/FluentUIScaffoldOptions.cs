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
