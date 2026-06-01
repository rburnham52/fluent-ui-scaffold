using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Shared static lock for serializing process-level environment variable mutations
    /// across all generic instantiations of AspireHostingStrategy&lt;TEntryPoint&gt;.
    /// </summary>
    internal static class AspireEnvironmentLock
    {
        internal static readonly SemaphoreSlim Mutex = new SemaphoreSlim(1, 1);
    }

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
        /// When true, the Docker daemon pre-flight check is skipped. Default false.
        /// Useful for users running against remote container runtimes where a local
        /// <c>docker info</c> probe would (incorrectly) fail.
        /// </summary>
        public bool SkipDockerPreflightCheck { get; set; }

        /// <summary>
        /// Maximum time to wait for the Aspire AppHost to finish StartAsync.
        /// When exceeded, a <see cref="TimeoutException"/> is thrown instead of hanging indefinitely.
        /// Default 90 seconds.
        /// </summary>
        public TimeSpan AspireStartupTimeout { get; set; } = TimeSpan.FromSeconds(90);

        /// <summary>
        /// Interval between "still starting..." heartbeat log lines emitted during Aspire startup.
        /// Default 10 seconds.
        /// </summary>
        public TimeSpan StartupHeartbeatInterval { get; set; } = TimeSpan.FromSeconds(10);

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

            // Fail fast if Docker is unreachable, before Aspire gets a chance to hang.
            // Aspire's own daemon health check is buried ~20 stack frames deep behind
            // DistributedApplicationFactory → DcpHost → DcpDependencyCheck, so the real
            // cause is invisible without --logger "console;verbosity=detailed".
            if (!SkipDockerPreflightCheck)
            {
                await DockerPreflightCheck.EnsureDockerHealthyAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            // Serialize env var mutations across all AspireHostingStrategy<T> instances.
            // Environment.SetEnvironmentVariable is process-global and not thread-safe
            // against concurrent reads/writes from parallel test runners.
            await AspireEnvironmentLock.Mutex.WaitAsync(cancellationToken);

            try
            {
                // Snapshot current env vars before mutation, then apply unified config.
                // Aspire's DistributedApplicationTestingBuilder reads env vars from the test process
                // during CreateAsync, so they must be set before that call.
                _envVarSnapshot = CaptureEnvironmentSnapshot();

                ApplyEnvironmentVariables();

                // Create and configure the distributed application
                var appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>(cancellationToken);

                _configureAction(appBuilder);

                _app = await appBuilder.BuildAsync(cancellationToken);

                logger.LogInformation("Starting distributed application");
                await StartAppWithTimeoutAndHeartbeatAsync(_app, logger, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // A failed or timed-out start can leave a partially-initialized
                // DistributedApplication with DCP child processes (dcp/dcpproc) and
                // containers still running. Dispose it here so they are reaped
                // immediately, instead of relying on a later outer teardown that may
                // never run (e.g. the test process is force-killed / CI-cancelled).
                if (_app != null)
                {
                    try { await _app.DisposeAsync().ConfigureAwait(false); }
                    catch (Exception disposeEx)
                    {
                        logger.LogWarning(disposeEx,
                            "Error disposing partially-started Aspire app after a failed start.");
                    }
                    _app = null;
                }

                throw;
            }
            finally
            {
                // Restore env vars immediately after CreateAsync + Start.
                // This narrows the mutation window to just the Aspire bootstrap.
                RestoreEnvironmentSnapshot();

                AspireEnvironmentLock.Mutex.Release();
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

        /// <summary>
        /// Wraps <see cref="DistributedApplication.StartAsync"/> with a bounded timeout
        /// and periodic heartbeat log lines so the caller can see whether startup is
        /// progressing or stuck. On timeout, throws <see cref="TimeoutException"/> with
        /// an informative message naming the timeout duration.
        /// </summary>
        private Task StartAppWithTimeoutAndHeartbeatAsync(
            DistributedApplication app,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            return StartWithTimeoutAndHeartbeatAsync(
                ct => app.StartAsync(ct),
                AspireStartupTimeout,
                StartupHeartbeatInterval,
                logger,
                cancellationToken);
        }

        /// <summary>
        /// Test-visible core of the timeout + heartbeat logic. Runs <paramref name="startupFactory"/>,
        /// emits a "still starting..." log line every <paramref name="heartbeatInterval"/>, and throws
        /// <see cref="TimeoutException"/> if the task does not complete within <paramref name="timeout"/>.
        /// </summary>
        internal static async Task StartWithTimeoutAndHeartbeatAsync(
            Func<CancellationToken, Task> startupFactory,
            TimeSpan timeout,
            TimeSpan heartbeatInterval,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            using var startupCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var sw = Stopwatch.StartNew();
            var heartbeatTask = RunHeartbeatAsync(sw, heartbeatInterval, logger, heartbeatCts.Token);

            var startupTask = startupFactory(startupCts.Token);
            var timeoutTask = Task.Delay(timeout, cancellationToken);

            var winner = await Task.WhenAny(startupTask, timeoutTask).ConfigureAwait(false);

            heartbeatCts.Cancel();
            try { await heartbeatTask.ConfigureAwait(false); } catch { /* heartbeat is best-effort */ }

            if (winner == startupTask)
            {
                // Surface startup exceptions normally
                await startupTask.ConfigureAwait(false);
                return;
            }

            // Timeout fired first — try to cancel the in-flight startup so it doesn't
            // continue churning in the background.
            try { startupCts.Cancel(); } catch { /* best-effort */ }

            // Observe the abandoned startup task so that if it later faults (e.g. the
            // cancelled StartAsync throws), it doesn't surface as an UnobservedTaskException
            // and tear down the process on GC. We deliberately do NOT await it: a startup
            // that ignores cancellation could otherwise block us indefinitely — the very
            // thing this timeout exists to prevent. Reaping the started resources is the
            // caller's responsibility (it disposes the DistributedApplication on failure).
            _ = startupTask.ContinueWith(
                static t => { _ = t.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            throw new TimeoutException(
                $"Aspire AppHost did not start within {timeout.TotalSeconds:F0}s. " +
                "Increase the timeout via .WithAspireStartupTimeout(TimeSpan) if your AppHost legitimately needs longer, " +
                "or check Docker / container logs for a stuck resource.");
        }

        private static async Task RunHeartbeatAsync(
            Stopwatch sw,
            TimeSpan interval,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            if (interval <= TimeSpan.Zero) return;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
                    logger.LogInformation(
                        "Aspire host starting... ({Elapsed}s elapsed)",
                        (int)sw.Elapsed.TotalSeconds);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on completion
            }
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
