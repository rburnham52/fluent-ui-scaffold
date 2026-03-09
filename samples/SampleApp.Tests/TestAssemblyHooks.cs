using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests
{
    /// <summary>
    /// Assembly-level hooks for standard (non-Aspire) testing.
    /// Starts the sample app via DotNet hosting and shares it across all tests.
    /// </summary>
    [TestClass]
    public static class TestAssemblyHooks
    {
        private static AppScaffold<WebApp>? _app;

        public static AppScaffold<WebApp> App => SharedTestApp.App;

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            var projectRoot = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");
            var workingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");

            _app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .WithHeadlessMode(TestConfiguration.IsHeadlessMode)
                .WithEnvironmentName("Development")
                .WithSpaProxy(false)
                .UseDotNetHosting(opts =>
                {
                    opts.BaseUrl = TestConfiguration.BaseUri;
                    opts.ProjectPath = projectPath;
                    opts.Framework = "net8.0";
                    opts.Configuration = "Release";
                    opts.HealthCheckEndpoints = new[] { "/", "/index.html" };
                    opts.StartupTimeout = TimeSpan.FromSeconds(120);
                    opts.ProcessName = "SampleApp";
                    opts.WorkingDirectory = workingDirectory;
                })
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                })
                .Build<WebApp>();

            SharedTestApp.App = _app;
            await _app.StartAsync();
            Console.WriteLine("Web server started successfully via DotNet hosting.");
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
                Console.WriteLine("Web server stopped.");
            }
        }
    }
}
