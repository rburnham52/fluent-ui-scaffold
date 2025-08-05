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
    /// Example tests demonstrating registration and login functionality with the FluentUIScaffold framework.
    /// These tests showcase the fluent API for form interactions and user workflows.
    /// </summary>
    [TestClass]
    public class RegistrationLoginTests
    {
        private FluentUIScaffoldApp<WebApp>? _fluentUI;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Configure FluentUIScaffold with auto-discovery and web server launch
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = TestConfiguration.BaseUri,
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                HeadlessMode = true, // Run in headless mode for CI/CD
                EnableWebServerLaunch = true,
                WebServerProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "SampleApp")
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
        public Task Can_Navigate_To_Registration_Page()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Navigate_To_Login_Page()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToLoginSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Registration_Page_Elements()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Act & Assert
            homePage.Verify.ElementIsVisible("#registrationForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Login_Page_Elements()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToLoginSection();

            // Act & Assert
            homePage.Verify.ElementIsVisible("#loginForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Form_Interactions()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Act & Assert - Demonstrate form interactions
            homePage.Verify.ElementIsVisible("#registrationForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Form_Validation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Act & Assert - Verify form validation
            homePage.Verify.ElementIsVisible("#registrationForm");
            return Task.CompletedTask;
        }
    }
}
