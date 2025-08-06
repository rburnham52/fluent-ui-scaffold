// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Configuration options for FluentUIScaffold.
    /// </summary>
    public class FluentUIScaffoldOptions
    {
        /// <summary>
        /// The base URL for the application under test.
        /// </summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// The default timeout for wait operations.
        /// </summary>
        public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// The logging level for the framework.
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        /// <summary>
        /// Whether to run the browser in headless mode.
        /// </summary>
        public bool HeadlessMode { get; set; } = true;

        /// <summary>
        /// Path to the ASP.NET Core project for web server launching.
        /// </summary>
        public string? WebServerProjectPath { get; set; }

        /// <summary>
        /// Gets or sets whether to run in debug mode.
        /// When enabled, this overrides HeadlessMode to false and sets SlowMo to 1000ms.
        /// Automatically enables when a debugger is attached unless explicitly set to false.
        /// </summary>
        public bool DebugMode
        {
            get => _debugMode ?? System.Diagnostics.Debugger.IsAttached;
            set => _debugMode = value;
        }

        private bool? _debugMode;
    }
}
