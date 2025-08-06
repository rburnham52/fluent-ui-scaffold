using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Profile page of the FluentUIScaffold sample application.
    /// Demonstrates form validation, complex form interactions, and state management.
    /// </summary>
    public class ProfilePage : BasePageComponent<PlaywrightDriver, ProfilePage>
    {
        public ProfilePage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the profile page
        }

        public ProfilePage EnterName(string name)
        {
            Driver.Type("#name-input", name);
            return this;
        }

        public ProfilePage EnterEmail(string email)
        {
            Driver.Type("#email-input", email);
            return this;
        }

        public ProfilePage ClickSave()
        {
            Driver.Click("#save-button");
            return this;
        }

        public string GetName()
        {
            return Driver.GetText("#name-display");
        }

        public string GetEmail()
        {
            return Driver.GetText("#email-display");
        }
    }
}
