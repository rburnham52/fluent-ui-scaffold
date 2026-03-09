using System;
using System.Threading.Tasks;

using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Assembly-level hooks for Aspire-hosted testing.
    /// Starts the Aspire distributed application once for the entire test session.
    /// Shares the same test Pages and Examples from SampleApp.Tests via linked files.
    /// </summary>
    [TestClass]
    public static class TestAssemblyHooks
    {
        private static AppScaffold<WebApp>? _app;

        public static AppScaffold<WebApp> App => SharedTestApp.App;

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            if (!TestEnvironmentHelper.CanRunAspireTests)
            {
                Console.WriteLine(TestEnvironmentHelper.GetEnvironmentStatus());
                Assert.Inconclusive("Aspire tests require Docker and the Aspire workload. Skipping.");
                return;
            }

            _app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .WithHeadlessMode(TestConfiguration.IsHeadlessMode)
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { },
                    "sampleapp")
                .Web<WebApp>(options => { })
                .Build<WebApp>();

            SharedTestApp.App = _app;
            await _app.StartAsync();
            Console.WriteLine("Aspire-hosted server started successfully.");
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
                Console.WriteLine("Aspire-hosted server stopped.");
            }
        }
    }
}
