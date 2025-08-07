using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Simple core functionality tests.
    /// These tests demonstrate basic FluentUIScaffold usage patterns.
    /// </summary>
    [TestClass]
    public class SimpleCoreTest
    {
        [TestMethod]
        public void Can_Create_Basic_Options()
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
        public void Can_Create_Options_With_Builder()
        {
            // Arrange & Act
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("http://localhost:5000"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .Build();

            // Assert
            Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
            Assert.AreEqual(TimeSpan.FromSeconds(30), options.DefaultWaitTimeout);
        }
    }
}
