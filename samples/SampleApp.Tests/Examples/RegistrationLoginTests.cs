using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;


namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating registration and login functionality.
    /// These tests show how to handle user registration and login workflows.
    /// </summary>
    [TestClass]
    public class RegistrationLoginTests
    {
        private AppScaffold<WebApp>? _app;
        private RegistrationPage? _registrationPage;
        private LoginPage? _loginPage;

        [TestInitialize]
        public async Task Setup()
        {
            // Arrange - Set up the application and page objects
            _app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await _app.StartAsync();

            // Create page objects
            _registrationPage = new RegistrationPage(_app.ServiceProvider);
            _loginPage = new LoginPage(_app.ServiceProvider, TestConfiguration.BaseUri);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task CompleteRegistrationFlow_WithValidData_CreatesAccountSuccessfully()
        {
            // Arrange
            await _registrationPage!.NavigateToRegistrationAsync();

            // Act - Complete registration with valid data
            _registrationPage
                .CompleteRegistration("test@example.com", "password123", "John", "Doe")
                .VerifySuccessfulRegistration();

            // Assert - Verify form is cleared after successful registration
            _registrationPage.VerifyFormIsCleared();
        }

        [TestMethod]
        public async Task CompleteLoginFlow_WithValidCredentials_AuthenticatesUserSuccessfully()
        {
            // Arrange
            await _registrationPage!.NavigateToRegistrationAsync();

            // Act - Complete registration first
            _registrationPage
                .CompleteRegistration("login@example.com", "password123", "Login", "User")
                .VerifySuccessfulRegistration();

            // Now test login with the created account
            _loginPage!.CompleteLogin("login@example.com", "password123")
                .VerifyLoginSuccess("Login successful!");

            // Assert - Verify login was successful
            var successMessage = _loginPage.GetSuccessMessage();
            Assert.IsTrue(successMessage.Contains("Login successful!"), "Login should be successful");
        }
    }
}
