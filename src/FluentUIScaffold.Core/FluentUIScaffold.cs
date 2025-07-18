using System;
using System.Collections.Generic;
using System.Linq;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Plugins;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FluentUIScaffold.Core
{
    /// <summary>
    /// Main entry point for the FluentUIScaffold testing framework.
    /// Provides a fluent API for configuring and executing UI tests.
    /// </summary>
    /// <typeparam name="TApp">The type of application being tested (WebApp, MobileApp, etc.)</typeparam>
    public class FluentUIScaffoldApp<TApp> : IDisposable where TApp : class
    {
        private readonly FluentUIScaffoldOptions _options;
        private readonly IUIDriver _driver;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PluginManager _pluginManager;
        private bool _disposed = false;

        internal FluentUIScaffoldApp(IServiceProvider serviceProvider, IUIDriver driver, ILogger logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
            _pluginManager = serviceProvider.GetRequiredService<PluginManager>();
        }

        /// <summary>
        /// Creates a new FluentUIScaffoldApp instance with proper driver initialization.
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="logger">The logger</param>
        internal FluentUIScaffoldApp(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
            _pluginManager = serviceProvider.GetRequiredService<PluginManager>();

            // Create driver using PluginManager
            _driver = _pluginManager.CreateDriver(_options);
        }

        /// <summary>
        /// Configures the base URL for the application.
        /// </summary>
        /// <param name="baseUrl">The base URL</param>
        /// <returns>The current FluentUIScaffold instance</returns>
        public FluentUIScaffoldApp<TApp> WithBaseUrl(Uri baseUrl)
        {
            if (baseUrl == null)
                throw new FluentUIScaffoldValidationException("Base URL cannot be null", nameof(baseUrl));
            _options.BaseUrl = baseUrl;
            return this;
        }

        /// <summary>
        /// Navigates to a specific URL.
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        /// <returns>The current FluentUIScaffold instance</returns>
        public FluentUIScaffoldApp<TApp> NavigateToUrl(Uri url)
        {
            if (url == null)
                throw new FluentUIScaffoldValidationException("URL cannot be null", nameof(url));
            _driver.NavigateToUrl(url);
            return this;
        }

        /// <summary>
        /// Gets a framework-specific driver instance.
        /// </summary>
        /// <typeparam name="TDriver">The driver type</typeparam>
        /// <returns>The driver instance</returns>
        public TDriver Framework<TDriver>() where TDriver : class
        {
            return _serviceProvider.GetRequiredService<TDriver>() ?? throw new InvalidOperationException($"Service of type {typeof(TDriver).Name} is not registered.");
        }

        /// <summary>
        /// Navigates to a page component of the specified type.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <returns>The page component instance.</returns>
        public TPage NavigateTo<TPage>() where TPage : class
        {
            return _serviceProvider.GetRequiredService<TPage>() ?? throw new InvalidOperationException($"Service of type {typeof(TPage).Name} is not registered.");
        }

        /// <summary>
        /// Disposes the FluentUIScaffoldApp and its resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the FluentUIScaffoldApp and its resources.
        /// </summary>
        /// <param name="disposing">True if called from Dispose; false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Dispose managed resources
                if (_driver is IDisposable disposableDriver)
                {
                    disposableDriver.Dispose();
                }

                if (_serviceProvider is IDisposable disposableServiceProvider)
                {
                    disposableServiceProvider.Dispose();
                }

                _disposed = true;
            }
        }
    }

    public static class FluentUIScaffoldBuilder
    {
        public static FluentUIScaffoldApp<TApp> Web<TApp>(
            Action<FluentUIScaffoldOptions> configureOptions,
            Action<FrameworkOptions>? configureFramework = null)
            where TApp : class
        {
            var options = new FluentUIScaffoldOptions();
            configureOptions?.Invoke(options);

            var services = new ServiceCollection();
            ConfigureServices(services, options, configureFramework);
            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FluentUIScaffold");

            return new FluentUIScaffoldApp<TApp>(serviceProvider, logger);
        }

        private static void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options, Action<FrameworkOptions>? configureFramework = null)
        {
            // Register core services
            services.AddSingleton(options);
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(options.LogLevel);
                builder.AddConsole();
            });

            // Register ILogger (non-generic) for DI - must be registered before pages are instantiated
            services.AddSingleton<ILogger>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("FluentUIScaffold");
            });

            // Configure framework options if provided
            if (configureFramework != null)
            {
                // User must provide a concrete FrameworkOptions instance
                // (e.g., PlaywrightOptions, SeleniumOptions) via DI in their own code
                // So we do not instantiate FrameworkOptions here
                // Optionally, you could throw or log if configureFramework is not null but not supported
            }

            // Register PluginManager
            services.AddSingleton<PluginManager>();

            // Note: IUIDriver registration is now handled by plugins
            // The framework will throw an exception if no plugins are configured
            // This ensures explicit plugin configuration is required

            // Register pages with their URL patterns
            RegisterPages(services, options);
        }

        private static void RegisterPages(IServiceCollection services, FluentUIScaffoldOptions options)
        {
            // Register pages with their URL patterns
            var sampleAppPagesAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "SampleApp.Tests");
            if (sampleAppPagesAssembly != null)
            {
                // Register existing pages with URL patterns
                var homePageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.HomePage");
                var todosPageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.TodosPage");
                var profilePageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.ProfilePage");

                if (homePageType != null)
                {
                    services.AddTransient(homePageType, provider =>
                        Activator.CreateInstance(homePageType, provider, options.BaseUrl ?? new Uri("http://localhost")) ?? throw new InvalidOperationException($"Failed to create instance of {homePageType.Name}"));
                }

                if (todosPageType != null)
                {
                    services.AddTransient(todosPageType, provider =>
                        Activator.CreateInstance(todosPageType, provider, options.BaseUrl ?? new Uri("http://localhost")) ?? throw new InvalidOperationException($"Failed to create instance of {todosPageType.Name}"));
                }

                if (profilePageType != null)
                {
                    services.AddTransient(profilePageType, provider =>
                        Activator.CreateInstance(profilePageType, provider, options.BaseUrl ?? new Uri("http://localhost")) ?? throw new InvalidOperationException($"Failed to create instance of {profilePageType.Name}"));
                }

                // Register new pages with URL patterns
                var registrationPageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.RegistrationPage");
                var loginPageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.LoginPage");

                if (registrationPageType != null)
                {
                    services.AddTransient(registrationPageType, provider =>
                        Activator.CreateInstance(registrationPageType, provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/register")) ?? throw new InvalidOperationException($"Failed to create instance of {registrationPageType.Name}"));
                }

                if (loginPageType != null)
                {
                    services.AddTransient(loginPageType, provider =>
                        Activator.CreateInstance(loginPageType, provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/login")) ?? throw new InvalidOperationException($"Failed to create instance of {loginPageType.Name}"));
                }
            }
        }
    }

    /// <summary>
    /// Marker class for web applications.
    /// </summary>
    public sealed class WebApp
    {
        public static readonly WebApp Instance = new WebApp();
        private WebApp() { }
    }

    /// <summary>
    /// Marker class for mobile applications.
    /// </summary>
    public sealed class MobileApp
    {
        public static readonly MobileApp Instance = new MobileApp();
        private MobileApp() { }
    }
}
