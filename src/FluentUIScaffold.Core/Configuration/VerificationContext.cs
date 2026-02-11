using System;
using System.Threading;

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
        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

        public VerificationContext(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger, TPage page)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _page = page ?? throw new ArgumentNullException(nameof(page));
        }

        public TPage And => _page;

        /// <summary>
        /// Polls until the condition is met or timeout occurs.
        /// Treats exceptions from the condition as transient failures (continues polling).
        /// </summary>
        private void PollUntil(Func<bool> condition, string failureMessage)
        {
            var timeout = _options.DefaultWaitTimeout;
            var startTime = DateTime.UtcNow;
            Exception? lastException = null;

            while (DateTime.UtcNow - startTime < timeout)
            {
                try
                {
                    if (condition())
                    {
                        return;
                    }
                }
                catch (Exception ex) when (!(ex is VerificationException))
                {
                    // Treat driver exceptions as transient failures during polling
                    // (e.g., element temporarily detached during re-render)
                    lastException = ex;
                }

                Thread.Sleep(PollInterval);
            }

            // Include the last exception in the verification failure for debugging
            var message = $"{failureMessage} (timeout: {timeout.TotalSeconds}s)";
            throw lastException != null
                ? new VerificationException(message, lastException)
                : new VerificationException(message);
        }

        /// <summary>
        /// Wraps driver exceptions in VerificationException with context.
        /// </summary>
        private void SafeExecute(Action action, string failureMessage)
        {
            try
            {
                action();
            }
            catch (VerificationException)
            {
                throw;
            }
            catch (System.TimeoutException ex)
            {
                throw new VerificationException($"{failureMessage} (timeout)", ex);
            }
            catch (Exceptions.TimeoutException ex)
            {
                throw new VerificationException($"{failureMessage} (timeout)", ex);
            }
            catch (ElementTimeoutException ex)
            {
                throw new VerificationException($"{failureMessage} (element timeout)", ex);
            }
            catch (AggregateException ex)
            {
                var inner = ex.InnerException ?? ex;
                throw new VerificationException($"{failureMessage} ({inner.GetType().Name})", inner);
            }
            catch (Exception ex)
            {
                throw new VerificationException($"{failureMessage} ({ex.GetType().Name})", ex);
            }
        }

        // Legacy bridge removed as we no longer expose the non-generic interface publicly.

        // New richer, chainable APIs
        public IVerificationContext<TPage> UrlIs(string url)
        {
            _logger.LogInformation($"Verifying URL is '{url}'");

            PollUntil(
                () =>
                {
                    var current = _driver.CurrentUrl?.ToString() ?? string.Empty;
                    return string.Equals(current, url, StringComparison.Ordinal);
                },
                $"Expected URL to be '{url}', but it never matched");

            return this;
        }

        public IVerificationContext<TPage> UrlContains(string segment)
        {
            _logger.LogInformation($"Verifying URL contains '{segment}'");

            PollUntil(
                () =>
                {
                    var current = _driver.CurrentUrl?.ToString() ?? string.Empty;
                    return current.Contains(segment, StringComparison.Ordinal);
                },
                $"Expected URL to contain '{segment}', but it never did");

            return this;
        }

        public IVerificationContext<TPage> TitleIs(string title)
        {
            _logger.LogInformation($"Verifying title is '{title}'");

            PollUntil(
                () =>
                {
                    var actual = _driver.GetPageTitle();
                    return string.Equals(actual, title, StringComparison.Ordinal);
                },
                $"Expected title to be '{title}', but it never matched");

            return this;
        }

        public IVerificationContext<TPage> TitleContains(string text)
        {
            _logger.LogInformation($"Verifying title contains '{text}'");

            PollUntil(
                () =>
                {
                    var actual = _driver.GetPageTitle();
                    return actual.Contains(text, StringComparison.Ordinal);
                },
                $"Expected title to contain '{text}', but it never did");

            return this;
        }

        public IVerificationContext<TPage> TextContains(Func<TPage, IElement> elementSelector, string contains)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' contains text '{contains}'");

            // First, wait for the element to be visible
            SafeExecute(
                () => _driver.WaitForElementToBeVisible(element.Selector),
                $"Element '{element.Selector}' never became visible");

            // Then poll for the expected text
            PollUntil(
                () =>
                {
                    var actual = _driver.GetText(element.Selector);
                    return actual.Contains(contains, StringComparison.Ordinal);
                },
                $"Element '{element.Selector}' text never contained '{contains}'");

            return this;
        }

        public IVerificationContext<TPage> TextIs(Func<TPage, IElement> elementSelector, string text)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' text is '{text}'");

            // First, wait for the element to be visible
            SafeExecute(
                () => _driver.WaitForElementToBeVisible(element.Selector),
                $"Element '{element.Selector}' never became visible");

            // Then poll for the exact text
            PollUntil(
                () =>
                {
                    var actual = _driver.GetText(element.Selector);
                    return string.Equals(actual, text, StringComparison.Ordinal);
                },
                $"Element '{element.Selector}' text never matched '{text}'");

            return this;
        }

        public IVerificationContext<TPage> HasAttribute(Func<TPage, IElement> elementSelector, string attributeName, string expectedValue)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' has attribute '{attributeName}' with value '{expectedValue}'");

            // First, wait for the element to be visible
            SafeExecute(
                () => _driver.WaitForElementToBeVisible(element.Selector),
                $"Element '{element.Selector}' never became visible");

            // Then poll for the expected attribute value
            string? lastActualValue = null;
            PollUntil(
                () =>
                {
                    lastActualValue = _driver.GetAttribute(element.Selector, attributeName);
                    return string.Equals(lastActualValue, expectedValue, StringComparison.Ordinal);
                },
                $"Element '{element.Selector}' attribute '{attributeName}' never matched '{expectedValue}' (last value: '{lastActualValue}')");

            return this;
        }

        public IVerificationContext<TPage> Visible(Func<TPage, IElement> elementSelector)
        {
            var element = elementSelector(_page);
            _logger.LogInformation($"Verifying element '{element.Selector}' is visible");

            // Wait for element to be visible, then assert
            SafeExecute(
                () => _driver.WaitForElementToBeVisible(element.Selector),
                $"Element '{element.Selector}' never became visible");

            // Final assertion to ensure it's still visible
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

            // Wait for element to be hidden, then assert
            SafeExecute(
                () => _driver.WaitForElementToBeHidden(element.Selector),
                $"Element '{element.Selector}' never became hidden");

            // Final assertion to ensure it's still hidden
            if (_driver.IsVisible(element.Selector))
            {
                throw new VerificationException($"Element '{element.Selector}' is visible but should be hidden");
            }

            return this;
        }
    }
}
