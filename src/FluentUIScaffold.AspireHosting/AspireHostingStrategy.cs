using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Aspire.Hosting;
using Aspire.Hosting.Testing;

using FluentUIScaffold.Core.Hosting;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.AspireHosting
{
    /// <summary>
    /// Hosting strategy that wraps DistributedApplicationTestingBuilder to manage Aspire hosts.
    /// Delegates all lifecycle management to Aspire's testing infrastructure.
    /// </summary>
    /// <typeparam name="TEntryPoint">The Aspire AppHost entry point type.</typeparam>
    public sealed class AspireHostingStrategy<TEntryPoint> : IHostingStrategy
        where TEntryPoint : class
    {
        private readonly Action<IDistributedApplicationTestingBuilder> _configureAction;
        private readonly string? _baseUrlResourceName;
        private readonly string _configHash;

        private DistributedApplication? _app;
        private Uri? _baseUrl;
        private bool _isStarted;

        /// <summary>
        /// Creates a new AspireHostingStrategy for the specified AppHost entry point.
        /// </summary>
        /// <param name="configureAction">Action to configure the distributed application builder.</param>
        /// <param name="baseUrlResourceName">Optional resource name to extract the base URL from.</param>
        public AspireHostingStrategy(
            Action<IDistributedApplicationTestingBuilder> configureAction,
            string? baseUrlResourceName = null)
        {
            _configureAction = configureAction ?? throw new ArgumentNullException(nameof(configureAction));
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

            // Set up environment for testing
            Environment.SetEnvironmentVariable("DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true");
            Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

            // Create and configure the distributed application
            var appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>(cancellationToken);

            _configureAction(appBuilder);

            _app = await appBuilder.BuildAsync(cancellationToken);

            logger.LogInformation("Starting distributed application");
            await _app.StartAsync(cancellationToken);

            // Extract base URL from resource if specified
            if (!string.IsNullOrEmpty(_baseUrlResourceName))
            {
                try
                {
                    var httpClient = _app.CreateHttpClient(_baseUrlResourceName);
                    _baseUrl = httpClient.BaseAddress;

                    if (_baseUrl != null)
                    {
                        Environment.SetEnvironmentVariable("ASPIRE_URL", _baseUrl.ToString());

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
