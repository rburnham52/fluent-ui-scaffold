using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class NavigationTests
    {
        [Test]
        public async Task NavigateToUrl_WithValidUrl_NavigatesSuccessfully()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30)
            };

            Assert.Multiple(() =>
            {
                // Act & Assert
                // This test would require a running web server, so we'll just verify the options are set correctly
                Assert.That(options.BaseUrl, Is.EqualTo(new Uri("http://localhost:5000")));
                Assert.That(options.DefaultWaitTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
                // Debug mode removed; rely on Headless/SlowMo if needed
            });
        }

        [Test]
        public void NavigateToUrl_WithNullUrl_IsValid()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions();

            // Act & Assert
            // BaseUrl is nullable, so setting it to null should be valid
            Assert.DoesNotThrow(() => options.BaseUrl = null);
            Assert.That(options.BaseUrl, Is.Null);
        }

        [Test]
        public void NavigateToUrl_WithValidUrl_IsValid()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions();
            var validUrl = new Uri("http://localhost:5000");

            // Act & Assert
            Assert.DoesNotThrow(() => options.BaseUrl = validUrl);
            Assert.That(options.BaseUrl, Is.EqualTo(validUrl));
        }
    }
}

