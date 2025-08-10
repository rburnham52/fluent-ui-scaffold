using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Tests.Mocks;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the FluentUIScaffold class using the new auto-discovery pattern.
    /// </summary>
    [TestFixture]
    public class FluentUIScaffoldTests
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // Ensure a plugin is available for driver creation in these unit tests
            FluentUIScaffoldBuilder.UsePlugin(new MockPlugin());
        }

        [Test]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30)
            };

            // Act
            var app = new FluentUIScaffoldApp<object>(options);

            // Assert
            Assert.That(app, Is.Not.Null);
        }

        [Test]
        public void Constructor_WithNullOptions_ThrowsException()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FluentUIScaffoldApp<object>(null!));
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

        // Removed debug/extra timeout API; covered by Headless/SlowMo

        [Test]
        public void WithWebServerProjectPath_WithValidPath_SetsWebServerProjectPath()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions();

            // Act
            options.WebServerProjectPath = "/path/to/project.csproj";

            // Assert
            Assert.That(options.WebServerProjectPath, Is.EqualTo("/path/to/project.csproj"));
        }

        [Test]
        public void HeadlessMode_WithValidValue_SetsHeadlessMode()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            bool? expectedHeadless = false;

            // Act
            var result = builder.WithHeadlessMode(expectedHeadless);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.HeadlessMode, Is.EqualTo(expectedHeadless));
            });
        }

        [Test]
        public void HeadlessMode_WithNullValue_SetsHeadlessModeToNull()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithHeadlessMode(null);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.HeadlessMode, Is.Null);
            });
        }

        [Test]
        public void SlowMo_WithValidValue_SetsSlowMo()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            int? expectedSlowMo = 500;

            // Act
            var result = builder.WithSlowMo(expectedSlowMo);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.SlowMo, Is.EqualTo(expectedSlowMo));
            });
        }

        [Test]
        public void SlowMo_WithNullValue_SetsSlowMoToNull()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithSlowMo(null);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.SlowMo, Is.Null);
            });
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
        public void Placeholder_NoOp_ToMaintainTestStructure()
        {
            Assert.Pass();
        }
    }
}
