using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Hosting;
using FluentUIScaffold.Core.Interfaces;

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

        private IUITestingPlugin _plugin;

        #region Plugin Methods

        /// <summary>
        /// Registers a new-style plugin instance. Only one plugin can be registered.
        /// </summary>
        public FluentUIScaffoldBuilder UsePlugin(IUITestingPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            _plugin = plugin;

            // Register the plugin in DI
            _services.AddSingleton(plugin.GetType(), plugin);
            _services.AddSingleton<IUITestingPlugin>(plugin);

            // Let the plugin configure its services
            plugin.ConfigureServices(_services);

            return this;
        }


        #endregion

        /// <summary>
        /// Builds the AppScaffold with all configured services and startup actions.
        /// Resolves HeadlessMode if not explicitly set.
        /// </summary>
        public AppScaffold<TWebApp> Build<TWebApp>()
        {
            if (_plugin == null)
                throw new InvalidOperationException(
                    "No UI testing plugin configured. Call UsePlugin() before Build().");

            // Resolve HeadlessMode: explicit > debugger attached (visible) > default (headless)
            if (_options.HeadlessMode == null)
            {
                _options.HeadlessMode = !System.Diagnostics.Debugger.IsAttached;
            }

            var sp = _services.BuildServiceProvider();

            // Create a composite startup task
            Func<IServiceProvider, Task> startupAction = async (provider) =>
            {
                foreach (var action in _startupActions)
                {
                    await action(provider).ConfigureAwait(false);
                }
            };

            return new AppScaffold<TWebApp>(sp, startupAction, _plugin);
        }
    }
}
