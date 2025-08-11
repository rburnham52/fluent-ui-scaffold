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

        // Removed debug-mode-specific timeout in favor of a single DefaultWaitTimeout

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

        // Server configuration is handled exclusively by WebServerManager; no server-related APIs here

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

        /// <summary>
        /// Explicitly requests a specific driver type to be used by the framework.
        /// </summary>
        /// <typeparam name="TDriver">The driver type to request.</typeparam>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDriver<TDriver>() where TDriver : class
        {
            _options.RequestedDriverType = typeof(TDriver);
            return this;
        }
    }
}
