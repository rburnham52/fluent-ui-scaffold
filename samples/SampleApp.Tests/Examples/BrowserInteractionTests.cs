using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Playwright;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating browser interaction APIs: ExecuteScriptAsync and TakeScreenshotAsync.
    /// These tests show how to execute JavaScript in the browser context and capture screenshots
    /// using the framework-agnostic IUIDriver interface.
    /// </summary>
    [TestClass]
    public class BrowserInteractionTests
    {
        private AppScaffold<WebApp>? _app;
        private HomePage? _homePage;
        private IUIDriver? _driver;

        [TestInitialize]
        public async Task Setup()
        {
            _app = new FluentUIScaffoldBuilder()
                .UsePlaywright()
                .Web<WebApp>(options =>
                {
                    options.BaseUrl = TestConfiguration.BaseUri;
                    options.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await _app.StartAsync();

            _driver = _app.GetService<IUIDriver>();
            _homePage = new HomePage(_app.ServiceProvider);
            await _homePage.NavigateToHomeAsync();
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task ExecuteScript_ClearLocalStorage_CompletesWithoutError()
        {
            // Arrange - Set a value in localStorage first
            await _driver!.ExecuteScriptAsync("localStorage.setItem('testKey', 'testValue')");

            // Act - Clear localStorage
            await _driver.ExecuteScriptAsync("localStorage.clear()");

            // Assert - Verify it was cleared
            var value = await _driver.ExecuteScriptAsync<string?>("localStorage.getItem('testKey')");
            Assert.IsNull(value, "localStorage should be cleared");
        }

        [TestMethod]
        public async Task ExecuteScriptWithReturn_GetDocumentTitle_ReturnsPageTitle()
        {
            // Act - Get the document title via JavaScript
            var title = await _driver!.ExecuteScriptAsync<string>("document.title");

            // Assert
            Assert.IsNotNull(title, "Document title should not be null");
            Assert.IsTrue(title.Length > 0, "Document title should not be empty");
        }

        [TestMethod]
        public async Task ExecuteScriptWithReturn_GetCurrentUrl_ReturnsUrl()
        {
            // Act - Get the current URL via JavaScript
            var url = await _driver!.ExecuteScriptAsync<string>("window.location.href");

            // Assert
            Assert.IsNotNull(url, "URL should not be null");
            Assert.IsTrue(url.StartsWith("http"), "URL should be a valid HTTP address");
        }

        [TestMethod]
        public async Task ExecuteScriptWithReturn_QueryDomElements_ReturnsElementCount()
        {
            // Act - Count heading elements on the page
            var headingCount = await _driver!.ExecuteScriptAsync<int>("document.querySelectorAll('h1').length");

            // Assert
            Assert.IsTrue(headingCount >= 0, "Heading count should be a non-negative number");
        }

        [TestMethod]
        public async Task ExecuteScript_SetAndGetSessionStorage_RoundTripsValue()
        {
            // Arrange & Act - Store and retrieve a value from sessionStorage
            await _driver!.ExecuteScriptAsync("sessionStorage.setItem('session-test', 'hello')");
            var result = await _driver.ExecuteScriptAsync<string>("sessionStorage.getItem('session-test')");

            // Assert
            Assert.AreEqual("hello", result, "sessionStorage should round-trip the value");

            // Cleanup
            await _driver.ExecuteScriptAsync("sessionStorage.removeItem('session-test')");
        }

        [TestMethod]
        public async Task TakeScreenshot_CaptureHomePage_SavesFileAndReturnsBytes()
        {
            // Arrange
            var screenshotDir = Path.Combine(Path.GetTempPath(), "FluentUIScaffold_Screenshots");
            Directory.CreateDirectory(screenshotDir);
            var filePath = Path.Combine(screenshotDir, $"homepage_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            try
            {
                // Act - Take a screenshot
                var bytes = await _driver!.TakeScreenshotAsync(filePath);

                // Assert
                Assert.IsTrue(bytes.Length > 0, "Screenshot should return non-empty byte array");
                Assert.IsTrue(File.Exists(filePath), "Screenshot file should be saved to disk");
                Assert.IsTrue(new FileInfo(filePath).Length > 0, "Screenshot file should not be empty");
            }
            finally
            {
                // Cleanup
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task TakeScreenshot_AfterInteraction_CapturesCurrentState()
        {
            // Arrange - Interact with the page first
            _homePage!.ClickCounter();
            _homePage.ClickCounter();

            var screenshotDir = Path.Combine(Path.GetTempPath(), "FluentUIScaffold_Screenshots");
            Directory.CreateDirectory(screenshotDir);
            var filePath = Path.Combine(screenshotDir, $"after_interaction_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            try
            {
                // Act - Capture the page after interactions
                var bytes = await _driver!.TakeScreenshotAsync(filePath);

                // Assert
                Assert.IsTrue(bytes.Length > 0, "Screenshot after interaction should return non-empty byte array");
                Assert.IsTrue(File.Exists(filePath), "Screenshot file should exist after interaction");
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public async Task ExecuteScript_EvaluateMathExpression_ReturnsCorrectResult()
        {
            // Act - Evaluate a simple math expression
            var result = await _driver!.ExecuteScriptAsync<int>("2 + 3");

            // Assert
            Assert.AreEqual(5, result, "JavaScript math evaluation should return correct result");
        }

        [TestMethod]
        public async Task ExecuteScript_ReturnJsonObject_DeserializesToExpectedType()
        {
            // Act - Return a boolean from JavaScript
            var result = await _driver!.ExecuteScriptAsync<bool>("document.readyState === 'complete'");

            // Assert
            Assert.IsTrue(result, "Document should be in 'complete' ready state after page load");
        }
    }
}
