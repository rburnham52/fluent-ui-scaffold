using System;
using System.Threading.Tasks;

namespace FluentUIScaffold.Core.Interfaces
{
    /// <summary>
    /// Represents an isolated browser session for a single test.
    /// Each session owns its own browser context and page.
    /// </summary>
    public interface IBrowserSession : IAsyncDisposable
    {
        /// <summary>
        /// Navigates the session's page to the specified URL.
        /// </summary>
        Task NavigateToUrlAsync(Uri url);

        /// <summary>
        /// Gets the service provider for this session.
        /// This is a wrapper provider that resolves session-specific services (IPage, IBrowserContext)
        /// first, then falls back to the root provider for shared services.
        /// </summary>
        IServiceProvider ServiceProvider { get; }
    }
}
