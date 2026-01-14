using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Server;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Hosting
{
    /// <summary>
    /// Hosting strategy for .NET applications started via 'dotnet run'.
    /// Manages the lifecycle of the host process including startup, health checking, and shutdown.
    /// Supports server reuse across test runs via configuration hashing and process persistence.
    /// </summary>
    public sealed class DotNetHostingStrategy : IHostingStrategy
    {
        private readonly LaunchPlan _launchPlan;
        private readonly IServerManager _serverManager;
        private readonly string _configHash;

        private Uri? _baseUrl;
        private bool _isStarted;
        private int? _processId;

        /// <summary>
        /// Creates a new DotNetHostingStrategy with the specified launch plan.
        /// </summary>
        /// <param name="launchPlan">The launch plan containing process configuration.</param>
        /// <param name="serverManager">Optional server manager for testing/DI. Defaults to DotNetServerManager.</param>
        public DotNetHostingStrategy(LaunchPlan launchPlan, IServerManager? serverManager = null)
        {
            _launchPlan = launchPlan ?? throw new ArgumentNullException(nameof(launchPlan));
            _serverManager = serverManager ?? new DotNetServerManager();
            _configHash = ConfigHasher.Compute(launchPlan);
            _baseUrl = launchPlan.BaseUrl;
        }

        /// <inheritdoc />
        public string ConfigurationHash => _configHash;

        /// <inheritdoc />
        public Uri? BaseUrl => _baseUrl;

        /// <inheritdoc />
        public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken cancellationToken = default)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("Starting .NET application via DotNetHostingStrategy");
            logger.LogDebug("Configuration hash: {ConfigHash}", _configHash);

            var serverStatus = await _serverManager.EnsureStartedAsync(_launchPlan, logger, cancellationToken);

            _baseUrl = serverStatus.BaseUrl;
            _processId = serverStatus.Pid;
            _isStarted = true;

            var wasReused = serverStatus.ConfigHash == _configHash && serverStatus.IsHealthy;

            logger.LogInformation(
                wasReused
                    ? "Reused existing .NET server (PID: {Pid}, URL: {BaseUrl})"
                    : "Started new .NET server (PID: {Pid}, URL: {BaseUrl})",
                serverStatus.Pid, serverStatus.BaseUrl);

            return new HostingResult(serverStatus.BaseUrl, wasReused);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_isStarted) return;

            await _serverManager.StopAsync(cancellationToken);

            _isStarted = false;
            _processId = null;
        }

        /// <inheritdoc />
        public HostingStatus GetStatus()
        {
            var serverStatus = _serverManager.GetStatus();

            return new HostingStatus(
                serverStatus.IsRunning,
                serverStatus.BaseUrl,
                serverStatus.Pid > 0 ? serverStatus.Pid : null);
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await StopAsync();

            if (_serverManager is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
