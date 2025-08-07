using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;


namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating registration flow functionality using Page Object Models.
    /// These tests show how to use POMs to create reusable, maintainable UI tests.
    /// </summary>
    [TestClass]
    public class RegistrationFlowTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private RegistrationPage? _registrationPage;

        [TestInitialize]
        public async Task Setup()
        {
            // Arrange - Set up the application and page objects
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            // Create the registration page object using the driver directly
            var playwright = _app.Framework<FluentUIScaffold.Playwright.PlaywrightDriver>();
            _registrationPage = new RegistrationPage(_app.ServiceProvider);
            await _registrationPage.NavigateToRegistrationAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
        }

        [TestMethod]
        public async Task CompleteRegistration_WithValidData_CreatesAccountAndShowsSuccessMessage()
        {
            // Act - Use the Page Object Model to complete registration
            _registrationPage!
                .CompleteRegistration("test@example.com", "password123", "John", "Doe")
                .VerifySuccessfulRegistration();
        }

        [TestMethod]
        public async Task TestValidation_WithInvalidData_ShowsValidationErrors()
        {
            // Act & Assert - Test validation scenarios using POM methods
            _registrationPage!
                .TestEmptyFieldsValidation()
                .TestInvalidEmailValidation("invalid-email", "password123", "John", "Doe")
                .TestShortPasswordValidation("test@example.com", "123", "John", "Doe");
        }

        [TestMethod]
        public async Task VerifyFormStructure_WhenRegistrationPageLoaded_ShowsAllRequiredFields()
        {
            // Act & Assert - Verify form structure using POM
            _registrationPage!
                .VerifyFormStructure();
        }

        [TestMethod]
        public async Task FillRegistrationForm_WithValidData_FillsAllFieldsCorrectly()
        {
            // Act - Test form interactions using POM
            _registrationPage!
                .FillRegistrationForm("user@test.com", "securepassword", "Jane", "Smith")
                .ClearAllFields()
                .FocusOnEmailField();
        }

        [TestMethod]
        public async Task CompleteRegistration_WithValidData_ClearsFormAfterSuccess()
        {
            // Act - Complete registration and verify form is cleared
            _registrationPage!
                .CompleteRegistration("test@example.com", "password123", "John", "Doe")
                .VerifyFormIsCleared();
        }

        [TestMethod]
        public async Task CompleteRegistration_WithMultipleAttempts_HandlesAllAttemptsSuccessfully()
        {
            // Act - Test multiple registration attempts
            _registrationPage!
                .CompleteRegistration("user1@test.com", "password123", "User1", "Test")
                .VerifySuccessfulRegistration()
                .CompleteRegistration("user2@test.com", "password456", "User2", "Test")
                .VerifySuccessfulRegistration();
        }

        [TestMethod]
        public async Task TestAllValidationScenarios_WhenVariousInvalidInputsProvided_ShowsAppropriateErrors()
        {
            // Act & Assert - Test all validation scenarios in sequence
            _registrationPage!
                // Test empty fields
                .TestEmptyFieldsValidation()
                // Test invalid email
                .TestInvalidEmailValidation("not-an-email", "password123", "John", "Doe")
                // Test short password
                .TestShortPasswordValidation("valid@email.com", "123", "John", "Doe")
                // Test successful registration after validation errors
                .CompleteRegistration("valid@email.com", "password123", "John", "Doe")
                .VerifySuccessfulRegistration();
        }
    }
}
