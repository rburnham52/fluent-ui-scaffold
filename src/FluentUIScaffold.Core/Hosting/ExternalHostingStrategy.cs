using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Hosting
{
    /// <summary>
    /// Hosting strategy for externally managed servers (CI environments, staging, production).
    /// Performs health check verification only - does not manage any processes.
    /// Use this when the server is started outside of the test framework.
    /// </summary>
    public sealed class ExternalHostingStrategy : IHostingStrategy
    {
        private readonly Uri _baseUrl;
        private readonly string[] _healthCheckEndpoints;
        private readonly TimeSpan _healthCheckTimeout;
        private readonly string _configHash;

        private bool _isVerified;

        /// <summary>
        /// Creates a new ExternalHostingStrategy for the specified server URL.
        /// </summary>
        /// <param name="baseUrl">The base URL of the externally hosted server.</param>
        /// <param name="healthCheckEndpoints">Optional health check endpoints to verify. Defaults to "/".</param>
        /// <param name="healthCheckTimeout">Timeout for health check verification. Defaults to 30 seconds.</param>
        public ExternalHostingStrategy(
            Uri baseUrl,
            string[]? healthCheckEndpoints = null,
            TimeSpan? healthCheckTimeout = null)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _healthCheckEndpoints = healthCheckEndpoints ?? new[] { "/" };
            _healthCheckTimeout = healthCheckTimeout ?? TimeSpan.FromSeconds(30);
            _configHash = ComputeHash(baseUrl, _healthCheckEndpoints);
        }

        /// <inheritdoc />
        public string ConfigurationHash => _configHash;

        /// <inheritdoc />
        public Uri? BaseUrl => _baseUrl;

        /// <inheritdoc />
        public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken cancellationToken = default)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("Verifying external server at {BaseUrl}", _baseUrl);

            using var httpClient = new HttpClient { Timeout = _healthCheckTimeout };

            foreach (var endpoint in _healthCheckEndpoints)
            {
                var url = new Uri(_baseUrl, endpoint);
                logger.LogDebug("Checking health endpoint: {Url}", url);

                try
                {
                    var response = await httpClient.GetAsync(url, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("External server verified successfully at {BaseUrl}", _baseUrl);
                        _isVerified = true;
                        return new HostingResult(_baseUrl, WasReused: true);
                    }

                    logger.LogWarning(
                        "Health check returned non-success status: {StatusCode} for {Url}",
                        response.StatusCode, url);
                }
                catch (HttpRequestException ex)
                {
                    logger.LogWarning(ex, "Health check failed for {Url}", url);
                }
                catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning(ex, "Health check timed out for {Url}", url);
                }
            }

            throw new InvalidOperationException(
                $"External server at {_baseUrl} is not responding. " +
                $"Checked endpoints: {string.Join(", ", _healthCheckEndpoints)}");
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            // External servers are not managed by us, so nothing to stop
            _isVerified = false;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public HostingStatus GetStatus()
        {
            return new HostingStatus(
                IsRunning: _isVerified,
                BaseUrl: _baseUrl,
                ProcessId: null); // No process ID for external servers
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            _isVerified = false;
            return ValueTask.CompletedTask;
        }

        private static string ComputeHash(Uri baseUrl, string[] healthCheckEndpoints)
        {
            var combined = $"{baseUrl}|{string.Join(",", healthCheckEndpoints)}";
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
