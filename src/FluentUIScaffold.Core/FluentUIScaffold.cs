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

        internal FluentUIScaffoldApp(FluentUIScaffoldOptions options, IUIDriver driver, ILogger logger, IServiceProvider serviceProvider)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _pluginManager = new PluginManager(logger);
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
            return _serviceProvider.GetRequiredService<TDriver>();
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
        public static FluentUIScaffoldApp<WebApp> Web(Action<FluentUIScaffoldOptions>? configureOptions = null)
        {
            var options = new FluentUIScaffoldOptions();
            configureOptions?.Invoke(options);

            var services = new ServiceCollection();
            ConfigureServices(services, options);
            var serviceProvider = services.BuildServiceProvider();

            var driver = serviceProvider.GetRequiredService<IUIDriver>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FluentUIScaffold");

            return new FluentUIScaffoldApp<WebApp>(options, driver, logger, serviceProvider);
        }

        public static FluentUIScaffoldApp<MobileApp> Mobile(Action<FluentUIScaffoldOptions>? configureOptions = null)
        {
            var options = new FluentUIScaffoldOptions();
            configureOptions?.Invoke(options);

            var services = new ServiceCollection();
            ConfigureServices(services, options);
            var serviceProvider = services.BuildServiceProvider();

            var driver = serviceProvider.GetRequiredService<IUIDriver>();
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FluentUIScaffold");

            return new FluentUIScaffoldApp<MobileApp>(options, driver, logger, serviceProvider);
        }

        private static void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options)
        {
            services.AddSingleton(options);

            // Register Playwright plugin if available
            try
            {
                var playwrightPluginType = Type.GetType("FluentUIScaffold.Playwright.PlaywrightPlugin, FluentUIScaffold.Playwright");
                if (playwrightPluginType != null)
                {
                    var playwrightDriverType = Type.GetType("FluentUIScaffold.Playwright.PlaywrightDriver, FluentUIScaffold.Playwright");
                    if (playwrightDriverType != null)
                    {
                        services.AddSingleton(typeof(IUIDriver), playwrightDriverType);
                    }
                    else
                    {
                        services.AddSingleton<IUIDriver, DefaultUIDriver>();
                    }
                }
                else
                {
                    services.AddSingleton<IUIDriver, DefaultUIDriver>();
                }
            }
            catch
            {
                services.AddSingleton<IUIDriver, DefaultUIDriver>();
            }

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(options.LogLevel);
                builder.AddConsole();
            });
            // Register ILogger (non-generic) for DI
            services.AddSingleton<ILogger>(provider => provider.GetRequiredService<ILoggerFactory>().CreateLogger("FluentUIScaffold"));
            // Register page objects for DI if available
            var sampleAppPagesAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "SampleApp.Tests");
            if (sampleAppPagesAssembly != null)
            {
                var homePageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.HomePage");
                var todosPageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.TodosPage");
                var profilePageType = sampleAppPagesAssembly.GetType("SampleApp.Tests.Pages.ProfilePage");
                if (homePageType != null) services.AddTransient(homePageType);
                if (todosPageType != null) services.AddTransient(todosPageType);
                if (profilePageType != null) services.AddTransient(profilePageType);
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
