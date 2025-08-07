using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Core functionality tests demonstrating basic FluentUIScaffold usage.
    /// These tests show the fundamental capabilities of the framework.
    /// </summary>
    [TestClass]
    public class CoreFunctionalityTest
    {
        [TestMethod]
        public void Can_Create_FluentUIScaffoldOptions_With_Builder()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            // Assert
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }

        [TestMethod]
        public void Can_Create_FluentUIScaffoldOptions_Directly()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Assert
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.IsFalse(options.EnableDebugMode);
        }

        [TestMethod]
        public void Can_Configure_Server_Options()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                EnableWebServerLaunch = true,
                EnableProjectDetection = true,
                WebServerProjectPath = "/path/to/project.csproj"
            };

            // Assert
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.IsTrue(options.EnableWebServerLaunch);
            Assert.IsTrue(options.EnableProjectDetection);
            Assert.AreEqual("/path/to/project.csproj", options.WebServerProjectPath);
        }
    }
}
