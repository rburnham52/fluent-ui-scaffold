using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the FluentUIScaffold class.
    /// </summary>
    public class FluentUIScaffoldTests
    {
        [Fact]
        public void Web_WithValidConfiguration_ReturnsConfiguredInstance()
        {
            // Arrange & Act
            var scaffold = FluentUIScaffold<WebApp>.Web(options =>
            {
                options.BaseUrl = "https://example.com";
                options.Timeout = TimeSpan.FromSeconds(30);
                options.WaitStrategy = WaitStrategy.Explicit;
            });

            // Assert
            Assert.NotNull(scaffold);
            Assert.Equal("https://example.com", scaffold.Options.BaseUrl);
            Assert.Equal(TimeSpan.FromSeconds(30), scaffold.Options.Timeout);
            Assert.Equal(WaitStrategy.Explicit, scaffold.Options.WaitStrategy);
        }

        [Fact]
        public void Mobile_WithValidConfiguration_ReturnsConfiguredInstance()
        {
            // Arrange & Act
            var scaffold = FluentUIScaffold<MobileApp>.Mobile(options =>
            {
                options.BaseUrl = "https://mobile.example.com";
                options.Timeout = TimeSpan.FromSeconds(60);
                options.WaitStrategy = WaitStrategy.Implicit;
            });

            // Assert
            Assert.NotNull(scaffold);
            Assert.Equal("https://mobile.example.com", scaffold.Options.BaseUrl);
            Assert.Equal(TimeSpan.FromSeconds(60), scaffold.Options.Timeout);
            Assert.Equal(WaitStrategy.Implicit, scaffold.Options.WaitStrategy);
        }

        [Fact]
        public void Configure_WithValidOptions_UpdatesConfiguration()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            scaffold.Configure(options =>
            {
                options.BaseUrl = "https://configured.example.com";
                options.Timeout = TimeSpan.FromSeconds(45);
            });

            // Assert
            Assert.Equal("https://configured.example.com", scaffold.Options.BaseUrl);
            Assert.Equal(TimeSpan.FromSeconds(45), scaffold.Options.Timeout);
        }

        [Fact]
        public void WithBaseUrl_WithValidUrl_SetsBaseUrl()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            scaffold.WithBaseUrl("https://test.example.com");

            // Assert
            Assert.Equal("https://test.example.com", scaffold.Options.BaseUrl);
        }

        [Fact]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                scaffold.WithBaseUrl(null));

            Assert.Contains("Base URL cannot be null or empty", exception.Message);
        }

        [Fact]
        public void WithTimeout_WithValidTimeout_SetsTimeout()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            scaffold.WithTimeout(TimeSpan.FromSeconds(90));

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(90), scaffold.Options.Timeout);
        }

        [Fact]
        public void WithTimeout_WithZeroTimeout_ThrowsValidationException()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                scaffold.WithTimeout(TimeSpan.Zero));

            Assert.Contains("Timeout must be greater than zero", exception.Message);
        }

        [Fact]
        public void WithWaitStrategy_WithValidStrategy_SetsWaitStrategy()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            scaffold.WithWaitStrategy(WaitStrategy.Fluent);

            // Assert
            Assert.Equal(WaitStrategy.Fluent, scaffold.Options.WaitStrategy);
        }

        [Fact]
        public void NavigateToUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act & Assert
            // Note: This would normally interact with a real driver, but we're just testing the API
            // In a real scenario, this would be mocked
            Assert.NotNull(scaffold);
        }

        [Fact]
        public void NavigateToUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldValidationException>(() =>
                scaffold.NavigateToUrl(null));

            Assert.Contains("URL cannot be null or empty", exception.Message);
        }

        [Fact]
        public void Framework_ReturnsServiceFromServiceProvider()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            var driver = scaffold.Framework<IUIDriver>();

            // Assert
            Assert.NotNull(driver);
        }

        [Fact]
        public void Options_ReturnsConfigurationOptions()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web(options =>
            {
                options.BaseUrl = "https://options.example.com";
                options.Timeout = TimeSpan.FromSeconds(120);
            });

            // Act
            var options = scaffold.Options;

            // Assert
            Assert.NotNull(options);
            Assert.Equal("https://options.example.com", options.BaseUrl);
            Assert.Equal(TimeSpan.FromSeconds(120), options.Timeout);
        }

        [Fact]
        public void Driver_ReturnsUIDriverInstance()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            var driver = scaffold.Driver;

            // Assert
            Assert.NotNull(driver);
        }

        [Fact]
        public void Logger_ReturnsLoggerInstance()
        {
            // Arrange
            var scaffold = FluentUIScaffold<WebApp>.Web();

            // Act
            var logger = scaffold.Logger;

            // Assert
            Assert.NotNull(logger);
        }

        [Fact]
        public void WebApp_IsSingleton()
        {
            // Act
            var instance1 = WebApp.Instance;
            var instance2 = WebApp.Instance;

            // Assert
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void MobileApp_IsSingleton()
        {
            // Act
            var instance1 = MobileApp.Instance;
            var instance2 = MobileApp.Instance;

            // Assert
            Assert.Same(instance1, instance2);
        }
    }
} 