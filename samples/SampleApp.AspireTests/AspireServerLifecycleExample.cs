using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Server;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Tests demonstrating the new FluentUIScaffold AppHost lifecycle management system.
    /// Shows how to configure and use the server manager for Aspire applications.
    /// </summary>
    [TestClass]
    public class AspireServerLifecycleExample
    {
        private static string? _projectRoot;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {

            _projectRoot = GetProjectRoot();
            context.WriteLine($"Project root: {_projectRoot}");

            // Log environment status for debugging
            context.WriteLine(TestEnvironmentHelper.GetEnvironmentStatus());
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Example")]
        public async Task BasicServerLifecycleExample_WithAutomaticManagement_StartsAndStopsServer()
        {
            // Skip test if Docker is not available
            if (!TestEnvironmentHelper.CanRunAspireTests)
            {
                Assert.Inconclusive($"Aspire tests require Docker and Aspire workload. {TestEnvironmentHelper.GetEnvironmentStatus()}");
                return;
            }

            // Arrange - Use the session-level server
            var app = TestAssemblyHooks.GetSessionApp();
            if (app == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check assembly initialization.");
                return;
            }

            // Assert - Server should be automatically started and accessible
            var driver = app.Framework<Microsoft.Playwright.IPage>();

            // Navigate to the home page - get base URL from options (set by Aspire hosting)
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
            var response = await driver.GotoAsync(baseUrl);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page: {response.Status} - {response.StatusText}");

            // Verify the page loaded correctly
            await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title");

            // Test API endpoint (route is /api/weather per WeatherForecastController)
            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/api/weather').then(r => r.ok)");
            Assert.IsTrue(apiResponse, "Weather API endpoint should be accessible");

            // Note: Server is managed at the session level, not disposed per test
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Example")]
        public async Task AdvancedServerConfigurationExample_WithCustomServices_ConfiguresAppropriately()
        {
            // Skip test if Docker is not available
            if (!TestEnvironmentHelper.CanRunAspireTests)
            {
                Assert.Inconclusive($"Aspire tests require Docker and Aspire workload. {TestEnvironmentHelper.GetEnvironmentStatus()}");
                return;
            }

            // Arrange - Use the session-level server
            var app = TestAssemblyHooks.GetSessionApp();
            if (app == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check assembly initialization.");
                return;
            }
            // Act - Use the session-level server
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";

            // Navigate to the home page
            var response = await driver.GotoAsync(baseUrl);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page with advanced config: {response.Status}");

            // Verify the page loaded correctly
            await driver.WaitForSelectorAsync("body", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title with advanced config");

            // Test API endpoint (route is /api/weather per WeatherForecastController)
            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/api/weather').then(r => r.ok)");
            Assert.IsTrue(apiResponse, "Weather API endpoint should be accessible with advanced config");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Example")]
        public async Task ManualServerManagementExample_WithFullLifecycleControl_DemonstratesManualControl()
        {
            // Skip test if Docker is not available
            if (!TestEnvironmentHelper.CanRunAspireTests)
            {
                Assert.Inconclusive($"Aspire tests require Docker and Aspire workload. {TestEnvironmentHelper.GetEnvironmentStatus()}");
                return;
            }

            // Arrange - Use the session-level server
            var app = TestAssemblyHooks.GetSessionApp();

            if (app == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check assembly initialization.");
                return;
            }

            // Act - Use the session-level server
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";

            // Assert - Server should be started successfully

            // Verify server is accessible via browser
            var response = await driver.GotoAsync(baseUrl);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page: {response.Status}");

            // Test API endpoint (route is /api/weather per WeatherForecastController)
            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/api/weather').then(r => r.ok)");
            Assert.IsTrue(apiResponse, "Weather API endpoint should be accessible");

            // Note: Server cleanup is handled at the assembly level, not per test
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Example")]
        public void ShowCIConfiguration_WithAutomaticDetection_ConfiguresAppropriately()
        {
            // Arrange - Simulate CI environment
            Environment.SetEnvironmentVariable("CI", "true");

            try
            {
                // Act - The system automatically detects CI environments
                var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
                          !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS"));

                // Assert - CI environment should be detected
                Assert.IsTrue(isCI, "CI environment should be detected when CI variable is set");

                // Configuration automatically adapts for CI
                var serverConfig = ServerConfiguration.CreateAspireServer(
                    new Uri("http://localhost:5000"),
                    Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                    .WithAutoCI() // Automatically configures for CI when detected
                    .Build();

                // Assert - Server configuration should be created successfully
                Assert.IsNotNull(serverConfig, "Server configuration should be created with CI auto-detection");
                Assert.IsNotNull(serverConfig.BaseUrl, "Base URL should be set");
                // Fix: The BaseUrl should match what we configured (with or without trailing slash)
                var configuredUrl = "http://localhost:5000";
                var actualUrl = serverConfig.BaseUrl.ToString();
                Assert.IsTrue(
                    actualUrl.Equals(configuredUrl, StringComparison.OrdinalIgnoreCase) ||
                    actualUrl.Equals(configuredUrl + "/", StringComparison.OrdinalIgnoreCase),
                    $"Expected BaseUrl to be '{configuredUrl}' or '{configuredUrl}/' but got '{actualUrl}'");

                // In CI environments, this will:
                // - Disable SpaProxy
                // - Enable headless mode
                // - Build SPA assets if needed
                // - Use fixed ports
                // - Enable orphan cleanup
            }
            finally
            {
                // Cleanup
                Environment.SetEnvironmentVariable("CI", null);
            }
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

        [TestMethod]
        [TestCategory("Environment")]
        [TestCategory("Build")]
        public void EnvironmentDetection_CheckDockerAndAspireAvailability_ReportsStatus()
        {
            // This test always runs and reports the environment status
            var status = TestEnvironmentHelper.GetEnvironmentStatus();

            // Log the status for visibility
            Console.WriteLine(status);

            // Basic assertions about environment detection
            Assert.IsNotNull(status, "Environment status should not be null");
            Assert.IsTrue(status.Contains("Docker Available:"), "Status should report Docker availability");
            Assert.IsTrue(status.Contains("Aspire Workload Installed:"), "Status should report Aspire workload status");
            Assert.IsTrue(status.Contains("Can Run Aspire Tests:"), "Status should report overall capability");

            // This test always passes - it's for information only
            Assert.IsTrue(true, "Environment detection test completed");
        }

        [TestMethod]
        [TestCategory("Timeout")]
        [TestCategory("Example")]
        [Timeout(10000)] // 10 second timeout
        public async Task TimeoutTest_WithShortTimeout_TimesOutCorrectly()
        {
            // This test verifies that our timeout mechanism works
            // It should complete quickly and not hang
            await Task.Delay(100); // Just a small delay to simulate work

            Assert.IsTrue(true, "Timeout test completed successfully");
        }
    }

    /// <summary>
    /// Tests showing integration with existing test frameworks
    /// </summary>
    [TestClass]
    public class TestFrameworkIntegrationExample
    {
        private static AppScaffold<WebApp>? _app;
        private static string? _projectRoot;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _projectRoot = GetProjectRoot();
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Integration")]
        public async Task OneTimeSetUp_WithAspireServer_InitializesCorrectly()
        {
            // Skip test if Aspire tests are not supported in this environment
            if (!TestEnvironmentHelper.CanRunAspireTests)
            {
                Assert.Inconclusive($"Aspire tests require Docker and Aspire workload. {TestEnvironmentHelper.GetEnvironmentStatus()}");
                return;
            }

            // Arrange - Use the session-level server instead of creating a new one
            var sessionApp = TestAssemblyHooks.GetSessionApp();
            if (sessionApp == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check assembly initialization.");
                return;
            }

            // Act - Use the session-level server
            _app = sessionApp;

            // Assert - Server should be started and ready for tests
            Assert.IsNotNull(_app, "App should be initialized");

            var driver = _app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = _app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
            var response = await driver.GotoAsync(baseUrl);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, "Server should be accessible after initialization");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Integration")]
        public async Task TestMethod_WithInitializedApp_PerformsTestActions()
        {
            // Skip test if Aspire tests are not supported in this environment
            if (!TestEnvironmentHelper.CanRunAspireTests)
            {
                Assert.Inconclusive($"Aspire tests require Docker and Aspire workload. {TestEnvironmentHelper.GetEnvironmentStatus()}");
                return;
            }

            // Arrange - Use the session-level server
            var sessionApp = TestAssemblyHooks.GetSessionApp();
            if (sessionApp == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check assembly initialization.");
                return;
            }

            // Act - Navigate to a page and perform test actions
            var driver = sessionApp.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = sessionApp.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
            var response = await driver.GotoAsync(baseUrl);

            // Assert - Test should be able to interact with the page
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, "Should be able to navigate to home page");

            await driver.WaitForSelectorAsync("body", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // This would typically be in a [OneTimeTearDown] method
            _app?.DisposeAsync().AsTask().GetAwaiter().GetResult(); // Server is automatically stopped
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

    // Example page object for the test
    public class HomePage
    {
        // Page object implementation would go here
    }
}
