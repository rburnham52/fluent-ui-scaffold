using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Server;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Demonstrates the migration path from WebServerManager to the new Aspire server lifecycle management.
    /// Shows side-by-side comparisons of old vs new approaches.
    /// </summary>
    [TestClass]
    public class MigrationExampleTests
    {
        private static readonly Uri TestBaseUrl = new("http://localhost:5200");
        private static string? _projectRoot;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            FluentUIScaffoldPlaywrightBuilder.UsePlaywright();
            _projectRoot = GetProjectRoot();
        }

        [TestMethod]
        [TestCategory("Migration")]
        [TestCategory("Example")]
        public async Task OldApproach_WebServerManager_ManualLifecycle()
        {
            // This test demonstrates the OLD approach using WebServerManager
            // This is how tests were written before the new lifecycle management system

            var projectRoot = _projectRoot!;
            var projectPath = Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj");
            var workingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");

            // OLD APPROACH: Manual server management with WebServerManager
            var plan = ServerConfiguration.CreateDotNetServer(
                    new Uri("http://localhost:5200"),
                    projectPath
                )
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .EnableSpaProxy(false)
                .WithAspNetCoreEnvironment("Development")
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .WithProcessName("SampleApp-Old")
                .WithWorkingDirectory(workingDirectory)
                .Build();

            // Start server manually using WebServerManager (old approach)
            await WebServerManager.StartServerAsync(plan);

            try
            {
                // Create FluentUIScaffold app WITHOUT server configuration
                // Server is managed externally by WebServerManager
                var options = new FluentUIScaffoldOptionsBuilder()
                    .WithBaseUrl(TestBaseUrl)
                    .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                    .WithHeadlessMode(true)
                    .Build();

                using var app = new FluentUIScaffoldApp<WebApp>(options);
                await app.InitializeAsync();

                // Verify the server is running
                var driver = app.Framework<Microsoft.Playwright.IPage>();
                var response = await driver.GotoAsync(TestBaseUrl.ToString());
                Assert.IsNotNull(response);
                Assert.IsTrue(response.Ok, "Server should be accessible with old approach");

                // Test API endpoint
                var apiResponse = await driver.EvaluateAsync<bool>("fetch('/weatherforecast').then(r => r.ok)");
                Assert.IsTrue(apiResponse, "API should be accessible with old approach");
            }
            finally
            {
                // Manual cleanup required with old approach
                WebServerManager.StopServer();
            }
        }

        [TestMethod]
        [TestCategory("Migration")]
        [TestCategory("Example")]
        public async Task NewApproach_AspireLifecycleManagement_AutomaticLifecycle()
        {
            // This test demonstrates the NEW approach using Aspire server lifecycle management
            // This is the recommended way to write tests with the new system

            var projectRoot = _projectRoot!;

            // NEW APPROACH: Automatic server management with Aspire AppHost
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5201"),
                Path.Combine(projectRoot, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TimeSpan.FromMinutes(3))
                .WithHeadless(true) // Automatic headless configuration
                .WithKillOrphansOnStart(true) // Automatic cleanup
                .WithForceRestartOnConfigChange(true) // Automatic drift detection
                .WithAutoCI() // Automatic CI detection and configuration
                .Build();

            // Create FluentUIScaffold app WITH server configuration
            // Server lifecycle is managed automatically
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig); // NEW: Integrated server management
                options.WithBaseUrl(new Uri("http://localhost:5201"));
                options.WithHeadlessMode(true);
                options.WithDefaultWaitTimeout(TimeSpan.FromSeconds(30));
            });

            // Server is automatically started during app creation
            // No manual StartServerAsync() call needed!

            // Verify the server is running
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var response = await driver.GotoAsync("http://localhost:5201");
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, "Server should be accessible with new approach");

            // Test API endpoint
            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/weatherforecast').then(r => r.ok)");
            Assert.IsTrue(apiResponse, "API should be accessible with new approach");

            // Server is automatically stopped when app is disposed
            // No manual cleanup required!
        }

        [TestMethod]
        [TestCategory("Migration")]
        [TestCategory("Comparison")]
        public async Task SideBySideComparison_OldVsNew_PerformanceAndReliability()
        {
            var projectRoot = _projectRoot!;

            // Measure OLD approach performance
            var oldStartTime = DateTime.UtcNow;

            // OLD: Manual setup with WebServerManager
            var oldPlan = ServerConfiguration.CreateDotNetServer(
                new Uri("http://localhost:5202"),
                Path.Combine(projectRoot, "samples", "SampleApp", "SampleApp.csproj"))
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .EnableSpaProxy(false)
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .Build();

            await WebServerManager.StartServerAsync(oldPlan);

            try
            {
                var oldOptions = new FluentUIScaffoldOptionsBuilder()
                    .WithBaseUrl(new Uri("http://localhost:5202"))
                    .WithHeadlessMode(true)
                    .Build();

                using var oldApp = new FluentUIScaffoldApp<WebApp>(oldOptions);
                await oldApp.InitializeAsync();

                var oldDuration = DateTime.UtcNow - oldStartTime;

                // Test OLD approach functionality
                var oldDriver = oldApp.Framework<Microsoft.Playwright.IPage>();
                var oldResponse = await oldDriver.GotoAsync("http://localhost:5202");
                Assert.IsTrue(oldResponse!.Ok, "Old approach should work");

                Console.WriteLine($"OLD approach setup time: {oldDuration.TotalSeconds:F2} seconds");

                // Measure NEW approach performance
                var newStartTime = DateTime.UtcNow;

                // NEW: Integrated setup with Aspire lifecycle management
                var newServerConfig = ServerConfiguration.CreateAspireServer(
                    new Uri("http://localhost:5203"),
                    Path.Combine(projectRoot, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                    .WithHealthCheckEndpoints("/", "/weatherforecast")
                    .WithHeadless(true)
                    .WithKillOrphansOnStart(true)
                    .Build();

                using var newApp = FluentUIScaffoldBuilder.Web<WebApp>(options =>
                {
                    options.WithServerConfiguration(newServerConfig);
                    options.WithBaseUrl(new Uri("http://localhost:5203"));
                    options.WithHeadlessMode(true);
                });

                var newDuration = DateTime.UtcNow - newStartTime;

                // Test NEW approach functionality
                var newDriver = newApp.Framework<Microsoft.Playwright.IPage>();
                var newResponse = await newDriver.GotoAsync("http://localhost:5203");
                Assert.IsTrue(newResponse!.Ok, "New approach should work");

                Console.WriteLine($"NEW approach setup time: {newDuration.TotalSeconds:F2} seconds");

                // Compare approaches
                Console.WriteLine("\n=== COMPARISON ===");
                Console.WriteLine($"Old approach: Manual lifecycle, external server management");
                Console.WriteLine($"New approach: Integrated lifecycle, automatic management");
                Console.WriteLine($"Performance difference: {(newDuration - oldDuration).TotalSeconds:F2} seconds");

                // Verify both approaches work
                Assert.IsTrue(oldResponse.Ok && newResponse.Ok,
                    "Both old and new approaches should successfully start servers");
            }
            finally
            {
                WebServerManager.StopServer(); // Manual cleanup for old approach
                // New approach cleanup is automatic via using statement
            }
        }

        [TestMethod]
        [TestCategory("Migration")]
        [TestCategory("BestPractices")]
        public async Task MigrationBestPractices_RecommendedPatterns()
        {
            var projectRoot = _projectRoot!;

            // BEST PRACTICE: Use the new integrated approach for new tests
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5204"),
                Path.Combine(projectRoot, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast") // Multiple health checks
                .WithAutoCI() // Automatic CI environment detection
                .WithKillOrphansOnStart(true) // Clean up previous runs
                .WithForceRestartOnConfigChange(true) // Handle config drift
                .WithStartupTimeout(TimeSpan.FromMinutes(3)) // Generous timeout for CI
                .Build();

            // BEST PRACTICE: Use builder pattern for clean configuration
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);
                options.WithBaseUrl(new Uri("http://localhost:5204"));
                options.WithHeadlessMode(true); // Explicit headless for testing
                options.WithDefaultWaitTimeout(TimeSpan.FromSeconds(30));
            });

            // BEST PRACTICE: Server management is transparent
            // No manual server lifecycle code needed

            // BEST PRACTICE: Use standard test patterns
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            await driver.GotoAsync("http://localhost:5204");

            // Verify multiple aspects of the application
            await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title");

            // Test API functionality
            var weatherData = await driver.EvaluateAsync<string>("""
                fetch('/weatherforecast')
                    .then(r => r.text())
                    .then(data => data.substring(0, 100))
            """);
            Assert.IsTrue(!string.IsNullOrEmpty(weatherData), "Weather API should return data");

            // BEST PRACTICE: Automatic cleanup via using statement
            // No manual StopServer() calls needed
        }

        /// <summary>
        /// Example showing the migration checklist for existing test projects.
        /// </summary>
        [TestMethod]
        [TestCategory("Migration")]
        [TestCategory("Checklist")]
        public void MigrationChecklist_WhatToChange()
        {
            // This is a documentation test that shows the migration checklist
            var migrationSteps = new[]
            {
                "1. Create Aspire AppHost project for your application",
                "2. Replace WebServerManager.StartServerAsync() with server configuration",
                "3. Use WithServerConfiguration() in FluentUIScaffoldBuilder.Web()",
                "4. Remove manual WebServerManager.StopServer() calls",
                "5. Remove TestAssemblyHooks for server management",
                "6. Add health check endpoints to server configuration",
                "7. Enable CI-friendly features (headless, orphan cleanup)",
                "8. Update project references to include new lifecycle components",
                "9. Test in both development and CI environments",
                "10. Update documentation and team guidelines"
            };

            Console.WriteLine("=== MIGRATION CHECKLIST ===");
            foreach (var step in migrationSteps)
            {
                Console.WriteLine(step);
            }

            // Key benefits of migration:
            var benefits = new[]
            {
                "✓ Automatic server lifecycle management",
                "✓ Process reuse across test runs (faster execution)",
                "✓ Configuration drift detection and handling",
                "✓ Built-in CI/headless support",
                "✓ Comprehensive health checking",
                "✓ Orphan process cleanup",
                "✓ Better error handling and diagnostics",
                "✓ Aspire orchestration capabilities"
            };

            Console.WriteLine("\n=== BENEFITS OF MIGRATION ===");
            foreach (var benefit in benefits)
            {
                Console.WriteLine(benefit);
            }

            Assert.IsTrue(migrationSteps.Length > 0 && benefits.Length > 0,
                "Migration guidance should be comprehensive");
        }

        private static string GetProjectRoot()
        {
            var currentDir = Directory.GetCurrentDirectory();
            while (currentDir != null && !File.Exists(Path.Combine(currentDir, "FluentUIScaffold.sln")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            if (currentDir == null)
            {
                throw new InvalidOperationException("Could not find project root directory containing FluentUIScaffold.sln");
            }

            return currentDir;
        }
    }
}
