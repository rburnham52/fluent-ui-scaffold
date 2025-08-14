using System;

using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    // Legacy non-generic VerificationContext removed as part of Verify v2 cleanup.

    /// <summary>
    /// Page-aware, chainable verification context.
    /// </summary>
    /// <typeparam name="TPage">Page type for fluent element selectors and And return</typeparam>
    public sealed class VerificationContext<TPage> : IVerificationContext<TPage>
    {
        private readonly IUIDriver _driver;
        private readonly FluentUIScaffoldOptions _options;
        private readonly ILogger _logger;
        private readonly TPage _page;

        public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger, TPage page)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public TPage And => _page;

        // Legacy bridge removed as we no longer expose the non-generic interface publicly.

        // New richer, chainable APIs
        public IVerificationContext<TPage> UrlIs(string url)
        {
            _logger.LogInformation($"Verifying URL is '{url}'");
            var current = _driver.CurrentUrl?.ToString() ?? string.Empty;
            if (!string.Equals(current, url, StringComparison.Ordinal))
            {
                throw new VerificationException($"Expected URL to be '{url}', but was '{current}'");
            }
            return this;
        }

        public IVerificationContext<TPage> UrlContains(string segment)
        {
            _logger.LogInformation($"Verifying URL contains '{segment}'");
            var current = _driver.CurrentUrl?.ToString() ?? string.Empty;
            if (!current.Contains(segment, StringComparison.Ordinal))
            {
                throw new VerificationException($"Expected URL to contain '{segment}', but was '{current}'");
            }
            return this;
        }

        public IVerificationContext<TPage> TitleIs(string title)
        {
            _logger.LogInformation($"Verifying title is '{title}'");
            var actual = _driver.GetPageTitle();
            if (!string.Equals(actual, title, StringComparison.Ordinal))
            {
                throw new VerificationException($"Expected title to be '{title}', but was '{actual}'");
            }
            return this;
        }

        public IVerificationContext<TPage> TitleContains(string text)
        {
            _logger.LogInformation($"Verifying title contains '{text}'");
            var actual = _driver.GetPageTitle();
            if (!actual.Contains(text, StringComparison.Ordinal))
            {
                throw new VerificationException($"Expected title to contain '{text}', but was '{actual}'");
            }
            return this;
        }

        public IVerificationContext<TPage> TextContains(Func<TPage, IElement> elementSelector, string contains)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' contains text '{contains}'");
            var actual = _driver.GetText(element.Selector);
            if (!actual.Contains(contains, StringComparison.Ordinal))
            {
                throw new VerificationException($"Element '{element.Selector}' text '{actual}' does not contain '{contains}'");
            }
            return this;
        }

        public IVerificationContext<TPage> Visible(Func<TPage, IElement> elementSelector)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' is visible");
            if (!_driver.IsVisible(element.Selector))
            {
                throw new VerificationException($"Element '{element.Selector}' is not visible");
            }
            return this;
        }

        public IVerificationContext<TPage> NotVisible(Func<TPage, IElement> elementSelector)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' is not visible");
            if (_driver.IsVisible(element.Selector))
            {
                throw new VerificationException($"Element '{element.Selector}' is visible but should be hidden");
            }
            return this;
        }
    }
}
