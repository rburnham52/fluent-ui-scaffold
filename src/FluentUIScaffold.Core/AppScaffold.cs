using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Hosting;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core
{
    /// <summary>
    /// Represents the built scaffold application with its services.
    /// Acts as the central hub for accessing test infrastructure and state.
    /// </summary>
    /// <typeparam name="TWebApp">The type of the web application under test (e.g. WebApp)</typeparam>
    public class AppScaffold<TWebApp> : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Func<IServiceProvider, Task> _startAction;
        private bool _isStarted;
        private bool _isDisposed;
        private IHostingStrategy? _hostingStrategy;

        public AppScaffold(IServiceProvider serviceProvider, Func<IServiceProvider, Task> startAction)
        {
            _serviceProvider = serviceProvider;
            _startAction = startAction;
        }

        /// <summary>
        /// Gets the service provider for the test session.
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Starts any configured background services or hosts (e.g. Aspire, WebServer).
        /// </summary>
        public async Task StartAsync()
        {
            if (_isStarted) return;

            await _startAction(_serviceProvider);

            // Cache the hosting strategy for disposal later
            _hostingStrategy = _serviceProvider.GetService<IHostingStrategy>();

            _isStarted = true;
        }

        /// <summary>
        /// Resolve a dependency from the internal service provider.
        /// </summary>
        public T GetService<T>() where T : notnull
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Helper to access framework specific tools (e.g. IPage from Playwright).
        /// </summary>
        public TResult Framework<TResult>() where TResult : notnull
        {
            return _serviceProvider.GetRequiredService<TResult>();
        }

        #region Page Navigation

        /// <summary>
        /// Navigates to a page component of the specified type.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <returns>The page component instance after navigation.</returns>
        public TPage NavigateTo<TPage>() where TPage : class
        {
            var page = _serviceProvider.GetRequiredService<TPage>()
                ?? throw new InvalidOperationException($"Service of type {typeof(TPage).Name} is not registered.");

            // Use reflection to call Navigate() if the page has that method
            var navigateMethod = page.GetType().GetMethod("Navigate");
            if (navigateMethod != null)
            {
                navigateMethod.Invoke(page, null);
            }

            return page;
        }

        /// <summary>
        /// Attaches to a page component of the specified type without navigating.
        /// Use this when you're already on the page and just need the page object.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <param name="validate">If true, validates that the current page matches the expected page.</param>
        /// <returns>The page component instance.</returns>
        public TPage On<TPage>(bool validate = false) where TPage : class
        {
            var page = _serviceProvider.GetRequiredService<TPage>()
                ?? throw new InvalidOperationException($"Service of type {typeof(TPage).Name} is not registered.");

            if (validate)
            {
                // Use reflection to call ValidateCurrentPage() if it exists
                var validateMethod = page.GetType().GetMethod("ValidateCurrentPage");
                if (validateMethod != null)
                {
                    validateMethod.Invoke(page, null);
                }
            }

            return page;
        }

        /// <summary>
        /// Waits for a page to be available and validates its state.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <returns>The current AppScaffold instance for fluent chaining.</returns>
        public AppScaffold<TWebApp> WaitFor<TPage>() where TPage : class
        {
            var page = _serviceProvider.GetRequiredService<TPage>()
                ?? throw new InvalidOperationException($"Service of type {typeof(TPage).Name} is not registered.");

            // Use reflection to call ValidateCurrentPage() if it exists
            var validateMethod = page.GetType().GetMethod("ValidateCurrentPage");
            if (validateMethod != null)
            {
                validateMethod.Invoke(page, null);
            }

            return this;
        }

        /// <summary>
        /// Waits for an element on the current page.
        /// </summary>
        /// <typeparam name="TPage">The type of the page component.</typeparam>
        /// <param name="elementSelector">Function to select the element to wait for.</param>
        /// <returns>The current AppScaffold instance for fluent chaining.</returns>
        public AppScaffold<TWebApp> WaitFor<TPage>(Func<TPage, IElement> elementSelector) where TPage : class
        {
            var page = _serviceProvider.GetRequiredService<TPage>()
                ?? throw new InvalidOperationException($"Service of type {typeof(TPage).Name} is not registered.");

            var element = elementSelector(page);
            element.WaitForVisible();

            return this;
        }

        /// <summary>
        /// Navigates to a specific URL.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        /// <returns>The current AppScaffold instance for fluent chaining.</returns>
        public AppScaffold<TWebApp> NavigateToUrl(Uri url)
        {
            if (url == null)
                throw new FluentUIScaffoldValidationException("URL cannot be null", nameof(url));

            var driver = _serviceProvider.GetRequiredService<IUIDriver>();
            driver.NavigateToUrl(url);
            return this;
        }

        /// <summary>
        /// Configures the base URL for the application.
        /// </summary>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns>The current AppScaffold instance for fluent chaining.</returns>
        public AppScaffold<TWebApp> WithBaseUrl(Uri baseUrl)
        {
            if (baseUrl == null)
                throw new FluentUIScaffoldValidationException("Base URL cannot be null", nameof(baseUrl));

            var options = _serviceProvider.GetService<FluentUIScaffoldOptions>();
            if (options != null)
            {
                options.BaseUrl = baseUrl;
            }

            return this;
        }

        #endregion

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            // Stop any hosting strategies using the cached reference
            // (don't try to resolve from service provider as it may already be disposed)
            if (_hostingStrategy != null)
            {
                try
                {
                    await _hostingStrategy.DisposeAsync();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }
            }

            if (_serviceProvider is IAsyncDisposable asyncDisposable)
            {
                try
                {
                    await asyncDisposable.DisposeAsync();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }
            }
            else if (_serviceProvider is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }
            }
        }
    }
}
