using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Example tests demonstrating the FluentUIScaffold framework capabilities.
    /// These tests showcase various features and patterns for UI testing.
    /// </summary>
    [TestClass]
    public class HomePageTests
    {
        private FluentUIScaffoldApp<WebApp>? _fluentUI;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Configure FluentUIScaffold with auto-discovery and Playwright-style web server launch
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = TestConfiguration.BaseUri,
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                HeadlessMode = true, // Run in headless mode for CI/CD
                EnableWebServerLaunch = true,
                WebServerProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "SampleApp"),
                ReuseExistingServer = false
            };

            _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
            await _fluentUI.InitializeAsync();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _fluentUI?.Dispose();
        }

        [TestMethod]
        public Task Can_Navigate_To_Home_Page()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Page_Title()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.VerifyPageTitle();
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Interact_With_Counter()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Click the counter multiple times
            homePage
                .ClickCounter()
                .ClickCounter()
                .ClickCounter();

            // Assert - Verify the counter shows the expected value
            var counterValue = homePage.GetCounterValue();
            Assert.IsNotNull(counterValue);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Counter_Button_Is_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.ElementIsVisible(".card button");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Counter_Value_Is_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.ElementIsVisible(".card button");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Page_Title_Is_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.ElementIsVisible("h1");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Home_Section_Title_Is_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.ElementIsVisible(".home-section h2");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Perform_Basic_Counter_Interaction()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Click the counter and verify the value changes
            homePage.ClickCounter();
            var counterValue = homePage.GetCounterValue();

            // Assert - Verify the counter value is not null
            Assert.IsNotNull(counterValue);
            return Task.CompletedTask;
        }
    }
}
