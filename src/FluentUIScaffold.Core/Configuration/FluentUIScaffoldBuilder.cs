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

        // Options instance - registered in IOC but we keep a reference to allow configuration before build
        private FluentUIScaffoldOptions Options
        {
            get
            {
                // Find the registered options instance from the service collection
                var descriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(FluentUIScaffoldOptions));
                if (descriptor?.ImplementationInstance is FluentUIScaffoldOptions options)
                {
                    return options;
                }
                throw new InvalidOperationException("FluentUIScaffoldOptions not registered");
            }
        }

        public FluentUIScaffoldBuilder()
        {
            _services = new ServiceCollection();
            _services.AddLogging(); // Base logging support

            // Register options as singleton instance
            _services.AddSingleton(new FluentUIScaffoldOptions());
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

            // Configure the options instance that's already registered in the IOC
            configureOptions(Options);

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

            return RegisterHostingStrategy(new DotNetHostingStrategy(launchPlan));
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

            return RegisterHostingStrategy(new NodeHostingStrategy(launchPlan));
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
            var strategy = new ExternalHostingStrategy(
                baseUrl,
                healthCheckEndpoints.Length > 0 ? healthCheckEndpoints : null);

            return RegisterHostingStrategy(strategy);
        }

        private FluentUIScaffoldBuilder RegisterHostingStrategy(IHostingStrategy strategy)
        {
            // Register both the concrete type and the interface for flexibility
            _services.AddSingleton(strategy.GetType(), strategy);
            _services.AddSingleton<IHostingStrategy>(strategy);

            // Add startup action to start the hosting strategy
            AddStartupAction(async (provider) =>
            {
                var logger = provider.GetRequiredService<ILogger<FluentUIScaffoldBuilder>>();
                var hostingStrategy = provider.GetRequiredService<IHostingStrategy>();

                var result = await hostingStrategy.StartAsync(logger);

                // Update options with discovered base URL
                var options = provider.GetRequiredService<FluentUIScaffoldOptions>();
                options.BaseUrl = result.BaseUrl;
            });

            return this;
        }

        #endregion

        #region Plugin Methods

        /// <summary>
        /// Registers a plugin instance for this builder. Only one plugin can be registered.
        /// </summary>
        /// <param name="plugin">The plugin to register.</param>
        public FluentUIScaffoldBuilder UsePlugin(IUITestingFrameworkPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            // Register the plugin by its concrete type and interface
            _services.AddSingleton(plugin.GetType(), plugin);
            _services.AddSingleton<IUITestingFrameworkPlugin>(plugin);

            // Let the plugin configure its services
            try
            {
                plugin.ConfigureServices(_services);
            }
            catch
            {
                // Allow plugin participation even if DI wiring throws
            }

            return this;
        }

        /// <summary>
        /// Registers a plugin by type for this builder. Only one plugin can be registered.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin type with a parameterless constructor.</typeparam>
        public FluentUIScaffoldBuilder UsePlugin<TPlugin>()
            where TPlugin : IUITestingFrameworkPlugin, new()
        {
            return UsePlugin(new TPlugin());
        }

        #endregion

        #region Page Discovery Methods

        /// <summary>
        /// Enables automatic discovery and registration of page components from loaded assemblies.
        /// Discovers all classes that inherit from Page&lt;TSelf&gt;.
        /// </summary>
        public FluentUIScaffoldBuilder WithAutoPageDiscovery()
        {
            Options.AutoDiscoverPages = true;
            return this;
        }

        /// <summary>
        /// Explicitly registers a page type for dependency injection.
        /// Use this when not using auto-discovery or to ensure specific pages are registered.
        /// </summary>
        /// <typeparam name="TPage">The page type to register.</typeparam>
        public FluentUIScaffoldBuilder RegisterPage<TPage>() where TPage : class
        {
            RegisterPageType(typeof(TPage));
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
            // Process any plugins added via options.Plugins (backward compatibility with options.UsePlaywright())
            // Only register if not already registered via UsePlugin()
            var existingPlugin = _services.FirstOrDefault(d => d.ServiceType == typeof(IUITestingFrameworkPlugin));
            if (existingPlugin == null)
            {
                var plugin = Options.Plugins.FirstOrDefault();
                if (plugin != null)
                {
                    _services.AddSingleton(plugin.GetType(), plugin);
                    _services.AddSingleton<IUITestingFrameworkPlugin>(plugin);

                    try
                    {
                        plugin.ConfigureServices(_services);
                    }
                    catch
                    {
                        // Allow plugin participation even if DI wiring throws
                    }
                }
            }

            // Auto-discover pages if enabled
            if (Options.AutoDiscoverPages)
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
