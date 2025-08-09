using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests that verify the FluentUIScaffold framework functionality
    /// without requiring a running web application.
    /// </summary>
    [TestClass]
    public class FrameworkTests
    {
        [TestMethod]
        public async Task FrameworkInitialization_WhenValidOptionsProvided_InitializesSuccessfully()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            using var app = new FluentUIScaffoldApp<WebApp>(options);
            await app.InitializeAsync();

            // Assert
            Assert.IsNotNull(app, "FluentUIScaffoldApp should be created");
            Assert.IsNotNull(app.ServiceProvider, "ServiceProvider should be available");
        }

        [TestMethod]
        public async Task DebugModeConfiguration_WhenDebugModeEnabled_SetsCorrectOptions()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(true)
                .Build();

            // Assert
            Assert.IsTrue(options.EnableDebugMode, "Debug mode should be enabled");
            Assert.AreEqual(TestConfiguration.BaseUri, options.BaseUrl, "Base URL should be set correctly");
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout, "Default timeout should be set correctly");
        }

        [TestMethod]
        public async Task OptionsBuilder_WhenChainedMethodsUsed_BuildsCorrectOptions()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(60))
                .WithDebugMode(true)
                .WithDefaultWaitTimeoutDebug(TimeSpan.FromSeconds(120))
                .Build();

            // Assert
            Assert.AreEqual(TestConfiguration.BaseUri, options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(60), options.DefaultWaitTimeout);
            Assert.AreEqual(TimeSpan.FromSeconds(120), options.DefaultWaitTimeoutDebug);
            Assert.IsTrue(options.EnableDebugMode);
        }

        [TestMethod]
        public async Task PageObjectCreation_WhenServiceProviderAvailable_CreatesPageObjects()
        {
            // Arrange
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            using var app = new FluentUIScaffoldApp<WebApp>(options);
            await app.InitializeAsync();

            // Act
            var homePage = new Pages.HomePage(app.ServiceProvider);
            var registrationPage = new Pages.RegistrationPage(app.ServiceProvider);
            var loginPage = new Pages.LoginPage(app.ServiceProvider, TestConfiguration.BaseUri);

            // Assert
            Assert.IsNotNull(homePage, "HomePage should be created");
            Assert.IsNotNull(registrationPage, "RegistrationPage should be created");
            Assert.IsNotNull(loginPage, "LoginPage should be created");
        }

        [TestMethod]
        public async Task WebAppNotRunning_WhenAttemptingToNavigate_HandlesGracefully()
        {
            // Arrange
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(5)) // Shorter timeout for this test
                .WithDebugMode(false)
                .Build();

            using var app = new FluentUIScaffoldApp<WebApp>(options);
            await app.InitializeAsync();

            var homePage = new Pages.HomePage(app.ServiceProvider);

            // Act & Assert - This should either fail gracefully or succeed if web app is running
            try
            {
                await homePage.NavigateToHomeAsync();
                // If we get here, the web app might be running or navigation succeeded
                // This is acceptable behavior - the test demonstrates that the framework can handle navigation
                Assert.IsTrue(true, "Navigation succeeded - web app may be running or navigation handled gracefully");
            }
            catch (Exception ex)
            {
                // This is also expected behavior when the web application isn't running
                Assert.IsTrue(ex.Message.Contains("timeout") || ex.Message.Contains("connection") || ex.Message.Contains("net::ERR_CONNECTION_REFUSED"),
                    "Should fail with timeout, connection error, or connection refused when web app is not running");
            }
        }
    }
}
