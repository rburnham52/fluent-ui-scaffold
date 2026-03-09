using System;

using FluentUIScaffold.Core.Pages;

using Microsoft.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Home tab of the sample app.
    /// Demonstrates the new deferred execution chain pattern.
    /// </summary>
    [Route("/")]
    public class HomePage : Page<HomePage>
    {
        protected HomePage(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        /// <summary>
        /// Clicks the Home navigation tab.
        /// </summary>
        public HomePage ClickHomeTab()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='nav-home']").ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Clicks the counter increment button.
        /// </summary>
        public HomePage ClickCounter()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Verifies the page title contains the expected text.
        /// </summary>
        public HomePage VerifyTitle(string expectedText)
        {
            return Enqueue<IPage>(async page =>
            {
                var title = await page.TitleAsync().ConfigureAwait(false);
                if (!title.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Expected title to contain '{expectedText}' but was '{title}'.");
            });
        }

        /// <summary>
        /// Verifies the welcome heading is visible.
        /// </summary>
        public HomePage VerifyWelcomeVisible()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Verifies weather data is displayed.
        /// </summary>
        public HomePage VerifyWeatherDataVisible()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.Locator("[data-testid='weather-item']").First.WaitForAsync().ConfigureAwait(false);
            });
        }
    }
}
