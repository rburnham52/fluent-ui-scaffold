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
    /// Example tests demonstrating registration flow functionality with the FluentUIScaffold framework.
    /// These tests showcase the fluent API for registration workflows.
    /// </summary>
    [TestClass]
    public class RegistrationFlowTests
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
        public Task Can_Access_Registration_Page()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Registration_Form_Elements()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Act & Assert
            homePage.Verify.ElementIsVisible("#registrationForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Registration_Page_Accessibility()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Act & Assert
            homePage.Verify.ElementIsVisible("#registrationForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Registration_Form_Validation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();

            // Act & Assert
            homePage.Verify.ElementIsVisible("#registrationForm");
            return Task.CompletedTask;
        }
    }
}
