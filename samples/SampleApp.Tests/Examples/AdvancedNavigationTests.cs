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
    [TestClass]
    public class AdvancedNavigationTests
    {
        private FluentUIScaffoldApp<WebApp> _fluentUI;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Configure FluentUIScaffold with auto-discovery and web server launch
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = TestConfiguration.BaseUri,
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                HeadlessMode = true,
                // Web server launching configuration
                EnableWebServerLaunch = true,
                WebServerProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SampleApp"),
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
        public Task Can_Navigate_To_Home_Page_With_UrlPattern()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_To_Login_Section_With_Custom_Navigation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act
            homePage.NavigateToLoginSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_To_Register_Section_With_Custom_Navigation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act
            homePage.NavigateToRegisterSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_To_Profile_Section_With_Custom_Navigation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act
            homePage.NavigateToProfileSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_To_Todos_Section_With_Custom_Navigation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act
            homePage.NavigateToTodosSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Direct_URL_Navigation()
        {
            // Arrange & Act
            _fluentUI!.NavigateToUrl(new Uri(TestConfiguration.BaseUri, "/"));

            // Assert
            // Navigation should not throw an exception
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Chain_Navigation_Methods()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Chain multiple navigation methods
            homePage
                .NavigateToLoginSection()
                .NavigateToRegisterSection()
                .NavigateToProfileSection()
                .NavigateToTodosSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_And_Verify_Elements()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act
            homePage.NavigateToLoginSection();

            // Assert - Verify that login form elements are visible after navigation
            homePage.Verify.ElementIsVisible("#loginForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_And_Interact_With_Elements()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Navigate to login section and verify it's accessible
            homePage.NavigateToLoginSection();

            // Assert - Verify that login form is visible after navigation
            homePage.Verify.ElementIsVisible("#loginForm");
            return Task.CompletedTask;
        }
    }
}
