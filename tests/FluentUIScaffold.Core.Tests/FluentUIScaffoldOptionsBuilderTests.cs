using System;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the FluentUIScaffoldOptionsBuilder class.
    /// </summary>
    public class FluentUIScaffoldOptionsBuilderTests
    {
        [Fact]
        public void Constructor_WithNoParameters_CreatesInstance()
        {
            // Act
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void Constructor_WithExistingOptions_CreatesInstance()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions();

            // Act
            var builder = new FluentUIScaffoldOptionsBuilder(options);

            // Assert
            Assert.NotNull(builder);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new FluentUIScaffoldOptionsBuilder(null));
        }

        [Fact]
        public void WithBaseUrl_WithValidUrl_SetsBaseUrl()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithBaseUrl("https://test.example.com");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithBaseUrl(null));

            Assert.Contains("Base URL cannot be null or empty", exception.Message);
        }

        [Fact]
        public void WithBaseUrl_WithEmptyUrl_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithBaseUrl(""));

            Assert.Contains("Base URL cannot be null or empty", exception.Message);
        }

        [Fact]
        public void WithTimeout_WithValidTimeout_SetsTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(30);

            // Act
            var result = builder.WithTimeout(timeout);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithTimeout(TimeSpan.Zero));

            Assert.Contains("Timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithTimeout_WithNegativeTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithTimeout(TimeSpan.FromSeconds(-1)));

            Assert.Contains("Timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithRetryInterval_WithValidInterval_SetsRetryInterval()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var interval = TimeSpan.FromSeconds(5);

            // Act
            var result = builder.WithRetryInterval(interval);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithRetryInterval_WithZeroInterval_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithRetryInterval(TimeSpan.Zero));

            Assert.Contains("Retry interval must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithWaitStrategy_WithValidStrategy_SetsWaitStrategy()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithWaitStrategy(WaitStrategy.Explicit);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithLogLevel_WithValidLevel_SetsLogLevel()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithLogLevel(LogLevel.Information);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithScreenshotPath_WithValidPath_SetsScreenshotPath()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithScreenshotPath("/path/to/screenshots");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithScreenshotPath_WithNullPath_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithScreenshotPath(null));

            Assert.Contains("Screenshot path cannot be null or empty", exception.Message);
        }

        [Fact]
        public void WithFrameworkOption_WithValidKeyValue_SetsFrameworkOption()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithFrameworkOption("testKey", "testValue");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithFrameworkOption_WithNullKey_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithFrameworkOption(null, "value"));

            Assert.Contains("Framework option key cannot be null or empty", exception.Message);
        }

        [Fact]
        public void WithDriver_WithValidDriverType_SetsFrameworkType()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithDriver<TestDriver>();

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithPageValidationStrategy_WithValidStrategy_SetsPageValidationStrategy()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithPageValidationStrategy(PageValidationStrategy.Url);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithAutomaticScreenshots_WithEnabled_SetsAutomaticScreenshots()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithAutomaticScreenshots(true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithHeadlessMode_WithEnabled_SetsHeadlessMode()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithHeadlessMode(true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithWindowSize_WithValidDimensions_SetsWindowSize()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithWindowSize(1920, 1080);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithWindowSize_WithZeroWidth_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithWindowSize(0, 1080));

            Assert.Contains("Window width must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithWindowSize_WithZeroHeight_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithWindowSize(1920, 0));

            Assert.Contains("Window height must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithImplicitWaits_WithEnabled_SetsImplicitWaits()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithImplicitWaits(true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithDefaultWaitTimeout_WithValidTimeout_SetsDefaultWaitTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(10);

            // Act
            var result = builder.WithDefaultWaitTimeout(timeout);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithDefaultWaitTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithDefaultWaitTimeout(TimeSpan.Zero));

            Assert.Contains("Default wait timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithRetryCount_WithValidCount_SetsRetryCount()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithRetryCount(3);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithRetryCount_WithNegativeCount_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithRetryCount(-1));

            Assert.Contains("Retry count must be non-negative", exception.Message);
        }

        [Fact]
        public void WithDetailedLogging_WithEnabled_SetsDetailedLogging()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithDetailedLogging(true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithUserAgent_WithValidUserAgent_SetsUserAgent()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithUserAgent("Custom User Agent");

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithUserAgent_WithNullUserAgent_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithUserAgent(null));

            Assert.Contains("User agent cannot be null or empty", exception.Message);
        }

        [Fact]
        public void WithJavaScriptEnabled_WithEnabled_SetsJavaScriptEnabled()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithJavaScriptEnabled(true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithAcceptInsecureCertificates_WithEnabled_SetsAcceptInsecureCertificates()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithAcceptInsecureCertificates(true);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithPageLoadTimeout_WithValidTimeout_SetsPageLoadTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(60);

            // Act
            var result = builder.WithPageLoadTimeout(timeout);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithPageLoadTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithPageLoadTimeout(TimeSpan.Zero));

            Assert.Contains("Page load timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithScriptTimeout_WithValidTimeout_SetsScriptTimeout()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            var timeout = TimeSpan.FromSeconds(30);

            // Act
            var result = builder.WithScriptTimeout(timeout);

            // Assert
            Assert.Same(builder, result);
        }

        [Fact]
        public void WithScriptTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                builder.WithScriptTimeout(TimeSpan.Zero));

            Assert.Contains("Script timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void Build_WithValidConfiguration_ReturnsOptions()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl("https://test.example.com")
                .WithTimeout(TimeSpan.FromSeconds(30))
                .WithRetryInterval(TimeSpan.FromSeconds(5))
                .WithWaitStrategy(WaitStrategy.Explicit)
                .WithLogLevel(LogLevel.Information)
                .WithScreenshotPath("/path/to/screenshots")
                .WithAutomaticScreenshots(true)
                .WithHeadlessMode(true)
                .WithWindowSize(1920, 1080)
                .WithImplicitWaits(true)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(10))
                .WithRetryCount(3)
                .WithDetailedLogging(true)
                .WithUserAgent("Custom User Agent")
                .WithJavaScriptEnabled(true)
                .WithAcceptInsecureCertificates(true)
                .WithPageLoadTimeout(TimeSpan.FromSeconds(60))
                .WithScriptTimeout(TimeSpan.FromSeconds(30));

            // Act
            var options = builder.Build();

            // Assert
            Assert.NotNull(options);
            Assert.Equal("https://test.example.com", options.BaseUrl);
            Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
            Assert.Equal(TimeSpan.FromSeconds(5), options.RetryInterval);
            Assert.Equal(WaitStrategy.Explicit, options.WaitStrategy);
            Assert.Equal(LogLevel.Information, options.LogLevel);
            Assert.Equal("/path/to/screenshots", options.ScreenshotPath);
            Assert.True(options.AutomaticScreenshots);
            Assert.True(options.HeadlessMode);
            Assert.Equal(1920, options.WindowWidth);
            Assert.Equal(1080, options.WindowHeight);
            Assert.True(options.ImplicitWaits);
            Assert.Equal(TimeSpan.FromSeconds(10), options.DefaultWaitTimeout);
            Assert.Equal(3, options.RetryCount);
            Assert.True(options.DetailedLogging);
            Assert.Equal("Custom User Agent", options.UserAgent);
            Assert.True(options.JavaScriptEnabled);
            Assert.True(options.AcceptInsecureCertificates);
            Assert.Equal(TimeSpan.FromSeconds(60), options.PageLoadTimeout);
            Assert.Equal(TimeSpan.FromSeconds(30), options.ScriptTimeout);
        }

        [Fact]
        public void Build_WithInvalidConfiguration_ThrowsValidationException()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder()
                .WithTimeout(TimeSpan.Zero); // Invalid timeout

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() => builder.Build());
            Assert.Contains("Configuration validation failed", exception.Message);
        }

        // Test driver class for testing
        private class TestDriver
        {
        }
    }
} 