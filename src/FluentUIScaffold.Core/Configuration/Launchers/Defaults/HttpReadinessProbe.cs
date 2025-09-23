using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers.Abstractions;

using Microsoft.Extensions.Logging;
namespace FluentUIScaffold.Core.Configuration.Launchers.Defaults
{
    public sealed class HttpReadinessProbe : IReadinessProbe
    {
        private readonly HttpClient _httpClient;

        public HttpReadinessProbe(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task WaitUntilReadyAsync(LaunchPlan plan, ILogger? logger, CancellationToken cancellationToken = default)
        {
            if (plan.BaseUrl == null)
                throw new ArgumentException("BaseUrl cannot be null for readiness probe", nameof(plan));

            if (plan.InitialDelay > TimeSpan.Zero)
            {
                await Task.Delay(plan.InitialDelay, cancellationToken);
            }

            var startTime = DateTime.UtcNow;
            var attempt = 0;

            var testUrls = new List<Uri> { plan.BaseUrl };
            foreach (var endpoint in plan.HealthCheckEndpoints)
            {
                if (string.IsNullOrWhiteSpace(endpoint)) continue;
                var uri = endpoint.StartsWith("/") ? new Uri(plan.BaseUrl, endpoint) : new Uri($"{plan.BaseUrl}{endpoint}");
                testUrls.Add(uri);
            }

            while (DateTime.UtcNow - startTime < plan.StartupTimeout)
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

                // Check if we've exceeded the timeout before waiting
                if (DateTime.UtcNow - startTime >= plan.StartupTimeout)
                {
                    break;
                }

                await Task.Delay(plan.PollInterval, cancellationToken);
            }

            throw new TimeoutException($"Server did not become ready within {plan.StartupTimeout}");
        }
    }
}
