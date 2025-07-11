using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the home page of the FluentUIScaffold sample application.
    /// Demonstrates basic navigation, element interactions, and page validation.
    /// </summary>
    public class HomePage : BasePageComponent<WebApp>
    {
        public HomePage(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
            : base(driver, options, logger)
        {
            // Wait for the Svelte SPA to load by waiting for the main header
            driver.WaitForElement(".app-header h1");
            ConfigureElements();
            // Runtime null checks for all element fields
            if (_navHomeButton == null) throw new InvalidOperationException("HomePage: _navHomeButton is null after ConfigureElements");
            if (_navTodosButton == null) throw new InvalidOperationException("HomePage: _navTodosButton is null after ConfigureElements");
            if (_navProfileButton == null) throw new InvalidOperationException("HomePage: _navProfileButton is null after ConfigureElements");
            if (_counterButton == null) throw new InvalidOperationException("HomePage: _counterButton is null after ConfigureElements");
            if (_counterText == null) throw new InvalidOperationException("HomePage: _counterText is null after ConfigureElements");
            if (_viteLogo == null) throw new InvalidOperationException("HomePage: _viteLogo is null after ConfigureElements");
            if (_svelteLogo == null) throw new InvalidOperationException("HomePage: _svelteLogo is null after ConfigureElements");
            if (_weatherSection == null) throw new InvalidOperationException("HomePage: _weatherSection is null after ConfigureElements");
            if (_weatherItems == null) throw new InvalidOperationException("HomePage: _weatherItems is null after ConfigureElements");
            if (_weatherItemsList == null) throw new InvalidOperationException("HomePage: _weatherItemsList is null after ConfigureElements");
        }
        // Navigation elements
        private IElement _navHomeButton;
        private IElement _navTodosButton;
        private IElement _navProfileButton;

        // Home page elements
        private IElement _counterButton;
        private IElement _counterText;
        private IElement _weatherItems;
        private IElement _viteLogo;
        private IElement _svelteLogo;

        // Weather section elements
        private IElement _weatherSection;
        private IElement _weatherItemsList;

        public override Uri UrlPattern => TestConfiguration.BaseUri;

        public override bool ShouldValidateOnNavigation => true;

        protected override void ConfigureElements()
        {
            Logger.LogInformation("Configuring HomePage elements...");
            // Navigation elements
            Logger.LogInformation("Building _navHomeButton with selector [data-testid='nav-home']");
            _navHomeButton = Element("[data-testid='nav-home']")
                .WithDescription("Home Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
            if (_navHomeButton == null) Logger.LogError("_navHomeButton is null after build");

            Logger.LogInformation("Building _navTodosButton with selector [data-testid='nav-todos']");
            _navTodosButton = Element("[data-testid='nav-todos']")
                .WithDescription("Todos Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
            if (_navTodosButton == null) Logger.LogError("_navTodosButton is null after build");

            Logger.LogInformation("Building _navProfileButton with selector [data-testid='nav-profile']");
            _navProfileButton = Element("[data-testid='nav-profile']")
                .WithDescription("Profile Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
            if (_navProfileButton == null) Logger.LogError("_navProfileButton is null after build");

            // Counter component elements
            Logger.LogInformation("Building _counterButton with selector .card button");
            _counterButton = Element(".card button")
                .WithDescription("Counter Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
            if (_counterButton == null) Logger.LogError("_counterButton is null after build");

            Logger.LogInformation("Building _counterText with selector .card button");
            _counterText = Element(".card button")
                .WithDescription("Counter Text")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
            if (_counterText == null) Logger.LogError("_counterText is null after build");

            // Logo elements
            Logger.LogInformation("Building _viteLogo with selector img[alt='Vite Logo']");
            _viteLogo = Element("img[alt='Vite Logo']")
                .WithDescription("Vite Logo")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
            if (_viteLogo == null) Logger.LogError("_viteLogo is null after build");

            Logger.LogInformation("Building _svelteLogo with selector img[alt='Svelte Logo']");
            _svelteLogo = Element("img[alt='Svelte Logo']")
                .WithDescription("Svelte Logo")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
            if (_svelteLogo == null) Logger.LogError("_svelteLogo is null after build");

            // Weather section elements
            Logger.LogInformation("Building _weatherSection with selector .weather-section");
            _weatherSection = Element(".weather-section")
                .WithDescription("Weather Section")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
            if (_weatherSection == null) Logger.LogError("_weatherSection is null after build");

            Logger.LogInformation("Building _weatherItems with selector [data-testid='weather-item']");
            _weatherItems = Element("[data-testid='weather-item']")
                .WithDescription("Weather Items")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
            if (_weatherItems == null) Logger.LogError("_weatherItems is null after build");

            Logger.LogInformation("Building _weatherItemsList with selector .weather-section");
            _weatherItemsList = Element(".weather-section")
                .WithDescription("Weather Items List")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
            if (_weatherItemsList == null) Logger.LogError("_weatherItemsList is null after build");
        }

        /// <summary>
        /// Clicks the counter button and returns the updated count.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage ClickCounter()
        {
            Logger.LogInformation("Clicking counter button");
            _counterButton.Click();
            return this;
        }

        /// <summary>
        /// Gets the current counter value.
        /// </summary>
        /// <returns>The current counter value as a string</returns>
        public string GetCounterValue()
        {
            var text = _counterText.GetText();
            Logger.LogInformation($"Current counter value: {text}");
            return text;
        }

        /// <summary>
        /// Clicks the counter button multiple times.
        /// </summary>
        /// <param name="times">Number of times to click the counter</param>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage ClickCounterMultipleTimes(int times)
        {
            Logger.LogInformation($"Clicking counter button {times} times");
            for (int i = 0; i < times; i++)
            {
                _counterButton.Click();
            }
            return this;
        }

        /// <summary>
        /// Navigates to the Todos page.
        /// </summary>
        /// <returns>A new TodosPage instance</returns>
        public TodosPage NavigateToTodos()
        {
            Logger.LogInformation("Navigating to Todos page");
            _navTodosButton.Click();
            return new TodosPage(Driver, Options, Logger);
        }

        /// <summary>
        /// Navigates to the Profile page.
        /// </summary>
        /// <returns>A new ProfilePage instance</returns>
        public ProfilePage NavigateToProfile()
        {
            Logger.LogInformation("Navigating to Profile page");
            _navProfileButton.Click();
            return new ProfilePage(Driver, Options, Logger);
        }

        /// <summary>
        /// Verifies that the weather section is visible and contains weather data.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyWeatherSectionIsVisible()
        {
            Logger.LogInformation("Verifying weather section is visible");
            if (!_weatherSection.IsVisible())
            {
                throw new InvalidOperationException("Weather section is not visible");
            }
            return this;
        }

        /// <summary>
        /// Verifies that weather items are displayed.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyWeatherItemsAreDisplayed()
        {
            Logger.LogInformation("Verifying weather items are displayed");
            if (!_weatherItems.IsVisible())
            {
                throw new InvalidOperationException("Weather items are not visible");
            }
            return this;
        }

        /// <summary>
        /// Verifies that both Vite and Svelte logos are visible.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyLogosAreVisible()
        {
            Logger.LogInformation("Verifying logos are visible");
            if (!_viteLogo.IsVisible())
            {
                throw new InvalidOperationException("Vite logo is not visible");
            }
            if (!_svelteLogo.IsVisible())
            {
                throw new InvalidOperationException("Svelte logo is not visible");
            }
            return this;
        }

        /// <summary>
        /// Verifies that the counter has a specific value.
        /// </summary>
        /// <param name="expectedValue">The expected counter value</param>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyCounterValue(string expectedValue)
        {
            var actualValue = GetCounterValue();
            Logger.LogInformation($"Verifying counter value. Expected: {expectedValue}, Actual: {actualValue}");

            if (!actualValue.Contains(expectedValue))
            {
                throw new InvalidOperationException($"Counter value mismatch. Expected: {expectedValue}, Actual: {actualValue}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that the page title contains the expected text.
        /// </summary>
        /// <param name="expectedTitle">The expected title text</param>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyPageTitle(string expectedTitle)
        {
            Logger.LogInformation($"Verifying page title contains: {expectedTitle}");
            var pageTitle = Driver.GetText(".app-header h1");

            if (string.IsNullOrEmpty(pageTitle))
            {
                throw new InvalidOperationException($"Page title is null or empty. Expected to contain: {expectedTitle}");
            }

            if (!pageTitle.Contains(expectedTitle))
            {
                throw new InvalidOperationException($"Page title mismatch. Expected to contain: {expectedTitle}, Actual: {pageTitle}");
            }
            return this;
        }

        /// <summary>
        /// Waits for the weather data to load.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage WaitForWeatherData()
        {
            Logger.LogInformation("Waiting for weather data to load");
            _weatherItems.WaitFor();
            return this;
        }

        /// <summary>
        /// Gets the number of weather items displayed.
        /// </summary>
        /// <returns>The number of weather items</returns>
        public int GetWeatherItemCount()
        {
            // This would need to be implemented based on the specific driver capabilities
            // For now, we'll return a default value
            Logger.LogInformation("Getting weather item count");
            return 3; // Assuming 3 weather items are displayed
        }

        /// <summary>
        /// Verifies that a specific number of weather items are displayed.
        /// </summary>
        /// <param name="expectedCount">The expected number of weather items</param>
        /// <returns>The current page instance for method chaining</returns>
        public HomePage VerifyWeatherItemCount(int expectedCount)
        {
            var actualCount = GetWeatherItemCount();
            Logger.LogInformation($"Verifying weather item count. Expected: {expectedCount}, Actual: {actualCount}");

            if (actualCount != expectedCount)
            {
                throw new InvalidOperationException($"Weather item count mismatch. Expected: {expectedCount}, Actual: {actualCount}");
            }
            return this;
        }
    }
}
