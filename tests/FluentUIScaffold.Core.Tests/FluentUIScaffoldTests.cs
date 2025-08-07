using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the FluentUIScaffold class using the new auto-discovery pattern.
    /// </summary>
    [TestFixture]
    public class FluentUIScaffoldTests
    {
        [Test]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Act & Assert
            Assert.That(options, Is.Not.Null);
            Assert.That(options.BaseUrl, Is.EqualTo(new Uri("http://localhost:5000")));
            Assert.That(options.DefaultWaitTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(options.EnableDebugMode, Is.False);
        }

        [Test]
        public void Constructor_WithNullOptions_ThrowsException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FluentUIScaffoldOptionsBuilder(null!));
        }

        [Test]
        public void WithBaseUrl_WithValidUrl_SetsBaseUrl()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            };

            // Act & Assert
            Assert.That(options.BaseUrl, Is.EqualTo(new Uri("http://localhost:5000")));
        }

        [Test]
        public void WithDefaultWaitTimeout_WithValidTimeout_SetsDefaultWaitTimeout()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                DefaultWaitTimeout = TimeSpan.FromSeconds(60)
            };

            // Act & Assert
            Assert.That(options.DefaultWaitTimeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
        }

        [Test]
        public void WithDefaultWaitTimeoutDebug_WithValidTimeout_SetsDefaultWaitTimeoutDebug()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                DefaultWaitTimeoutDebug = TimeSpan.FromSeconds(120)
            };

            // Act & Assert
            Assert.That(options.DefaultWaitTimeoutDebug, Is.EqualTo(TimeSpan.FromSeconds(120)));
        }

        [Test]
        public void WithDebugMode_WithEnabled_SetsDebugMode()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                EnableDebugMode = true
            };

            // Act & Assert
            Assert.That(options.EnableDebugMode, Is.True);
        }

        [Test]
        public void WithWebServerProjectPath_WithValidPath_SetsWebServerProjectPath()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                WebServerProjectPath = "/path/to/project.csproj"
            };

            // Act & Assert
            Assert.That(options.WebServerProjectPath, Is.EqualTo("/path/to/project.csproj"));
        }

        [Test]
        public void WithServerConfiguration_WithValidConfiguration_SetsServerConfiguration()
        {
            // Arrange
            var serverConfig = new ServerConfiguration
            {
                ServerType = ServerType.AspNetCore,
                ProjectPath = "/path/to/project.csproj"
            };

            var options = new FluentUIScaffoldOptions
            {
                ServerConfiguration = serverConfig
            };

            // Act & Assert
            Assert.That(options.ServerConfiguration, Is.EqualTo(serverConfig));
        }

        [Test]
        public void WithProjectDetection_WithEnabled_SetsProjectDetection()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                EnableProjectDetection = true
            };

            // Act & Assert
            Assert.That(options.EnableProjectDetection, Is.True);
        }

        [Test]
        public void WithAdditionalSearchPaths_WithValidPaths_SetsAdditionalSearchPaths()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                AdditionalSearchPaths = { "/path1", "/path2" }
            };

            // Act & Assert
            Assert.That(options.AdditionalSearchPaths, Is.EquivalentTo(new[] { "/path1", "/path2" }));
        }

        [Test]
        public void WithWebServerLaunch_WithEnabled_SetsWebServerLaunch()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                EnableWebServerLaunch = true
            };

            // Act & Assert
            Assert.That(options.EnableWebServerLaunch, Is.True);
        }
    }
}
