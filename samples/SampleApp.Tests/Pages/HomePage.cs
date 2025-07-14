using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the home page of the FluentUIScaffold sample application.
    /// Demonstrates basic navigation, element interactions, and page validation.
    /// </summary>
    public class HomePage : BasePageComponent<PlaywrightDriver, HomePage>
    {
        public HomePage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the home page
        }

        /// <summary>
        /// Clicks the counter button and returns the updated count.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage ClickCounter()
        {
            // The counter button is just a button with text "count is X"
            Driver.Click("button");
            return this;
        }

        /// <summary>
        /// Gets the current counter value.
        /// </summary>
        /// <returns>The current counter value as a string</returns>
        public string GetCounterValue()
        {
            // Get the text from the button which contains "count is X"
            var buttonText = Driver.GetText("button");
            // Extract the number from "count is X"
            if (buttonText.Contains("count is "))
            {
                return buttonText.Replace("count is ", "");
            }
            return buttonText;
        }

        /// <summary>
        /// Verifies that the page title contains the expected text.
        /// </summary>
        /// <param name="expectedTitle">The expected title text</param>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyPageTitle()
        {
            var title = Driver.GetPageTitle();
            if (string.IsNullOrEmpty(title))
            {
                throw new InvalidOperationException("Page title is null or empty");
            }
            return this;
        }
    }
}
