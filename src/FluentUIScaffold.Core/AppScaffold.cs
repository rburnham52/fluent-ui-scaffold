using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Hosting;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core
{
    /// <summary>
    /// The central hub for test infrastructure. Manages hosting, plugin lifecycle,
    /// and per-test browser session creation.
    /// </summary>
    public class AppScaffold<TWebApp> : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, Task> _startAction;
        private readonly IUITestingPlugin _plugin;
        private bool _isStarted;
        private bool _isDisposed;
        private IHostingStrategy? _hostingStrategy;

        // Instance field for session tracking. The AppScaffold instance is shared
        // across test lifecycle methods (TestInitialize/TestMethod/TestCleanup),
        // so a simple instance field is reliable regardless of thread scheduling.
        // For parallel test execution, consider a ConcurrentDictionary keyed by test ID.
        private IBrowserSession? _currentSession;

        public AppScaffold(
            IServiceProvider serviceProvider,
            Func<IServiceProvider, Task> startAction,
            IUITestingPlugin plugin)
        {
            _serviceProvider = serviceProvider;
            _startAction = startAction;
            _plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        /// <summary>
        /// Gets the root service provider.
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Starts hosting strategies and initializes the plugin (launches browser).
        /// Call once during assembly/class setup.
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_isStarted) return;

            await _startAction(_serviceProvider).ConfigureAwait(false);

            // Cache the hosting strategy for disposal later
            _hostingStrategy = _serviceProvider.GetService<IHostingStrategy>();

            var options = _serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
            await _plugin.InitializeAsync(options, cancellationToken).ConfigureAwait(false);

            _isStarted = true;
        }

        /// <summary>
        /// Creates an isolated browser session for the current test.
        /// Stores the session in AsyncLocal for per-test tracking.
        /// Call in [TestInitialize] / [SetUp].
        /// </summary>
        public async Task<IBrowserSession> CreateSessionAsync()
        {
            var session = await _plugin.CreateSessionAsync(_serviceProvider).ConfigureAwait(false);
            _currentSession = session;
            return session;
        }

        /// <summary>
        /// Disposes the current test's browser session.
        /// Call in [TestCleanup] / [TearDown].
        /// </summary>
        public async Task DisposeSessionAsync()
        {
            var session = _currentSession;
            if (session != null)
            {
                _currentSession = null;
                await session.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Navigates to a page. Creates the page via the session provider,
        /// enqueues navigation to the page's route, and returns the page
        /// for fluent chaining.
        /// </summary>
        public TPage NavigateTo<TPage>() where TPage : Page<TPage>
        {
            var session = _currentSession
                ?? throw new InvalidOperationException(
                    "No browser session is active. Call CreateSessionAsync() in [TestInitialize] before navigating.");

            var page = (TPage)(Activator.CreateInstance(
                typeof(TPage),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                args: new object[] { session.ServiceProvider },
                culture: null)
                ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TPage).Name}."));

            // Enqueue navigation to the page's route
            EnqueueNavigation(page, session);

            return page;
        }

        /// <summary>
        /// Navigates to a page with route parameters.
        /// </summary>
        public TPage NavigateTo<TPage>(object routeParams) where TPage : Page<TPage>
        {
            if (routeParams == null) throw new ArgumentNullException(nameof(routeParams));

            var session = _currentSession
                ?? throw new InvalidOperationException(
                    "No browser session is active. Call CreateSessionAsync() in [TestInitialize] before navigating.");

            var page = (TPage)(Activator.CreateInstance(
                typeof(TPage),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                args: new object[] { session.ServiceProvider },
                culture: null)
                ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TPage).Name}."));

            // Enqueue navigation with route parameters
            EnqueueNavigationWithParams(page, session, routeParams);

            return page;
        }

        /// <summary>
        /// Resolves a page without navigating.
        /// Use when you're already on the page and just need the page object.
        /// </summary>
        public TPage On<TPage>() where TPage : Page<TPage>
        {
            var session = _currentSession
                ?? throw new InvalidOperationException(
                    "No browser session is active. Call CreateSessionAsync() in [TestInitialize] before navigating.");

            return (TPage)(Activator.CreateInstance(
                typeof(TPage),
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
                binder: null,
                args: new object[] { session.ServiceProvider },
                culture: null)
                ?? throw new InvalidOperationException($"Failed to create instance of {typeof(TPage).Name}."));
        }

        /// <summary>
        /// Resolve a dependency from the internal service provider.
        /// </summary>
        public T GetService<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            // Dispose any active session
            var session = _currentSession;
            if (session != null)
            {
                _currentSession = null;
                await session.DisposeAsync().ConfigureAwait(false);
            }

            // Dispose plugin (closes browser)
            await _plugin.DisposeAsync().ConfigureAwait(false);

            // Stop any hosting strategies
            if (_hostingStrategy != null)
            {
                try
                {
                    await _hostingStrategy.DisposeAsync().ConfigureAwait(false);
                }
                catch (ObjectDisposedException) { }
            }

            if (_serviceProvider is IAsyncDisposable asyncDisposable)
            {
                try
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                catch (ObjectDisposedException) { }
            }
            else if (_serviceProvider is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (ObjectDisposedException) { }
            }
        }

        private static void EnqueueNavigation<TPage>(TPage page, IBrowserSession session) where TPage : Page<TPage>
        {
            var routeAttribute = typeof(TPage).GetCustomAttributes(typeof(RouteAttribute), inherit: true);
            if (routeAttribute.Length == 0) return;

            var route = ((RouteAttribute)routeAttribute[0]).Path;

            // Use reflection to call the protected Enqueue method on the page
            // We need to add the navigation action to the page's action queue
            var enqueueMethod = typeof(Page<TPage>).GetMethod("Enqueue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new[] { typeof(Func<Task>) },
                null);

            if (enqueueMethod != null)
            {
                Func<Task> navigationAction = async () =>
                {
                    var options = session.ServiceProvider.GetService(typeof(FluentUIScaffoldOptions))
                        as FluentUIScaffoldOptions;
                    if (options?.BaseUrl != null)
                    {
                        var baseUrlString = options.BaseUrl.ToString().TrimEnd('/');
                        var routePath = route.StartsWith("/") ? route : "/" + route;
                        var url = new Uri(baseUrlString + routePath);
                        await session.NavigateToUrlAsync(url).ConfigureAwait(false);
                    }
                };

                enqueueMethod.Invoke(page, new object[] { navigationAction });
            }
        }

        private static void EnqueueNavigationWithParams<TPage>(TPage page, IBrowserSession session, object routeParams)
            where TPage : Page<TPage>
        {
            var routeAttribute = typeof(TPage).GetCustomAttributes(typeof(RouteAttribute), inherit: true);
            if (routeAttribute.Length == 0) return;

            var route = ((RouteAttribute)routeAttribute[0]).Path;

            var enqueueMethod = typeof(Page<TPage>).GetMethod("Enqueue",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                new[] { typeof(Func<Task>) },
                null);

            if (enqueueMethod != null)
            {
                Func<Task> navigationAction = async () =>
                {
                    var options = session.ServiceProvider.GetService(typeof(FluentUIScaffoldOptions))
                        as FluentUIScaffoldOptions;
                    if (options?.BaseUrl != null)
                    {
                        var baseUrlString = options.BaseUrl.ToString().TrimEnd('/');
                        var routePath = route.StartsWith("/") ? route : "/" + route;

                        // Substitute route parameters
                        var urlString = baseUrlString + routePath;
                        if (routeParams is System.Collections.Generic.IDictionary<string, object> dict)
                        {
                            foreach (var kvp in dict)
                                urlString = urlString.Replace($"{{{kvp.Key}}}", Uri.EscapeDataString(kvp.Value?.ToString() ?? ""));
                        }
                        else
                        {
                            foreach (var prop in routeParams.GetType().GetProperties())
                            {
                                var value = prop.GetValue(routeParams);
                                urlString = urlString.Replace($"{{{prop.Name}}}", Uri.EscapeDataString(value?.ToString() ?? ""));
                            }
                        }

                        await session.NavigateToUrlAsync(new Uri(urlString)).ConfigureAwait(false);
                    }
                };

                enqueueMethod.Invoke(page, new object[] { navigationAction });
            }
        }
    }
}
