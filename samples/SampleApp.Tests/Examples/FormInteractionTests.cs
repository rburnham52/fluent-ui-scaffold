using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;


namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating form interaction capabilities.
    /// These tests show how to interact with form elements like inputs, buttons, and selects.
    /// </summary>
    [TestClass]
    public class FormInteractionTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private RegistrationPage? _registrationPage;
        private LoginPage? _loginPage;

        [TestInitialize]
        public async Task Setup()
        {
            // Arrange - Set up the application and page objects
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithHeadlessMode(false)
                .WithSlowMo(1000)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            // Create page objects
            _registrationPage = new RegistrationPage(_app.ServiceProvider);
            _loginPage = new LoginPage(_app.ServiceProvider, TestConfiguration.BaseUri);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
        }

        [TestMethod]
        public async Task InteractWithFormElements_WhenRegistrationFormLoaded_AllowsFormInteraction()
        {
            // Arrange
            await _registrationPage!.NavigateToRegistrationAsync();

            // Act - Test form interactions
            _registrationPage
                .FillRegistrationForm("user@test.com", "securepassword", "Jane", "Smith")
                .ClearAllFields()
                .FocusOnEmailField();

            // Assert - Verify form structure
            _registrationPage.VerifyFormStructure();

            // Test form validation
            _registrationPage
                .TestEmptyFieldsValidation()
                .TestInvalidEmailValidation("invalid-email", "password123", "John", "Doe")
                .TestShortPasswordValidation("test@example.com", "123", "John", "Doe");
        }

        [TestMethod]
        public async Task HandleComplexFormInteractions_WhenLoginFormLoaded_HandlesAllInteractions()
        {
            // Arrange
            await _registrationPage!.NavigateToRegistrationAsync();

            // Act - Complete registration first to create a user
            _registrationPage
                .CompleteRegistration("complex@example.com", "password123", "Complex", "User")
                .VerifySuccessfulRegistration();

            // Act - Test complex login form interactions
            _loginPage!
                .FillLoginForm("complex@example.com", "password123")
                .SubmitLogin()
                .VerifyLoginSuccess("Login successful!");

            // Assert - Verify login was successful
            var successMessage = _loginPage.GetSuccessMessage();
            Assert.IsTrue(successMessage.Contains("Login successful!"), "Login should be successful");

            // Test form clearing after successful login
            _loginPage.EnterEmail("")
                .EnterPassword("");

            // Test error handling with invalid credentials
            _loginPage
                .FillLoginForm("wrong@example.com", "wrongpassword")
                .SubmitLogin()
                .VerifyLoginError("Invalid credentials");

            var errorMessage = _loginPage.GetErrorMessage();
            Assert.IsTrue(errorMessage.Contains("Invalid credentials"), "Should show error for invalid credentials");
        }
    }
}
