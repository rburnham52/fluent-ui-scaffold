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
        public FluentUIScaffoldOptionsBuilder WithTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Timeout must be greater than zero", nameof(timeout));

            _options.Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the retry interval for UI operations.
        /// </summary>
        /// <param name="interval">The retry interval duration</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithRetryInterval(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Retry interval must be greater than zero", nameof(interval));

            _options.RetryInterval = interval;
            return this;
        }

        /// <summary>
        /// Sets the wait strategy for UI operations.
        /// </summary>
        /// <param name="strategy">The wait strategy to use</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithWaitStrategy(WaitStrategy strategy)
        {
            _options.WaitStrategy = strategy;
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
        /// Sets the screenshot path for capturing screenshots during test execution.
        /// </summary>
        /// <param name="path">The path where screenshots will be saved</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithScreenshotPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new FluentUIScaffoldValidationException("Screenshot path cannot be null or empty", nameof(path));

            _options.ScreenshotPath = path;
            return this;
        }

        /// <summary>
        /// Sets a framework-specific option.
        /// </summary>
        /// <param name="key">The option key</param>
        /// <param name="value">The option value</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithFrameworkOption(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new FluentUIScaffoldValidationException("Framework option key cannot be null or empty", nameof(key));

            _options.FrameworkOptions[key] = value;
            return this;
        }

        /// <summary>
        /// Sets the UI driver type to use.
        /// </summary>
        /// <typeparam name="TDriver">The driver type</typeparam>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDriver<TDriver>() where TDriver : class
        {
            _options.FrameworkType = typeof(TDriver);
            return this;
        }

        /// <summary>
        /// Sets the page validation strategy.
        /// </summary>
        /// <param name="strategy">The page validation strategy to use</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithPageValidationStrategy(PageValidationStrategy strategy)
        {
            _options.PageValidationStrategy = strategy;
            return this;
        }

        /// <summary>
        /// Sets whether to enable automatic screenshots on test failures.
        /// </summary>
        /// <param name="enabled">True to enable automatic screenshots, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithAutomaticScreenshots(bool enabled = true)
        {
            _options.AutomaticScreenshots = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether to enable headless mode for browser drivers.
        /// </summary>
        /// <param name="enabled">True to enable headless mode, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithHeadlessMode(bool enabled = true)
        {
            _options.HeadlessMode = enabled;
            return this;
        }

        /// <summary>
        /// Sets the browser window size for web testing.
        /// </summary>
        /// <param name="width">The window width in pixels</param>
        /// <param name="height">The window height in pixels</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithWindowSize(int width, int height)
        {
            if (width <= 0)
                throw new FluentUIScaffoldValidationException("Window width must be greater than zero", nameof(width));
            if (height <= 0)
                throw new FluentUIScaffoldValidationException("Window height must be greater than zero", nameof(height));

            _options.WindowWidth = width;
            _options.WindowHeight = height;
            return this;
        }

        /// <summary>
        /// Sets whether to enable implicit waits.
        /// </summary>
        /// <param name="enabled">True to enable implicit waits, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithImplicitWaits(bool enabled = true)
        {
            _options.ImplicitWaits = enabled;
            return this;
        }

        /// <summary>
        /// Sets the default wait timeout for element operations.
        /// </summary>
        /// <param name="timeout">The default wait timeout</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDefaultWaitTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Default wait timeout must be greater than zero", nameof(timeout));

            _options.DefaultWaitTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the retry count for failed operations.
        /// </summary>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithRetryCount(int retryCount)
        {
            if (retryCount < 0)
                throw new FluentUIScaffoldValidationException("Retry count must be non-negative", nameof(retryCount));

            _options.RetryCount = retryCount;
            return this;
        }

        /// <summary>
        /// Sets whether to enable detailed logging.
        /// </summary>
        /// <param name="enabled">True to enable detailed logging, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithDetailedLogging(bool enabled = true)
        {
            _options.DetailedLogging = enabled;
            return this;
        }

        /// <summary>
        /// Sets the custom user agent string for browser drivers.
        /// </summary>
        /// <param name="userAgent">The custom user agent string</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithUserAgent(string userAgent)
        {
            if (string.IsNullOrWhiteSpace(userAgent))
                throw new FluentUIScaffoldValidationException("User agent cannot be null or empty", nameof(userAgent));

            _options.UserAgent = userAgent;
            return this;
        }

        /// <summary>
        /// Sets whether to enable JavaScript execution.
        /// </summary>
        /// <param name="enabled">True to enable JavaScript execution, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithJavaScriptEnabled(bool enabled = true)
        {
            _options.JavaScriptEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets whether to accept insecure SSL certificates.
        /// </summary>
        /// <param name="enabled">True to accept insecure SSL certificates, false otherwise</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithAcceptInsecureCertificates(bool enabled = true)
        {
            _options.AcceptInsecureCertificates = enabled;
            return this;
        }

        /// <summary>
        /// Sets the page load timeout.
        /// </summary>
        /// <param name="timeout">The page load timeout duration</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithPageLoadTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Page load timeout must be greater than zero", nameof(timeout));

            _options.PageLoadTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the script timeout.
        /// </summary>
        /// <param name="timeout">The script timeout duration</param>
        /// <returns>The current builder instance for method chaining</returns>
        public FluentUIScaffoldOptionsBuilder WithScriptTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new FluentUIScaffoldValidationException("Script timeout must be greater than zero", nameof(timeout));

            _options.ScriptTimeout = timeout;
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

            if (_options.BaseUrl == null)
            {
                errors.Add("Base URL is required");
            }

            if (_options.Timeout <= TimeSpan.Zero)
            {
                errors.Add("Timeout must be greater than zero");
            }

            if (_options.RetryInterval <= TimeSpan.Zero)
            {
                errors.Add("Retry interval must be greater than zero");
            }

            if (_options.WindowWidth <= 0)
            {
                errors.Add("Window width must be greater than zero");
            }

            if (_options.WindowHeight <= 0)
            {
                errors.Add("Window height must be greater than zero");
            }

            if (_options.DefaultWaitTimeout <= TimeSpan.Zero)
            {
                errors.Add("Default wait timeout must be greater than zero");
            }

            if (_options.PageLoadTimeout <= TimeSpan.Zero)
            {
                errors.Add("Page load timeout must be greater than zero");
            }

            if (_options.ScriptTimeout <= TimeSpan.Zero)
            {
                errors.Add("Script timeout must be greater than zero");
            }

            if (errors.Count == 0)
            {
                throw new FluentUIScaffoldValidationException(
                    $"Configuration validation failed: {string.Join("; ", errors)}",
                    "Configuration");
            }
        }
    }
}
