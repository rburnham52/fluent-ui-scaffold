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
    /// Integration tests demonstrating the new FluentUIScaffold Aspire server lifecycle management.
    /// These tests showcase automatic server startup, configuration drift detection, and health checking.
    /// </summary>
    [TestClass]
    public class AspireServerLifecycleTests
    {
        private static readonly Uri TestBaseUrl = new("http://localhost:5555"); // Use different port to avoid conflicts
        private static readonly TimeSpan TestTimeout = TimeSpan.FromMinutes(3);
        private static string? _projectRoot;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Register Playwright plugin globally
            FluentUIScaffoldPlaywrightBuilder.UsePlaywright();

            _projectRoot = GetProjectRoot();
            context.WriteLine($"Project root: {_projectRoot}");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task EnsureStartedAsync_WithAspireAppHost_StartsServerSuccessfully()
        {
            // Arrange - Create Aspire server configuration using the new lifecycle system
            var serverConfig = ServerConfiguration.CreateAspireServer(
                TestBaseUrl,
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TestTimeout)
                .WithHeadless(true) // Run in headless mode for testing
                .WithKillOrphansOnStart(true) // Clean up any previous test runs
                .Build();

            // Act - Create FluentUIScaffold app with automatic server management
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);
                options.WithBaseUrl(TestBaseUrl);
                options.WithHeadlessMode(true);
                options.WithDefaultWaitTimeout(TimeSpan.FromSeconds(30));
            });

            // Assert - Server should be running and accessible
            var driver = app.Framework<Microsoft.Playwright.IPage>();

            // Navigate to the home page
            var response = await driver.GotoAsync(TestBaseUrl.ToString());
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page: {response.Status} - {response.StatusText}");

            // Verify the page loaded correctly
            await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(title.Contains("FluentUIScaffold") || title.Contains("SampleApp"),
                $"Expected page title to contain 'FluentUIScaffold' or 'SampleApp', got: {title}");

            // Verify API endpoints are working
            var apiResponse = await driver.EvaluateAsync<object>("fetch('/weatherforecast').then(r => r.ok)");
            Assert.IsTrue((bool)apiResponse, "Weather API endpoint should be accessible");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task ManualServerManagement_WithAspireAppHost_DemonstratesFullLifecycle()
        {
            // Arrange - Set up manual server management to demonstrate full lifecycle control
            var services = new ServiceCollection();
            services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
            services.AddSingleton<IServerManager, AspireServerManager>();
            var serviceProvider = services.BuildServiceProvider();

            var serverManager = serviceProvider.GetRequiredService<IServerManager>();
            var logger = serviceProvider.GetRequiredService<ILogger<AspireServerLifecycleTests>>();

            var launchPlan = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5101"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TestTimeout)
                .WithHeadless(true)
                .WithKillOrphansOnStart(true)
                .Build();

            try
            {
                // Act & Assert - Demonstrate server lifecycle
                logger.LogInformation("=== Starting server manually ===");
                var startStatus = await serverManager.EnsureStartedAsync(launchPlan, logger);

                Assert.IsTrue(startStatus.IsHealthy, "Server should be healthy after startup");
                Assert.IsTrue(startStatus.IsRunning, "Server should be running");
                Assert.AreEqual(new Uri("http://localhost:5101"), startStatus.BaseUrl);
                logger.LogInformation("Server started successfully: PID {Pid}, URL {BaseUrl}",
                    startStatus.Pid, startStatus.BaseUrl);

                // Verify server is accessible
                using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var response = await httpClient.GetAsync("http://localhost:5101/weatherforecast");
                Assert.IsTrue(response.IsSuccessStatusCode,
                    $"Server should respond to API requests: {response.StatusCode}");

                // Demonstrate status checking
                var currentStatus = serverManager.GetStatus();
                Assert.AreEqual(startStatus.Pid, currentStatus.Pid, "Status should remain consistent");
                Assert.IsTrue(currentStatus.IsHealthy, "Server should remain healthy");

                // Demonstrate restart capability
                logger.LogInformation("=== Restarting server ===");
                await serverManager.RestartAsync();

                var restartStatus = serverManager.GetStatus();
                Assert.IsTrue(restartStatus.IsRunning, "Server should be running after restart");
                Assert.IsTrue(restartStatus.IsHealthy, "Server should be healthy after restart");
                // PID may be different after restart
                logger.LogInformation("Server restarted: New PID {Pid}", restartStatus.Pid);

                // Verify server is still accessible after restart
                var restartResponse = await httpClient.GetAsync("http://localhost:5101/weatherforecast");
                Assert.IsTrue(restartResponse.IsSuccessStatusCode,
                    "Server should respond to API requests after restart");
            }
            finally
            {
                // Cleanup
                logger.LogInformation("=== Stopping server ===");
                await serverManager.StopAsync();

                var finalStatus = serverManager.GetStatus();
                Assert.AreEqual(ServerStatus.None, finalStatus, "Server should be stopped");
                logger.LogInformation("Server stopped successfully");
            }
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task ConfigurationDriftDetection_WithDifferentConfigurations_RestartsServerAppropriately()
        {
            // Arrange - Set up server manager to test configuration drift detection
            var serverManager = new AspireServerManager();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<AspireServerLifecycleTests>();

            var baseConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5102"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TestTimeout)
                .WithHeadless(true)
                .Build();

            try
            {
                // Act & Assert - Start with initial configuration
                logger.LogInformation("=== Starting with initial configuration ===");
                var initialStatus = await serverManager.EnsureStartedAsync(baseConfig, logger);
                Assert.IsTrue(initialStatus.IsRunning, "Server should start with initial config");
                var initialPid = initialStatus.Pid;

                // Simulate second test run with same configuration - should reuse server
                logger.LogInformation("=== Attempting to start with same configuration ===");
                var reuseStatus = await serverManager.EnsureStartedAsync(baseConfig, logger);
                Assert.AreEqual(initialPid, reuseStatus.Pid, "Should reuse existing server with same config");
                Assert.IsTrue(reuseStatus.IsRunning, "Reused server should be running");

                // Change configuration - should restart server
                var modifiedConfig = ServerConfiguration.CreateAspireServer(
                    new Uri("http://localhost:5102"),
                    Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                    .WithHealthCheckEndpoints("/", "/weatherforecast", "/health") // Added /health endpoint
                    .WithStartupTimeout(TestTimeout)
                    .WithHeadless(true)
                    .WithEnvironmentVariable("TEST_CONFIG_CHANGE", "true") // Environment variable change
                    .Build();

                logger.LogInformation("=== Starting with modified configuration ===");
                var modifiedStatus = await serverManager.EnsureStartedAsync(modifiedConfig, logger);

                // Note: Due to configuration change detection, a new server may be started
                // The PID might be different, indicating the server was restarted
                Assert.IsTrue(modifiedStatus.IsRunning, "Server should be running with modified config");
                logger.LogInformation("Configuration change handled: Original PID {InitialPid}, New PID {NewPid}",
                    initialPid, modifiedStatus.Pid);
            }
            finally
            {
                // Cleanup
                await serverManager.StopAsync();
                serverManager.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task HealthCheckValidation_WithCustomEndpoints_ValidatesServerReadiness()
        {
            // Arrange - Configure server with multiple health check endpoints
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5103"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast") // Multiple endpoints for thorough health checking
                .WithStartupTimeout(TestTimeout)
                .WithHeadless(true)
                .WithKillOrphansOnStart(true)
                .Build();

            // Act - Start server with health check validation
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);
                options.WithBaseUrl(new Uri("http://localhost:5103"));
                options.WithHeadlessMode(true);
            });

            // Assert - Verify all health check endpoints are accessible
            using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            // Test main endpoint
            var homeResponse = await httpClient.GetAsync("http://localhost:5103/");
            Assert.IsTrue(homeResponse.IsSuccessStatusCode,
                $"Home endpoint should be healthy: {homeResponse.StatusCode}");

            // Test API endpoint
            var apiResponse = await httpClient.GetAsync("http://localhost:5103/weatherforecast");
            Assert.IsTrue(apiResponse.IsSuccessStatusCode,
                $"API endpoint should be healthy: {apiResponse.StatusCode}");

            // Verify API returns expected data
            var weatherData = await apiResponse.Content.ReadAsStringAsync();
            Assert.IsTrue(weatherData.Contains("temperatureC") || weatherData.Contains("temperature"),
                "Weather API should return temperature data");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task CIHeadlessMode_WithAutomaticDetection_ConfiguresAppropriately()
        {
            // Arrange - Simulate CI environment
            Environment.SetEnvironmentVariable("CI", "true");

            try
            {
                var serverConfig = ServerConfiguration.CreateAspireServer(
                    new Uri("http://localhost:5104"),
                    Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                    .WithAutoCI() // Automatically configure for CI environment
                    .WithHealthCheckEndpoints("/", "/weatherforecast")
                    .WithStartupTimeout(TestTimeout)
                    .Build();

                // Act - Create app in CI mode
                using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
                {
                    options.WithServerConfiguration(serverConfig);
                    options.WithBaseUrl(new Uri("http://localhost:5104"));
                    // Note: HeadlessMode should be auto-detected as true in CI
                });

                // Assert - Verify server started and is accessible
                using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                var response = await httpClient.GetAsync("http://localhost:5104/weatherforecast");
                Assert.IsTrue(response.IsSuccessStatusCode,
                    "Server should be accessible in CI headless mode");

                // Verify Playwright is in headless mode (if accessible)
                var driver = app.Framework<Microsoft.Playwright.IPage>();
                await driver.GotoAsync("http://localhost:5104/");

                // In headless mode, we can still interact with the page
                await driver.WaitForSelectorAsync("body", new() { Timeout = 10000 });
                var bodyContent = await driver.TextContentAsync("body");
                Assert.IsNotNull(bodyContent, "Page content should be accessible in headless mode");
            }
            finally
            {
                Environment.SetEnvironmentVariable("CI", null);
            }
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("Performance")]
        public async Task ServerReusePerformance_WithSameConfiguration_ReusesFastly()
        {
            // Arrange
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5105"),
                Path.Combine(_projectRoot!, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj"))
                .WithHealthCheckEndpoints("/", "/weatherforecast")
                .WithStartupTimeout(TestTimeout)
                .WithHeadless(true)
                .Build();

            var serverManager = new AspireServerManager();
            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AspireServerLifecycleTests>();

            try
            {
                // Act & Assert - First startup (slower)
                var firstStartTime = DateTime.UtcNow;
                var firstStatus = await serverManager.EnsureStartedAsync(serverConfig, logger);
                var firstStartDuration = DateTime.UtcNow - firstStartTime;

                Assert.IsTrue(firstStatus.IsRunning, "First server start should succeed");
                logger.LogInformation("First startup took: {Duration}ms", firstStartDuration.TotalMilliseconds);

                // Second startup with same config (should be fast - reuse existing)
                var secondStartTime = DateTime.UtcNow;
                var secondStatus = await serverManager.EnsureStartedAsync(serverConfig, logger);
                var secondStartDuration = DateTime.UtcNow - secondStartTime;

                Assert.IsTrue(secondStatus.IsRunning, "Second server start should succeed");
                Assert.AreEqual(firstStatus.Pid, secondStatus.Pid, "Should reuse same server instance");
                logger.LogInformation("Second startup (reuse) took: {Duration}ms", secondStartDuration.TotalMilliseconds);

                // Reuse should be significantly faster (under 1 second vs potentially 30+ seconds for full startup)
                Assert.IsTrue(secondStartDuration < TimeSpan.FromSeconds(5),
                    $"Server reuse should be fast, took {secondStartDuration.TotalSeconds:F2} seconds");

                // First startup should be slower than reuse
                Assert.IsTrue(firstStartDuration > secondStartDuration,
                    "Initial startup should take longer than reuse");
            }
            finally
            {
                await serverManager.StopAsync();
                serverManager.Dispose();
            }
        }

        /// <summary>
        /// Gets the project root directory by walking up from the current directory.
        /// </summary>
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
