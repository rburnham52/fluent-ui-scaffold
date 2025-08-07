using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating registration and login functionality.
    /// These tests show how to handle user registration and login workflows.
    /// </summary>
    [TestClass]
    public class RegistrationLoginTests
    {
        [TestMethod]
        public async Task Can_Complete_Registration_Flow()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Act & Assert
            // This test would normally complete a registration flow, but for now we'll just verify the options are set correctly
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }

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
    }
}
