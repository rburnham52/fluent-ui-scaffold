using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;
using Microsoft.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the home page of the sample application.
    /// Demonstrates how to create page objects using the FluentUIScaffold framework.
    /// </summary>
    public class HomePage : BasePageComponent<PlaywrightDriver, HomePage>
    {
        public HomePage(IServiceProvider serviceProvider)
            : base(serviceProvider, TestConfiguration.BaseUri)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure home page elements
            WelcomeMessage = Element("h2")
                .WithDescription("Welcome Message")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            Subtitle = Element(".subtitle")
                .WithDescription("Subtitle")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            CounterButton = Element(".card button")
                .WithDescription("Counter Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            // The counter value is part of the button text, not a separate element
            CounterValue = Element(".card button")
                .WithDescription("Counter Value")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            NavigationButtons = Element("nav")
                .WithDescription("Navigation Buttons")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            HomeButton = Element("nav button[data-testid='nav-home']")
                .WithDescription("Home Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            TodosButton = Element("nav button[data-testid='nav-todos']")
                .WithDescription("Todos Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            ProfileButton = Element("nav button[data-testid='nav-profile']")
                .WithDescription("Profile Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            RegisterButton = Element("nav button[data-testid='nav-register']")
                .WithDescription("Register Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            LoginButton = Element("nav button[data-testid='nav-login']")
                .WithDescription("Login Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
        }

        // Element properties
        public IElement WelcomeMessage { get; set; } = null!;
        public IElement Subtitle { get; set; } = null!;
        public IElement CounterButton { get; set; } = null!;
        public IElement CounterValue { get; set; } = null!;
        public IElement NavigationButtons { get; set; } = null!;
        public IElement HomeButton { get; set; } = null!;
        public IElement TodosButton { get; set; } = null!;
        public IElement ProfileButton { get; set; } = null!;
        public IElement RegisterButton { get; set; } = null!;
        public IElement LoginButton { get; set; } = null!;

        /// <summary>
        /// Navigates to the home page.
        /// </summary>
        public async Task NavigateToHomeAsync()
        {
            Driver.NavigateToUrl(TestConfiguration.BaseUri);
            await WaitForPageToLoadAsync();
        }

        /// <summary>
        /// Waits for the home page to be fully loaded.
        /// </summary>
        public async Task WaitForPageToLoadAsync()
        {
            Driver.WaitForElementToBeVisible("h2");
            Driver.WaitForElementToBeVisible(".subtitle");
        }

        /// <summary>
        /// Gets the page title.
        /// </summary>
        public string GetPageTitle()
        {
            return Driver.GetPageTitle();
        }

        /// <summary>
        /// Gets the welcome message text.
        /// </summary>
        public string GetWelcomeMessage()
        {
            return WelcomeMessage.GetText();
        }

        /// <summary>
        /// Gets the subtitle text.
        /// </summary>
        public string GetSubtitle()
        {
            return Subtitle.GetText();
        }

        /// <summary>
        /// Clicks the counter button.
        /// </summary>
        public async Task ClickCounterButtonAsync()
        {
            CounterButton.Click();
        }

        /// <summary>
        /// Clicks the counter button and returns the new count (alias for ClickCounterButtonAsync).
        /// </summary>
        public int ClickCounter()
        {
            CounterButton.Click();
            return GetCounterValue();
        }

        /// <summary>
        /// Gets the current counter value.
        /// </summary>
        public int GetCounterValue()
        {
            var text = CounterValue.GetText();
            // The button text is "count is {count}", so extract the number
            var match = System.Text.RegularExpressions.Regex.Match(text, @"count is (\d+)");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        /// <summary>
        /// Verifies that weather data is displayed.
        /// </summary>
        public async Task<bool> IsWeatherDataDisplayedAsync()
        {
            try
            {
                Driver.WaitForElementToBeVisible(".weather-section");
                Driver.WaitForElementToBeVisible("[data-testid='weather-item']");
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the weather section title.
        /// </summary>
        public string GetWeatherSectionTitle()
        {
            return Driver.GetText(".weather-section h3");
        }

        /// <summary>
        /// Gets the text of the first weather item.
        /// </summary>
        public string GetFirstWeatherItemText()
        {
            return Driver.GetText("[data-testid='weather-item']");
        }

        /// <summary>
        /// Navigates to a specific section.
        /// </summary>
        public async Task NavigateToSectionAsync(string section)
        {
            Driver.Click($"nav button[data-testid='nav-{section}']");
            // Don't wait for subtitle on other pages since it only exists on home page
            if (section == "home")
            {
                await WaitForPageToLoadAsync();
            }
            else
            {
                // Wait for the section to be visible instead
                Driver.WaitForElementToBeVisible($".{section}-section");
            }
        }

        /// <summary>
        /// Checks if a navigation button is active.
        /// </summary>
        public async Task<bool> IsNavigationButtonActiveAsync(string section)
        {
            // Get the button element and check its class attribute
            var page = Driver.GetFrameworkDriver<IPage>();
            var buttonElement = await page.QuerySelectorAsync($"nav button[data-testid='nav-{section}']");
            if (buttonElement != null)
            {
                var className = await buttonElement.GetAttributeAsync("class");
                return className?.Contains("active") ?? false;
            }
            return false;
        }

        /// <summary>
        /// Verifies that the home page has the expected content.
        /// </summary>
        public async Task<bool> HasExpectedContentAsync()
        {
            try
            {
                // Wait for the page to load
                await WaitForPageToLoadAsync();
                
                // Check page title (browser title)
                var pageTitle = GetPageTitle();
                var hasPageTitle = !string.IsNullOrEmpty(pageTitle);
                
                // Check welcome message (h2 element)
                var welcomeMessage = GetWelcomeMessage();
                var hasWelcomeMessage = welcomeMessage.Contains("Welcome to");
                
                // Check subtitle
                var subtitle = GetSubtitle();
                var hasSubtitle = subtitle.Contains("sample application");
                
                // Check counter button
                var hasCounterButton = CounterButton.IsVisible();
                
                return hasPageTitle && hasWelcomeMessage && hasSubtitle && hasCounterButton;
            }
            catch
            {
                return false;
            }
        }
    }
}
