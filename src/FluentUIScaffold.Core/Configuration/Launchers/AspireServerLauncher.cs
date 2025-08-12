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
    /// Server launcher for Aspire App Host applications.
    /// Handles the specific requirements for launching Aspire applications with proper environment configuration.
    /// </summary>
    public class AspireServerLauncher : IServerLauncher
    {
        private readonly ILogger? _logger;
        private Process? _process;
        private bool _disposed;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IEnvVarProvider _envVarProvider;

        public AspireServerLauncher(ILogger? logger = null)
        {
            _logger = logger;
            _commandBuilder = new AspireCommandBuilder();
            _envVarProvider = new AspNetEnvVarProvider();
        }

        public string Name => "AspireServerLauncher";

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.Aspire;
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AspireServerLauncher));

            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            _logger?.LogInformation("Launching Aspire server with configuration: {ProjectPath}", configuration.ProjectPath);

            // Kill existing processes on the port
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port);

            // Build command arguments (reuse unified .NET builder to respect framework/configuration from builder)
            var arguments = _commandBuilder.BuildCommand(configuration);

            // Set up environment variables
            var environmentVariables = new Dictionary<string, string>(configuration.EnvironmentVariables);

            // Start the process
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = configuration.WorkingDirectory ?? Path.GetDirectoryName(configuration.ProjectPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Apply environment variables via provider
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

            _process = new Process { StartInfo = startInfo };

            _logger?.LogInformation("Starting Aspire process: dotnet {Arguments}", arguments);

            if (!_process.Start())
            {
                throw new InvalidOperationException("Failed to start Aspire server process");
            }

            // Wait for server to be ready
            await WaitForServerReadyAsync(configuration);

            _logger?.LogInformation("Aspire server is ready at {BaseUrl}", configuration.BaseUrl);
        }

        private static string AspNetServerLauncher_BuildCommandArguments(ServerConfiguration configuration)
        {
            var aspNetLauncher = typeof(AspNetServerLauncher);
            var buildMethod = aspNetLauncher.GetMethod("BuildCommandArguments", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (buildMethod == null)
            {
                // Fallback to minimal sensible defaults
                var arguments = new List<string> { "run", "--no-launch-profile" };
                arguments.AddRange(configuration.Arguments);
                return string.Join(" ", arguments);
            }
            return (string)buildMethod.Invoke(null, new object[] { configuration });
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
                            return;
                        }
                    }
                    catch
                    {
                        // ignore and retry
                    }
                }

                await Task.Delay(2000);
                attempt++;
            }

            throw new TimeoutException($"Aspire server did not become ready within {configuration.StartupTimeout}");
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
                            }
                            catch
                            {
                                // ignore kill issues in launcher
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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
