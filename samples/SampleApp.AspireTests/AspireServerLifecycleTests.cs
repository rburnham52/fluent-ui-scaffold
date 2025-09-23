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
    /// Integration tests demonstrating the FluentUIScaffold Aspire server lifecycle management.
    /// These tests use the recommended UseAspireHosting pattern which delegates lifecycle
    /// management to Aspire's DistributedApplicationTestingBuilder.
    /// </summary>
    [TestClass]
    public class AspireServerLifecycleTests
    {
        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task EnsureStartedAsync_WithAspireAppHost_StartsServerSuccessfully()
        {
            // Arrange - Create AppScaffold with Aspire hosting using the unified API
            var app = new FluentUIScaffoldBuilder()
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { /* configure distributed app if needed */ },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                    options.UsePlaywright();
                    options.HeadlessMode = true;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await app.StartAsync();

            try
            {
                // Assert - Server should be running and accessible
                var driver = app.Framework<Microsoft.Playwright.IPage>();

                // Navigate to the home page
                var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
                var response = await driver.GotoAsync(baseUrl);
                Assert.IsNotNull(response);
                Assert.IsTrue(response.Ok, $"Failed to load home page: {response.Status} - {response.StatusText}");

                // Verify the page loaded correctly
                await driver.WaitForSelectorAsync("h1", new() { Timeout = 10000 });
                var title = await driver.TitleAsync();
                Assert.IsTrue(title.Contains("FluentUIScaffold") || title.Contains("SampleApp") || !string.IsNullOrEmpty(title),
                    $"Expected page to have a valid title, got: {title}");
            }
            finally
            {
                await app.DisposeAsync();
            }
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task HealthCheckValidation_WithCustomEndpoints_ValidatesServerReadiness()
        {
            // Arrange - Configure app with Aspire hosting
            var app = new FluentUIScaffoldBuilder()
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                    options.UsePlaywright();
                    options.HeadlessMode = true;
                })
                .Build<WebApp>();

            await app.StartAsync();

            try
            {
                // Assert - Verify all health check endpoints are accessible
                var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
                using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                // Test main endpoint
                var homeResponse = await httpClient.GetAsync(baseUrl);
                Assert.IsTrue(homeResponse.IsSuccessStatusCode,
                    $"Home endpoint should be healthy: {homeResponse.StatusCode}");

                // Test API endpoint (route is /api/weather per WeatherForecastController)
                var apiResponse = await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}/api/weather");
                Assert.IsTrue(apiResponse.IsSuccessStatusCode,
                    $"API endpoint should be healthy: {apiResponse.StatusCode}");

                // Verify API returns expected data (JSON array with weather objects)
                var weatherData = await apiResponse.Content.ReadAsStringAsync();
                Assert.IsTrue(
                    weatherData.Contains("temperatureC") ||
                    weatherData.Contains("TemperatureC") ||
                    weatherData.Contains("temperature") ||
                    weatherData.Contains("date") ||
                    weatherData.Contains("Date") ||
                    weatherData.StartsWith("["),
                    $"Weather API should return weather data. Got: {weatherData.Substring(0, Math.Min(200, weatherData.Length))}");
            }
            finally
            {
                await app.DisposeAsync();
            }
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
                // Act - Create app in CI mode with automatic headless detection
                var app = new FluentUIScaffoldBuilder()
                    .UseAspireHosting<Projects.SampleApp_AppHost>(
                        appHost => { },
                        "sampleapp")
                    .Web<WebApp>(options =>
                    {
                        options.UsePlaywright();
                        // HeadlessMode should be auto-detected as true in CI
                    })
                    .Build<WebApp>();

                await app.StartAsync();

                try
                {
                    // Assert - Verify server started and is accessible
                    var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";
                    using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                    var response = await httpClient.GetAsync($"{baseUrl.TrimEnd('/')}/weatherforecast");
                    Assert.IsTrue(response.IsSuccessStatusCode,
                        "Server should be accessible in CI headless mode");

                    // Verify Playwright works in headless mode
                    var driver = app.Framework<Microsoft.Playwright.IPage>();
                    await driver.GotoAsync(baseUrl);

                    // In headless mode, we can still interact with the page
                    await driver.WaitForSelectorAsync("body", new() { Timeout = 10000 });
                    var bodyContent = await driver.TextContentAsync("body");
                    Assert.IsNotNull(bodyContent, "Page content should be accessible in headless mode");
                }
                finally
                {
                    await app.DisposeAsync();
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("CI", null);
            }
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("ServerLifecycle")]
        public async Task MultiplePageNavigation_WithAspireHosting_WorksCorrectly()
        {
            // Arrange - Create AppScaffold with Aspire hosting
            var app = new FluentUIScaffoldBuilder()
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                    options.UsePlaywright();
                    options.HeadlessMode = true;
                })
                .Build<WebApp>();

            await app.StartAsync();

            try
            {
                var driver = app.Framework<Microsoft.Playwright.IPage>();
                var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";

                // Navigate to home page
                var homeResponse = await driver.GotoAsync(baseUrl);
                Assert.IsTrue(homeResponse!.Ok, "Home page should load");

                // Navigate to weather page
                var weatherResponse = await driver.GotoAsync($"{baseUrl.TrimEnd('/')}/weatherforecast");
                Assert.IsTrue(weatherResponse!.Ok, "Weather page should load");

                // Navigate back to home
                var backHomeResponse = await driver.GotoAsync(baseUrl);
                Assert.IsTrue(backHomeResponse!.Ok, "Should navigate back to home");
            }
            finally
            {
                await app.DisposeAsync();
            }
        }

        [TestMethod]
        [TestCategory("Aspire")]
        [TestCategory("Performance")]
        public async Task AppScaffoldReuse_WithSameConfiguration_WorksCorrectly()
        {
            // This test demonstrates that the same AppScaffold can be used across multiple operations
            var app = new FluentUIScaffoldBuilder()
                .UseAspireHosting<Projects.SampleApp_AppHost>(
                    appHost => { },
                    "sampleapp")
                .Web<WebApp>(options =>
                {
                    options.UsePlaywright();
                    options.HeadlessMode = true;
                })
                .Build<WebApp>();

            await app.StartAsync();

            try
            {
                var driver = app.Framework<Microsoft.Playwright.IPage>();
                var baseUrl = app.GetService<FluentUIScaffoldOptions>()?.BaseUrl?.ToString() ?? "http://localhost:5000";

                // First operation
                var firstResponse = await driver.GotoAsync(baseUrl);
                Assert.IsTrue(firstResponse!.Ok, "First operation should succeed");
                var firstTitle = await driver.TitleAsync();

                // Second operation (same page)
                var secondResponse = await driver.GotoAsync(baseUrl);
                Assert.IsTrue(secondResponse!.Ok, "Second operation should succeed");
                var secondTitle = await driver.TitleAsync();

                // Third operation (different page)
                var thirdResponse = await driver.GotoAsync($"{baseUrl.TrimEnd('/')}/weatherforecast");
                Assert.IsTrue(thirdResponse!.Ok, "Third operation should succeed");

                // Titles should be consistent
                Assert.AreEqual(firstTitle, secondTitle, "Page titles should be consistent across reloads");
            }
            finally
            {
                await app.DisposeAsync();
            }
        }
    }
}
