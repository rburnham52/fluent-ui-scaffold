using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Hosting;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

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
        private readonly FluentUIScaffoldOptions _options;
        private readonly List<Func<IServiceProvider, Task>> _startupActions = new();
        private bool _hostingStrategyRegistered;

        public FluentUIScaffoldBuilder()
        {
            _services = new ServiceCollection();
            _services.AddLogging(); // Base logging support

            // Store options as field and register the same instance in DI
            _options = new FluentUIScaffoldOptions();
            _services.AddSingleton(_options);
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
            configureOptions(_options);
            return this;
        }

        #region Environment Configuration Methods

        /// <summary>
        /// Sets the logical environment name (e.g., "Testing", "Development", "Staging").
        /// Default is "Testing". "Production" is rejected as a safety guard.
        /// </summary>
        public FluentUIScaffoldBuilder WithEnvironmentName(string environmentName)
        {
            if (string.IsNullOrWhiteSpace(environmentName))
                throw new ArgumentException("Environment name cannot be null or empty.", nameof(environmentName));
            if (string.Equals(environmentName.Trim(), "Production", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    "Setting environment to 'Production' is not allowed for test scaffolding. " +
                    "Use 'Testing', 'Development', or 'Staging' instead.");
            _options.EnvironmentName = environmentName;
            return this;
        }

        /// <summary>
        /// Enables or disables the ASP.NET SPA dev server proxy.
        /// Default is false (disabled for testing).
        /// </summary>
        public FluentUIScaffoldBuilder WithSpaProxy(bool enabled)
        {
            _options.SpaProxyEnabled = enabled;
            return this;
        }

        /// <summary>
        /// Sets the headless mode for browser automation.
        /// When null, resolved at Build() time: debugger attached = visible, otherwise headless.
        /// </summary>
        public FluentUIScaffoldBuilder WithHeadlessMode(bool? headless)
        {
            _options.HeadlessMode = headless;
            return this;
        }

        /// <summary>
        /// Adds a custom environment variable to be passed to hosted applications.
        /// User-set variables override framework defaults.
        /// Rejects dangerous keys that could compromise process security.
        /// </summary>
        public FluentUIScaffoldBuilder WithEnvironmentVariable(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Environment variable key cannot be null or empty.", nameof(key));

            if (IsDangerousEnvironmentKey(key))
                throw new ArgumentException(
                    $"Environment variable key '{key}' is blocked because it can alter process loading behavior.", nameof(key));

            _options.EnvironmentVariables[key] = value;
            return this;
        }

        private static readonly string[] DangerousEnvironmentKeys = new[]
        {
            "LD_PRELOAD",
            "LD_LIBRARY_PATH",
            "DYLD_INSERT_LIBRARIES",
            "DYLD_LIBRARY_PATH",
            "DYLD_FRAMEWORK_PATH",
            "PATH",
            "COMSPEC"
        };

        private static bool IsDangerousEnvironmentKey(string key)
        {
            foreach (var dangerous in DangerousEnvironmentKeys)
            {
                if (string.Equals(key, dangerous, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion

        #region Hosting Strategy Methods

        /// <summary>
        /// Configures hosting via 'dotnet run' for .NET applications.
        /// </summary>
        /// <param name="configure">Configuration action for DotNet-specific hosting options.</param>
        public FluentUIScaffoldBuilder UseDotNetHosting(Action<DotNetHostingOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var hostingOptions = new DotNetHostingOptions();
            configure(hostingOptions);

            if (hostingOptions.BaseUrl == null)
                throw new ArgumentException("BaseUrl is required for DotNet hosting.", nameof(configure));
            if (string.IsNullOrWhiteSpace(hostingOptions.ProjectPath))
                throw new ArgumentException("ProjectPath is required for DotNet hosting.", nameof(configure));

            return RegisterHostingStrategy(
                sp => new DotNetHostingStrategy(hostingOptions, sp.GetRequiredService<FluentUIScaffoldOptions>()));
        }

        /// <summary>
        /// Configures hosting via 'npm run' for Node.js applications.
        /// </summary>
        /// <param name="configure">Configuration action for Node-specific hosting options.</param>
        public FluentUIScaffoldBuilder UseNodeHosting(Action<NodeHostingOptions> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var hostingOptions = new NodeHostingOptions();
            configure(hostingOptions);

            if (hostingOptions.BaseUrl == null)
                throw new ArgumentException("BaseUrl is required for Node hosting.", nameof(configure));
            if (string.IsNullOrWhiteSpace(hostingOptions.ProjectPath))
                throw new ArgumentException("ProjectPath is required for Node hosting.", nameof(configure));

            return RegisterHostingStrategy(
                sp => new NodeHostingStrategy(hostingOptions, sp.GetRequiredService<FluentUIScaffoldOptions>()));
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

            return RegisterHostingStrategy(_ => strategy);
        }

        /// <summary>
        /// Marks a hosting strategy as registered. Used by extension methods (e.g., Aspire)
        /// that register strategies externally.
        /// </summary>
        public void SetHostingStrategyRegistered()
        {
            if (_hostingStrategyRegistered)
                throw new InvalidOperationException(
                    "A hosting strategy is already registered. Only one hosting strategy can be configured per builder.");
            _hostingStrategyRegistered = true;
        }

        private FluentUIScaffoldBuilder RegisterHostingStrategy(Func<IServiceProvider, IHostingStrategy> factory)
        {
            SetHostingStrategyRegistered();

            _services.AddSingleton<IHostingStrategy>(factory);

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
            plugin.ConfigureServices(_services);

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
            _options.AutoDiscoverPages = true;
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
        /// Pages can use the [Route] attribute to specify their path, which is combined with BaseUrl.
        /// </summary>
        private void RegisterPageType(Type pageType)
        {
            _services.AddTransient(pageType, provider =>
            {
                var options = provider.GetService<FluentUIScaffoldOptions>();
                var baseUrl = options?.BaseUrl ?? new Uri("http://localhost");

                // Check for [Route] attribute to get the page's route path
                var routeAttribute = pageType.GetCustomAttributes(typeof(Pages.RouteAttribute), inherit: true)
                    .OfType<Pages.RouteAttribute>()
                    .FirstOrDefault();

                // Combine base URL with route path
                Uri pageUrl;
                if (routeAttribute != null && !string.IsNullOrEmpty(routeAttribute.Path))
                {
                    var path = routeAttribute.Path;
                    var baseUrlString = baseUrl.ToString().TrimEnd('/');
                    var routePath = path.StartsWith("/") ? path : "/" + path;
                    pageUrl = new Uri(baseUrlString + routePath);
                }
                else
                {
                    pageUrl = baseUrl;
                }

                return Activator.CreateInstance(pageType, provider, pageUrl)
                    ?? throw new InvalidOperationException($"Failed to create instance of {pageType.Name}");
            });
        }

        #endregion

        /// <summary>
        /// Builds the AppScaffold with all configured services and startup actions.
        /// Resolves HeadlessMode if not explicitly set.
        /// </summary>
        public AppScaffold<TWebApp> Build<TWebApp>()
        {
            // Resolve HeadlessMode: explicit > debugger attached (visible) > default (headless)
            if (_options.HeadlessMode == null)
            {
                _options.HeadlessMode = !System.Diagnostics.Debugger.IsAttached;
            }

            // Auto-discover pages if enabled
            if (_options.AutoDiscoverPages)
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
