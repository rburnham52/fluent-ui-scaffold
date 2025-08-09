using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
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
        private static readonly Regex CounterRegex = new Regex("count is (\\d+)", RegexOptions.Compiled);

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
            var match = CounterRegex.Match(text);
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
            // Ensure base app shell is loaded
            if (!Driver.IsVisible("nav"))
            {
                Driver.NavigateToUrl(TestConfiguration.BaseUri);
                Driver.WaitForElementToBeVisible("nav");
            }

            Driver.Click($"nav button[data-testid='nav-{section}']");
            // Wait until the nav button reflects active state
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < TimeSpan.FromSeconds(5))
            {
                if (await IsNavigationButtonActiveAsync(section)) break;
                Thread.Sleep(50);
            }
            // If the section still does not appear quickly, attempt a JS-based click fallback
            var pageForJs = Driver.GetFrameworkDriver<IPage>();
            bool SectionVisibleQuickly(string css) => Driver.IsVisible(css);
            if (section == "register")
            {
                if (!SectionVisibleQuickly(".registration-form"))
                {
                    await pageForJs.EvaluateAsync("selector => document.querySelector(selector)?.click()", $"nav button[data-testid='nav-{section}']");
                }
            }
            else if (section == "profile")
            {
                if (!SectionVisibleQuickly(".profile-section"))
                {
                    await pageForJs.EvaluateAsync("selector => document.querySelector(selector)?.click()", $"nav button[data-testid='nav-{section}']");
                }
            }
            if (section == "home")
            {
                await WaitForPageToLoadAsync();
            }
            else if (section == "todos")
            {
                Driver.WaitForElementToBeVisible(".todos-section");
            }
            else if (section == "profile")
            {
                Driver.WaitForElementToBeVisible(".profile-section");
            }
            else if (section == "register")
            {
                Driver.WaitForElementToBeVisible(".registration-form");
                Driver.WaitForElementToBeVisible(".registration-form h2");
            }
            else if (section == "login")
            {
                Driver.WaitForElementToBeVisible(".login-section");
            }
        }

        /// <summary>
        /// Checks if a navigation button is active.
        /// </summary>
        public async Task<bool> IsNavigationButtonActiveAsync(string section)
        {
            // Prefer section visibility as the source of truth; then check button class
            try
            {
                switch (section)
                {
                    case "home":
                        if (Driver.IsVisible(".home-section")) return true;
                        break;
                    case "todos":
                        if (Driver.IsVisible(".todos-section")) return true;
                        break;
                    case "profile":
                        if (Driver.IsVisible(".profile-section")) return true;
                        break;
                    case "register":
                        if (Driver.IsVisible(".registration-form")) return true;
                        break;
                    case "login":
                        if (Driver.IsVisible(".login-section")) return true;
                        break;
                }
            }
            catch { /* ignore and fall back to button class check */ }

            // Button class fallback with small wait for class to flip
            var page = Driver.GetFrameworkDriver<IPage>();
            var selector = $"nav button[data-testid='nav-{section}']";
            try { await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions { State = WaitForSelectorState.Attached, Timeout = 3000 }); } catch { }

            var deadline = DateTime.UtcNow + TimeSpan.FromMilliseconds(1500);
            while (DateTime.UtcNow < deadline)
            {
                var buttonElement = await page.QuerySelectorAsync(selector);
                if (buttonElement != null)
                {
                    try
                    {
                        var hasActive = await buttonElement.EvaluateAsync<bool>("el => el.classList.contains('active')");
                        if (hasActive) return true;
                    }
                    catch { }
                }
                await Task.Delay(50);
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
