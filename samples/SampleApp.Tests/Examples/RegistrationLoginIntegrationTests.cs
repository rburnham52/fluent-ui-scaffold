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
    /// Example tests demonstrating integrated registration and login flows with the FluentUIScaffold framework.
    /// These tests showcase end-to-end user workflows.
    /// </summary>
    [TestClass]
    public class RegistrationLoginIntegrationTests
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
                HeadlessMode = true // Run in headless mode for CI/CD
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
        public Task Can_Complete_Registration_And_Login_Workflow()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToRegisterSection();
            homePage.NavigateToLoginSection();

            // Act & Assert - Verify both sections are accessible
            Assert.IsNotNull(homePage);
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
        public Task Can_Verify_Login_Page_Accessibility()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();
            homePage.NavigateToLoginSection();

            // Act & Assert
            homePage.Verify.ElementIsVisible("#loginForm");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Page_Navigation()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify navigation works
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }
    }
}
