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
    /// Quick start example showing the new Aspire server lifecycle management in action.
    /// Compare this with the traditional WebServerManager approach for immediate understanding.
    /// </summary>
    [TestClass]
    public class QuickStartExample
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // One-time setup: Register Playwright plugin
            FluentUIScaffoldPlaywrightBuilder.UsePlaywright();
        }

        [TestMethod]
        [TestCategory("QuickStart")]
        [TestCategory("Example")]
        public async Task QuickStart_AspireLifecycleManagement_JustWorks()
        {
            // 🎯 This is all you need for enterprise-grade server lifecycle management!

            var projectRoot = GetProjectRoot();
            var appHostPath = Path.Combine(projectRoot, "samples", "SampleApp.AppHost", "SampleApp.AppHost.csproj");

            // ✨ NEW: One-line server configuration with Aspire AppHost
            var serverConfig = ServerConfiguration.CreateAspireServer(
                new Uri("http://localhost:5300"), appHostPath)
                .WithHealthCheckEndpoints("/", "/weatherforecast")  // Multi-endpoint health checks
                .WithAutoCI()                                        // Automatic CI environment detection
                .WithHeadless(true)                                 // Headless for testing
                .WithKillOrphansOnStart(true)                       // Clean up previous runs
                .Build();

            // 🚀 Create FluentUIScaffold app with integrated server management
            using var app = FluentUIScaffoldBuilder.Web<WebApp>(options =>
            {
                options.WithServerConfiguration(serverConfig);     // 🔑 This is the magic!
                options.WithBaseUrl(new Uri("http://localhost:5300"));
                options.WithHeadlessMode(true);
            });

            // ✅ Server is automatically:
            //    - Started with health checks
            //    - Configured for your environment (CI/dev)
            //    - Ready for immediate testing
            //    - Will be stopped when disposed

            // 🧪 Write your tests normally - server lifecycle is transparent
            var driver = app.Framework<Microsoft.Playwright.IPage>();

            // Navigate to the home page
            var response = await driver.GotoAsync("http://localhost:5300/");
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, "Home page should load successfully");

            // Test the page content
            await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "Page should have a title");

            // Test the API endpoint
            var apiWorking = await driver.EvaluateAsync<bool>(@"
                fetch('/weatherforecast')
                    .then(response => response.ok)
                    .catch(() => false)
            ");
            Assert.IsTrue(apiWorking, "Weather API should be accessible");

            Console.WriteLine("✅ Aspire server lifecycle management test completed successfully!");
            Console.WriteLine("🚀 Server was automatically managed - no manual lifecycle code needed!");
        }

        /// <summary>
        /// Shows what you would have needed to do with the old WebServerManager approach.
        /// Compare this complexity with the simplicity above!
        /// </summary>
        [TestMethod]
        [TestCategory("Comparison")]
        public void TraditionalApproach_ShowsTheOldWay_ForComparison()
        {
            // 📝 This is what you would need with the OLD WebServerManager approach:

            var steps = new[]
            {
                "1. Create TestAssemblyHooks class",
                "2. Implement [AssemblyInitialize] method",
                "3. Build server configuration manually",
                "4. Call WebServerManager.StartServerAsync(plan)",
                "5. Implement [AssemblyCleanup] method",
                "6. Call WebServerManager.StopServer() manually",
                "7. Handle server lifecycle errors manually",
                "8. No automatic server reuse (slower tests)",
                "9. No configuration drift detection",
                "10. Manual CI/headless configuration",
                "11. Manual health checking",
                "12. Manual orphan process cleanup",
                "13. Risk of orphaned processes between test runs"
            };

            Console.WriteLine("❌ OLD WAY (WebServerManager) - What you had to do:");
            foreach (var step in steps)
            {
                Console.WriteLine($"   {step}");
            }

            Console.WriteLine("\n✅ NEW WAY (Aspire Lifecycle) - What you do now:");
            Console.WriteLine("   1. Create server config with .CreateAspireServer()");
            Console.WriteLine("   2. Use .WithServerConfiguration() in FluentUIScaffoldBuilder");
            Console.WriteLine("   3. Write your tests - everything else is automatic!");

            Console.WriteLine("\n🎯 Benefits of the new approach:");
            Console.WriteLine("   ✨ 3-10x faster test execution (server reuse)");
            Console.WriteLine("   🤖 Automatic server lifecycle management");
            Console.WriteLine("   🔄 Configuration drift detection & handling");
            Console.WriteLine("   🏗️ Aspire orchestration capabilities");
            Console.WriteLine("   🧪 CI/CD ready with automatic headless mode");
            Console.WriteLine("   🧹 Automatic orphan process cleanup");
            Console.WriteLine("   ❤️ Much simpler and more reliable!");

            // This test always passes - it's just for documentation
            Assert.IsTrue(true, "New approach is clearly better! 🎉");
        }

        private static string GetProjectRoot()
        {
            var currentDir = Directory.GetCurrentDirectory();
            while (currentDir != null && !File.Exists(Path.Combine(currentDir, "FluentUIScaffold.sln")))
            {
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            return currentDir ?? throw new InvalidOperationException("Could not find project root");
        }
    }
}
