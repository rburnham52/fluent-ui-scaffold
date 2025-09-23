using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Base class for BDD (Behavior Driven Development) step definitions that demonstrates
    /// the shared options pattern for FluentUIScaffold integration with Reqnroll.
    /// This ensures consistent options between WebServerManager and AppScaffold.
    /// </summary>
    public abstract class BDDStepDefinitionsBase : IAsyncDisposable
    {
        /// <summary>
        /// Gets the AppScaffold instance with shared options.
        /// </summary>
        public AppScaffold<WebApp> FluentUi { get; }

        /// <summary>
        /// Initializes a new instance of BDDStepDefinitionsBase with shared options.
        /// </summary>
        protected BDDStepDefinitionsBase()
        {
            // Create framework options; server lifecycle is handled by WebServerManager
            FluentUi = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                    options.HeadlessMode = TestConfiguration.IsHeadlessMode;
                    options.SlowMo = 250;
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();
        }

        /// <summary>
        /// Initializes the AppScaffold asynchronously.
        /// </summary>
        /// <returns>A task that completes when initialization is done.</returns>
        public async Task InitializeAsync()
        {
            await FluentUi.StartAsync();
        }

        /// <summary>
        /// Disposes the AppScaffold asynchronously.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (FluentUi != null)
            {
                await FluentUi.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Example Reqnroll steps class that inherits from BDDStepDefinitionsBase.
    /// This demonstrates how to use the shared options pattern in BDD scenarios.
    /// </summary>
    public class HomePageBDDSteps : BDDStepDefinitionsBase
    {
        // Example Reqnroll step definitions would go here
        // [Given(@"I am on the home page")]
        // public async Task GivenIAmOnTheHomePage()
        // {
        //     await InitializeAsync();
        //     FluentUi.NavigateToUrl(TestConfiguration.BaseUri);
        // }
        //
        // [When(@"I click the login button")]
        // public async Task WhenIClickTheLoginButton()
        // {
        //     var playwright = FluentUi.Framework<FluentUIScaffold.Playwright.PlaywrightDriver>();
        //     // Perform actual UI interaction
        // }
        //
        // [Then(@"I should see the login form")]
        // public async Task ThenIShouldSeeTheLoginForm()
        // {
        //     var playwright = FluentUi.Framework<FluentUIScaffold.Playwright.PlaywrightDriver>();
        //     // Verify UI elements
        // }
    }
}
