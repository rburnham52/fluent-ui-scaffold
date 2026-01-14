using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    /// <summary>
    /// Handles waiting for server health checks to pass before considering the server ready.
    /// </summary>
    public interface IHealthWaiter
    {
        /// <summary>
        /// Waits for the server to become healthy by polling configured health check endpoints.
        /// </summary>
        /// <param name="plan">The launch plan containing health check configuration</param>
        /// <param name="logger">Logger for diagnostics</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task WaitUntilHealthyAsync(LaunchPlan plan, ILogger logger, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of IHealthWaiter using the configured readiness probe.
    /// </summary>
    public sealed class HealthWaiter : IHealthWaiter
    {
        public async Task WaitUntilHealthyAsync(LaunchPlan plan, ILogger logger, CancellationToken cancellationToken = default)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("Waiting for server to become healthy at {BaseUrl}", plan.BaseUrl);
            logger.LogInformation("Health check endpoints: {Endpoints}", string.Join(", ", plan.HealthCheckEndpoints));
            logger.LogInformation("Startup timeout: {Timeout}", plan.StartupTimeout);

            var endpoints = plan.HealthCheckEndpoints?.ToList() ?? new List<string> { "/" };
            if (!endpoints.Any())
            {
                endpoints = new List<string> { "/" };
            }

            // Wait for initial delay before starting health checks
            if (plan.InitialDelay > TimeSpan.Zero)
            {
                logger.LogDebug("Waiting {InitialDelay}ms before starting health checks", plan.InitialDelay.TotalMilliseconds);
                await Task.Delay(plan.InitialDelay, cancellationToken);
            }

            var startTime = DateTimeOffset.UtcNow;
            var timeout = plan.StartupTimeout > TimeSpan.Zero ? plan.StartupTimeout : TimeSpan.FromSeconds(60);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            try
            {
                // Use the configured readiness probe to check health
                await plan.ReadinessProbe.WaitUntilReadyAsync(plan, logger, timeoutCts.Token);

                var elapsed = DateTimeOffset.UtcNow - startTime;
                logger.LogInformation("Server became healthy after {ElapsedMs}ms", elapsed.TotalMilliseconds);
            }
            catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
            {
                var elapsed = DateTimeOffset.UtcNow - startTime;

                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("Health check was cancelled after {ElapsedMs}ms", elapsed.TotalMilliseconds);
                    throw;
                }

                logger.LogError("Server failed to become healthy within {TimeoutMs}ms (elapsed: {ElapsedMs}ms)",
                    timeout.TotalMilliseconds, elapsed.TotalMilliseconds);

                throw new TimeoutException($"Server did not become healthy within {timeout.TotalSeconds:F1} seconds");
            }
            catch (Exception ex)
            {
                var elapsed = DateTimeOffset.UtcNow - startTime;
                logger.LogError(ex, "Health check failed after {ElapsedMs}ms", elapsed.TotalMilliseconds);
                throw;
            }
        }
    }
}
