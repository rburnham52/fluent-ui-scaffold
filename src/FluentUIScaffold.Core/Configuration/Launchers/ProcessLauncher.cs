using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public sealed class ProcessLauncher : IDisposable
    {
        private readonly ILogger? _logger;
        private readonly IProcessRunner _processRunner;
        private readonly IClock _clock;
        private Process? _process;
        private bool _disposed;

        public ProcessLauncher(ILogger? logger = null, IProcessRunner? processRunner = null, IClock? clock = null)
        {
            _logger = logger;
            _processRunner = processRunner ?? new ProcessRunner();
            _clock = clock ?? new SystemClock();
        }

        public async Task StartAsync(ServerConfiguration configuration, LaunchPlan plan, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ProcessLauncher));
            if (configuration.BaseUrl == null) throw new ArgumentException("BaseUrl cannot be null", nameof(configuration));

            // Ensure env contains ASNET/urls as already serialized in StartInfo
            _logger?.LogInformation("Starting process: {FileName} {Arguments}", plan.StartInfo.FileName, plan.StartInfo.Arguments);

            _process = Process.Start(plan.StartInfo);
            if (_process == null)
            {
                throw new InvalidOperationException("Failed to start process");
            }
            _logger?.LogInformation("Process started with PID: {Pid}", _process.Id);

            if (plan.StreamProcessOutput)
            {
                _ = Task.Run(async () => await StreamOutputAsync(_process, _logger, isError: false), cancellationToken);
                _ = Task.Run(async () => await StreamOutputAsync(_process, _logger, isError: true), cancellationToken);
            }

            // Wait for readiness
            var originalTimeout = configuration.StartupTimeout;
            try
            {
                // Temporarily override timeout contextually for the probe
                configuration.StartupTimeout = plan.StartupTimeout;
                configuration.HealthCheckEndpoints = new System.Collections.Generic.List<string>(plan.HealthCheckEndpoints);
                await plan.ReadinessProbe.WaitUntilReadyAsync(configuration, _logger, plan.InitialDelay, plan.PollInterval, cancellationToken);
            }
            finally
            {
                configuration.StartupTimeout = originalTimeout;
            }
        }

        private static async Task StreamOutputAsync(Process process, ILogger? logger, bool isError)
        {
            try
            {
                var reader = isError ? process.StandardError : process.StandardOutput;
                while (!process.HasExited)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;
                    if (isError)
                        logger?.LogWarning("{Line}", line);
                    else
                        logger?.LogInformation("{Line}", line);
                }
            }
            catch (Exception ex)
            {
                logger?.LogDebug(ex, "Error streaming process {(isError ? "stderr" : "stdout")}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _logger?.LogInformation("Stopping process PID {Pid}", _process.Id);
                    _process.Kill();
                    _process.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to stop process");
            }
            finally
            {
                try { _process?.Dispose(); } catch { }
                _process = null;
            }
        }
    }
}