using System;
using System.Threading.Tasks;

using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Demonstrates the recommended patterns for using FluentUIScaffold with Aspire hosting.
    /// These tests show best practices for test setup and teardown using the unified API.
    /// </summary>
    [TestClass]
    public class MigrationExampleTests
    {
        [TestMethod]
        [TestCategory("Example")]
        [TestCategory("BestPractices")]
        public async Task RecommendedPattern_AspireHosting_WithUnifiedAPI()
        {
            // RECOMMENDED: Use the unified FluentUIScaffoldBuilder API with Aspire hosting
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .WithHeadlessMode(true)
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { /* configure distributed app if needed */ },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

            try
            {
                // Server is automatically started during StartAsync()
                // No manual server management needed!

                // Verify the server is running
                var driver = app.Framework<Microsoft.Playwright.IPage>();
                var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
                var response = await driver.GotoAsync(baseUrl);
                Assert.IsNotNull(response);
                Assert.IsTrue(response.Ok, "Server should be accessible");

                // Test API endpoint (route is /api/weather per WeatherForecastController)
                var apiResponse = await driver.EvaluateAsync<bool>("fetch('/api/weather').then(r => r.ok)");
                Assert.IsTrue(apiResponse, "API should be accessible");
            }
            finally
            {
                // Server is automatically stopped when app is disposed
                await app.DisposeAsync();
            }
        }

        [TestMethod]
        [TestCategory("Example")]
        [TestCategory("BestPractices")]
        public async Task RecommendedPattern_AssemblyLevelSetup()
        {
            // This demonstrates the recommended pattern for assembly-level setup
            // In practice, this would be in a TestAssemblyHooks class

            // PATTERN: Create app once in AssemblyInitialize
            var app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .WithHeadlessMode(true)
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

            try
            {
                // PATTERN: Use app across multiple tests
                await SimulateTest1(app);
                await SimulateTest2(app);
                await SimulateTest3(app);
            }
            finally
            {
                // PATTERN: Dispose once in AssemblyCleanup
                await app.DisposeAsync();
            }
        }

        private static async Task SimulateTest1(AppScaffold<WebApp> app)
        {
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
            await driver.GotoAsync(baseUrl);
            Console.WriteLine("Test 1 completed");
        }

        private static async Task SimulateTest2(AppScaffold<WebApp> app)
        {
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
            await driver.GotoAsync($"{baseUrl.TrimEnd('/')}/weatherforecast");
            Console.WriteLine("Test 2 completed");
        }

        private static async Task SimulateTest3(AppScaffold<WebApp> app)
        {
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var title = await driver.TitleAsync();
            Console.WriteLine($"Test 3 completed - Page title: {title}");
        }

        [TestMethod]
        [TestCategory("Documentation")]
        public void DocumentationTest_BestPracticesChecklist()
        {
            // This is a documentation test that shows best practices
            var bestPractices = new[]
            {
                "1. Use FluentUIScaffoldBuilder for all test setup",
                "2. Choose the appropriate hosting strategy:",
                "   - UseAspireHosting<T>() for Aspire apps",
                "   - UseDotNetHosting() for standard .NET apps",
                "   - UseExternalServer() for pre-started servers",
                "3. Call StartAsync() before running tests",
                "4. Call DisposeAsync() in cleanup (or use 'await using')",
                "5. Use assembly-level setup for server reuse across tests",
                "6. Configure WithHeadlessMode(true) for CI environments",
                "7. Use WithAutoPageDiscovery() for automatic page registration",
                "8. Access framework-specific features via Framework<T>()"
            };

            Console.WriteLine("=== BEST PRACTICES ===");
            foreach (var practice in bestPractices)
            {
                Console.WriteLine(practice);
            }

            // Key benefits
            var benefits = new[]
            {
                "Automatic server lifecycle management",
                "Process reuse across test runs",
                "Built-in CI/headless support",
                "Comprehensive health checking",
                "Aspire orchestration capabilities",
                "Unified API across all hosting strategies"
            };

            Console.WriteLine("\n=== BENEFITS ===");
            foreach (var benefit in benefits)
            {
                Console.WriteLine($"  {benefit}");
            }

            Assert.IsTrue(bestPractices.Length > 0, "Best practices should be documented");
        }
    }
}
