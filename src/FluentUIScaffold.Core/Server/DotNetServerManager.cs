using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    /// <summary>
    /// Server manager implementation for .NET applications started via 'dotnet run'.
    /// Provides full lifecycle management with process persistence, health checking, and orphan cleanup.
    /// Used by DotNetHostingStrategy and NodeHostingStrategy for non-Aspire scenarios.
    /// </summary>
    public sealed class DotNetServerManager : IServerManager, IDisposable
    {
        private readonly object _sync = new();
        private readonly IProcessRegistry _registry;
        private readonly IProcessLauncher _launcher;
        private readonly IHealthWaiter _healthWaiter;

        private ServerStatus? _status;
        private LaunchPlan? _lastPlan;
        private Process? _currentProcess;
        private bool _disposed;

        /// <summary>
        /// Initializes a new DotNetServerManager with the specified dependencies.
        /// </summary>
        /// <param name="registry">Process registry for state persistence</param>
        /// <param name="launcher">Process launcher for starting servers</param>
        /// <param name="healthWaiter">Health checker for server readiness</param>
        public DotNetServerManager(
            IProcessRegistry? registry = null,
            IProcessLauncher? launcher = null,
            IHealthWaiter? healthWaiter = null)
        {
            _registry = registry ?? new ProcessRegistry();
            _launcher = launcher ?? new ProcessLauncher();
            _healthWaiter = healthWaiter ?? new HealthWaiter();
        }

        /// <summary>
        /// Ensures a server is started with the given launch plan.
        /// Reuses existing servers when configuration matches, otherwise starts new instances.
        /// </summary>
        public async Task<ServerStatus> EnsureStartedAsync(LaunchPlan plan, ILogger logger, CancellationToken cancellationToken = default)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            if (_disposed) throw new ObjectDisposedException(nameof(DotNetServerManager));

            lock (_sync)
            {
                var configHash = ConfigHasher.Compute(plan);
                logger.LogDebug("Configuration hash: {ConfigHash}", configHash);

                // Check if we have a running server with the same configuration
                var persistedStatus = _registry.TryLoad(configHash);
                if (persistedStatus is { IsHealthy: true })
                {
                    logger.LogInformation("Reusing existing healthy server (PID: {Pid})", persistedStatus.Pid);
                    _status = persistedStatus;
                    _lastPlan = plan;
                    return _status;
                }

                // If we have a persisted server but it's not healthy, kill it
                if (persistedStatus is { Pid: > 0 })
                {
                    logger.LogInformation("Found existing server (PID: {Pid}) but it's not healthy, terminating", persistedStatus.Pid);
                    _registry.TryKill(persistedStatus.Pid, logger);
                    _registry.Delete(configHash);
                }

                // Kill any orphaned processes
                var orphansKilled = _registry.KillOrphans(logger);
                if (orphansKilled > 0)
                {
                    logger.LogInformation("Cleaned up {OrphanCount} orphaned server processes", orphansKilled);
                }

                // Clean up our current process if it exists
                if (_currentProcess != null && !_currentProcess.HasExited)
                {
                    logger.LogInformation("Terminating previously managed process (PID: {Pid})", _currentProcess.Id);
                    _registry.TryKill(_currentProcess.Id, logger);
                }

                _currentProcess?.Dispose();
                _currentProcess = null;
                _status = null;

                // Check if we need to run asset build step
                var assetsBuild = GetAssetsBuildFunc(plan);
                if (assetsBuild != null)
                {
                    logger.LogInformation("Running assets build step");
                    assetsBuild(cancellationToken).GetAwaiter().GetResult();
                }

                // Start new server process
                logger.LogInformation("Starting new server process");
                _currentProcess = _launcher.StartProcess(plan, logger);
                _registry.Save(configHash, _currentProcess, plan.BaseUrl);

                _lastPlan = plan;
            }

            // Wait for server to become healthy (outside of lock to avoid blocking)
            await _healthWaiter.WaitUntilHealthyAsync(plan, logger, cancellationToken);

            // Update status to healthy
            lock (_sync)
            {
                var configHash = ConfigHasher.Compute(plan);
                _status = _registry.UpdateWithReady(configHash, plan.BaseUrl);
                logger.LogInformation("Server is now healthy and ready (PID: {Pid}, URL: {BaseUrl})",
                    _status.Pid, _status.BaseUrl);
                return _status;
            }
        }

        /// <summary>
        /// Restarts the server with the last known configuration.
        /// </summary>
        public async Task RestartAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DotNetServerManager));

            if (_lastPlan == null)
            {
                throw new InvalidOperationException("No previous configuration available for restart. Call EnsureStartedAsync first.");
            }

            var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DotNetServerManager>();
            logger.LogInformation("Restarting server with previous configuration");

            await StopAsync(cancellationToken);
            await EnsureStartedAsync(_lastPlan, logger, cancellationToken);
        }

        /// <summary>
        /// Stops the currently managed server and cleans up resources.
        /// Uses aggressive cleanup to ensure all processes are terminated.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DotNetServerManager));

            await Task.Run(() =>
            {
                lock (_sync)
                {
                    if (_status == null) return;

                    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DotNetServerManager>();
                    logger.LogInformation("Stopping server (PID: {Pid})", _status.Pid);

                    // Use registry to kill process (handles tree kill)
                    _registry.TryKill(_status.Pid, logger);

                    _registry.Delete(_status.ConfigHash);

                    _currentProcess?.Dispose();
                    _currentProcess = null;
                    _status = null;
                }
            }, cancellationToken);
        }



        /// <summary>
        /// Gets the current status of the managed server.
        /// </summary>
        public ServerStatus GetStatus()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DotNetServerManager));

            lock (_sync)
            {
                return _status ?? ServerStatus.None;
            }
        }

        /// <summary>
        /// Disposes the server manager and stops any managed processes.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore errors during disposal
            }

            _disposed = true;
        }

        /// <summary>
        /// Extracts an assets build function from the launch plan environment variables.
        /// This looks for CI/headless scenarios where SPA assets need to be built.
        /// </summary>
        private static Func<CancellationToken, Task>? GetAssetsBuildFunc(LaunchPlan plan)
        {
            // Check if we're in a headless/CI environment
            var isHeadless = plan.StartInfo.EnvironmentVariables.ContainsKey("CI") ||
                           plan.StartInfo.EnvironmentVariables.ContainsKey("HEADLESS") ||
                           string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));

            // Check if SpaProxy is disabled (indicating we need prebuilt assets)
            var hostingAssemblies = plan.StartInfo.EnvironmentVariables.ContainsKey("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES")
                ? plan.StartInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"]
                : null;
            var spaProxyDisabled = string.IsNullOrEmpty(hostingAssemblies);

            if (!isHeadless || !spaProxyDisabled) return null;

            // Return a function that builds SPA assets if the dist folder is missing
            return async (cancellationToken) =>
            {
                var workingDir = plan.StartInfo.WorkingDirectory ?? Environment.CurrentDirectory;

                // Look for common SPA directories
                var spaDirectories = new[] { "ClientApp", "client", "frontend", "ui" };

                foreach (var spaDir in spaDirectories)
                {
                    var spaPath = System.IO.Path.Combine(workingDir, spaDir);
                    var distPath = System.IO.Path.Combine(spaPath, "dist");
                    var packageJsonPath = System.IO.Path.Combine(spaPath, "package.json");

                    if (System.IO.File.Exists(packageJsonPath) && !System.IO.Directory.Exists(distPath))
                    {
                        // SPA directory exists but no dist folder - build it
                        var buildProcess = new ProcessStartInfo
                        {
                            FileName = "npm",
                            Arguments = "ci && npm run build",
                            WorkingDirectory = spaPath,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using var process = Process.Start(buildProcess);
                        if (process != null)
                        {
                            await process.WaitForExitAsync(cancellationToken);
                            if (process.ExitCode != 0)
                            {
                                var error = await process.StandardError.ReadToEndAsync();
                                throw new InvalidOperationException($"SPA build failed in {spaPath}: {error}");
                            }
                        }

                        break; // Only build the first SPA directory found
                    }
                }
            };
        }
    }
}
