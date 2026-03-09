using System;

using FluentUIScaffold.Core.Pages;

using Microsoft.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Profile tab of the sample app.
    /// Demonstrates form editing and save/cancel workflows.
    /// </summary>
    public class ProfilePage : Page<ProfilePage>
    {
        protected ProfilePage(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public ProfilePage ClickProfileTab()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='nav-profile']").ConfigureAwait(false);
            });
        }

        public ProfilePage ClickEdit()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='edit-profile-btn']").ConfigureAwait(false);
            });
        }

        public ProfilePage EnterFirstName(string name)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.FillAsync("[data-testid='first-name-input']", name).ConfigureAwait(false);
            });
        }

        public ProfilePage EnterLastName(string name)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.FillAsync("[data-testid='last-name-input']", name).ConfigureAwait(false);
            });
        }

        public ProfilePage EnterEmail(string email)
        {
            return Enqueue<IPage>(async page =>
            {
                await page.FillAsync("[data-testid='email-input']", email).ConfigureAwait(false);
            });
        }

        public ProfilePage Save()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='save-profile-btn']").ConfigureAwait(false);
            });
        }

        public ProfilePage Cancel()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='cancel-edit-btn']").ConfigureAwait(false);
            });
        }

        public ProfilePage Reset()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='reset-profile-btn']").ConfigureAwait(false);
            });
        }
    }
}
