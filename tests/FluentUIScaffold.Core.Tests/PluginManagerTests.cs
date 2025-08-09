using System;
using System.Collections.Generic;
using System.Linq;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Plugins;
using FluentUIScaffold.Core.Tests.Mocks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the PluginManager class.
    /// </summary>
    [TestFixture]
    public class PluginManagerTests
    {
        [Test]
        public void Constructor_WithLogger_CreatesInstance()
        {
            // Arrange
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PluginManager>();

            // Act
            var pluginManager = new PluginManager(logger);

            // Assert
            Assert.That(pluginManager, Is.Not.Null);
        }

        [Test]
        public void RegisterPlugin_WithValidPluginType_RegistersPlugin()
        {
            // Arrange
            var pluginManager = new PluginManager();

            // Act
            pluginManager.RegisterPlugin<TestPlugin>();

            // Assert
            var plugins = pluginManager.GetPlugins().ToList();
            Assert.That(plugins, Has.Count.EqualTo(1));
            Assert.That(plugins.First(), Is.InstanceOf<TestPlugin>());
        }

        [Test]
        public void RegisterPlugin_WithValidPluginInstance_RegistersPlugin()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var plugin = new TestPlugin();

            // Act
            pluginManager.RegisterPlugin(plugin);

            // Assert
            var plugins = pluginManager.GetPlugins().ToList();
            Assert.That(plugins, Has.Count.EqualTo(1));
            Assert.That(plugins.First(), Is.SameAs(plugin));
        }

        [Test]
        public void RegisterPlugin_WithNullPlugin_ThrowsArgumentNullException()
        {
            // Arrange
            var pluginManager = new PluginManager();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => pluginManager.RegisterPlugin((IUITestingFrameworkPlugin)null!));
        }

        [Test]
        public void RegisterPlugin_WithDuplicatePlugin_DoesNotAddDuplicate()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var plugin = new TestPlugin();

            // Act
            pluginManager.RegisterPlugin(plugin);
            pluginManager.RegisterPlugin(plugin);

            // Assert
            var plugins = pluginManager.GetPlugins().ToList();
            Assert.That(plugins, Has.Count.EqualTo(1));
        }

        [Test]
        public void GetPlugins_ReturnsReadOnlyCollection()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<TestPlugin>();

            // Act
            var plugins = pluginManager.GetPlugins();

            // Assert
            Assert.That(plugins, Is.Not.Null);
            Assert.That(plugins, Has.Count.EqualTo(1));
        }

        [Test]
        public void ValidatePlugins_WithValidPlugins_DoesNotThrow()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<TestPlugin>();

            // Act & Assert
            Assert.DoesNotThrow(() => pluginManager.ValidatePlugins());
        }

        [Test]
        public void ValidatePlugins_WithInvalidPlugin_ThrowsPluginException()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<InvalidTestPlugin>();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldPluginException>(() => pluginManager.ValidatePlugins());
        }

        [Test]
        public void GetPlugins_ReturnsRegisteredPlugins()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<TestPlugin>();
            pluginManager.RegisterPlugin<AnotherTestPlugin>();

            // Act
            var plugins = pluginManager.GetPlugins();

            // Assert
            Assert.That(plugins, Has.Count.EqualTo(2));
        }

        [Test]
        public void CreateDriver_WithValidOptions_ReturnsDriver()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<MockPlugin>();
            var options = new FluentUIScaffoldOptions();

            // Act
            var driver = pluginManager.CreateDriver(options);

            // Assert
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<MockUIDriver>());
        }

        [Test]
        public void CreateDriver_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var pluginManager = new PluginManager();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => pluginManager.CreateDriver(null!));
        }

        [Test]
        public void CreateDriver_WithNoPlugins_ThrowsFluentUIScaffoldPluginException()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var options = new FluentUIScaffoldOptions();

            // Act & Assert
            var exception = Assert.Throws<FluentUIScaffoldPluginException>(() => pluginManager.CreateDriver(options));
            Assert.That(exception.Message, Does.Contain("No UI testing framework plugins are configured"));
        }

        // Test plugin implementations
        private class TestPlugin : IUITestingFrameworkPlugin
        {
            public string Name => "TestPlugin";
            public string Version => "1.0.0";
            public IReadOnlyList<Type> SupportedDriverTypes => new List<Type> { typeof(MockUIDriver) };

            public bool CanHandle(Type driverType)
            {
                return driverType == typeof(MockUIDriver);
            }

            public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
            {
                return new MockUIDriver();
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient<IUIDriver, MockUIDriver>();
            }
        }

        private class AnotherTestPlugin : IUITestingFrameworkPlugin
        {
            public string Name => "AnotherTestPlugin";
            public string Version => "1.0.0";
            public IReadOnlyList<Type> SupportedDriverTypes => new List<Type> { typeof(MockUIDriver) };

            public bool CanHandle(Type driverType)
            {
                return driverType == typeof(MockUIDriver);
            }

            public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
            {
                return new MockUIDriver();
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient<IUIDriver, MockUIDriver>();
            }
        }

        private class InvalidTestPlugin : IUITestingFrameworkPlugin
        {
            public string Name => "InvalidTestPlugin";
            public string Version => "1.0.0";
            public IReadOnlyList<Type> SupportedDriverTypes => new List<Type>();

            public bool CanHandle(Type driverType)
            {
                return false;
            }

            public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
            {
                throw new FluentUIScaffoldPluginException("Invalid plugin");
            }

            public void ConfigureServices(IServiceCollection services)
            {
                throw new FluentUIScaffoldPluginException("Invalid plugin");
            }
        }
    }
}
