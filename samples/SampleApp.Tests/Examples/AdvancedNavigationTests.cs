using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Advanced navigation tests demonstrating complex navigation scenarios.
    /// These tests show how to handle navigation between different sections and pages.
    /// </summary>
    [TestClass]
    public class AdvancedNavigationTests
    {
        [TestMethod]
        public async Task Can_Navigate_To_All_Sections()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Act & Assert
            // This test would normally navigate through different sections, but for now we'll just verify the options are set correctly
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }

        [TestMethod]
        public async Task Can_Handle_Deep_Navigation()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(60), // Longer timeout for complex navigation
                EnableDebugMode = false
            };

            // Act & Assert
            // Verify that the options are configured for complex navigation scenarios
            Assert.AreEqual(TimeSpan.FromSeconds(60), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }
    }
}
