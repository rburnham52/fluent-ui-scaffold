using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the FluentUIScaffoldOptionsBuilder class.
    /// </summary>
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
            Assert.Throws<ArgumentNullException>(() => new FluentUIScaffoldOptionsBuilder(null));
        }

        [Test]
        public void WithBaseUrl_WithValidUrl_SetsBaseUrl()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithBaseUrl(new Uri("https://test.example.com"));

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithBaseUrl(null));
        }

        [Test]
        public void WithTimeout_WithValidTimeout_SetsTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(30);

            // Act
            var result = builder.WithTimeout(timeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => builder.WithTimeout(TimeSpan.Zero));
        }

        [Test]
        public void WithRetryInterval_WithValidInterval_SetsRetryInterval()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var interval = TimeSpan.FromSeconds(5);

            // Act
            var result = builder.WithRetryInterval(interval);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithWaitStrategy_WithValidStrategy_SetsWaitStrategy()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithWaitStrategy(WaitStrategy.Visible);

            // Assert
            Assert.That(result, Is.SameAs(builder));
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
        }

        [Test]
        public void WithScreenshotPath_WithValidPath_SetsScreenshotPath()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithScreenshotPath("/path/to/screenshots");

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithFrameworkOption_WithValidKeyValue_SetsFrameworkOption()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithFrameworkOption("testKey", "testValue");

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithPageValidationStrategy_WithValidStrategy_SetsPageValidationStrategy()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithPageValidationStrategy(PageValidationStrategy.Always);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithAutomaticScreenshots_WithEnabled_SetsAutomaticScreenshots()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithAutomaticScreenshots(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
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
        }

        [Test]
        public void WithWindowSize_WithValidDimensions_SetsWindowSize()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithWindowSize(1920, 1080);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithImplicitWaits_WithEnabled_SetsImplicitWaits()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithImplicitWaits(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithDefaultWaitTimeout_WithValidTimeout_SetsDefaultWaitTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(10);

            // Act
            var result = builder.WithDefaultWaitTimeout(timeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithRetryCount_WithValidCount_SetsRetryCount()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithRetryCount(3);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithDetailedLogging_WithEnabled_SetsDetailedLogging()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithDetailedLogging(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithUserAgent_WithValidUserAgent_SetsUserAgent()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithUserAgent("Test User Agent");

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithJavaScriptEnabled_WithEnabled_SetsJavaScriptEnabled()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithJavaScriptEnabled(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithAcceptInsecureCertificates_WithEnabled_SetsAcceptInsecureCertificates()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithAcceptInsecureCertificates(true);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithPageLoadTimeout_WithValidTimeout_SetsPageLoadTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(30);

            // Act
            var result = builder.WithPageLoadTimeout(timeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void WithScriptTimeout_WithValidTimeout_SetsScriptTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(30);

            // Act
            var result = builder.WithScriptTimeout(timeout);

            // Assert
            Assert.That(result, Is.SameAs(builder));
        }

        [Test]
        public void Build_WithValidConfiguration_ReturnsOptions()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var options = builder
                .WithBaseUrl(new Uri("https://test.example.com"))
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithWaitStrategy(WaitStrategy.Visible)
                .WithLogLevel(LogLevel.Information)
                .Build();

            // Assert
            Assert.That(options, Is.Not.Null);
            Assert.That(options.BaseUrl, Is.EqualTo(new Uri("https://test.example.com")));
            Assert.That(options.DefaultTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
            Assert.That(options.WaitStrategy, Is.EqualTo(WaitStrategy.Visible));
            Assert.That(options.LogLevel, Is.EqualTo(LogLevel.Information));
        }
    }
}
