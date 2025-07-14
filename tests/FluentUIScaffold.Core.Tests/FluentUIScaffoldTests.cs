using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Plugins;

using Microsoft.Extensions.DependencyInjection;
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
            var scaffold = CreateScaffoldWithMockPlugin(options =>
            {
                options.BaseUrl = new Uri("https://example.com");
                options.DefaultTimeout = TimeSpan.FromSeconds(30);
                options.WaitStrategy = WaitStrategy.Visible;
            });

            // Assert
            Assert.That(scaffold, Is.Not.Null);
        }

        [Test]
        public void Web_WithFrameworkConfiguration_ReturnsConfiguredInstance()
        {
            // Arrange & Act
            var scaffold = CreateScaffoldWithMockPlugin(options =>
            {
                options.BaseUrl = new Uri("https://example.com");
                options.DefaultTimeout = TimeSpan.FromSeconds(30);
                options.WaitStrategy = WaitStrategy.Visible;
            });

            // Assert
            Assert.That(scaffold, Is.Not.Null);
        }

        [Test]
        public void WithBaseUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var scaffold = CreateScaffoldWithMockPlugin(options => { });

            // Act & Assert
            Assert.DoesNotThrow(() => scaffold.WithBaseUrl(new Uri("https://test.example.com")));
        }

        [Test]
        public void WithBaseUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var scaffold = CreateScaffoldWithMockPlugin(options => { });

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => scaffold.WithBaseUrl(null!));
        }

        [Test]
        public void NavigateToUrl_WithValidUrl_DoesNotThrow()
        {
            // Arrange
            var scaffold = CreateScaffoldWithMockPlugin(options => { });

            // Act & Assert
            Assert.DoesNotThrow(() => scaffold.NavigateToUrl(new Uri("https://test.example.com")));
        }

        [Test]
        public void NavigateToUrl_WithNullUrl_ThrowsValidationException()
        {
            // Arrange
            var scaffold = CreateScaffoldWithMockPlugin(options => { });

            // Act & Assert
            Assert.Throws<FluentUIScaffoldValidationException>(() => scaffold.NavigateToUrl(null!));
        }

        [Test]
        public void Framework_ReturnsServiceFromServiceProvider()
        {
            // Arrange
            var scaffold = CreateScaffoldWithMockPlugin(options => { });

            // Act
            var driver = scaffold.Framework<MockUIDriver>();

            // Assert
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<MockUIDriver>());
        }

        private static FluentUIScaffoldApp<WebApp> CreateScaffoldWithMockPlugin(Action<FluentUIScaffoldOptions> configureOptions)
        {
            // Create services manually to register MockPlugin
            var services = new ServiceCollection();

            // Register core services
            var options = new FluentUIScaffoldOptions();
            configureOptions?.Invoke(options);
            services.AddSingleton(options);

            // Register logging
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(options.LogLevel);
                builder.AddConsole();
            });

            // Register ILogger
            services.AddSingleton<ILogger>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("FluentUIScaffold");
            });

            // Register PluginManager
            services.AddSingleton<PluginManager>();

            // Register MockPlugin
            services.AddSingleton<MockPlugin>();
            var mockPlugin = new MockPlugin();
            mockPlugin.ConfigureServices(services);

            var serviceProvider = services.BuildServiceProvider();

            // Register MockPlugin with PluginManager
            var pluginManager = serviceProvider.GetRequiredService<PluginManager>();
            pluginManager.RegisterPlugin(mockPlugin);

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FluentUIScaffold");

            // Use reflection to access the internal constructor
            var constructor = typeof(FluentUIScaffoldApp<WebApp>).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(IServiceProvider), typeof(ILogger) },
                null);

            return (FluentUIScaffoldApp<WebApp>)constructor!.Invoke(new object[] { serviceProvider, logger });
        }
    }
}
