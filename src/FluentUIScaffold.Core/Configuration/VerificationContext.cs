using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Implementation of IVerificationContext that provides fluent verification methods.
    /// </summary>
    public class VerificationContext : IVerificationContext
    {
        private readonly IUIDriver _driver;
        private readonly FluentUIScaffoldOptions _options;
        private readonly ILogger _logger;

        public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IVerificationContext ElementIsVisible(string selector)
        {
            _logger.LogInformation($"Verifying element '{selector}' is visible");
            if (!_driver.IsVisible(selector))
            {
                throw new VerificationException($"Element '{selector}' is not visible");
            }
            return this;
        }

        public IVerificationContext ElementIsHidden(string selector)
        {
            _logger.LogInformation($"Verifying element '{selector}' is hidden");
            if (_driver.IsVisible(selector))
            {
                throw new VerificationException($"Element '{selector}' is visible but should be hidden");
            }
            return this;
        }

        public IVerificationContext ElementIsEnabled(string selector)
        {
            _logger.LogInformation($"Verifying element '{selector}' is enabled");
            if (!_driver.IsEnabled(selector))
            {
                throw new VerificationException($"Element '{selector}' is not enabled");
            }
            return this;
        }

        public IVerificationContext ElementIsDisabled(string selector)
        {
            _logger.LogInformation($"Verifying element '{selector}' is disabled");
            if (_driver.IsEnabled(selector))
            {
                throw new VerificationException($"Element '{selector}' is enabled but should be disabled");
            }
            return this;
        }

        public IVerificationContext ElementContainsText(string selector, string text)
        {
            _logger.LogInformation($"Verifying element '{selector}' contains text '{text}'");
            var elementText = _driver.GetText(selector);
            if (!elementText.Contains(text))
            {
                throw new VerificationException($"Element '{selector}' text '{elementText}' does not contain '{text}'");
            }
            return this;
        }

        public IVerificationContext ElementHasAttribute(string selector, string attribute, string value)
        {
            _logger.LogInformation($"Verifying element '{selector}' has attribute '{attribute}' with value '{value}'");
            // This would need to be implemented in the driver
            throw new NotImplementedException("ElementHasAttribute not yet implemented");
        }

        public IVerificationContext UrlMatches(string pattern)
        {
            _logger.LogInformation($"Verifying URL matches pattern '{pattern}'");
            var currentUrl = _driver.CurrentUrl?.ToString() ?? "";
            if (!currentUrl.Contains(pattern))
            {
                throw new VerificationException($"Current URL '{currentUrl}' does not match pattern '{pattern}'");
            }
            return this;
        }

        public IVerificationContext TitleContains(string text)
        {
            _logger.LogInformation($"Verifying page title contains '{text}'");
            var pageTitle = _driver.GetPageTitle();
            if (!pageTitle.Contains(text))
            {
                throw new VerificationException($"Page title '{pageTitle}' does not contain '{text}'");
            }
            return this;
        }

        public IVerificationContext That(Func<bool> condition, string description)
        {
            _logger.LogInformation($"Verifying custom condition: {description}");
            if (!condition())
            {
                throw new VerificationException($"Custom verification failed: {description}");
            }
            return this;
        }

        public IVerificationContext That<T>(Func<T> actual, Func<T, bool> condition, string description)
        {
            _logger.LogInformation($"Verifying custom condition with value: {description}");
            var actualValue = actual();
            if (!condition(actualValue))
            {
                throw new VerificationException($"Custom verification failed: {description}. Actual value: {actualValue}");
            }
            return this;
        }
    }
}
