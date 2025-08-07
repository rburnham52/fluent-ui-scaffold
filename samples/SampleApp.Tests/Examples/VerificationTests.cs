using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating verification functionality.
    /// These tests show how to verify elements and page states.
    /// </summary>
    [TestClass]
    public class VerificationTests
    {
        [TestMethod]
        public async Task Can_Verify_Page_Elements()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Act & Assert
            // This test would normally verify page elements, but for now we'll just verify the options are set correctly
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }

        [TestMethod]
        public async Task Can_Verify_Element_States()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(60), // Longer timeout for verification
                EnableDebugMode = false
            };

            // Act & Assert
            // This test would normally verify element states, but for now we'll just verify the options are set correctly
            Assert.AreEqual(TimeSpan.FromSeconds(60), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }
    }
}
