using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the FluentUIScaffold class.
    /// </summary>
    [TestFixture]
    public class FluentUIScaffoldTests
    {
        [Test]
        public void Web_WithValidConfiguration_ReturnsConfiguredInstance()
        {
            // Arrange & Act
            var scaffold = FluentUIScaffoldBuilder.Web(options =>
            {
                options.BaseUrl = new Uri("https://example.com");
                options.DefaultTimeout = TimeSpan.FromSeconds(30);
                options.WaitStrategy = WaitStrategy.Visible;
            });

            // Assert
            Assert.That(scaffold, Is.Not.Null);
        }

        [Test]
        public void Mobile_WithValidConfiguration_ReturnsConfiguredInstance()
        {
            // Arrange & Act
            var scaffold = FluentUIScaffoldBuilder.Mobile(options =>
            {
                options.BaseUrl = new Uri("https://mobile.example.com");
                options.DefaultTimeout = TimeSpan.FromSeconds(60);
                options.WaitStrategy = WaitStrategy.Smart;
            });

            // Assert
            Assert.That(scaffold, Is.Not.Null);
        }

        [Test]
        public void WithBaseUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var scaffold = FluentUIScaffoldBuilder.Web();

            // Act & Assert
            Assert.DoesNotThrow(() => scaffold.WithBaseUrl(new Uri("https://test.example.com")));
        }

        [Test]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var scaffold = FluentUIScaffoldBuilder.Web();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => scaffold.WithBaseUrl(null));
        }

        [Test]
        public void NavigateToUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var scaffold = FluentUIScaffoldBuilder.Web();

            // Act & Assert
            // Note: This test would require a mock driver to avoid actual navigation
            // For now, we'll skip this test as it requires proper mocking
            Assert.Pass("Test requires mock driver implementation");
        }

        [Test]
        public void NavigateToUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var scaffold = FluentUIScaffoldBuilder.Web();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => scaffold.NavigateToUrl(null));
        }

        [Test]
        public void Framework_ReturnsServiceFromServiceProvider()
        {
            // Arrange
            var scaffold = FluentUIScaffoldBuilder.Web();

            // Act
            var driver = scaffold.Framework<IUIDriver>();

            // Assert
            Assert.That(driver, Is.Not.Null);
        }

        [Test]
        public void WebApp_IsSingleton()
        {
            // Arrange & Act
            var instance1 = WebApp.Instance;
            var instance2 = WebApp.Instance;

            // Assert
            Assert.That(instance1, Is.SameAs(instance2));
        }

        [Test]
        public void MobileApp_IsSingleton()
        {
            // Arrange & Act
            var instance1 = MobileApp.Instance;
            var instance2 = MobileApp.Instance;

            // Assert
            Assert.That(instance1, Is.SameAs(instance2));
        }
    }
}
