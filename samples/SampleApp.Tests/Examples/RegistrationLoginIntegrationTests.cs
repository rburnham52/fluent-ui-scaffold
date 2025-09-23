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
    /// Tests demonstrating registration and login integration functionality.
    /// These tests show how to handle combined registration and login workflows.
    /// </summary>
    [TestClass]
    public class RegistrationLoginIntegrationTests
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
        public async Task CompleteRegistrationAndLoginFlow_WithValidData_CompletesBothFlowsSuccessfully()
        {
            // Arrange
            await _registrationPage!.NavigateToRegistrationAsync();

            // Act - Complete registration flow
            _registrationPage
                .CompleteRegistration("integration@example.com", "password123", "Integration", "User")
                .VerifySuccessfulRegistration();

            // Act - Complete login flow with the same credentials
            _loginPage!.CompleteLogin("integration@example.com", "password123")
                .VerifyLoginSuccess("Login successful!");

            // Assert - Verify both flows completed successfully
            var successMessage = _loginPage.GetSuccessMessage();
            Assert.IsTrue(successMessage.Contains("Login successful!"), "Login should be successful after registration");

            // Verify form is cleared after registration
            _registrationPage.VerifyFormIsCleared();
        }

        [TestMethod]
        public async Task HandleIntegrationValidation_WithInvalidData_ShowsAppropriateValidationErrors()
        {
            // Arrange
            await _registrationPage!.NavigateToRegistrationAsync();

            // Act & Assert - Test validation scenarios in sequence
            _registrationPage
                // Test empty fields validation
                .TestEmptyFieldsValidation()
                // Test invalid email validation
                .TestInvalidEmailValidation("not-an-email", "password123", "John", "Doe")
                // Test short password validation
                .TestShortPasswordValidation("valid@email.com", "123", "John", "Doe")
                // Test successful registration after validation errors
                .CompleteRegistration("valid@email.com", "password123", "John", "Doe")
                .VerifySuccessfulRegistration();

            // Now test login with invalid credentials
            _loginPage!.FillLoginForm("invalid@email.com", "wrongpassword")
                .SubmitLogin()
                .VerifyLoginError("Invalid credentials");

            // Assert - Verify error message is shown
            var errorMessage = _loginPage.GetErrorMessage();
            Assert.IsTrue(errorMessage.Contains("Invalid credentials"), "Should show error for invalid credentials");
        }
    }
}
