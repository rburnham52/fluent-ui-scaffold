using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Configuration.Launchers.Defaults;
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
        private readonly DotNetHostingOptions _hostingOptions;
        private readonly FluentUIScaffoldOptions _scaffoldOptions;
        private readonly IServerManager _serverManager;

        private LaunchPlan? _launchPlan;
        private string _configHash = string.Empty;
        private Uri? _baseUrl;
        private bool _isStarted;
        private int? _processId;

        /// <summary>
        /// Creates a new DotNetHostingStrategy with the specified options.
        /// </summary>
        /// <param name="hostingOptions">DotNet-specific hosting configuration.</param>
        /// <param name="scaffoldOptions">Shared scaffold options with environment configuration.</param>
        /// <param name="serverManager">Optional server manager for testing/DI. Defaults to DotNetServerManager.</param>
        public DotNetHostingStrategy(
            DotNetHostingOptions hostingOptions,
            FluentUIScaffoldOptions scaffoldOptions,
            IServerManager? serverManager = null)
        {
            _hostingOptions = hostingOptions ?? throw new ArgumentNullException(nameof(hostingOptions));
            _scaffoldOptions = scaffoldOptions ?? throw new ArgumentNullException(nameof(scaffoldOptions));
            _serverManager = serverManager ?? new DotNetServerManager();
            _baseUrl = hostingOptions.BaseUrl;
        }

        /// <inheritdoc />
        /// <remarks>Returns empty string until StartAsync is called and the LaunchPlan is built.</remarks>
        public string ConfigurationHash => _configHash;

        /// <inheritdoc />
        public Uri? BaseUrl => _baseUrl;

        /// <inheritdoc />
        public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken cancellationToken = default)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("Starting .NET application via DotNetHostingStrategy");

            _launchPlan = BuildLaunchPlan();
            _configHash = ConfigHasher.Compute(_launchPlan);

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

        private LaunchPlan BuildLaunchPlan()
        {
            var arguments = new List<string> { "run", "--no-launch-profile" };

            if (!string.IsNullOrEmpty(_hostingOptions.Framework))
            {
                arguments.Add("--framework");
                arguments.Add(_hostingOptions.Framework);
            }
            if (!string.IsNullOrEmpty(_hostingOptions.Configuration))
            {
                arguments.Add("--configuration");
                arguments.Add(_hostingOptions.Configuration);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = string.Join(" ", arguments),
                WorkingDirectory = _hostingOptions.WorkingDirectory
                    ?? System.IO.Path.GetDirectoryName(_hostingOptions.ProjectPath)
                    ?? Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // 1. Framework defaults: environment name and SPA proxy
            startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = _scaffoldOptions.EnvironmentName;
            startInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"] = _scaffoldOptions.EnvironmentName;
            startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] =
                _scaffoldOptions.SpaProxyEnabled ? "Microsoft.AspNetCore.SpaProxy" : "";

            // 2. User env vars override framework defaults (last-write-wins)
            foreach (var kv in _scaffoldOptions.EnvironmentVariables)
                startInfo.EnvironmentVariables[kv.Key] = kv.Value;

            var endpoints = _hostingOptions.HealthCheckEndpoints.Length > 0
                ? _hostingOptions.HealthCheckEndpoints
                : new[] { "/" };

            return new LaunchPlan(
                startInfo,
                _hostingOptions.BaseUrl!,
                _hostingOptions.StartupTimeout,
                new HttpReadinessProbe(),
                endpoints,
                initialDelay: TimeSpan.FromSeconds(2),
                pollInterval: TimeSpan.FromMilliseconds(200));
        }
    }
}
