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
    }
}
