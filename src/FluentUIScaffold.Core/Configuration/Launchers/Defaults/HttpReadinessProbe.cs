namespace FluentUIScaffold.Core.Configuration.Launchers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public sealed class HttpReadinessProbe : IReadinessProbe
    {
        private readonly HttpClient _httpClient;
        private readonly IClock _clock;

        public HttpReadinessProbe(HttpClient? httpClient = null, IClock? clock = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _clock = clock ?? new SystemClock();
        }

        public async Task WaitUntilReadyAsync(ServerConfiguration configuration, ILogger? logger, TimeSpan initialDelay, TimeSpan pollInterval, CancellationToken cancellationToken = default)
        {
            if (configuration.BaseUrl == null)
                throw new ArgumentException("BaseUrl cannot be null for readiness probe", nameof(configuration));

            if (initialDelay > TimeSpan.Zero)
            {
                await _clock.Delay(initialDelay);
            }

            var startTime = DateTime.UtcNow;
            var attempt = 0;

            // Build test URLs
            var endpoints = configuration.HealthCheckEndpoints.Count > 0
                ? configuration.HealthCheckEndpoints
                : new List<string> { "/", "/health" };

            var testUrls = new List<Uri> { configuration.BaseUrl };
            foreach (var endpoint in endpoints)
            {
                if (string.IsNullOrWhiteSpace(endpoint)) continue;
                var uri = endpoint.StartsWith("/") ? new Uri(configuration.BaseUrl, endpoint) : new Uri($"{configuration.BaseUrl}{endpoint}");
                testUrls.Add(uri);
            }

            while (DateTime.UtcNow - startTime < configuration.StartupTimeout)
            {
                attempt++;
                foreach (var url in testUrls)
                {
                    try
                    {
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        cts.CancelAfter(TimeSpan.FromSeconds(5));
                        var response = await _httpClient.GetAsync(url, cts.Token);
                        if (response.IsSuccessStatusCode)
                        {
                            logger?.LogInformation("Server ready after {Attempt} attempts at {Url}", attempt, url);
                            return;
                        }
                        else
                        {
                            logger?.LogDebug("Readiness non-2xx {Status} at {Url} attempt {Attempt}", response.StatusCode, url, attempt);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogDebug("Readiness error at {Url} attempt {Attempt}: {Message}", url, attempt, ex.Message);
                    }
                }

                await _clock.Delay(pollInterval);
            }

            throw new TimeoutException($"Server did not become ready within {configuration.StartupTimeout}");
        }
    }
}