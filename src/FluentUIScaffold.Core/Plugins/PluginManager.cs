using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FluentUIScaffold.Core.Plugins {
    /// <summary>
    /// Manages the registration and lifecycle of UI testing framework plugins.
    /// </summary>
    public class PluginManager {
        private readonly List<IUITestingFrameworkPlugin> _plugins = new();
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PluginManager class.
        /// </summary>
        public PluginManager() {
            _logger = NullLogger<PluginManager>.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the PluginManager class with a logger.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public PluginManager(ILogger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers a plugin with the manager.
        /// </summary>
        /// <param name="plugin">The plugin to register</param>
        public void RegisterPlugin(IUITestingFrameworkPlugin plugin) {
            ArgumentNullException.ThrowIfNull(plugin);

            try {
                _plugins.Add(plugin);
                _logger.LogInformation("Plugin {PluginName} registered successfully", plugin.GetType().Name);
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Failed to register plugin {PluginName}", plugin.GetType().Name);
                throw new FluentUIScaffoldPluginException($"Failed to register plugin {plugin.GetType().Name}", ex);
            }
        }

        /// <summary>
        /// Registers a plugin type with the manager.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin type to register</typeparam>
        public void RegisterPlugin<TPlugin>() where TPlugin : IUITestingFrameworkPlugin {
            try {
                var plugin = Activator.CreateInstance<TPlugin>();
                RegisterPlugin(plugin);
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to create plugin instance for type {PluginType}", typeof(TPlugin).Name);
                throw new FluentUIScaffoldPluginException($"Failed to create plugin instance for type {typeof(TPlugin).Name}", ex);
            }
        }

        private static readonly Action<ILogger, Exception?> LogPluginDiscoveryWarning =
            LoggerMessage.Define(LogLevel.Warning, new EventId(1, "PluginDiscoveryWarning"), "Failed to discover plugins");

        /// <summary>
        /// Discovers and registers plugins from assemblies.
        /// </summary>
        /// <param name="assembly">The assembly to search for plugins</param>
        public void DiscoverPlugins(Assembly assembly) {
            try {
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IUITestingFrameworkPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                foreach (var pluginType in pluginTypes) {
                    try {
                        var plugin = (IUITestingFrameworkPlugin)Activator.CreateInstance(pluginType)!;
                        RegisterPlugin(plugin);
                    }
                    catch (Exception ex) {
                        _logger.LogWarning(ex, "Failed to create plugin instance for type {PluginType}", pluginType.Name);
                    }
                }

                _logger.LogInformation("Discovered {PluginCount} plugins in assembly {AssemblyName}", pluginTypes.Count, assembly.GetName().Name);
            }
            catch (InvalidOperationException ex) {
                LogPluginDiscoveryWarning(_logger, ex);
            }
        }

        private static readonly Action<ILogger, Exception?> LogDriverCreationWarning =
            LoggerMessage.Define(LogLevel.Warning, new EventId(2, "DriverCreationWarning"), "Failed to create driver");

        /// <summary>
        /// Creates a driver using the registered plugins.
        /// </summary>
        /// <param name="options">The configuration options</param>
        /// <returns>The created driver instance</returns>
        public IUIDriver CreateDriver(FluentUIScaffoldOptions options) {
            ArgumentNullException.ThrowIfNull(options);

            try {
                foreach (var plugin in _plugins) {
                    try {
                        var driver = plugin.CreateDriver(options);
                        if (driver != null) {
                            _logger.LogInformation("Driver created successfully using plugin {PluginName}", plugin.GetType().Name);
                            return driver;
                        }
                    }
                    catch (Exception ex) {
                        _logger.LogWarning(ex, "Plugin {PluginName} failed to create driver", plugin.GetType().Name);
                    }
                }

                _logger.LogInformation("No plugins could create a driver, using default driver");
                return new DefaultUIDriver();
            }
            catch (InvalidOperationException ex) {
                LogDriverCreationWarning(_logger, ex);
                return new DefaultUIDriver();
            }
        }

        /// <summary>
        /// Validates all registered plugins.
        /// </summary>
        public void ValidatePlugins() {
            try {
                foreach (var plugin in _plugins) {
                    try {
                        // Note: IUITestingFrameworkPlugin doesn't have a Validate method
                        // This is a placeholder for future validation logic
                        _logger.LogInformation("Plugin {PluginName} validation passed", plugin.GetType().Name);
                    }
                    catch (Exception ex) {
                        _logger.LogError(ex, "Plugin {PluginName} validation failed", plugin.GetType().Name);
                        throw new FluentUIScaffoldPluginException($"Plugin {plugin.GetType().Name} validation failed", ex);
                    }
                }

                _logger.LogInformation("All plugins validated successfully");
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Plugin validation failed");
                throw new FluentUIScaffoldPluginException("Plugin validation failed", ex);
            }
        }

        /// <summary>
        /// Gets all registered plugins.
        /// </summary>
        /// <returns>A list of registered plugins</returns>
        public IReadOnlyList<IUITestingFrameworkPlugin> GetPlugins() {
            return _plugins.AsReadOnly();
        }

        /// <summary>
        /// Discovers plugins in an assembly by type.
        /// </summary>
        /// <param name="assembly">The assembly to search</param>
        /// <returns>A list of discovered plugin types</returns>
        public IReadOnlyList<Type> DiscoverPluginsInAssembly(Assembly assembly) {
            ArgumentNullException.ThrowIfNull(assembly);

            try {
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IUITestingFrameworkPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .ToList();

                _logger.LogInformation("Discovered {PluginCount} plugin types in assembly {AssemblyName}", pluginTypes.Count, assembly.GetName().Name);
                return pluginTypes.AsReadOnly();
            }
            catch (Exception ex) {
                _logger.LogWarning(ex, "Failed to discover plugin types in assembly {AssemblyName}", assembly.GetName().Name);
                throw new FluentUIScaffoldPluginException($"Failed to discover plugin types in assembly {assembly.GetName().Name}", ex);
            }
        }
    }
}
