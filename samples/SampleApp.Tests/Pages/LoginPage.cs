using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the login page of the FluentUIScaffold sample application.
    /// Demonstrates form interactions and validation for user login.
    /// </summary>
    public class LoginPage : Page<LoginPage>
    {
        public IElement EmailInput { get; set; } = null!;
        public IElement PasswordInput { get; set; } = null!;
        public IElement LoginButton { get; set; } = null!;
        public IElement WelcomeMessage { get; set; } = null!;
        public IElement ErrorMessage { get; set; } = null!;
        public IElement Form { get; set; } = null!;
        public IElement SuccessMessage { get; set; } = null!;

        public LoginPage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        /// <summary>
        /// Navigates to the login page.
        /// </summary>
        public async Task NavigateToLoginPageAsync()
        {
            Driver.NavigateToUrl(TestConfiguration.BaseUri);
            Driver.WaitForElementToBeVisible("nav");
            Driver.WaitForElementToBeVisible("nav button[data-testid='nav-login']");
            // Navigate to login section
            Driver.Click("nav button[data-testid='nav-login']");
            await WaitForPageToLoadAsync();
        }

        /// <summary>
        /// Waits for the login page to be fully loaded.
        /// </summary>
        public async Task WaitForPageToLoadAsync()
        {
            Driver.WaitForElementToBeVisible(".login-form");
            // Ensure inputs are present and visible
            Driver.WaitForElementToBeVisible("#email-input");
            Driver.WaitForElementToBeVisible("#password-input");
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the login page using ElementFactory
            EmailInput = Element("#email-input")
                .WithDescription("Email Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            PasswordInput = Element("#password-input")
                .WithDescription("Password Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            LoginButton = Element("#login-button")
                .WithDescription("Login Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            ErrorMessage = Element("#error-message")
                .WithDescription("Error Message")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            SuccessMessage = Element("#success-message")
                .WithDescription("Success Message")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            WelcomeMessage = Element("#success-message")
                .WithDescription("Welcome Message")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            Form = Element("#loginForm")
                .WithDescription("Login Form")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();
        }

        // Fluent API methods using the new element actions
        public LoginPage EnterEmail(string email)
        {
            if (!Driver.IsVisible(".login-form"))
            {
                // Ensure base app shell is loaded
                if (!Driver.IsVisible("nav"))
                {
                    Driver.NavigateToUrl(TestConfiguration.BaseUri);
                    Driver.WaitForElementToBeVisible("nav");
                }
                Driver.Click("nav button[data-testid='nav-login']");
                Driver.WaitForElementToBeVisible(".login-form");
                Driver.WaitForElementToBeVisible("#email-input");
            }
            return Type(e => e.EmailInput, email);
        }

        public LoginPage EnterPassword(string password)
        {
            return Type(e => e.PasswordInput, password);
        }

        public LoginPage ClickLogin()
        {
            return Click(e => e.LoginButton);
        }

        public string GetErrorMessage()
        {
            return ErrorMessage.GetText();
        }

        public bool IsErrorMessageVisible()
        {
            return ErrorMessage.IsVisible();
        }

        public string GetSuccessMessage()
        {
            return SuccessMessage.GetText();
        }

        public bool IsSuccessMessageVisible()
        {
            return SuccessMessage.IsVisible();
        }

        // Convenience method for complete login flow
        public LoginPage CompleteLogin(string email, string password)
        {
            // Ensure we are on the login section
            if (!Driver.IsVisible(".login-form"))
            {
                if (!Driver.IsVisible("nav"))
                {
                    Driver.NavigateToUrl(TestConfiguration.BaseUri);
                    Driver.WaitForElementToBeVisible("nav");
                }
                Driver.Click("nav button[data-testid='nav-login']");
                Driver.WaitForElementToBeVisible(".login-form");
            }
            return EnterEmail(email)
                .EnterPassword(password)
                .ClickLogin();
        }

        // Custom methods for login flow as specified in the story
        public LoginPage FillLoginForm(string email, string password)
        {
            // Ensure we are on the login section before typing
            if (!Driver.IsVisible(".login-form"))
            {
                if (!Driver.IsVisible("nav"))
                {
                    Driver.NavigateToUrl(TestConfiguration.BaseUri);
                    Driver.WaitForElementToBeVisible("nav");
                }
                Driver.Click("nav button[data-testid='nav-login']");
                Driver.WaitForElementToBeVisible(".login-form");
                Driver.WaitForElementToBeVisible("#email-input");
                Driver.WaitForElementToBeVisible("#password-input");
            }

            return this
                .Type(e => e.EmailInput, email)
                .Type(e => e.PasswordInput, password);
        }

        public LoginPage SubmitLogin()
        {
            return this.Click(e => e.LoginButton);
        }

        // Verification methods as specified in the story
        public LoginPage VerifyLoginSuccess(string expectedWelcomeMessage)
        {
            Verify.TextContains(p => p.SuccessMessage, expectedWelcomeMessage);
            return this;
        }

        public LoginPage VerifyLoginError(string expectedError)
        {
            Verify.TextContains(p => p.ErrorMessage, expectedError);
            return this;
        }
    }
}
