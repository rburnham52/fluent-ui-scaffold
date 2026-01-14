using System;
using System.IO;
using System.Threading.Tasks;

using Aspire.Hosting;
using Aspire.Hosting.Testing;

using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Test assembly hooks for managing Aspire server lifecycle at the test session level.
    /// Simple configuration-focused approach - delegates complex cleanup to the framework.
    /// </summary>
    [TestClass]
    public static class TestAssemblyHooks
    {
        private static AppScaffold<WebApp>? _sessionApp;

        /// <summary>
        /// Assembly initialization - starts the Aspire server once for the entire test session.
        /// </summary>
        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            // Create and build the app scaffold
            _sessionApp = new FluentUIScaffold.Core.Configuration.FluentUIScaffoldBuilder()
                .UseAspireHosting<Projects.SampleApp_AppHost>(appHost =>
                    {
                        // Configure the app host builder if needed
                    },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                    // Base URL is typically determined by Aspire, but we can set defaults
                    // options.WithBaseUrl(...);
                    options.HeadlessMode = false;
                    options.UsePlaywright();
                })
                .Build<WebApp>();

            // Start the scaffolding (which starts Aspire)
            await _sessionApp.StartAsync();

            Console.WriteLine($"Registered plugins: {FluentUIScaffold.Core.Plugins.PluginRegistry.GetAll().Count}");
        }

        /// <summary>
        /// Assembly cleanup - stops the Aspire server and cleans up resources.
        /// </summary>
        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_sessionApp != null)
            {
                await _sessionApp.DisposeAsync();
                Console.WriteLine("Web server (Aspire) stopped.");
            }
        }

        /// <summary>
        /// Gets the shared FluentUIScaffold app instance for the test session.
        /// </summary>
        public static AppScaffold<WebApp>? GetSessionApp()
        {
            return _sessionApp;
        }
    }
}
