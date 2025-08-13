using System;
using System.Diagnostics;
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
    /// MSTest-specific assembly hooks that use the new WebServerManager for web server management.
    /// This provides automatic project detection and flexible server startup.
    /// </summary>
    [TestClass]
    public class TestAssemblyHooks
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Explicitly register the Playwright plugin for all tests (single registration)
            FluentUIScaffoldPlaywrightBuilder.UsePlaywright();
            Console.WriteLine($"Registered plugins: {FluentUIScaffold.Core.Plugins.PluginRegistry.GetAll().Count}");

            // Start web server via WebServerManager
            StartServerAsync().Wait();
            Console.WriteLine("Web server started successfully.");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            WebServerManager.StopServer();
            Console.WriteLine("Web server stopped.");
        }

        private static async Task StartServerAsync()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");
            var workingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");

            var plan = ServerConfiguration.CreateDotNetServer(
                    TestConfiguration.BaseUri,
                    projectPath
                )
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .EnableSpaProxy(false)
                .WithAspNetCoreEnvironment("Development")
                .WithHealthCheckEndpoints("/", "/index.html")
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .WithProcessName("SampleApp")
                .WithWorkingDirectory(workingDirectory)
                .Build();

            await WebServerManager.StartServerAsync(plan);
        }

        // No additional build steps here; MSBuild handles SPA build/copy for Release
    }
}
