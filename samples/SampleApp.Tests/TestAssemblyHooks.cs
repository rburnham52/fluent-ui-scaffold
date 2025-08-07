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
            // Disable web server startup since we're running it manually
            StartServerAsync().Wait();
            // Console.WriteLine("Web server started successfully.");
            Console.WriteLine("Web server startup disabled - running manually.");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Disable web server cleanup since we're not starting it automatically
            WebServerManager.StopServer();
            // Console.WriteLine("Web server stopped.");
            Console.WriteLine("Web server cleanup disabled - running manually.");
        }

        private static async Task StartServerAsync()
        {
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");
            var workingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");

            // For ASP.NET Core applications
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://localhost:5001"),
                EnableWebServerLaunch = true,
                WebServerLogLevel = LogLevel.Information, // Control launcher log level,
                HeadlessMode = true,
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                ServerConfiguration = ServerConfiguration.CreateDotNetServer(
                    new Uri("https://localhost:5001"),
                    projectPath
                )
                    .WithFramework("net8.0")
                    .WithConfiguration("Release")
                    .WithSpaProxy(true)
                    .WithAspNetCoreEnvironment("Development")
                    .WithAspNetCoreHostingStartupAssemblies("Microsoft.AspNetCore.SpaProxy") // Disable SPA proxy
                    .WithProcessName("SampleApp")
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
