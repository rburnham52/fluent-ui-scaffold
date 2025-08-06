using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
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
        [Test]
        public void Constructor_WithValidOptions_CreatesInstanceWithAutoDiscovery()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                HeadlessMode = true
            };

            // Act
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Assert
            Assert.That(fluentUIApp, Is.Not.Null);
            Assert.That(fluentUIApp, Is.InstanceOf<FluentUIScaffoldApp<WebApp>>());
        }

        [Test]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FluentUIScaffoldApp<WebApp>(null!));
        }

        [Test]
        public void WithBaseUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Act & Assert
            Assert.DoesNotThrow(() => fluentUIApp.WithBaseUrl(new Uri("https://test.example.com")));
        }

        [Test]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => fluentUIApp.WithBaseUrl(null!));
        }

        [Test]
        public void NavigateToUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Act & Assert
            Assert.DoesNotThrow(() => fluentUIApp.NavigateToUrl(new Uri("https://test.example.com")));
        }

        [Test]
        public void NavigateToUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => fluentUIApp.NavigateToUrl(null!));
        }

        [Test]
        public void Framework_ReturnsServiceFromServiceProvider()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Act
            var driver = fluentUIApp.Framework<MockUIDriver>();

            // Assert
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<MockUIDriver>());
        }

        [Test]
        public void Constructor_WithAutoDiscovery_RegistersMockPlugin()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };

            // Act
            var fluentUIApp = new FluentUIScaffoldApp<WebApp>(options);

            // Assert
            Assert.That(fluentUIApp, Is.Not.Null);
            // The auto-discovery should have registered the MockPlugin as a fallback
            Assert.DoesNotThrow(() => fluentUIApp.Framework<MockUIDriver>());
        }

        [Test]
        public void Constructor_WithAutoDiscovery_HandlesExceptionsGracefully()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information
            };

            // Act & Assert - Should not throw even if auto-discovery encounters issues
            Assert.DoesNotThrow(() => new FluentUIScaffoldApp<WebApp>(options));
        }
    }
}
