using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    [TestClass]
    public class DebugModeTests
    {
        private FluentUIScaffoldApp<WebApp> _fluentUI;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Configure FluentUIScaffold with debug mode enabled
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = TestConfiguration.BaseUri,
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                DebugMode = true // Enable debug mode for this test
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
        public Task Can_Run_Test_With_Debug_Mode()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Navigate to login section (this will be visible and slowed down in debug mode)
            homePage.NavigateToLoginSection();

            // Assert - Verify login form is visible
            homePage.Verify.ElementIsVisible("#loginForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Interact_With_Elements_In_Debug_Mode()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Interact with counter directly on home page
            homePage.ClickCounter(); // This interaction will be slowed down in debug mode

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_Between_Sections_In_Debug_Mode()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Navigate through different sections (all interactions will be slowed down)
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
        public Task Can_Verify_Elements_With_Debug_Mode()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Navigate to login and verify elements
            homePage.NavigateToLoginSection();

            // Assert - These verifications will be more visible in debug mode
            homePage.Verify
                .ElementIsVisible("#loginForm")
                .ElementIsVisible("[data-testid=\"nav-login\"]")
                .ElementIsVisible("[data-testid=\"nav-register\"]");

            return Task.CompletedTask;
        }
    }
}
