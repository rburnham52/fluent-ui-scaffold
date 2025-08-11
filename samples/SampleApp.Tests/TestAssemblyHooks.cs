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

            // SPA assets are built into wwwroot during Release build via MSBuild target

            // For ASP.NET Core applications: build only the server configuration
            var serverConfig = ServerConfiguration.CreateDotNetServer(
                    TestConfiguration.BaseUri,
                    projectPath
                )
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .WithSpaProxy(false)
                .WithAspNetCoreEnvironment("Development")
                .WithAspNetCoreHostingStartupAssemblies("")
                .WithHealthCheckEndpoints("/", "/index.html")
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .WithProcessName("SampleApp")
                .Build();

            // For Aspire applications, just change the builder:
            /*
            var aspireProjectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.AppHost.csproj");
            options.ServerConfiguration = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5000"),
                aspireProjectPath
            )
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .WithAspireDashboardOtlpEndpoint("https://localhost:21097")
                .WithAspireResourceServiceEndpoint("https://localhost:22268")
                .WithAspNetCoreEnvironment("Development")
                .WithDotNetEnvironment("Development")
                .Build();
            */

            // For Node.js applications:
            /*
            var nodeProjectPath = Path.Combine(projectRoot, "samples", "NodeApp", "package.json");
            options.ServerConfiguration = ServerConfiguration.CreateNodeJsServer(
                new Uri("http://localhost:3000"),
                nodeProjectPath
            )
                .WithNpmScript("dev") // Use "dev" script instead of "start"
                .WithNodeEnvironment("development")
                .WithHealthCheckEndpoints("/", "/health", "/api/status")
                .WithEnvironmentVariable("DEBUG", "app:*") // Add custom environment variable
                .WithStartupTimeout(TimeSpan.FromSeconds(90))
                .Build();
            */

            await WebServerManager.StartServerAsync(serverConfig);
        }

        // No additional build steps here; MSBuild handles SPA build/copy for Release
    }
}
