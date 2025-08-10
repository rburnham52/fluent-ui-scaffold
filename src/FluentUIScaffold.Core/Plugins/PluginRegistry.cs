using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FluentUIScaffold.Core.Interfaces;

namespace FluentUIScaffold.Core.Plugins
{
    /// <summary>
    /// Global registry for UI testing framework plugins. Allows explicit registration
    /// via code, auto-registration via module initializers, and last-chance discovery
    /// across loaded assemblies.
    /// </summary>
    public static class PluginRegistry
    {
        private static readonly object SyncLock = new object();
        private static readonly List<IUITestingFrameworkPlugin> RegisteredPlugins = new();

        /// <summary>
        /// Registers a plugin instance globally. Duplicate types are ignored.
        /// </summary>
        public static void Register(IUITestingFrameworkPlugin plugin)
        {
            ArgumentNullException.ThrowIfNull(plugin);
            lock (SyncLock)
            {
                if (RegisteredPlugins.Any(p => p.GetType() == plugin.GetType()))
                {
                    return;
                }
                RegisteredPlugins.Add(plugin);
            }
        }

        /// <summary>
        /// Registers a plugin type globally.
        /// </summary>
        public static void Register<TPlugin>() where TPlugin : IUITestingFrameworkPlugin, new()
        {
            Register(new TPlugin());
        }

        /// <summary>
        /// Gets all registered plugins.
        /// </summary>
        public static IReadOnlyCollection<IUITestingFrameworkPlugin> GetAll()
        {
            lock (SyncLock)
            {
                return RegisteredPlugins.ToArray();
            }
        }

        /// <summary>
        /// Clears all registered plugins. Intended for test isolation only.
        /// </summary>
        public static void ClearForTests()
        {
            lock (SyncLock)
            {
                RegisteredPlugins.Clear();
            }
        }

        /// <summary>
        /// Discovers plugins from currently loaded assemblies and registers them.
        /// </summary>
        public static void DiscoverFromLoadedAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var pluginTypes = assembly.GetTypes()
                        .Where(t => typeof(IUITestingFrameworkPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .ToList();

                    foreach (var pluginType in pluginTypes)
                    {
                        try
                        {
                            if (Activator.CreateInstance(pluginType) is IUITestingFrameworkPlugin plugin)
                            {
                                Register(plugin);
                            }
                        }
                        catch
                        {
                            // Ignore type activation failures and continue discovery
                        }
                    }
                }
                catch
                {
                    // Ignore assembly inspection errors and continue
                }
            }
        }
    }
}


