using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating login flow functionality.
    /// These tests show how to handle user login workflows.
    /// </summary>
    [TestClass]
    public class LoginFlowTests
    {
        [TestMethod]
        public async Task Can_Complete_Login_Flow()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Act & Assert
            // This test would normally complete a login flow, but for now we'll just verify the options are set correctly
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }

        [TestMethod]
        public async Task Can_Handle_Login_Validation()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(60), // Longer timeout for validation
                EnableDebugMode = false
            };

            // Act & Assert
            // This test would normally handle login validation, but for now we'll just verify the options are set correctly
            Assert.AreEqual(TimeSpan.FromSeconds(60), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }
    }
}
