using System;

using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the login page of the FluentUIScaffold sample application.
    /// Demonstrates form interactions and validation for user login.
    /// </summary>
    public class LoginPage : BasePageComponent<PlaywrightDriver, LoginPage>
    {
        public LoginPage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the login page
        }

        public LoginPage EnterEmail(string email)
        {
            Driver.Type("#email-input", email);
            return this;
        }

        public LoginPage EnterPassword(string password)
        {
            Driver.Type("#password-input", password);
            return this;
        }

        public LoginPage ClickLogin()
        {
            Driver.Click("#login-button");
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
