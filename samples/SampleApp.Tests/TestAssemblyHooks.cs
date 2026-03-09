using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests
{
    /// <summary>
    /// Marker type for the web application under test.
    /// </summary>
    public class WebApp { }

    /// <summary>
    /// Assembly-level hooks for managing the shared app scaffold lifecycle.
    /// Start once, share across all tests.
    /// </summary>
    [TestClass]
    public static class TestAssemblyHooks
    {
        private static AppScaffold<WebApp>? _app;

        /// <summary>
        /// Gets the shared app scaffold instance.
        /// </summary>
        public static AppScaffold<WebApp> App => _app
            ?? throw new InvalidOperationException("App not initialized. AssemblyInitialize has not run.");

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            _app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .UseDotNetHosting(opts =>
                {
                    opts.BaseUrl = TestConfiguration.BaseUri;
                    opts.ProjectPath = "../SampleApp/SampleApp.csproj";
                })
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = TestConfiguration.BaseUri;
                })
                .Build<WebApp>();

            await _app.StartAsync();
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }
    }
}
