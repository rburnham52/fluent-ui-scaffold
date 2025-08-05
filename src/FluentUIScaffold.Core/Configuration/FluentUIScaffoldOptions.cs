// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Configuration options for FluentUIScaffold.
    /// </summary>
    public class FluentUIScaffoldOptions
    {
        public Uri? BaseUrl { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
        public WaitStrategy WaitStrategy { get; set; } = WaitStrategy.Smart;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public string? ScreenshotPath { get; set; }
        /// <summary>
        /// Framework-specific options (e.g., Playwright, Selenium, Appium, etc.)
        /// </summary>
        public IDictionary<string, object> FrameworkOptions { get; } = new Dictionary<string, object>();
        public Type? FrameworkType { get; set; }
        public PageValidationStrategy PageValidationStrategy { get; set; } = PageValidationStrategy.Configurable;
        public bool AutomaticScreenshots { get; set; }
        public bool HeadlessMode { get; set; }
        public int WindowWidth { get; set; } = 1920;
        public int WindowHeight { get; set; } = 1080;
        public bool ImplicitWaits { get; set; }
        public TimeSpan DefaultWaitTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public int RetryCount { get; set; }
        public bool DetailedLogging { get; set; }
        public string? UserAgent { get; set; }
        public bool JavaScriptEnabled { get; set; } = true;
        public bool AcceptInsecureCertificates { get; set; }
        public TimeSpan PageLoadTimeout { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan ScriptTimeout { get; set; } = TimeSpan.FromSeconds(30);

        // Additional properties for sample tests
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan DefaultRetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);
        public bool CaptureScreenshotsOnFailure { get; set; } = true;

        // Web server configuration
        public bool EnableWebServerLaunch { get; set; } = false;
        public string? WebServerProjectPath { get; set; }
        public bool ReuseExistingServer { get; set; } = false;
    }
}
