using System;

using FluentUIScaffold.Core.Pages;

using Microsoft.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Login tab of the sample app.
    /// </summary>
    public class LoginPage : Page<LoginPage>
    {
        protected LoginPage(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        /// <summary>
        /// Clicks the Login navigation tab.
        /// </summary>
        public LoginPage ClickLoginTab()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='nav-login']").ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Types into the email field.
        /// </summary>
        public LoginPage EnterEmail(string email)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.FillAsync("[data-testid='login-email']", email).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Types into the password field.
        /// </summary>
        public LoginPage EnterPassword(string password)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.FillAsync("[data-testid='login-password']", password).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Clicks the login submit button.
        /// </summary>
        public LoginPage Submit()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='login-submit']").ConfigureAwait(false);
            });
        }
    }
}
