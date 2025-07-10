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
    public class FluentUIScaffoldApp<TApp> where TApp : class
    {
        private readonly FluentUIScaffoldOptions _options;
        private readonly IUIDriver _driver;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly PluginManager _pluginManager;

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
            return _serviceProvider.GetService<TDriver>() ?? default!;
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
            var logger = serviceProvider.GetRequiredService<ILogger>();

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
            var logger = serviceProvider.GetRequiredService<ILogger>();

            return new FluentUIScaffoldApp<MobileApp>(options, driver, logger, serviceProvider);
        }

        private static void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton<IUIDriver, DefaultUIDriver>();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(options.LogLevel);
                builder.AddConsole();
            });
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
