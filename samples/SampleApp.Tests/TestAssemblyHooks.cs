using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

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
            // Enable web server startup for testing
            StartServerAsync().Wait();
            Console.WriteLine("Web server started successfully.");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Stop the web server after all tests in the assembly have run
            WebServerManager.StopServer();
            Console.WriteLine("Web server stopped.");
        }

        private static async Task StartServerAsync()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");
            var workingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");

            // For ASP.NET Core applications
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                EnableWebServerLaunch = true,
                WebServerLogLevel = LogLevel.Information, // Control launcher log level
                ServerConfiguration = ServerConfiguration.CreateDotNetServer(
                    new Uri("http://localhost:5000"),
                    projectPath
                )
                    .WithFramework("net8.0")
                    .WithConfiguration("Release")
                    .WithSpaProxy(false)
                    .WithAspNetCoreEnvironment("Development")
                    .WithAspNetCoreHostingStartupAssemblies("") // Disable SPA proxy
                    .Build()
            };

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

            await WebServerManager.StartServerAsync(options);
        }
    }
}
