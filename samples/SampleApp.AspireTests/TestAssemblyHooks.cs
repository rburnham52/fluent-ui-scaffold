using System;
using System.Threading.Tasks;

using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Marker type for the web application under test.
    /// </summary>
    public class WebApp { }

    /// <summary>
    /// Assembly-level hooks for Aspire-hosted test lifecycle.
    /// </summary>
    [TestClass]
    public static class TestAssemblyHooks
    {
        private static AppScaffold<WebApp>? _app;

        public static AppScaffold<WebApp> App => _app
            ?? throw new InvalidOperationException("App not initialized.");

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            _app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .WithHeadlessMode(true)
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { },
                    "sampleapp")
                .Web<WebApp>(options => { })
                .Build<WebApp>();

            await _app.StartAsync();
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_app != null)
                await _app.DisposeAsync();
        }
    }
}
