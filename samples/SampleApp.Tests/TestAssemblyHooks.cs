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
            // Get the absolute path to the SampleApp project
            var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");
            var workingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");

            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                EnableWebServerLaunch = true,
                EnableProjectDetection = true,
                AdditionalSearchPaths = { "samples" },
                ServerConfiguration = new ServerConfiguration
                {
                    ProjectPath = projectPath,
                    WorkingDirectory = workingDirectory,
                    BaseUrl = new Uri("http://localhost:5000"),
                    ServerType = ServerType.AspNetCore,
                    EnvironmentVariables =
                    {
                        ["ASPNETCORE_ENVIRONMENT"] = "Development",
                        ["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "Microsoft.AspNetCore.SpaProxy"
                    },
                    EnableSpaProxy = true,
                    StartupTimeout = TimeSpan.FromSeconds(90)
                }
            };

            await WebServerManager.StartServerAsync(options);
        }
    }
}
