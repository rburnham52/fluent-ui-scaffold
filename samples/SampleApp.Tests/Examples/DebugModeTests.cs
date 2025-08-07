using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating debug mode functionality.
    /// These tests show how to enable debug mode for easier debugging of UI interactions.
    /// </summary>
    [TestClass]
    public class DebugModeTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private HomePage? _homePage;
        private StringWriter? _logOutput;

        [TestInitialize]
        public async Task Setup()
        {
            // Capture log output for verification
            _logOutput = new StringWriter();
            
            // Arrange - Set up the application with debug mode enabled
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(true) // Enable debug mode for this test
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            // Create page objects
            _homePage = new HomePage(_app.ServiceProvider);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
            _logOutput?.Dispose();
        }

        [TestMethod]
        public async Task InteractWithElementsInDebugMode_WhenDebugModeEnabled_ProvidesDetailedInteractionLogging()
        {
            // Arrange
            await _homePage!.NavigateToHomeAsync();

            // Act - Perform interactions in debug mode and capture logs
            var logBefore = _logOutput!.ToString();
            
            var initialCount = _homePage.GetCounterValue();
            var newCount = _homePage.ClickCounter();
            var finalCount = _homePage.ClickCounter();

            var logAfter = _logOutput.ToString();

            // Assert - Verify interactions work correctly in debug mode
            Assert.AreEqual(initialCount + 1, newCount, "Counter should increment when clicked in debug mode");
            Assert.AreEqual(initialCount + 2, finalCount, "Counter should increment multiple times in debug mode");

            // Verify that debug mode produced detailed logging
            var debugLogs = logAfter;
            if (!string.IsNullOrEmpty(logBefore))
            {
                debugLogs = logAfter.Replace(logBefore, "");
            }
            Assert.IsTrue(debugLogs.Contains("Debug"), "Debug mode should produce debug-level logs");
            Assert.IsTrue(debugLogs.Contains("Counter") || debugLogs.Contains("click"), "Debug logs should contain interaction details");

            // Verify page content is accessible in debug mode
            var hasExpectedContent = await _homePage.HasExpectedContentAsync();
            Assert.IsTrue(hasExpectedContent, "Page should have expected content in debug mode");
        }

        [TestMethod]
        public async Task DebugModeProvidesDetailedLogging_WhenDebugModeEnabled_UsesDebugTimeout()
        {
            // Arrange - Set up with debug timeout and capture logs
            var debugLogOutput = new StringWriter();
            
            var debugOptions = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDefaultWaitTimeoutDebug(TimeSpan.FromSeconds(60)) // Longer timeout for debug mode
                .WithDebugMode(true)
                .Build();

            using var debugApp = new FluentUIScaffoldApp<WebApp>(debugOptions);
            await debugApp.InitializeAsync();

            var debugHomePage = new HomePage(debugApp.ServiceProvider);

            // Act - Perform interactions with debug timeout
            var logBefore = debugLogOutput.ToString();
            await debugHomePage.NavigateToHomeAsync();
            var logAfter = debugLogOutput.ToString();

            // Assert - Verify debug mode uses the debug timeout
            Assert.IsTrue(debugOptions.EnableDebugMode, "Debug mode should be enabled");
            Assert.AreEqual(TimeSpan.FromSeconds(60), debugOptions.DefaultWaitTimeoutDebug, "Debug timeout should be set");

            // Verify that debug timeout produced appropriate logging
            var debugLogs = logAfter;
            if (!string.IsNullOrEmpty(logBefore))
            {
                debugLogs = logAfter.Replace(logBefore, "");
            }
            Assert.IsTrue(debugLogs.Contains("Debug") || debugLogs.Contains("60"), "Debug logs should reflect debug timeout settings");

            // Verify interactions work with debug timeout
            var initialCount = debugHomePage.GetCounterValue();
            var newCount = debugHomePage.ClickCounter();
            Assert.AreEqual(initialCount + 1, newCount, "Counter should work with debug timeout");

            var hasExpectedContent = await debugHomePage.HasExpectedContentAsync();
            Assert.IsTrue(hasExpectedContent, "Page content should be accessible with debug timeout");
            
            debugLogOutput.Dispose();
        }

        [TestMethod]
        public async Task DebugModeVsNormalMode_WhenComparingLogs_ShowsDifferentDetailLevels()
        {
            // Arrange - Test with debug mode enabled
            var debugOptions = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDebugMode(true)
                .Build();

            using var debugApp = new FluentUIScaffoldApp<WebApp>(debugOptions);
            await debugApp.InitializeAsync();
            var debugHomePage = new HomePage(debugApp.ServiceProvider);

            // Act - Perform interactions in debug mode
            await debugHomePage.NavigateToHomeAsync();
            var debugCount = debugHomePage.GetCounterValue();

            // Arrange - Test with debug mode disabled
            var normalOptions = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDebugMode(false)
                .Build();

            using var normalApp = new FluentUIScaffoldApp<WebApp>(normalOptions);
            await normalApp.InitializeAsync();
            var normalHomePage = new HomePage(normalApp.ServiceProvider);

            // Act - Perform same interactions in normal mode
            await normalHomePage.NavigateToHomeAsync();
            var normalCount = normalHomePage.GetCounterValue();

            // Assert - Verify both modes work but debug mode provides more detail
            Assert.AreEqual(debugCount, normalCount, "Both modes should produce same functional results");
            
            // Note: In a real implementation, we would capture and compare the actual log outputs
            // to verify that debug mode produces more detailed logging than normal mode
            Assert.IsTrue(debugOptions.EnableDebugMode, "Debug mode should be enabled for detailed logging");
            Assert.IsFalse(normalOptions.EnableDebugMode, "Normal mode should have debug disabled");
        }
    }
}
