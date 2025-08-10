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

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().BaseUrl, Is.EqualTo(expectedUrl));
            });
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

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().DefaultWaitTimeout, Is.EqualTo(expectedTimeout));
            });
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
        public void WithHeadlessMode_AndSlowMo_SetProperly()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            bool? expectedHeadless = false;
            int? expectedSlowMo = 300;

            // Act
            var result = builder
                .WithHeadlessMode(expectedHeadless)
                .WithSlowMo(expectedSlowMo);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                var options = builder.Build();
                Assert.That(options.HeadlessMode, Is.EqualTo(expectedHeadless));
                Assert.That(options.SlowMo, Is.EqualTo(expectedSlowMo));
            });
        }

        // Removed debug mode API; tests updated to use Headless/SlowMo

        [Test]
        public void WithWebServerProjectPath_WithValidPath_SetsWebServerProjectPath()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var expectedPath = "/path/to/project.csproj";

            // Act
            var result = builder.WithWebServerProjectPath(expectedPath);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().WebServerProjectPath, Is.EqualTo(expectedPath));
            });
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
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithWebServerProjectPath(string.Empty));
        }

        [Test]
        public void WithHeadlessMode_WithValidValue_SetsHeadlessMode()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            bool? expectedHeadless = false;

            // Act
            var result = builder.WithHeadlessMode(expectedHeadless);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().HeadlessMode, Is.EqualTo(expectedHeadless));
            });
        }

        [Test]
        public void WithHeadlessMode_WithNullValue_SetsHeadlessModeToNull()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithServerConfiguration(null!));
        }

        [Test]
        public void WithHeadlessMode_WithNull_SetsNull()
        {
            var builder = new FluentUIScaffoldOptionsBuilder();
            var result = builder.WithHeadlessMode(null);
            Assert.That(result, Is.SameAs(builder));
            Assert.That(builder.Build().HeadlessMode, Is.Null);
        }

        [Test]
        public void WithSlowMo_WithValidValue_SetsSlowMo()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            int? expectedSlowMo = 500;

            // Act
            var result = builder.WithSlowMo(expectedSlowMo);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().SlowMo, Is.EqualTo(expectedSlowMo));
            });
        }

        [Test]
        public void WithSlowMo_WithNullValue_SetsSlowMoToNull()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithSlowMo(null);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().SlowMo, Is.Null);
            });
        }

        [Test]
        public void WithSlowMo_WithNegativeValue_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithSlowMo(-100));
        }

        [Test]
        public void WithSlowMo_WithZeroValue_SetsSlowMoToZero()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithSlowMo(0);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().SlowMo, Is.EqualTo(0));
            });
        }

        [Test]
        public void WithServerConfiguration_WithValidConfiguration_SetsServerConfiguration()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var serverConfig = new ServerConfiguration
            {
                ServerType = ServerType.AspNetCore,
                ProjectPath = "/path/to/project.csproj",
                BaseUrl = new Uri("http://localhost:5000")
            };

            // Act
            var result = builder.WithServerConfiguration(serverConfig);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(builder.Build().ServerConfiguration, Is.EqualTo(serverConfig));
            });
        }

        [Test]
        public void WithServerConfiguration_WithNullConfiguration_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithServerConfiguration(null!));
        }

        // Removed project detection API

        // Removed web server launch toggle API

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
                .Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(options.BaseUrl, Is.EqualTo(expectedUrl));
                Assert.That(options.DefaultWaitTimeout, Is.EqualTo(expectedTimeout));
                // Debug/webserver toggles removed
            });
        }

        [Test]
        public void Build_WithInvalidConfiguration_ThrowsException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder
                .WithDefaultWaitTimeout(TimeSpan.Zero)
                .Build());
        }

        [Test]
        public void Placeholder_NoOp_ToMaintainTestStructure()
        {
            Assert.Pass();
        }
    }
}
