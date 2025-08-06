using System;
using System.Collections.Generic;
using System.Linq;

using FluentUIScaffold.Core.Exceptions;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Builder class for creating FluentUIScaffoldOptions with a fluent API.
    /// </summary>
    public class FluentUIScaffoldOptionsBuilder
    {
        private readonly FluentUIScaffoldOptions _options;

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldOptionsBuilder class.
        /// </summary>
        public FluentUIScaffoldOptionsBuilder()
        {
            _options = new FluentUIScaffoldOptions();
        }

        /// <summary>
        /// Initializes a new instance of the FluentUIScaffoldOptionsBuilder class with existing options.
        /// </summary>
        /// <param name="options">Existing options to build upon</param>
        public FluentUIScaffoldOptionsBuilder(FluentUIScaffoldOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Sets the base URL for the application under test.
        /// </summary>
        /// <param name="baseUrl">The base URL to use for navigation</param>
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
        /// Sets the log level for the framework.
        /// </summary>
        /// <param name="level">The log level to use</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithLogLevel(LogLevel level)
        {
            _options.LogLevel = level;
            return this;
        }

        /// <summary>
        /// Sets whether to run the browser in headless mode.
        /// </summary>
        /// <param name="enabled">True to run in headless mode, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithHeadlessMode(bool enabled = true)
        {
            _options.HeadlessMode = enabled;
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
        /// Sets whether to run in debug mode.
        /// </summary>
        /// <param name="enabled">True to enable debug mode, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDebugMode(bool enabled = true)
        {
            _options.DebugMode = enabled;
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
