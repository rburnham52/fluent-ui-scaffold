using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Base class for BDD (Behavior Driven Development) step definitions that demonstrates
    /// the shared options pattern for FluentUIScaffold integration with Reqnroll.
    /// This ensures consistent options between WebServerManager and FluentUIScaffoldApp.
    /// </summary>
    public abstract class BDDStepDefinitionsBase
    {
        /// <summary>
        /// Gets the FluentUIScaffoldApp instance with shared options.
        /// </summary>
        public FluentUIScaffoldApp<WebApp> FluentUi { get; }

        /// <summary>
        /// Initializes a new instance of BDDStepDefinitionsBase with shared options.
        /// </summary>
        protected BDDStepDefinitionsBase()
        {
            // Create options that will be shared between WebServerManager and FluentUIScaffoldApp
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("https://localhost:5001"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .WithWebServerLaunch(true)
                .WithWebServerLogLevel(LogLevel.Information)
                .WithServerConfiguration(
                    ServerConfiguration.CreateDotNetServer(
                        new Uri("https://localhost:5001"),
                        "../../../SampleApp.Web/SampleApp.Web.csproj"
                    )
                    .WithFramework("net8.0")
                    .WithConfiguration("Debug")
                    .WithSpaProxy(true)
                    .WithAspNetCoreEnvironment("Development")
                    .Build()
                )
                .Build();

            // The FluentUIScaffoldApp will use shared options if WebServerManager has already set them
            FluentUi = new FluentUIScaffoldApp<WebApp>(options);
        }

        /// <summary>
        /// Initializes the FluentUIScaffoldApp asynchronously.
        /// </summary>
        /// <returns>A task that completes when initialization is done.</returns>
        public async Task InitializeAsync()
        {
            await FluentUi.InitializeAsync();
        }

        /// <summary>
        /// Disposes the FluentUIScaffoldApp.
        /// </summary>
        public void Dispose()
        {
            FluentUi?.Dispose();
        }
    }

    /// <summary>
    /// Example Reqnroll steps class that inherits from BDDStepDefinitionsBase.
    /// This demonstrates how to use the shared options pattern in BDD scenarios.
    /// </summary>
    public class HomePageBDDSteps : BDDStepDefinitionsBase, IDisposable
    {
        // Example Reqnroll step definitions would go here
        // [Given(@"I am on the home page")]
        // public async Task GivenIAmOnTheHomePage()
        // {
        //     await InitializeAsync();
        //     FluentUi.NavigateToUrl(new Uri("http://localhost:5000"));
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

        public new void Dispose()
        {
            base.Dispose();
        }
    }
}
