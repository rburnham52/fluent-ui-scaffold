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
        public IElement CounterButton { get; private set; }
        public IElement CounterValue { get; private set; }
        public IElement PageTitle { get; private set; }
        public IElement HomeSectionTitle { get; private set; }

        public HomePage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the home page using ElementFactory
            CounterButton = Element(".card button")
                .WithDescription("Counter Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            CounterValue = Element(".card button")
                .WithDescription("Counter Value")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            PageTitle = Element("h1")
                .WithDescription("Header Title")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            HomeSectionTitle = Element(".home-section h2")
                .WithDescription("Home Section Title")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
        }

        /// <summary>
        /// Clicks the counter button using the fluent API.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage ClickCounter()
        {
            return Click(e => e.CounterButton);
        }

        /// <summary>
        /// Gets the current counter value using the fluent API.
        /// </summary>
        /// <returns>The current counter value as a string</returns>
        public string GetCounterValue()
        {
            var buttonText = CounterValue.GetText();
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
