using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core.Interfaces
{
    /// <summary>
    /// Plugin contract for UI testing frameworks (e.g., Playwright).
    /// Owns the browser singleton and creates per-test sessions.
    /// </summary>
    public interface IUITestingPlugin : IAsyncDisposable
    {
        /// <summary>
        /// Registers shared services (logging, options forwarding) into the DI container.
        /// Called once during builder configuration.
        /// </summary>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Initializes the plugin (e.g., launches browser).
        /// Called once during AppScaffold.StartAsync().
        /// </summary>
        Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an isolated browser session for a single test.
        /// Each session owns its own browser context and page.
        /// The rootProvider is the application's root service provider,
        /// used as fallback for services not scoped to the session.
        /// </summary>
        Task<IBrowserSession> CreateSessionAsync(IServiceProvider rootProvider);
    }
}
