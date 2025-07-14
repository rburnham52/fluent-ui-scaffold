using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Playwright;

namespace SampleApp.Tests.Pages
{
    public class RegistrationPage : BasePageComponent<PlaywrightDriver, RegistrationPage>
    {
        public IElement EmailInput { get; private set; }
        public IElement PasswordInput { get; private set; }
        public IElement FirstNameInput { get; private set; }
        public IElement LastNameInput { get; private set; }
        public IElement RegisterButton { get; private set; }
        public IElement SuccessMessage { get; private set; }
        public IElement ErrorMessage { get; private set; }
        public IElement Form { get; private set; }

        public RegistrationPage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the registration page using ElementFactory
            EmailInput = Element("#email-input")
                .WithDescription("Email Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            PasswordInput = Element("#password-input")
                .WithDescription("Password Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            FirstNameInput = Element("#first-name-input")
                .WithDescription("First Name Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            LastNameInput = Element("#last-name-input")
                .WithDescription("Last Name Input")
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

            Form = Element("#registrationForm")
                .WithDescription("Registration Form")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
        }

        // Fluent API methods using the new element actions
        public RegistrationPage EnterEmail(string email)
        {
            return Type(e => e.EmailInput, email);
        }

        public RegistrationPage EnterPassword(string password)
        {
            return Type(e => e.PasswordInput, password);
        }

        public RegistrationPage EnterFirstName(string firstName)
        {
            return Type(e => e.FirstNameInput, firstName);
        }

        public RegistrationPage EnterLastName(string lastName)
        {
            return Type(e => e.LastNameInput, lastName);
        }

        public RegistrationPage ClickRegister()
        {
            return Click(e => e.RegisterButton);
        }

        public string GetSuccessMessage()
        {
            return SuccessMessage.GetText();
        }

        public bool IsSuccessMessageVisible()
        {
            return SuccessMessage.IsVisible();
        }

        public bool IsErrorMessageVisible()
        {
            return ErrorMessage.IsVisible();
        }

        // Convenience method for complete registration flow
        public RegistrationPage CompleteRegistration(string email, string password, string firstName, string lastName)
        {
            return EnterEmail(email)
                .EnterPassword(password)
                .EnterFirstName(firstName)
                .EnterLastName(lastName)
                .ClickRegister();
        }

        // Custom methods for registration flow as specified in the story
        public RegistrationPage FillRegistrationForm(string email, string password, string firstName, string lastName)
        {
            return this
                .Type(e => e.EmailInput, email)
                .Type(e => e.PasswordInput, password)
                .Type(e => e.FirstNameInput, firstName)
                .Type(e => e.LastNameInput, lastName);
        }

        public RegistrationPage SubmitRegistration()
        {
            return this.Click(e => e.RegisterButton);
        }

        // Verification methods as specified in the story
        public RegistrationPage VerifyRegistrationSuccess()
        {
            Verify.ElementContainsText("#success-message", "Registration successful!");
            return this;
        }

        public RegistrationPage VerifyRegistrationError(string expectedError)
        {
            Verify.ElementContainsText("#error-message", expectedError);
            return this;
        }
    }
}
