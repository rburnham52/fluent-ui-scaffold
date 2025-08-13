using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    /// <summary>
    /// Server launcher for Node.js applications.
    /// Handles launching Node.js applications with npm/yarn commands.
    /// </summary>
    public class NodeJsServerLauncher : IServerLauncher
    {
        private readonly ILogger? _logger;
        private Process? _process;
        private bool _disposed;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IEnvVarProvider _envVarProvider;
        private readonly IProcessRunner _processRunner;
        private readonly IClock _clock;
        private readonly IReadinessProbe _readinessProbe;

        public NodeJsServerLauncher(ILogger? logger = null, IProcessRunner? processRunner = null, IClock? clock = null, IReadinessProbe? readinessProbe = null)
        {
            _logger = logger;
            _commandBuilder = new NodeJsCommandBuilder();
            _envVarProvider = new AspNetEnvVarProvider();
            _processRunner = processRunner ?? new ProcessRunner();
            _clock = clock ?? new SystemClock();
            _readinessProbe = readinessProbe ?? new HttpReadinessProbe(null, _clock);
        }

        public string Name => "NodeJsServerLauncher";

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.NodeJs;
        }

        public LaunchPlan PlanLaunch(ServerConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));
            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            var arguments = _commandBuilder.BuildCommand(configuration);
            var startInfo = new ProcessStartInfo
            {
                FileName = "npm",
                Arguments = arguments,
                WorkingDirectory = configuration.WorkingDirectory ?? Path.GetDirectoryName(configuration.ProjectPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (System.Collections.DictionaryEntry kv in startInfo.EnvironmentVariables)
            {
                var key = kv.Key?.ToString();
                var value = kv.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(key)) env[key] = value;
            }
            _envVarProvider.Apply(env, configuration);
            foreach (var kv in env)
            {
                startInfo.EnvironmentVariables[kv.Key] = kv.Value;
            }

            var endpoints = configuration.HealthCheckEndpoints.Count > 0 ? configuration.HealthCheckEndpoints : new List<string> { "/" };
            return new LaunchPlan(startInfo, configuration.BaseUrl!, configuration.StartupTimeout, _readinessProbe, endpoints, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2), true);
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeJsServerLauncher));

            _logger?.LogInformation("Launching Node.js server with configuration: {ProjectPath}", configuration.ProjectPath);

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            // Kill existing processes on the port
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port);

            var plan = PlanLaunch(configuration);
            _logger?.LogInformation("Starting Node.js process: {File} {Arguments}", plan.StartInfo.FileName, plan.StartInfo.Arguments);
            var proc = _processRunner.Start(plan.StartInfo);

            // Wait for server to be ready
            await _readinessProbe.WaitUntilReadyAsync(configuration, _logger, plan.InitialDelay, plan.PollInterval);

            _logger?.LogInformation("Node.js server is ready at {BaseUrl}", configuration.BaseUrl);
        }

        private static string BuildCommandArguments(ServerConfiguration configuration)
        {
            var arguments = new List<string> { "start" };

            // Add custom arguments
            arguments.AddRange(configuration.Arguments);

            return string.Join(" ", arguments);
        }

        // readiness logic centralized in HttpReadinessProbe

        private async Task KillProcessesOnPortAsync(int port)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = $"-ano | findstr :{port}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 5 && int.TryParse(parts[4], out var pid))
                        {
                            try
                            {
                                var killProcess = Process.GetProcessById(pid);
                                killProcess.Kill();
                                _logger?.LogInformation("Killed process {PID} on port {Port}", pid, port);
                            }
                            catch (Exception ex)
                            {
                                _logger?.LogWarning(ex, "Failed to kill process {PID}", pid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to kill processes on port {Port}", port);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try { _process?.Kill(); } catch { }
                try { _process?.Dispose(); } catch { }
                _process = null;
                _disposed = true;
            }
        }
    }
}
