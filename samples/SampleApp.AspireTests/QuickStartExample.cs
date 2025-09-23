using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Quick start examples demonstrating FluentUIScaffold with Aspire hosting.
    /// These tests show the recommended patterns for E2E testing with the unified API.
    /// </summary>
    [TestClass]
    public class QuickStartExample
    {
        [TestMethod]
        [TestCategory("QuickStart")]
        [TestCategory("Example")]
        public async Task QuickStart_AspireLifecycleManagement_JustWorks()
        {
            // 🎯 This is all you need for Aspire-hosted E2E testing.
            // The server is automatically managed by the TestAssemblyHooks.

            var app = TestAssemblyHooks.GetSessionApp();
            if (app == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check TestAssemblyHooks initialization.");
                return;
            }

            // Get the Playwright page from the app
            var driver = app.Framework<Microsoft.Playwright.IPage>();

            // Get the base URL from the options (set by Aspire hosting)
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";

            // Navigate to the SampleApp
            var response = await driver.GotoAsync(baseUrl);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"SampleApp should load successfully. Status: {response.Status}");

            // Verify the app loaded correctly
            await driver.WaitForSelectorAsync("body", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(!string.IsNullOrEmpty(title), "App should have a title");

            Console.WriteLine("✅ Aspire server lifecycle management test completed successfully!");
            Console.WriteLine($"🚀 Server URL: {baseUrl}");
            Console.WriteLine("📝 Server was automatically managed - no manual lifecycle code needed!");
        }

        [TestMethod]
        [TestCategory("QuickStart")]
        [TestCategory("Example")]
        public async Task QuickStart_SimpleServerConfiguration_JustWorks()
        {
            // 🎯 Even simpler - just use the shared Aspire host and focus on tests.

            var app = TestAssemblyHooks.GetSessionApp();
            if (app == null)
            {
                Assert.Inconclusive("Session-level Aspire server is not available. Check TestAssemblyHooks initialization.");
                return;
            }

            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";

            // Navigate to the app
            var response = await driver.GotoAsync(baseUrl);
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, "SampleApp should be accessible");

            // Test the weather API endpoint
            var apiResponse = await driver.GotoAsync($"{baseUrl.TrimEnd('/')}/weatherforecast");
            Assert.IsNotNull(apiResponse);
            Assert.IsTrue(apiResponse.Ok, "Weather API should be accessible");

            // Verify it returns JSON data
            var content = await driver.ContentAsync();
            Assert.IsTrue(content.Contains("temperatureC") || content.Contains("Temperature") || content.Contains("date"),
                "Weather API should return weather data");

            Console.WriteLine("✅ Simple Aspire integration test completed!");
            Console.WriteLine("🚀 Server lifecycle was completely automatic!");
        }
    }
}
