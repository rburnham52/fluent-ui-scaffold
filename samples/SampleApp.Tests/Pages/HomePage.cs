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
        public IElement CounterButton { get; set; } = null!;
        public IElement CounterValue { get; set; } = null!;
        public IElement PageTitle { get; set; } = null!;
        public IElement HomeSectionTitle { get; set; } = null!;
        public IElement LoginNavButton { get; set; } = null!;
        public IElement RegisterNavButton { get; set; } = null!;
        public IElement ProfileNavButton { get; set; } = null!;
        public IElement TodosNavButton { get; set; } = null!;

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

            LoginNavButton = Element("[data-testid=\"nav-login\"]")
                .WithDescription("Login Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            RegisterNavButton = Element("[data-testid=\"nav-register\"]")
                .WithDescription("Register Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            ProfileNavButton = Element("[data-testid=\"nav-profile\"]")
                .WithDescription("Profile Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            TodosNavButton = Element("[data-testid=\"nav-todos\"]")
                .WithDescription("Todos Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
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

        /// <summary>
        /// Navigates to the login section by clicking the login navigation button.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage NavigateToLoginSection()
        {
            return Click(e => e.LoginNavButton);
        }

        /// <summary>
        /// Navigates to the register section by clicking the register navigation button.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage NavigateToRegisterSection()
        {
            return Click(e => e.RegisterNavButton);
        }

        /// <summary>
        /// Navigates to the profile section by clicking the profile navigation button.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage NavigateToProfileSection()
        {
            return Click(e => e.ProfileNavButton);
        }

        /// <summary>
        /// Navigates to the todos section by clicking the todos navigation button.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage NavigateToTodosSection()
        {
            return Click(e => e.TodosNavButton);
        }
    }
}
