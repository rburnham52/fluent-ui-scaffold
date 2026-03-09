using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// Playwright plugin for FluentUIScaffold.
    /// Owns the browser singleton and creates per-test sessions.
    /// </summary>
    public class PlaywrightPlugin : IUITestingPlugin
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private IServiceProvider _rootProvider;
        private bool _isDisposed;

        public void ConfigureServices(IServiceCollection services)
        {
            // Plugin registers itself; services from sessions are provided
            // via SessionServiceProvider, not via the root container.
        }

        public async Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken cancellationToken = default)
        {
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync().ConfigureAwait(false);

            var headless = options.HeadlessMode ?? true;
            var slowMo = options.SlowMo.HasValue ? (float)options.SlowMo.Value : (headless ? 0f : 50f);

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                SlowMo = slowMo,
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Sets the root service provider for session creation.
        /// Called by the builder after building the service provider.
        /// </summary>
        internal void SetRootProvider(IServiceProvider rootProvider)
        {
            _rootProvider = rootProvider;
        }

        public async Task<IBrowserSession> CreateSessionAsync()
        {
            if (_browser == null)
                throw new InvalidOperationException("Plugin not initialized. Call InitializeAsync first.");

            var context = await _browser.NewContextAsync().ConfigureAwait(false);
            var page = await context.NewPageAsync().ConfigureAwait(false);

            return new PlaywrightBrowserSession(context, page, _browser, _rootProvider);
        }

        public async ValueTask DisposeAsync()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            if (_browser != null)
            {
                await _browser.CloseAsync().ConfigureAwait(false);
            }

            _playwright?.Dispose();
        }
    }
}
