using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Hosting;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Plugins;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Main entry point for configuring the FluentUIScaffold.
    /// Provides a fluent API for setting up hosting strategies, plugins, and services.
    /// </summary>
    public partial class FluentUIScaffoldBuilder
    {
        private readonly IServiceCollection _services;
        private readonly List<Func<IServiceProvider, Task>> _startupActions = new();
        private readonly List<IUITestingFrameworkPlugin> _plugins = new();
        private readonly List<Type> _registeredPageTypes = new();
        private FluentUIScaffoldOptions? _options;
        private IHostingStrategy? _hostingStrategy;
        private bool _autoDiscoverPages;

        public FluentUIScaffoldBuilder()
        {
            _services = new ServiceCollection();
            _services.AddLogging(); // Base logging support
        }

        /// <summary>
        /// Configures services directly.
        /// </summary>
        public FluentUIScaffoldBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure(_services);
            return this;
        }

        /// <summary>
        /// Register a startup action to be executed when StartAsync is called on the AppScaffold.
        /// </summary>
        public FluentUIScaffoldBuilder AddStartupAction(Func<IServiceProvider, Task> action)
        {
            _startupActions.Add(action);
            return this;
        }

        /// <summary>
        /// Configures the generic web application options.
        /// </summary>
        public FluentUIScaffoldBuilder Web<TWebApp>(Action<FluentUIScaffoldOptions> configureOptions)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            // Create and configure options immediately so plugins can wire up DI before the provider is built.
            var options = new FluentUIScaffoldOptions();
            configureOptions(options);
            _options = options;

            // Register the options instance for consumers such as drivers or hosting helpers.
            _services.AddSingleton(_options);

            // Let any configured plugins register their service dependencies (e.g. Playwright IPage).
            if (_options.Plugins != null)
            {
                foreach (var plugin in _options.Plugins)
                {
                    if (plugin == null) continue;

                    // Make the plugin itself available from DI if needed.
                    _services.AddSingleton(plugin.GetType(), plugin);

                    try
                    {
                        plugin.ConfigureServices(_services);
                    }
                    catch
                    {
                        // Match core behavior: allow plugin participation even if DI wiring throws.
                    }
                }
            }

            return this;
        }

        #region Hosting Strategy Methods

        /// <summary>
        /// Configures hosting via 'dotnet run' for .NET applications.
        /// </summary>
        /// <param name="baseUrl">The base URL where the application will be accessible.</param>
        /// <param name="projectPath">Path to the .csproj file.</param>
        /// <param name="configure">Optional configuration action for DotNet-specific options.</param>
        public FluentUIScaffoldBuilder UseDotNetHosting(
            Uri baseUrl,
            string projectPath,
            Action<DotNetServerConfigurationBuilder>? configure = null)
        {
            var builder = ServerConfiguration.CreateDotNetServer(baseUrl, projectPath);
            configure?.Invoke(builder);
            var launchPlan = builder.Build();

            _hostingStrategy = new DotNetHostingStrategy(launchPlan);
            RegisterHostingStrategy();

            return this;
        }

        /// <summary>
        /// Configures hosting via 'npm run' for Node.js applications.
        /// </summary>
        /// <param name="baseUrl">The base URL where the application will be accessible.</param>
        /// <param name="projectPath">Path to the directory containing package.json.</param>
        /// <param name="configure">Optional configuration action for Node-specific options.</param>
        public FluentUIScaffoldBuilder UseNodeHosting(
            Uri baseUrl,
            string projectPath,
            Action<NodeJsServerConfigurationBuilder>? configure = null)
        {
            var builder = ServerConfiguration.CreateNodeJsServer(baseUrl, projectPath);
            configure?.Invoke(builder);
            var launchPlan = builder.Build();

            _hostingStrategy = new NodeHostingStrategy(launchPlan);
            RegisterHostingStrategy();

            return this;
        }

        /// <summary>
        /// Configures hosting for an externally managed server (CI, staging, production).
        /// Only performs health checking - does not manage any processes.
        /// </summary>
        /// <param name="baseUrl">The base URL of the external server.</param>
        /// <param name="healthCheckEndpoints">Optional health check endpoints to verify. Defaults to "/".</param>
        public FluentUIScaffoldBuilder UseExternalServer(
            Uri baseUrl,
            params string[] healthCheckEndpoints)
        {
            _hostingStrategy = new ExternalHostingStrategy(
                baseUrl,
                healthCheckEndpoints.Length > 0 ? healthCheckEndpoints : null);
            RegisterHostingStrategy();

            return this;
        }

        private void RegisterHostingStrategy()
        {
            if (_hostingStrategy == null) return;

            _services.AddSingleton(_hostingStrategy);

            // Add startup action to start the hosting strategy
            AddStartupAction(async (provider) =>
            {
                var logger = provider.GetRequiredService<ILogger<FluentUIScaffoldBuilder>>();
                var strategy = provider.GetRequiredService<IHostingStrategy>();

                var result = await strategy.StartAsync(logger);

                // Update options with discovered base URL
                var options = provider.GetService<FluentUIScaffoldOptions>();
                if (options != null)
                {
                    options.BaseUrl = result.BaseUrl;
                }
            });
        }

        #endregion

        #region Plugin Methods

        /// <summary>
        /// Registers a plugin instance for this builder.
        /// </summary>
        /// <param name="plugin">The plugin to register.</param>
        public FluentUIScaffoldBuilder UsePlugin(IUITestingFrameworkPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            _plugins.Add(plugin);
            return this;
        }

        /// <summary>
        /// Registers a plugin by type for this builder.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin type with a parameterless constructor.</typeparam>
        public FluentUIScaffoldBuilder UsePlugin<TPlugin>()
            where TPlugin : IUITestingFrameworkPlugin, new()
        {
            _plugins.Add(new TPlugin());
            return this;
        }

        #endregion

        #region Page Discovery Methods

        /// <summary>
        /// Enables automatic discovery and registration of page components from loaded assemblies.
        /// Discovers all classes that inherit from BasePageComponent.
        /// </summary>
        public FluentUIScaffoldBuilder WithAutoPageDiscovery()
        {
            _autoDiscoverPages = true;
            return this;
        }

        /// <summary>
        /// Explicitly registers a page type for dependency injection.
        /// Use this when not using auto-discovery or to ensure specific pages are registered.
        /// </summary>
        /// <typeparam name="TPage">The page type to register.</typeparam>
        public FluentUIScaffoldBuilder RegisterPage<TPage>() where TPage : class
        {
            _registeredPageTypes.Add(typeof(TPage));
            return this;
        }

        /// <summary>
        /// Auto-discovers and registers pages from loaded assemblies.
        /// Discovers all classes that inherit from Page&lt;TSelf&gt;.
        /// </summary>
        private void AutoDiscoverPages()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var pageTypes = assembly.GetTypes()
                        .Where(t =>
                            t.IsClass &&
                            !t.IsAbstract &&
                            !t.IsInterface &&
                            t.BaseType != null &&
                            t.BaseType.IsGenericType &&
                            t.BaseType.GetGenericTypeDefinition() == typeof(Page<>))
                        .ToList();

                    foreach (var pageType in pageTypes)
                    {
                        RegisterPageType(pageType);
                    }
                }
                catch (Exception ex)
                {
                    // Log warning but continue with other assemblies
                    Console.WriteLine($"Warning: Failed to discover pages in assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Registers a page type with the service collection.
        /// </summary>
        private void RegisterPageType(Type pageType)
        {
            _services.AddTransient(pageType, provider =>
            {
                // Determine URL pattern for the page
                var options = provider.GetService<FluentUIScaffoldOptions>();
                var urlPattern = options?.BaseUrl ?? new Uri("http://localhost");
                return Activator.CreateInstance(pageType, provider, urlPattern)
                    ?? throw new InvalidOperationException($"Failed to create instance of {pageType.Name}");
            });
        }

        #endregion

        /// <summary>
        /// Builds the AppScaffold with all configured services and startup actions.
        /// </summary>
        public AppScaffold<TWebApp> Build<TWebApp>()
        {
            // Ensure options exist
            _options ??= new FluentUIScaffoldOptions();

            // Copy plugins from builder to options
            foreach (var plugin in _plugins)
            {
                _options.Plugins.Add(plugin);
            }

            // Register options
            _services.AddSingleton(_options);

            // Register plugins and configure their services
            var pluginManager = new PluginManager();
            foreach (var plugin in _options.Plugins)
            {
                if (plugin == null) continue;

                pluginManager.RegisterPlugin(plugin);
                _services.AddSingleton(plugin.GetType(), plugin);

                try
                {
                    plugin.ConfigureServices(_services);
                }
                catch
                {
                    // Allow plugin participation even if DI wiring throws
                }
            }
            _services.AddSingleton(pluginManager);

            // Register explicitly added pages
            foreach (var pageType in _registeredPageTypes)
            {
                RegisterPageType(pageType);
            }

            // Auto-discover pages if enabled
            if (_autoDiscoverPages)
            {
                AutoDiscoverPages();
            }

            var sp = _services.BuildServiceProvider();

            // Create a composite startup task
            Func<IServiceProvider, Task> startupAction = async (provider) =>
            {
                foreach (var action in _startupActions)
                {
                    await action(provider);
                }
            };

            return new AppScaffold<TWebApp>(sp, startupAction);
        }
    }
}
