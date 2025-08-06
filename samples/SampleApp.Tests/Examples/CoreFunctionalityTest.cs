using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    [TestClass]
    public class CoreFunctionalityTest
    {
        [TestMethod]
        public void Can_Create_FluentUIScaffoldOptionsBuilder()
        {
            // Arrange & Act
            var builder = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithLogLevel(Microsoft.Extensions.Logging.LogLevel.Information)
                .WithHeadlessMode(true)
                .WithDebugMode(false);

            // Assert
            var options = builder.Build();
            Assert.IsNotNull(options);
            Assert.AreEqual("http://localhost:5000/", options.BaseUrl?.ToString());
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
            Assert.AreEqual(Microsoft.Extensions.Logging.LogLevel.Information, options.LogLevel);
            Assert.IsTrue(options.HeadlessMode);
            Assert.IsFalse(options.DebugMode);
        }

        [TestMethod]
        public void Can_Create_FluentUIScaffoldApp()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                LogLevel = Microsoft.Extensions.Logging.LogLevel.Information
            };

            // Act & Assert
            var app = new FluentUIScaffoldApp<object>(options);
            Assert.IsNotNull(app);
        }

        [TestMethod]
        public void Can_Validate_Options()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.ThrowsException<FluentUIScaffold.Core.Exceptions.FluentUIScaffoldValidationException>(() =>
            {
                builder.WithDefaultWaitTimeout(TimeSpan.Zero).Build();
            });
        }
    }
}
