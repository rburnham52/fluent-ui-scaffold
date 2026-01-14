using System;
using System.Collections.Generic;
using System.Linq;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Profile page of the FluentUIScaffold sample application.
    /// Demonstrates form validation, complex form interactions, and state management.
    /// </summary>
    public class ProfilePage : Page<ProfilePage>
    {
        public ProfilePage(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // Configure elements for the profile page
        }

        private void EnsureOnProfileAndEditing()
        {
            if (!Driver.IsVisible(".profile-section"))
            {
                if (!Driver.IsVisible("nav"))
                {
                    Driver.NavigateToUrl(TestConfiguration.BaseUri);
                    Driver.WaitForElementToBeVisible("nav");
                }
                Driver.Click("nav button[data-testid='nav-profile']");
                Driver.WaitForElementToBeVisible(".profile-section");
            }
            // Enter edit mode if not already
            if (!Driver.IsVisible("[data-testid='save-profile-btn']"))
            {
                Driver.Click("[data-testid='edit-profile-btn']");
                Driver.WaitForElementToBeVisible("[data-testid='save-profile-btn']");
            }
        }

        public ProfilePage EnterName(string name)
        {
            EnsureOnProfileAndEditing();
            var parts = (name ?? string.Empty).Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var first = parts.Length > 0 ? parts[0] : string.Empty;
            var last = parts.Length > 1 ? parts[1] : string.Empty;
            Driver.Type("[data-testid='first-name-input']", first);
            Driver.Type("[data-testid='last-name-input']", last);
            return this;
        }

        public ProfilePage EnterEmail(string email)
        {
            EnsureOnProfileAndEditing();
            Driver.Type("[data-testid='email-input']", email);
            return this;
        }

        public ProfilePage ClickSave()
        {
            Driver.Click("[data-testid='save-profile-btn']");
            return this;
        }

        public string GetName()
        {
            // Compose name from input values for deterministic verification
            var first = Driver.GetValue("[data-testid='first-name-input']");
            var last = Driver.GetValue("[data-testid='last-name-input']");
            return string.Join(" ", new[] { first, last }.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        public string GetEmail()
        {
            return Driver.GetValue("[data-testid='email-input']");
        }
    }
}
