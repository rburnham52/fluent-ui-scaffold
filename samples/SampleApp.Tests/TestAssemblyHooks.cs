using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests
{
    /// <summary>
    /// MSTest-specific assembly hooks that use the unified TestAssemblyWebHook for web server management.
    /// This provides a clean, unified approach to web server management.
    /// </summary>
    [TestClass]
    public class TestAssemblyHooks
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Enable web server startup for testing
            StartServerAsync().Wait();
            Console.WriteLine("Web server startup enabled for testing.");
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            TestAssemblyWebHook.StopServer();
        }

        private static async Task StartServerAsync()
        {
            try
            {
                // Find the repo root by walking up from AppContext.BaseDirectory
                string? repoRoot = AppContext.BaseDirectory;
                while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
                {
                    repoRoot = Directory.GetParent(repoRoot)?.FullName;
                }
                if (repoRoot == null)
                {
                    throw new InvalidOperationException("Could not find repo root (directory containing .git)");
                }
                var projectPath = Path.Combine(repoRoot, "samples", "SampleApp", "SampleApp.csproj");
                Console.WriteLine($"Repo root: {repoRoot}");
                Console.WriteLine($"Using project path: {projectPath}");

                // Create options for the unified TestAssemblyWebHook
                var options = new FluentUIScaffoldOptions
                {
                    BaseUrl = TestConfiguration.BaseUri,
                    DefaultWaitTimeout = TimeSpan.FromSeconds(60),
                    LogLevel = LogLevel.Information,
                    WebServerProjectPath = projectPath
                };

                // Use the unified TestAssemblyWebHook to start the server
                await TestAssemblyWebHook.StartServerAsync(options);

                Console.WriteLine("Sample app server started successfully via unified TestAssemblyWebHook.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start sample app server via unified TestAssemblyWebHook: {ex.Message}");
                throw;
            }
        }
    }
}
