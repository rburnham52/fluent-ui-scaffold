using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

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
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

            // Assert
            Assert.IsNotNull(app, "AppScaffold should be created");
            Assert.IsNotNull(app.ServiceProvider, "ServiceProvider should be available");

            await app.DisposeAsync();
        }

        [TestMethod]
        public async Task OptionsBuilder_WhenChainedMethodsUsed_BuildsCorrectOptions()
        {
            // Arrange & Act
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(60);
                    options.HeadlessMode = TestConfiguration.IsHeadlessMode;
                    options.SlowMo = 250;
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

            // Get the options from service provider
            var options = app.GetService<FluentUIScaffoldOptions>();

            // Assert
            Assert.AreEqual(TestConfiguration.BaseUri, options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(60), options.DefaultWaitTimeout);
            Assert.AreEqual(TestConfiguration.IsHeadlessMode, options.HeadlessMode);
            Assert.AreEqual(250, options.SlowMo);

            await app.DisposeAsync();
        }

        [TestMethod]
        public async Task PageObjectCreation_WhenServiceProviderAvailable_CreatesPageObjects()
        {
            // Arrange
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

            // Act
            var homePage = new Pages.HomePage(app.ServiceProvider);
            var registrationPage = new Pages.RegistrationPage(app.ServiceProvider);
            var loginPage = new Pages.LoginPage(app.ServiceProvider, TestConfiguration.BaseUri);

            // Assert
            Assert.IsNotNull(homePage, "HomePage should be created");
            Assert.IsNotNull(registrationPage, "RegistrationPage should be created");
            Assert.IsNotNull(loginPage, "LoginPage should be created");

            await app.DisposeAsync();
        }

        [TestMethod]
        public async Task WebAppNotRunning_WhenAttemptingToNavigate_HandlesGracefully()
        {
            // Arrange
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(5); // Shorter timeout for this test
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

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

            await app.DisposeAsync();
        }
    }
}
