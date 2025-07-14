using System;

using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the registration page of the FluentUIScaffold sample application.
    /// Demonstrates form interactions and validation for user registration.
    /// </summary>
    public class RegistrationPage : BasePageComponent<PlaywrightDriver, RegistrationPage>
    {
        public RegistrationPage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the registration page
        }

        public RegistrationPage EnterFirstName(string firstName)
        {
            Driver.Type("#first-name-input", firstName);
            return this;
        }

        public RegistrationPage EnterLastName(string lastName)
        {
            Driver.Type("#last-name-input", lastName);
            return this;
        }

        public RegistrationPage EnterEmail(string email)
        {
            Driver.Type("#email-input", email);
            return this;
        }

        public RegistrationPage EnterPassword(string password)
        {
            Driver.Type("#password-input", password);
            return this;
        }

        public RegistrationPage EnterConfirmPassword(string confirmPassword)
        {
            Driver.Type("#confirm-password-input", confirmPassword);
            return this;
        }

        public RegistrationPage ClickRegister()
        {
            Driver.Click("#register-button");
            return this;
        }

        public string GetErrorMessage()
        {
            return Driver.GetText("#error-message");
        }

        public bool IsErrorMessageVisible()
        {
            return Driver.IsVisible("#error-message");
        }
    }
}
