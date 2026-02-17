using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Aspire.Hosting;
using Aspire.Hosting.Testing;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Hosting;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.AspireHosting
{
    /// <summary>
    /// Hosting strategy that wraps DistributedApplicationTestingBuilder to manage Aspire hosts.
    /// Delegates all lifecycle management to Aspire's testing infrastructure.
    /// Applies environment variables from FluentUIScaffoldOptions as process-level env vars
    /// before CreateAsync, since Aspire reads them from the test process.
    /// </summary>
    /// <typeparam name="TEntryPoint">The Aspire AppHost entry point type.</typeparam>
    public sealed class AspireHostingStrategy<TEntryPoint> : IHostingStrategy
        where TEntryPoint : class
    {
        private readonly Action<IDistributedApplicationTestingBuilder> _configureAction;
        private readonly FluentUIScaffoldOptions _scaffoldOptions;
        private readonly string? _baseUrlResourceName;
        private readonly string _configHash;

        private Dictionary<string, string?>? _envVarSnapshot;
        private DistributedApplication? _app;
        private Uri? _baseUrl;
        private bool _isStarted;

        /// <summary>
        /// Creates a new AspireHostingStrategy for the specified AppHost entry point.
        /// </summary>
        /// <param name="configureAction">Action to configure the distributed application builder.</param>
        /// <param name="scaffoldOptions">Shared scaffold options with environment configuration.</param>
        /// <param name="baseUrlResourceName">Optional resource name to extract the base URL from.</param>
        public AspireHostingStrategy(
            Action<IDistributedApplicationTestingBuilder> configureAction,
            FluentUIScaffoldOptions scaffoldOptions,
            string? baseUrlResourceName = null)
        {
            _configureAction = configureAction ?? throw new ArgumentNullException(nameof(configureAction));
            _scaffoldOptions = scaffoldOptions ?? throw new ArgumentNullException(nameof(scaffoldOptions));
            _baseUrlResourceName = baseUrlResourceName;
            _configHash = ComputeHash(typeof(TEntryPoint), baseUrlResourceName);
        }

        /// <inheritdoc />
        public string ConfigurationHash => _configHash;

        /// <inheritdoc />
        public Uri? BaseUrl => _baseUrl;

        /// <summary>
        /// Gets the distributed application instance once started.
        /// Useful for accessing Aspire resources and creating HTTP clients.
        /// </summary>
        public DistributedApplication? Application => _app;

        /// <inheritdoc />
        public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken cancellationToken = default)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (_isStarted) return new HostingResult(_baseUrl!, WasReused: true);

            logger.LogInformation("Starting Aspire host via AspireHostingStrategy<{EntryPoint}>", typeof(TEntryPoint).Name);

            // Snapshot current env vars before mutation, then apply unified config.
            // Aspire's DistributedApplicationTestingBuilder reads env vars from the test process
            // during CreateAsync, so they must be set before that call.
            _envVarSnapshot = CaptureEnvironmentSnapshot();

            try
            {
                ApplyEnvironmentVariables();

                // Create and configure the distributed application
                var appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>(cancellationToken);

                _configureAction(appBuilder);

                _app = await appBuilder.BuildAsync(cancellationToken);

                logger.LogInformation("Starting distributed application");
                await _app.StartAsync(cancellationToken);
            }
            finally
            {
                // Restore env vars immediately after CreateAsync + Start.
                // This narrows the mutation window to just the Aspire bootstrap.
                RestoreEnvironmentSnapshot();
            }

            // Extract base URL from resource if specified
            if (!string.IsNullOrEmpty(_baseUrlResourceName))
            {
                try
                {
                    var httpClient = _app.CreateHttpClient(_baseUrlResourceName);
                    _baseUrl = httpClient.BaseAddress;

                    if (_baseUrl != null)
                    {
                        // Best-effort reachability probe
                        await VerifyHealthAsync(logger, cancellationToken);
                    }

                    logger.LogInformation("Extracted base URL from resource '{ResourceName}': {BaseUrl}",
                        _baseUrlResourceName, _baseUrl);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to extract base URL from resource '{ResourceName}'",
                        _baseUrlResourceName);
                }
            }

            _isStarted = true;

            logger.LogInformation("Aspire host started successfully");

            return new HostingResult(_baseUrl ?? new Uri("http://localhost"), WasReused: false);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (!_isStarted || _app == null) return;

            await _app.StopAsync(cancellationToken);

            _isStarted = false;
        }

        /// <inheritdoc />
        public HostingStatus GetStatus()
        {
            return new HostingStatus(
                IsRunning: _isStarted,
                BaseUrl: _baseUrl,
                ProcessId: null); // Aspire manages its own processes
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
                _app = null;
            }

            _isStarted = false;
        }

        /// <summary>
        /// Captures current values of all env vars we plan to mutate,
        /// so they can be restored after Aspire bootstrap.
        /// </summary>
        private Dictionary<string, string?> CaptureEnvironmentSnapshot()
        {
            var keysToCapture = new List<string>
            {
                "ASPNETCORE_ENVIRONMENT",
                "DOTNET_ENVIRONMENT",
                "ASPNETCORE_HOSTINGSTARTUPASSEMBLIES",
                "DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS",
                "ASPIRE_ALLOW_UNSECURED_TRANSPORT"
            };

            // Also capture any user-specified env var keys
            foreach (var key in _scaffoldOptions.EnvironmentVariables.Keys)
            {
                if (!keysToCapture.Contains(key))
                    keysToCapture.Add(key);
            }

            var snapshot = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in keysToCapture)
            {
                snapshot[key] = Environment.GetEnvironmentVariable(key);
            }
            return snapshot;
        }

        /// <summary>
        /// Applies unified environment configuration as process-level env vars.
        /// </summary>
        private void ApplyEnvironmentVariables()
        {
            // Framework defaults
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _scaffoldOptions.EnvironmentName);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", _scaffoldOptions.EnvironmentName);
            Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES",
                _scaffoldOptions.SpaProxyEnabled ? "Microsoft.AspNetCore.SpaProxy" : "");

            // Aspire-specific defaults
            Environment.SetEnvironmentVariable("DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true");
            Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

            // User env vars override framework defaults (last-write-wins)
            foreach (var kv in _scaffoldOptions.EnvironmentVariables)
                Environment.SetEnvironmentVariable(kv.Key, kv.Value);
        }

        /// <summary>
        /// Restores env vars to their pre-mutation values (null means remove).
        /// </summary>
        private void RestoreEnvironmentSnapshot()
        {
            if (_envVarSnapshot == null) return;

            foreach (var kv in _envVarSnapshot)
            {
                Environment.SetEnvironmentVariable(kv.Key, kv.Value);
            }

            _envVarSnapshot = null;
        }

        private async Task VerifyHealthAsync(ILogger logger, CancellationToken cancellationToken)
        {
            if (_baseUrl == null) return;

            using var probeClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

            try
            {
                var response = await probeClient.GetAsync(_baseUrl, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning(
                        "Aspire app at '{BaseUrl}' returned non-success status: {StatusCode}",
                        _baseUrl, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to reach Aspire app at '{BaseUrl}'", _baseUrl);
            }
        }

        private static string ComputeHash(Type entryPointType, string? resourceName)
        {
            var combined = $"{entryPointType.AssemblyQualifiedName}|{resourceName ?? ""}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
