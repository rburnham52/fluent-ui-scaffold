using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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



        /// <summary>
        /// Creates a new FluentUIScaffoldApp instance with auto-discovery of plugins and pages.
        /// </summary>
        /// <param name="options">The configuration options</param>
        public FluentUIScaffoldApp(FluentUIScaffoldOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            // Build service collection with auto-discovery
            var services = new ServiceCollection();
            ConfigureServicesWithAutoDiscovery(services, options);

            _serviceProvider = services.BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<FluentUIScaffoldApp<TApp>>>();
            _pluginManager = _serviceProvider.GetRequiredService<PluginManager>();

            // Create driver using PluginManager
            _driver = _pluginManager.CreateDriver(_options);
        }

        /// <summary>
        /// Initializes the FluentUIScaffoldApp asynchronously, including web server launch if configured.
        /// </summary>
        /// <returns>A task that completes when initialization is done.</returns>
        public async Task InitializeAsync()
        {
            // Launch web server if configured
            if (_options.EnableWebServerLaunch && !string.IsNullOrEmpty(_options.WebServerProjectPath))
            {
                await LaunchWebServerAsync(_options.WebServerProjectPath);
            }
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
            var page = _serviceProvider.GetRequiredService<TPage>() ?? throw new InvalidOperationException($"Service of type {typeof(TPage).Name} is not registered.");

            // Use reflection to call Navigate() if the page has that method
            var navigateMethod = page.GetType().GetMethod("Navigate");
            if (navigateMethod != null)
            {
                navigateMethod.Invoke(page, null);
            }

            return page;
        }

        /// <summary>
        /// Launches a web server for testing if the driver supports it.
        /// </summary>
        /// <param name="projectPath">The path to the ASP.NET Core project to launch.</param>
        /// <returns>A task that completes when the web server is ready.</returns>
        public async Task LaunchWebServerAsync(string projectPath)
        {
            // Use reflection to check if the driver supports web server launching
            var driverType = _driver.GetType();
            var launchMethod = driverType.GetMethod("LaunchWebServerAsync", new[] { typeof(string) });

            if (launchMethod != null)
            {
                var result = launchMethod.Invoke(_driver, new object[] { projectPath });
                if (result is Task task)
                {
                    await task;
                }
                else if (result is ValueTask valueTask)
                {
                    await valueTask.AsTask();
                }
            }
            else
            {
                throw new InvalidOperationException($"Web server launching is not supported by the current driver. Driver type: {driverType.Name}");
            }
        }

        /// <summary>
        /// Waits for an element on the current page.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <param name="elementSelector">Function to select the element to wait for.</param>
        /// <returns>The current FluentUIScaffold instance</returns>
        public FluentUIScaffoldApp<TApp> WaitFor<TPage>(Func<TPage, IElement> elementSelector) where TPage : class, IPageComponent<IUIDriver, TPage>
        {
            var page = NavigateTo<TPage>();
            if (page is BasePageComponent<IUIDriver, TPage> basePage)
            {
                basePage.WaitForElement(elementSelector);
            }
            return this;
        }

        /// <summary>
        /// Waits for a page to be available.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <returns>The current FluentUIScaffold instance</returns>
        public FluentUIScaffoldApp<TApp> WaitFor<TPage>() where TPage : class, IPageComponent<IUIDriver, TPage>
        {
            var page = NavigateTo<TPage>();
            if (page is BasePageComponent<IUIDriver, TPage> basePage)
            {
                // Wait for the page to be ready (you can add custom logic here)
                basePage.ValidateCurrentPage();
            }
            return this;
        }

        /// <summary>
        /// Disposes the FluentUIScaffoldApp and its resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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

        /// <summary>
        /// Configures services with auto-discovery of plugins and pages.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">The configuration options</param>
        private static void ConfigureServicesWithAutoDiscovery(IServiceCollection services, FluentUIScaffoldOptions options)
        {
            // Register core services
            services.AddSingleton(options);
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(options.LogLevel);
                builder.AddConsole();
            });

            // Register ILogger (non-generic) for DI
            services.AddSingleton<ILogger>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("FluentUIScaffold");
            });

            // Register PluginManager
            services.AddSingleton<PluginManager>();

            // Auto-discover and register plugins
            AutoDiscoverPlugins(services);

            // Auto-discover and register pages
            AutoDiscoverPages(services, options);
        }

        /// <summary>
        /// Auto-discovers and registers plugins from loaded assemblies.
        /// </summary>
        /// <param name="services">The service collection</param>
        private static void AutoDiscoverPlugins(IServiceCollection services)
        {
            var pluginManager = new PluginManager();
            var successfulPlugins = new List<IUITestingFrameworkPlugin>();

            // Discover plugins from all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    // Skip test assemblies to avoid test plugins, but allow sample test assemblies
                    var assemblyName = assembly.GetName().Name ?? "";
                    if (assemblyName.Contains("Test") && !assemblyName.Contains("Sample"))
                    {
                        continue;
                    }

                    pluginManager.DiscoverPlugins(assembly);
                }
                catch (Exception ex)
                {
                    // Log warning but continue with other assemblies
                    Console.WriteLine($"Warning: Failed to discover plugins in assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }

            // Try to explicitly load Playwright plugin if not found
            try
            {
                var playwrightAssembly = assemblies.FirstOrDefault(a => a.GetName().Name?.Contains("Playwright") == true);
                if (playwrightAssembly != null)
                {
                    pluginManager.DiscoverPlugins(playwrightAssembly);
                }
            }
            catch (Exception ex)
            {
                // Log warning but continue
            }

            // Register discovered plugins with DI
            var plugins = pluginManager.GetPlugins();

            // Clear the plugin manager to rebuild it with only successful plugins
            pluginManager = new PluginManager();

            foreach (var plugin in plugins)
            {
                try
                {
                    // Skip plugins that are likely test plugins
                    if (plugin.GetType().Name.Contains("Test") || plugin.GetType().Name.Contains("Mock"))
                    {
                        continue;
                    }

                    services.AddSingleton(plugin.GetType(), plugin);
                    plugin.ConfigureServices(services);
                    pluginManager.RegisterPlugin(plugin);
                    successfulPlugins.Add(plugin);
                }
                catch (Exception ex)
                {
                    // Log warning but continue with other plugins
                }
            }

            // If no plugins were successfully registered, add a mock plugin for testing
            if (successfulPlugins.Count == 0)
            {
                try
                {
                    var mockPlugin = new MockPlugin();
                    services.AddSingleton<MockPlugin>(mockPlugin);
                    mockPlugin.ConfigureServices(services);
                    pluginManager.RegisterPlugin(mockPlugin);
                    successfulPlugins.Add(mockPlugin);
                }
                catch (Exception ex)
                {
                    // Log warning but continue
                }
            }

            services.AddSingleton(pluginManager);
        }

        /// <summary>
        /// Auto-discovers and registers pages from loaded assemblies.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">The configuration options</param>
        private static void AutoDiscoverPages(IServiceCollection services, FluentUIScaffoldOptions options)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    // Look for classes that inherit from BasePageComponent with any generic parameters
                    var pageTypes = assembly.GetTypes()
                        .Where(t =>
                            t.IsClass &&
                            !t.IsAbstract &&
                            !t.IsInterface &&
                            t.BaseType != null &&
                            t.BaseType.IsGenericType &&
                            t.BaseType.GetGenericTypeDefinition() == typeof(BasePageComponent<,>))
                        .ToList();

                    foreach (var pageType in pageTypes)
                    {
                        // Register page as transient
                        services.AddTransient(pageType, provider =>
                        {
                            // Determine URL pattern for the page
                            var urlPattern = DetermineUrlPattern(pageType, options.BaseUrl);
                            return Activator.CreateInstance(pageType, provider, urlPattern)
                                ?? throw new InvalidOperationException($"Failed to create instance of {pageType.Name}");
                        });
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
        /// Determines the URL pattern for a page type.
        /// </summary>
        /// <param name="pageType">The page type</param>
        /// <param name="baseUrl">The base URL</param>
        /// <returns>The URL pattern for the page</returns>
        private static Uri DetermineUrlPattern(Type pageType, Uri? baseUrl)
        {
            var baseUri = baseUrl ?? new Uri("http://localhost");

            // For sample applications, use the root URL since they're typically SPAs
            // This can be overridden by specific page implementations if needed
            return baseUri;
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
