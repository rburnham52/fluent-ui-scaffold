using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests
{
    /// <summary>
    /// MSTest-specific assembly hooks that use the new AppScaffold pattern.
    /// This provides automatic project detection and flexible server startup.
    /// </summary>
    [TestClass]
    public class TestAssemblyHooks
    {
        private static AppScaffold<WebApp>? _sessionApp;

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");

            _sessionApp = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .UseDotNetHosting(
                    TestConfiguration.BaseUri,
                    projectPath,
                    config => config
                        .WithFramework("net8.0")
                        .WithConfiguration("Release")
                        .EnableSpaProxy(false)
                        .WithAspNetCoreEnvironment("Development")
                        .WithHealthCheckEndpoints("/", "/index.html")
                        .WithStartupTimeout(TimeSpan.FromSeconds(120))
                        .WithProcessName("SampleApp")
                        .WithWorkingDirectory(Path.Combine(projectRoot, "samples", "SampleApp")))
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await _sessionApp.StartAsync();
            Console.WriteLine("Web server started successfully via AppScaffold.");
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            if (_sessionApp != null)
            {
                await _sessionApp.DisposeAsync();
                Console.WriteLine("Web server stopped.");
            }
        }

        public static AppScaffold<WebApp> CreateApp()
        {
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            return app;
        }

        // No additional build steps here; MSBuild handles SPA build/copy for Release
    }
}
