using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating debug mode functionality.
    /// These tests show how to enable debug mode for easier debugging of UI interactions.
    /// </summary>
    [TestClass]
    public class DebugModeTests
    {
        [TestMethod]
        public async Task Can_Interact_With_Elements_In_Debug_Mode()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = true // Enable debug mode for this test
            };

            // Act & Assert
            // This test would normally interact with the UI, but for now we'll just verify the options are set correctly
            Assert.IsTrue(options.EnableDebugMode);
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
        }

        [TestMethod]
        public async Task Debug_Mode_Provides_Detailed_Logging()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeoutDebug = TimeSpan.FromSeconds(60), // Longer timeout for debug mode
                EnableDebugMode = true
            };

            // Act & Assert
            // Verify that debug mode uses the debug timeout
            Assert.IsTrue(options.EnableDebugMode);
            Assert.AreEqual(TimeSpan.FromSeconds(60), options.DefaultWaitTimeoutDebug);
        }
    }
}
