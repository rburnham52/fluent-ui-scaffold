using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class NavigationTests
    {
        private FluentUIScaffoldApp<WebApp> _fluentUI;

        [SetUp]
        public void Setup()
        {
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://test.example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(5),
                LogLevel = LogLevel.Information,
                HeadlessMode = true
            };

            _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
        }

        [TearDown]
        public void Cleanup()
        {
            _fluentUI?.Dispose();
        }

        [Test]
        public void NavigateToUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() => _fluentUI.NavigateToUrl(new Uri("https://test.example.com")));
        }

        [Test]
        public void NavigateToUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(
                () => _fluentUI.NavigateToUrl(null!));

            Assert.That(exception.Message, Does.Contain("URL cannot be null"));
        }

        [Test]
        public void NavigateToUrl_WithRelativePath_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() => _fluentUI.NavigateToUrl(new Uri("/test-path", UriKind.Relative)));
        }

        [Test]
        public void NavigateToUrl_WithQueryParameters_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() => _fluentUI.NavigateToUrl(new Uri("https://test.example.com/search?q=test&category=all")));
        }

        [Test]
        public void NavigateToUrl_WithComplexPath_DoesNotThrow()
        {
            // Arrange & Act & Assert
            Assert.DoesNotThrow(() => _fluentUI.NavigateToUrl(new Uri("https://test.example.com/profile/123/dashboard")));
        }
    }
}
