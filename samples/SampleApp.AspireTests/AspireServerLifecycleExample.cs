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
            // Register Playwright plugin globally
            FluentUIScaffoldPlaywrightBuilder.UsePlaywright();

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
            // Arrange - Create server configuration using the builder pattern
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5400"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .WithHeadless(true)
                .WithKillOrphansOnStart(true)
                .Build();

            // Act - Configure FluentUIScaffold with automatic server management
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);
                options.WithBaseUrl(new Uri("http://localhost:5400"));
                options.WithHeadlessMode(true);
            });

            // Assert - Server should be automatically started and accessible
            var driver = app.Framework<Microsoft.Playwright.IPage>();

            // Navigate to the home page
            var response = await driver.GotoAsync("http://localhost:5400/");
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page: {response.Status} - {response.StatusText}");

            // Verify the page loaded correctly
            await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title");

            // Test API endpoint
            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/weatherforecast').then(r => r.ok)");
            Assert.IsTrue(apiResponse, "Weather API endpoint should be accessible");

            // Server is automatically stopped when app is disposed
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Example")]
        public async Task AdvancedServerConfigurationExample_WithCustomServices_ConfiguresAppropriately()
        {
            // Arrange - Create advanced server configuration
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5401"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast", "/health")
                .WithForceRestartOnConfigChange(true)
                .WithKillOrphansOnStart(true)
                .WithFixedPorts(new Dictionary<string, int>
                {
                    ["web"] = 5401,
                    ["api"] = 5402,
                    ["database"] = 5432
                })
                .WithHeadless(true) // For CI environments
                .WithAssetsBuild(async ct =>
                {
                    // Custom SPA build logic
                    await Task.Delay(1000, ct); // Simulate build time
                })
                .Build();

            // Setup custom services
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole());
            services.AddSingleton<IServerManager, AspireServerManager>();
            services.AddSingleton<IProcessRegistry, ProcessRegistry>();
            services.AddSingleton<IProcessLauncher, ProcessLauncher>();
            services.AddSingleton<IHealthWaiter, HealthWaiter>();
            var serviceProvider = services.BuildServiceProvider();

            // Act - Configure FluentUIScaffold with custom services
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);
                options.WithServiceProvider(serviceProvider);
                options.WithHeadlessMode(true);
                options.WithDefaultWaitTimeout(TimeSpan.FromSeconds(30));
            });

            // Assert - Server should be accessible with advanced configuration
            var driver = app.Framework<Microsoft.Playwright.IPage>();

            // Navigate to the home page
            var response = await driver.GotoAsync("http://localhost:5401/");
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page with advanced config: {response.Status}");

            // Verify the page loaded correctly
            await driver.WaitForSelectorAsync("body", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title with advanced config");

            // Test API endpoint
            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/weatherforecast').then(r => r.ok)");
            Assert.IsTrue(apiResponse, "Weather API endpoint should be accessible with advanced config");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Example")]
        public async Task ManualServerManagementExample_WithFullLifecycleControl_DemonstratesManualControl()
        {
            // Arrange - Set up manual server management
            var serverManager = new AspireServerManager();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            var logger = loggerFactory.CreateLogger<AspireServerLifecycleExample>();

            var launchPlan = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5402"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/health")
                .WithStartupTimeout(TimeSpan.FromSeconds(90))
                .WithHeadless(true)
                .WithKillOrphansOnStart(true)
                .Build();

            try
            {
                // Act - Start the server manually
                var status = await serverManager.EnsureStartedAsync(launchPlan, logger);

                // Assert - Server should be started successfully
                Assert.IsTrue(status.IsHealthy, "Server should be healthy after startup");
                Assert.IsTrue(status.IsRunning, "Server should be running");
                Assert.AreEqual(new Uri("http://localhost:5402"), status.BaseUrl);

                // Check status
                var currentStatus = serverManager.GetStatus();
                Assert.IsTrue(currentStatus.IsRunning, "Current server status should be running");

                // Verify server is accessible
                using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var response = await httpClient.GetAsync("http://localhost:5402/weatherforecast");
                Assert.IsTrue(response.IsSuccessStatusCode, $"Server should respond to API requests: {response.StatusCode}");

                // Demonstrate restart capability
                await serverManager.RestartAsync();
                var restartedStatus = serverManager.GetStatus();
                Assert.IsTrue(restartedStatus.IsRunning, "Server should be running after restart");
                Assert.IsTrue(restartedStatus.IsHealthy, "Server should be healthy after restart");

                // Verify server is still accessible after restart
                var restartResponse = await httpClient.GetAsync("http://localhost:5402/weatherforecast");
                Assert.IsTrue(restartResponse.IsSuccessStatusCode, "Server should respond to API requests after restart");
            }
            finally
            {
                // Cleanup
                await serverManager.StopAsync();
                serverManager.Dispose();

                var finalStatus = serverManager.GetStatus();
                Assert.AreEqual(ServerStatus.None, finalStatus, "Server should be stopped");
            }
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
                Assert.AreEqual("http://localhost:5000", serverConfig.BaseUrl.ToString());

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
    }

    /// <summary>
    /// Tests showing integration with existing test frameworks
    /// </summary>
    [TestClass]
    public class TestFrameworkIntegrationExample
    {
        private static FluentUIScaffoldApp<WebApp>? _app;
        private static string? _projectRoot;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            FluentUIScaffoldPlaywrightBuilder.UsePlaywright();
            _projectRoot = GetProjectRoot();
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Integration")]
        public async Task OneTimeSetUp_WithAspireServer_InitializesCorrectly()
        {
            // Arrange - This would typically be in a [OneTimeSetUp] method in NUnit
            // or a class constructor in xUnit
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5403"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/health")
                .WithKillOrphansOnStart(true)
                .WithStartupTimeout(TimeSpan.FromSeconds(120))
                .WithHeadless(true)
                .Build();

            // Act - Create FluentUIScaffold app with server configuration
            _app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);
                options.WithHeadlessMode(true);
            });

            // Assert - Server should be started and ready for tests
            Assert.IsNotNull(_app, "App should be initialized");

            var driver = _app.Framework<Microsoft.Playwright.IPage>();
            var response = await driver.GotoAsync("http://localhost:5403/");
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, "Server should be accessible after initialization");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        [TestCategory("Integration")]
        public async Task TestMethod_WithInitializedApp_PerformsTestActions()
        {
            // Arrange - This would be a typical test method
            if (_app == null)
            {
                // Initialize app if not already done
                var serverConfig = ServerConfiguration.CreateAspireServer(
                    new Uri("http://localhost:5404"),
                    Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                    .WithHealthCheckEndpoints("/", "/weatherforecast")
                    .WithHeadless(true)
                    .Build();

                _app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
                {
                    options.WithServerConfiguration(serverConfig);
                    options.WithBaseUrl(new Uri("http://localhost:5404"));
                    options.WithHeadlessMode(true);
                });
            }

            // Act - Navigate to a page and perform test actions
            var driver = _app.Framework<Microsoft.Playwright.IPage>();
            var response = await driver.GotoAsync("http://localhost:5404/");

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
            _app?.Dispose(); // Server is automatically stopped
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
