using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using FluentUIScaffold.Core.Plugins;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Exceptions;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for the PluginManager class.
    /// </summary>
    public class PluginManagerTests
    {
        [Fact]
        public void Constructor_WithLogger_CreatesInstance()
        {
            // Arrange
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PluginManager>();

            // Act
            var pluginManager = new PluginManager(logger);

            // Assert
            Assert.NotNull(pluginManager);
        }

        [Fact]
        public void RegisterPlugin_WithValidPluginType_RegistersPlugin()
        {
            // Arrange
            var pluginManager = new PluginManager();

            // Act
            pluginManager.RegisterPlugin<TestPlugin>();

            // Assert
            var plugins = pluginManager.GetPlugins().ToList();
            Assert.Single(plugins);
            Assert.IsType<TestPlugin>(plugins.First());
        }

        [Fact]
        public void RegisterPlugin_WithValidPluginInstance_RegistersPlugin()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var plugin = new TestPlugin();

            // Act
            pluginManager.RegisterPlugin(plugin);

            // Assert
            var plugins = pluginManager.GetPlugins().ToList();
            Assert.Single(plugins);
            Assert.Same(plugin, plugins.First());
        }

        [Fact]
        public void RegisterPlugin_WithNullPlugin_ThrowsArgumentNullException()
        {
            // Arrange
            var pluginManager = new PluginManager();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => pluginManager.RegisterPlugin((IUITestingFrameworkPlugin)null));
        }

        [Fact]
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
            Assert.Single(plugins);
        }

        [Fact]
        public void GetPlugins_ReturnsReadOnlyCollection()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<TestPlugin>();

            // Act
            var plugins = pluginManager.GetPlugins();

            // Assert
            Assert.NotNull(plugins);
            Assert.Single(plugins);
        }

        [Fact]
        public void ValidatePlugins_WithValidPlugins_ReturnsTrue()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<TestPlugin>();

            // Act
            var isValid = pluginManager.ValidatePlugins();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidatePlugins_WithInvalidPlugin_ReturnsFalse()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<InvalidTestPlugin>();

            // Act
            var isValid = pluginManager.ValidatePlugins();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void ClearPlugins_RemovesAllPlugins()
        {
            // Arrange
            var pluginManager = new PluginManager();
            pluginManager.RegisterPlugin<TestPlugin>();
            pluginManager.RegisterPlugin<AnotherTestPlugin>();

            // Act
            pluginManager.ClearPlugins();

            // Assert
            var plugins = pluginManager.GetPlugins().ToList();
            Assert.Empty(plugins);
        }

        [Fact]
        public void CreateDriver_WithValidDriverType_ReturnsDriver()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var options = new FluentUIScaffoldOptions();

            // Act
            var driver = pluginManager.CreateDriver(typeof(DefaultUIDriver), options);

            // Assert
            Assert.NotNull(driver);
            Assert.IsType<DefaultUIDriver>(driver);
        }

        [Fact]
        public void CreateDriver_WithNullDriverType_ThrowsArgumentNullException()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var options = new FluentUIScaffoldOptions();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => pluginManager.CreateDriver(null, options));
        }

        [Fact]
        public void CreateDriver_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var pluginManager = new PluginManager();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => pluginManager.CreateDriver(typeof(DefaultUIDriver), null));
        }

        [Fact]
        public void CreateDriver_WithInvalidDriverType_ThrowsPluginException()
        {
            // Arrange
            var pluginManager = new PluginManager();
            var options = new FluentUIScaffoldOptions();

            // Act & Assert
            Assert.Throws<FluentUIScaffoldPluginException>(() => 
                pluginManager.CreateDriver(typeof(string), options));
        }

        // Test plugin implementations
        private class TestPlugin : IUITestingFrameworkPlugin
        {
            public IUIDriver CreateDriver(Type driverType, FluentUIScaffoldOptions options)
            {
                if (driverType == typeof(DefaultUIDriver))
                {
                    return new DefaultUIDriver();
                }
                return null;
            }

            public void Validate()
            {
                // Valid plugin - no validation errors
            }
        }

        private class AnotherTestPlugin : IUITestingFrameworkPlugin
        {
            public IUIDriver CreateDriver(Type driverType, FluentUIScaffoldOptions options)
            {
                return null;
            }

            public void Validate()
            {
                // Valid plugin - no validation errors
            }
        }

        private class InvalidTestPlugin : IUITestingFrameworkPlugin
        {
            public IUIDriver CreateDriver(Type driverType, FluentUIScaffoldOptions options)
            {
                return null;
            }

            public void Validate()
            {
                throw new InvalidOperationException("This plugin is invalid");
            }
        }
    }
} 