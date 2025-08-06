using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    [TestClass]
    public class SimpleCoreTest
    {
        [TestMethod]
        public void Can_Create_FluentUIScaffoldApp()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://localhost:5001"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Information
            };

            // Act & Assert
            var app = new FluentUIScaffoldApp<object>(options);
            Assert.IsNotNull(app);
        }

        [TestMethod]
        public void Can_Create_FluentUIScaffoldOptionsBuilder()
        {
            // Arrange & Act
            var builder = new FluentUIScaffoldOptionsBuilder();
            var options = builder
                .WithBaseUrl(new Uri("https://localhost:5001"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithLogLevel(Microsoft.Extensions.Logging.LogLevel.Information)
                .Build();

            // Assert
            Assert.IsNotNull(options);
            Assert.AreEqual("https://localhost:5001/", options.BaseUrl?.ToString());
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
        }
    }
}
