using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    /// <summary>
    /// Manages the lifecycle of application servers (Aspire AppHost, ASP.NET Core, Node.js, etc.)
    /// Provides deterministic startup, shutdown, and restart capabilities with health checking.
    /// </summary>
    public interface IServerManager
    {
        /// <summary>
        /// Ensures a server is started with the given configuration.
        /// If a compatible server is already running, reuses it. Otherwise starts a new instance.
        /// </summary>
        /// <param name="plan">The launch plan containing server configuration</param>
        /// <param name="logger">Logger for diagnostics</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The current server status</returns>
        Task<ServerStatus> EnsureStartedAsync(LaunchPlan plan, ILogger logger, CancellationToken cancellationToken = default);

        /// <summary>
        /// Restarts the currently managed server with the last known configuration.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task RestartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stops the currently managed server and cleans up resources.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        Task StopAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the current status of the managed server.
        /// </summary>
        /// <returns>The current server status</returns>
        ServerStatus GetStatus();
    }

    /// <summary>
    /// Represents the current status of a managed server instance.
    /// </summary>
    /// <param name="Pid">The process ID of the server</param>
    /// <param name="StartTime">When the server process was started</param>
    /// <param name="BaseUrl">The base URL where the server is accessible</param>
    /// <param name="ConfigHash">Hash of the configuration used to start this server</param>
    /// <param name="IsHealthy">Whether the server is currently healthy and responsive</param>
    public sealed record ServerStatus(
        int Pid,
        DateTimeOffset StartTime,
        Uri BaseUrl,
        string ConfigHash,
        bool IsHealthy
    )
    {
        /// <summary>
        /// Represents the state when no server is running.
        /// </summary>
        public static readonly ServerStatus None = new(0, DateTimeOffset.MinValue, new Uri("http://localhost"), "", false);

        /// <summary>
        /// Gets whether this status represents a running server.
        /// </summary>
        public bool IsRunning => Pid > 0 && IsHealthy;
    }
}
