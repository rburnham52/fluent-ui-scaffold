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

        public NodeJsServerLauncher(ILogger? logger = null)
        {
            _logger = logger;
        }

        public string Name => "NodeJsServerLauncher";

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.NodeJs;
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NodeJsServerLauncher));

            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            _logger?.LogInformation("Launching Node.js server with configuration: {ProjectPath}", configuration.ProjectPath);

            // Kill existing processes on the port
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port);

            // Build command arguments
            var arguments = NodeJsServerLauncher.BuildCommandArguments(configuration);

            // Use environment variables from configuration (set by builder defaults)
            var environmentVariables = new Dictionary<string, string>(configuration.EnvironmentVariables);

            // Start the process
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

            // Add environment variables
            foreach (var envVar in environmentVariables)
            {
                startInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }

            _process = new Process { StartInfo = startInfo };

            _logger?.LogInformation("Starting Node.js process: npm {Arguments}", arguments);

            if (!_process.Start())
            {
                throw new InvalidOperationException("Failed to start Node.js server process");
            }

            // Wait for server to be ready
            await WaitForServerReadyAsync(configuration);

            _logger?.LogInformation("Node.js server is ready at {BaseUrl}", configuration.BaseUrl);
        }

        private static string BuildCommandArguments(ServerConfiguration configuration)
        {
            var arguments = new List<string> { "start" };

            // Add custom arguments
            arguments.AddRange(configuration.Arguments);

            return string.Join(" ", arguments);
        }

        private async Task WaitForServerReadyAsync(ServerConfiguration configuration)
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = configuration.StartupTimeout;

            var healthCheckEndpoints = configuration.HealthCheckEndpoints.Count > 0
                ? configuration.HealthCheckEndpoints
                : new List<string> { "/", "/health" };

            var maxAttempts = (int)(configuration.StartupTimeout.TotalSeconds / 2);
            var attempt = 0;

            while (attempt < maxAttempts)
            {
                foreach (var endpoint in healthCheckEndpoints)
                {
                    try
                    {
                        var url = new Uri(configuration.BaseUrl, endpoint);
                        var response = await httpClient.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger?.LogInformation("Server health check passed for endpoint: {Endpoint}", endpoint);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Health check failed for endpoint: {Endpoint}", endpoint);
                    }
                }

                await Task.Delay(2000); // Wait 2 seconds before next attempt
                attempt++;
            }

            throw new TimeoutException($"Node.js server did not become ready within {configuration.StartupTimeout}");
        }

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
                _process?.Kill();
                _process?.Dispose();
                _process = null;
                _disposed = true;
            }
        }
    }
}
