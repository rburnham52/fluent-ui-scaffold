using System;

using FluentUIScaffold.Core.Pages;

using Microsoft.Playwright;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Registration tab of the sample app.
    /// Demonstrates multi-field form interaction.
    /// </summary>
    public class RegistrationPage : Page<RegistrationPage>
    {
        protected RegistrationPage(IServiceProvider serviceProvider)
            : base(serviceProvider) { }

        public RegistrationPage ClickRegisterTab()
        {
            return Enqueue<IPage>(async page =>
            {
                await page.ClickAsync("[data-testid='nav-register']").ConfigureAwait(false);
            });
        }
    }
}
