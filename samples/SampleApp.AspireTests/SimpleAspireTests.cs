using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Simple integration tests using the assembly-level Aspire instance.
    /// These tests use the session app started in TestAssemblyHooks.
    /// </summary>
    [TestClass]
    public class SimpleAspireTests
    {
        [TestMethod]
        [TestCategory("Aspire")]
        public async Task HomePage_ShouldLoad()
        {
            // Arrange - Get the session app (already started by TestAssemblyHooks)
            var app = TestAssemblyHooks.GetSessionApp();
            Assert.IsNotNull(app, "Session app should be initialized");

            // Act - Navigate to home page
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.ServiceProvider.GetRequiredService<FluentUIScaffoldOptions>().BaseUrl;
            Assert.IsNotNull(baseUrl, "Base URL should be set");

            var response = await driver.GotoAsync(baseUrl.ToString());

            // Assert
            Assert.IsNotNull(response);
            Assert.IsTrue(response.Ok, $"Failed to load home page: {response.Status} - {response.StatusText}");

            // Verify page loaded
            await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
            var title = await driver.TitleAsync();
            Assert.IsTrue(
                title.Contains("FluentUIScaffold") || title.Contains("SampleApp"),
                $"Expected page title to contain 'FluentUIScaffold' or 'SampleApp', got: {title}");

            Console.WriteLine($"✅ Home page loaded successfully at {baseUrl}");
        }

        [TestMethod]
        [TestCategory("Aspire")]
        public async Task WeatherAPI_ShouldBeAccessible()
        {
            // Arrange
            var app = TestAssemblyHooks.GetSessionApp();
            Assert.IsNotNull(app, "Session app should be initialized");

            // Act - Call the weather API
            var driver = app.Framework<Microsoft.Playwright.IPage>();
            var baseUrl = app.ServiceProvider.GetRequiredService<FluentUIScaffoldOptions>().BaseUrl;
            Assert.IsNotNull(baseUrl, "Base URL should be set");

            await driver.GotoAsync(baseUrl.ToString());

            var apiResponse = await driver.EvaluateAsync<bool>("fetch('/api/weather').then(r => r.ok)");

            // Assert
            Assert.IsTrue(apiResponse, "Weather API endpoint should be accessible");

            Console.WriteLine("✅ Weather API is accessible");
        }
    }
}
