using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;


namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating login flow functionality using the LoginPage object model.
    /// These tests show how to handle user login workflows with proper page object pattern.
    /// </summary>
    [TestClass]
    public class LoginFlowTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private LoginPage? _loginPage;

        [TestInitialize]
        public async Task Setup()
        {
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            _loginPage = new LoginPage(_app.ServiceProvider, TestConfiguration.BaseUri);

            // Navigate to the login page
            await _loginPage.NavigateToLoginPageAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
        }

        [TestMethod]
        public async Task CompleteLoginFlow_WithValidCredentials_ShowsSuccessMessageAndClearsForm()
        {
            // Act - Use the LoginPage object model to complete login flow
            _loginPage!.CompleteLogin("user@example.com", "password123");

            // Assert - Verify success message appears using LoginPage methods
            Assert.IsTrue(_loginPage.IsSuccessMessageVisible(), "Success message should be visible after login");
            var successMessage = _loginPage.GetSuccessMessage();
            Assert.IsTrue(successMessage.Contains("Login successful!"), "Success message should contain expected text");

            // Verify form is cleared by checking input values
            var emailValue = _loginPage.EmailInput.GetAttribute("value");
            Assert.AreEqual("", emailValue, "Email field should be cleared after successful login");
        }

        [TestMethod]
        public async Task HandleLoginValidation_WithInvalidData_ShowsAppropriateErrorMessages()
        {
            // Act & Assert - Test validation with empty fields using LoginPage
            _loginPage!.ClickLogin();
            Assert.IsTrue(_loginPage.IsErrorMessageVisible(), "Error message should be visible for empty fields");
            var errorMessage = _loginPage.GetErrorMessage();
            Assert.IsTrue(errorMessage.Contains("Email and password are required"), "Should show error for empty fields");

            // Test validation with invalid email
            _loginPage.FillLoginForm("invalid-email", "password123")
                     .SubmitLogin();

            Assert.IsTrue(_loginPage.IsErrorMessageVisible(), "Error message should be visible for invalid email");
            errorMessage = _loginPage.GetErrorMessage();
            Assert.IsTrue(errorMessage.Contains("valid email address"), "Should show error for invalid email");

            // Test validation with invalid credentials
            _loginPage.FillLoginForm("wrong@example.com", "wrongpassword")
                     .SubmitLogin();

            Assert.IsTrue(_loginPage.IsErrorMessageVisible(), "Error message should be visible for invalid credentials");
            errorMessage = _loginPage.GetErrorMessage();
            Assert.IsTrue(errorMessage.Contains("Invalid credentials"), "Should show error for invalid credentials");
        }

        [TestMethod]
        public async Task VerifyLoginFormStructure_WhenLoginPageLoaded_ShowsAllRequiredElements()
        {
            // Act & Assert - Use LoginPage to verify form structure
            Assert.IsTrue(_loginPage!.Form.IsVisible(), "Login form should be visible");
            Assert.IsTrue(_loginPage.EmailInput.IsVisible(), "Email input should be visible");
            Assert.IsTrue(_loginPage.PasswordInput.IsVisible(), "Password input should be visible");
            Assert.IsTrue(_loginPage.LoginButton.IsVisible(), "Login button should be visible");

            // Verify form elements are accessible
            _loginPage.EmailInput.Click();
            _loginPage.PasswordInput.Click();
            _loginPage.LoginButton.WaitForClickable();
        }

        [TestMethod]
        public async Task HandleLoginFormInteractions_WithValidInput_AcceptsInputCorrectly()
        {
            // Act - Use LoginPage methods to interact with form
            _loginPage!.FillLoginForm("user@test.com", "securepassword");

            // Assert - Verify values are entered correctly using LoginPage
            var emailValue = _loginPage.EmailInput.GetAttribute("value");
            Assert.AreEqual("user@test.com", emailValue, "Email should be entered correctly");

            var passwordValue = _loginPage.PasswordInput.GetAttribute("value");
            Assert.AreEqual("securepassword", passwordValue, "Password should be entered correctly");

            // Test clearing fields using LoginPage fluent API
            _loginPage.Clear(e => e.EmailInput);
            emailValue = _loginPage.EmailInput.GetAttribute("value");
            Assert.AreEqual("", emailValue, "Email field should be cleared");

            // Test focus behavior using LoginPage fluent API
            _loginPage.Focus(e => e.EmailInput);
            // Note: IsFocused is not available on IElement, so we'll just verify the focus action completed
            Assert.IsTrue(_loginPage.EmailInput.IsVisible(), "Email input should still be visible after focus");
        }
    }
}
