using System;
using System.Collections.Generic;
using System.Linq;

using FluentUIScaffold.Core.Exceptions;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Builder for creating and configuring FluentUIScaffoldOptions instances.
    /// Provides a fluent API for setting up test configuration.
    /// </summary>
    public class FluentUIScaffoldOptionsBuilder
    {
        private readonly FluentUIScaffoldOptions _options;

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldOptionsBuilder.
        /// </summary>
        public FluentUIScaffoldOptionsBuilder()
        {
            _options = new FluentUIScaffoldOptions();
        }

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldOptionsBuilder with existing options.
        /// </summary>
        /// <param name="options">The existing options to build upon</param>
        public FluentUIScaffoldOptionsBuilder(FluentUIScaffoldOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Sets the base URL for the application under test.
        /// </summary>
        /// <param name="baseUrl">The base URL</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithBaseUrl(Uri baseUrl)
        {
            if (baseUrl == null)
                throw new FluentUIScaffoldValidationException("Base URL cannot be null", nameof(baseUrl));
            _options.BaseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Sets the default timeout for UI operations.
        /// </summary>
        /// <param name="timeout">The timeout duration</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDefaultWaitTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Default wait timeout must be greater than zero", nameof(timeout));

            _options.DefaultWaitTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the default timeout for UI operations in debug mode.
        /// </summary>
        /// <param name="timeout">The timeout duration</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDefaultWaitTimeoutDebug(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Default wait timeout debug must be greater than zero", nameof(timeout));

            _options.DefaultWaitTimeoutDebug = timeout;
            return this;
        }

        /// <summary>
        /// Sets whether to enable debug mode.
        /// </summary>
        /// <param name="enabled">True to enable debug mode, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDebugMode(bool enabled = true)
        {
            _options.EnableDebugMode = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether to run the browser in headless mode.
        /// When not set, the framework will determine headless mode automatically based on debug mode and CI environment.
        /// </summary>
        /// <param name="headless">True for headless mode, false for visible browser, null for automatic determination</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithHeadlessMode(bool? headless)
        {
            _options.HeadlessMode = headless;
            return this;
        }

        /// <summary>
        /// Sets the SlowMo value for browser operations in milliseconds.
        /// When not set, the framework will determine SlowMo automatically based on debug mode.
        /// </summary>
        /// <param name="slowMo">SlowMo value in milliseconds, null for automatic determination</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithSlowMo(int? slowMo)
        {
            if (slowMo.HasValue && slowMo.Value < 0)
                throw new FluentUIScaffoldValidationException("SlowMo value must be non-negative", nameof(slowMo));

            _options.SlowMo = slowMo;
            return this;
        }

        /// <summary>
        /// Sets the path to the ASP.NET Core project for web server launching.
        /// </summary>
        /// <param name="projectPath">The path to the project</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithWebServerProjectPath(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new FluentUIScaffoldValidationException("Web server project path cannot be null or empty", nameof(projectPath));

            _options.WebServerProjectPath = projectPath;
            return this;
        }

        /// <summary>
        /// Sets the server configuration for web server launching.
        /// </summary>
        /// <param name="serverConfiguration">The server configuration</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithServerConfiguration(ServerConfiguration serverConfiguration)
        {
            if (serverConfiguration == null)
                throw new FluentUIScaffoldValidationException("Server configuration cannot be null", nameof(serverConfiguration));

            _options.ServerConfiguration = serverConfiguration;
            return this;
        }

        /// <summary>
        /// Sets whether to enable project detection.
        /// </summary>
        /// <param name="enabled">True to enable project detection, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithProjectDetection(bool enabled = true)
        {
            _options.EnableProjectDetection = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether to enable web server launching.
        /// </summary>
        /// <param name="enabled">True to enable web server launching, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithWebServerLaunch(bool enabled = true)
        {
            _options.EnableWebServerLaunch = enabled;
            return this;
        }

        /// <summary>
        /// Sets the log level for web server launcher operations.
        /// </summary>
        /// <param name="logLevel">The log level for web server operations</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithWebServerLogLevel(LogLevel logLevel)
        {
            _options.WebServerLogLevel = logLevel;
            return this;
        }

        /// <summary>
        /// Builds and returns the configured FluentUIScaffoldOptions.
        /// </summary>
        /// <returns>The configured options instance</returns>
        public FluentUIScaffoldOptions Build()
        {
            ValidateOptions();
            return _options;
        }

        private void ValidateOptions()
        {
            var errors = new List<string>();

            // Only validate timeout - BaseUrl validation should happen when actually using the options
            if (_options.DefaultWaitTimeout <= TimeSpan.Zero)
            {
                errors.Add("Default wait timeout must be greater than zero");
            }

            if (errors.Count > 0)
            {
                throw new FluentUIScaffoldValidationException(
                    $"Configuration validation failed: {string.Join("; ", errors)}",
                    "Configuration");
            }
        }
    }
}
