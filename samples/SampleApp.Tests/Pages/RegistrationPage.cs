using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page Object Model for the Registration page.
    /// Demonstrates how to encapsulate form interactions and validations.
    /// </summary>
    public class RegistrationPage : Page<RegistrationPage>
    {
        public RegistrationPage(IServiceProvider serviceProvider)
            : base(serviceProvider, TestConfiguration.BaseUri)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure form elements
            EmailInput = Element("#email-input")
                .WithDescription("Email Input Field")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            PasswordInput = Element("#password-input")
                .WithDescription("Password Input Field")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            FirstNameInput = Element("#first-name-input")
                .WithDescription("First Name Input Field")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            LastNameInput = Element("#last-name-input")
                .WithDescription("Last Name Input Field")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            RegisterButton = Element("#register-button")
                .WithDescription("Register Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            SuccessMessage = Element("#success-message")
                .WithDescription("Success Message")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            ErrorMessage = Element("#error-message")
                .WithDescription("Error Message")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            FormTitle = Element(".registration-form h2")
                .WithDescription("Registration Form Title")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
        }

        // Element properties
        public IElement EmailInput { get; set; } = null!;
        public IElement PasswordInput { get; set; } = null!;
        public IElement FirstNameInput { get; set; } = null!;
        public IElement LastNameInput { get; set; } = null!;
        public IElement RegisterButton { get; set; } = null!;
        public IElement SuccessMessage { get; set; } = null!;
        public IElement ErrorMessage { get; set; } = null!;
        public IElement FormTitle { get; set; } = null!;

        /// <summary>
        /// Navigates to the registration page.
        /// </summary>
        public async Task NavigateToRegistrationPageAsync()
        {
            Driver.NavigateToUrl(TestConfiguration.BaseUri);
            // Wait for nav to render before interacting
            Driver.WaitForElementToBeVisible("nav button[data-testid='nav-register']");
            // Click the registration navigation button
            Driver.Click("nav button[data-testid='nav-register']");
            await WaitForPageToLoadAsync();
        }

        /// <summary>
        /// Navigates to the registration page (alias for NavigateToRegistrationPageAsync).
        /// </summary>
        public async Task NavigateToRegistrationAsync()
        {
            await NavigateToRegistrationPageAsync();
        }

        /// <summary>
        /// Waits for the registration page to be fully loaded.
        /// </summary>
        public async Task WaitForPageToLoadAsync()
        {
            Driver.WaitForElementToBeVisible(".registration-form");
            Driver.WaitForElementToBeVisible("#email-input");
            Driver.WaitForElementToBeVisible("#password-input");
            Driver.WaitForElementToBeVisible("#first-name-input");
            Driver.WaitForElementToBeVisible("#last-name-input");
        }

        /// <summary>
        /// Fills the registration form with the provided data.
        /// </summary>
        public RegistrationPage FillRegistrationForm(string email, string password, string firstName, string lastName)
        {
            return Type(e => e.EmailInput, email)
                   .Type(e => e.PasswordInput, password)
                   .Type(e => e.FirstNameInput, firstName)
                   .Type(e => e.LastNameInput, lastName);
        }

        /// <summary>
        /// Submits the registration form.
        /// </summary>
        public RegistrationPage SubmitRegistrationForm()
        {
            return Click(e => e.RegisterButton);
        }

        /// <summary>
        /// Completes the full registration flow.
        /// </summary>
        public RegistrationPage CompleteRegistration(string email, string password, string firstName, string lastName)
        {
            return FillRegistrationForm(email, password, firstName, lastName)
                   .SubmitRegistrationForm();
        }

        /// <summary>
        /// Verifies that the success message is displayed.
        /// </summary>
        public RegistrationPage VerifySuccessMessage()
        {
            return Verify
                   .Visible(e => e.SuccessMessage)
                   .TextIs(e => e.SuccessMessage, "Registration successful!")
                   .And;
        }

        /// <summary>
        /// Verifies that an error message is displayed.
        /// </summary>
        public RegistrationPage VerifyErrorMessage(string expectedError)
        {
            return Verify
                   .Visible(e => e.ErrorMessage)
                   .TextIs(e => e.ErrorMessage, expectedError)
                   .And;
        }

        /// <summary>
        /// Verifies that the form is cleared after successful registration.
        /// </summary>
        public RegistrationPage VerifyFormIsCleared()
        {
            return Verify
                   .TextIs(e => e.EmailInput, "")
                   .TextIs(e => e.PasswordInput, "")
                   .TextIs(e => e.FirstNameInput, "")
                   .TextIs(e => e.LastNameInput, "")
                   .And;
        }

        /// <summary>
        /// Verifies that the form has the correct structure.
        /// </summary>
        public RegistrationPage VerifyFormStructure()
        {
            // Verify form structure using new Verify API (waits automatically)
            return Verify
                   .TextIs(e => e.FormTitle, "User Registration")
                   .TextIs(e => e.EmailInput, "") // Should be empty initially
                   .TextIs(e => e.PasswordInput, "")
                   .TextIs(e => e.FirstNameInput, "")
                   .TextIs(e => e.LastNameInput, "")
                   .And;
        }

        /// <summary>
        /// Tests validation with empty fields.
        /// </summary>
        public RegistrationPage TestEmptyFieldsValidation()
        {
            return SubmitRegistrationForm()
                   .VerifyErrorMessage("All fields are required");
        }

        /// <summary>
        /// Tests validation with invalid email.
        /// </summary>
        public RegistrationPage TestInvalidEmailValidation(string invalidEmail, string password, string firstName, string lastName)
        {
            return FillRegistrationForm(invalidEmail, password, firstName, lastName)
                   .SubmitRegistrationForm()
                   .VerifyErrorMessage("valid email address");
        }

        /// <summary>
        /// Tests validation with short password.
        /// </summary>
        public RegistrationPage TestShortPasswordValidation(string email, string shortPassword, string firstName, string lastName)
        {
            return FillRegistrationForm(email, shortPassword, firstName, lastName)
                   .SubmitRegistrationForm()
                   .VerifyErrorMessage("at least 8 characters");
        }

        /// <summary>
        /// Clears all form fields.
        /// </summary>
        public RegistrationPage ClearAllFields()
        {
            return Clear(e => e.EmailInput)
                   .Clear(e => e.PasswordInput)
                   .Clear(e => e.FirstNameInput)
                   .Clear(e => e.LastNameInput);
        }

        /// <summary>
        /// Focuses on the email input field.
        /// </summary>
        public RegistrationPage FocusOnEmailField()
        {
            return Focus(e => e.EmailInput);
        }

        /// <summary>
        /// Verifies that the registration was successful.
        /// </summary>
        public RegistrationPage VerifySuccessfulRegistration()
        {
            return VerifySuccessMessage()
                   .VerifyFormIsCleared();
        }
    }
}
