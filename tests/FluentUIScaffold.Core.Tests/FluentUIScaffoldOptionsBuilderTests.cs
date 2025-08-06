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
            var url = new Uri("https://example.com");

            // Act
            var result = builder.WithBaseUrl(url);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().BaseUrl, Is.EqualTo(url));
        }

        [Test]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
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
            var timeout = TimeSpan.FromSeconds(30);

            // Act
            var result = builder.WithDefaultWaitTimeout(timeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().DefaultWaitTimeout, Is.EqualTo(timeout));
        }

        [Test]
        public void WithDefaultWaitTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithDefaultWaitTimeout(TimeSpan.Zero));
        }

        [Test]
        public void WithLogLevel_WithValidLevel_SetsLogLevel()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithLogLevel(LogLevel.Information);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().LogLevel, Is.EqualTo(LogLevel.Information));
        }

        [Test]
        public void WithHeadlessMode_WithEnabled_SetsHeadlessMode()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithHeadlessMode(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().HeadlessMode, Is.True);
        }

        [Test]
        public void WithWebServerProjectPath_WithValidPath_SetsWebServerProjectPath()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var path = "/path/to/project";

            // Act
            var result = builder.WithWebServerProjectPath(path);

            // Assert
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().WebServerProjectPath, Is.EqualTo(path));
        }

        [Test]
        public void WithWebServerProjectPath_WithNullPath_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithWebServerProjectPath(null!));
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
            Assert.That(builder.Build().DebugMode, Is.True);
        }

        [Test]
        public void Build_WithValidConfiguration_ReturnsOptions()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("https://example.com"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithLogLevel(LogLevel.Information)
                .WithHeadlessMode(true)
                .WithWebServerProjectPath("/path/to/project")
                .WithDebugMode(false);

            // Act
            var options = builder.Build();

            // Assert
            Assert.That(options, Is.Not.Null);
            Assert.That(options.BaseUrl, Is.EqualTo(new Uri("https://example.com")));
            Assert.That(options.DefaultWaitTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(options.LogLevel, Is.EqualTo(LogLevel.Information));
            Assert.That(options.HeadlessMode, Is.True);
            Assert.That(options.WebServerProjectPath, Is.EqualTo("/path/to/project"));
            Assert.That(options.DebugMode, Is.False);
        }
    }
}
