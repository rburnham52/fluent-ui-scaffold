// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

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
        /// Gets or sets the default wait timeout for element operations in debug mode.
        /// </summary>
        public TimeSpan DefaultWaitTimeoutDebug { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets whether to enable debug mode.
        /// </summary>
        public bool EnableDebugMode { get; set; } = false;

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
        /// Gets or sets the path to the web server project.
        /// </summary>
        public string? WebServerProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the server configuration for web server launching.
        /// </summary>
        public ServerConfiguration? ServerConfiguration { get; set; }

        /// <summary>
        /// Gets or sets whether to enable project detection.
        /// </summary>
        public bool EnableProjectDetection { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable web server launching.
        /// </summary>
        public bool EnableWebServerLaunch { get; set; } = false;

        /// <summary>
        /// Gets or sets the log level for web server launcher operations.
        /// </summary>
        public LogLevel WebServerLogLevel { get; set; } = LogLevel.Information;
    }
}
