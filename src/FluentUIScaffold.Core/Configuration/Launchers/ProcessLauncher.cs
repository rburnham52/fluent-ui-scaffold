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
        private Process? _process;
        private bool _disposed;

        public ProcessLauncher(ILogger? logger = null)
        {
            _logger = logger;
        }

        public async Task StartAsync(LaunchPlan plan, CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(ProcessLauncher));
            if (plan.BaseUrl == null) throw new ArgumentException("BaseUrl cannot be null", nameof(plan));

            var wd = string.IsNullOrWhiteSpace(plan.StartInfo.WorkingDirectory) ? Environment.CurrentDirectory : plan.StartInfo.WorkingDirectory;
            _logger?.LogInformation("Starting process: {FileName} {Arguments}", plan.StartInfo.FileName, plan.StartInfo.Arguments);
            _logger?.LogInformation("Working directory: {WorkingDirectory}", wd);

            // Helpful when using Aspire AppHost
            var otlpGrpc = plan.StartInfo.EnvironmentVariables["DOTNET_DASHBOARD_OTLP_ENDPOINT_URL"];
            var otlpHttp = plan.StartInfo.EnvironmentVariables["DOTNET_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"];
            if (!string.IsNullOrEmpty(otlpGrpc) || !string.IsNullOrEmpty(otlpHttp))
            {
                _logger?.LogInformation("Aspire OTLP endpoints: gRPC={Grpc} HTTP={Http}", otlpGrpc ?? "<unset>", otlpHttp ?? "<unset>");
            }

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

            await plan.ReadinessProbe.WaitUntilReadyAsync(plan, _logger, cancellationToken);
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
                var streamName = isError ? "stderr" : "stdout";
                logger?.LogDebug(ex, "Error streaming process {Stream}", streamName);
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
