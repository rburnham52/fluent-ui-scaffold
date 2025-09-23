using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Hosting
{
    /// <summary>
    /// Abstraction for hosting strategies that manage application lifecycle during testing.
    /// Implementations handle starting, stopping, and monitoring different types of hosts
    /// (DotNet, Node.js, Aspire, external servers, etc.).
    /// </summary>
    public interface IHostingStrategy : IAsyncDisposable
    {
        /// <summary>
        /// Gets a unique identifier computed from the configuration.
        /// Used to determine if an existing server can be reused.
        /// </summary>
        string ConfigurationHash { get; }

        /// <summary>
        /// Gets the base URL of the hosted application once started.
        /// Returns null if the server has not been started.
        /// </summary>
        Uri? BaseUrl { get; }

        /// <summary>
        /// Starts the hosted application or reuses an existing server matching the configuration.
        /// This method is idempotent - calling it multiple times is safe.
        /// </summary>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Result containing the base URL and whether an existing server was reused.</returns>
        Task<HostingResult> StartAsync(ILogger logger, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the hosted application and cleans up resources.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current status of the hosted application.
        /// </summary>
        /// <returns>Current hosting status.</returns>
        HostingStatus GetStatus();
    }
}
