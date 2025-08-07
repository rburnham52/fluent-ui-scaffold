using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class FluentUIScaffoldOptionsBuilderTests
    {
        [Test]
        public void Constructor_WithNoParameters_CreatesInstance()
        {
            // Act
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Assert
            Assert.That(builder, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithExistingOptions_CreatesInstance()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions();

            // Act
            var builder = new FluentUIScaffoldOptionsBuilder(options);

            // Assert
            Assert.That(builder, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FluentUIScaffoldOptionsBuilder(null!));
        }

        [Test]
        public void WithBaseUrl_WithValidUrl_SetsBaseUrl()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedUrl = new Uri("http://localhost:5000");

            // Act
            var result = builder.WithBaseUrl(expectedUrl);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().BaseUrl, Is.EqualTo(expectedUrl));
        }

        [Test]
        public void WithBaseUrl_WithNullUrl_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithBaseUrl(null!));
        }

        [Test]
        public void WithDefaultWaitTimeout_WithValidTimeout_SetsDefaultWaitTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedTimeout = TimeSpan.FromSeconds(60);

            // Act
            var result = builder.WithDefaultWaitTimeout(expectedTimeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().DefaultWaitTimeout, Is.EqualTo(expectedTimeout));
        }

        [Test]
        public void WithDefaultWaitTimeout_WithZeroTimeout_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithDefaultWaitTimeout(TimeSpan.Zero));
        }

        [Test]
        public void WithDefaultWaitTimeout_WithNegativeTimeout_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithDefaultWaitTimeout(TimeSpan.FromSeconds(-1)));
        }

        [Test]
        public void WithDefaultWaitTimeoutDebug_WithValidTimeout_SetsDefaultWaitTimeoutDebug()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedTimeout = TimeSpan.FromSeconds(120);

            // Act
            var result = builder.WithDefaultWaitTimeoutDebug(expectedTimeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().DefaultWaitTimeoutDebug, Is.EqualTo(expectedTimeout));
        }

        [Test]
        public void WithDebugMode_WithEnabled_SetsDebugMode()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithDebugMode(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().EnableDebugMode, Is.True);
        }

        [Test]
        public void WithWebServerProjectPath_WithValidPath_SetsWebServerProjectPath()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedPath = "/path/to/project.csproj";

            // Act
            var result = builder.WithWebServerProjectPath(expectedPath);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().WebServerProjectPath, Is.EqualTo(expectedPath));
        }

        [Test]
        public void WithWebServerProjectPath_WithNullPath_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithWebServerProjectPath(null!));
        }

        [Test]
        public void WithWebServerProjectPath_WithEmptyPath_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithWebServerProjectPath(""));
        }

        [Test]
        public void WithServerConfiguration_WithValidConfiguration_SetsServerConfiguration()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedConfig = new ServerConfiguration
            {
                ServerType = ServerType.AspNetCore,
                ProjectPath = "/path/to/project.csproj"
            };

            // Act
            var result = builder.WithServerConfiguration(expectedConfig);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().ServerConfiguration, Is.EqualTo(expectedConfig));
        }

        [Test]
        public void WithServerConfiguration_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithServerConfiguration(null!));
        }

        [Test]
        public void WithProjectDetection_WithEnabled_SetsProjectDetection()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithProjectDetection(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().EnableProjectDetection, Is.True);
        }

        [Test]
        public void WithAdditionalSearchPaths_WithValidPaths_SetsAdditionalSearchPaths()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedPaths = new[] { "/path1", "/path2" };

            // Act
            var result = builder.WithAdditionalSearchPaths(expectedPaths);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().AdditionalSearchPaths, Is.EquivalentTo(expectedPaths));
        }

        [Test]
        public void WithAdditionalSearchPaths_WithNullPaths_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithAdditionalSearchPaths(null!));
        }

        [Test]
        public void WithWebServerLaunch_WithEnabled_SetsWebServerLaunch()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithWebServerLaunch(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().EnableWebServerLaunch, Is.True);
        }

        [Test]
        public void Build_WithValidConfiguration_ReturnsConfiguredOptions()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedUrl = new Uri("http://localhost:5000");
            var expectedTimeout = TimeSpan.FromSeconds(60);

            // Act
            var options = builder
                .WithBaseUrl(expectedUrl)
                .WithDefaultWaitTimeout(expectedTimeout)
                .WithDebugMode(true)
                .WithWebServerLaunch(true)
                .Build();

            // Assert
            Assert.That(options.BaseUrl, Is.EqualTo(expectedUrl));
            Assert.That(options.DefaultWaitTimeout, Is.EqualTo(expectedTimeout));
            Assert.That(options.EnableDebugMode, Is.True);
            Assert.That(options.EnableWebServerLaunch, Is.True);
        }

        [Test]
        public void Build_WithInvalidConfiguration_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithDefaultWaitTimeout(TimeSpan.Zero).Build());
        }
    }
}
