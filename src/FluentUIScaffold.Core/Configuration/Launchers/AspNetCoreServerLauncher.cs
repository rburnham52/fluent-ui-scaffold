using System;
using System.Collections.Generic; // Added missing import
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading; // Added missing import
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    /// <summary>
    /// Server launcher for ASP.NET Core applications with support for SPA proxy configuration.
    /// </summary>
    public class AspNetCoreServerLauncher : IServerLauncher
    {
        private readonly ILogger? _logger;
        private Process? _webServerProcess;
        private readonly HttpClient _httpClient;
        private bool _disposed;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IEnvVarProvider _envVarProvider;
        private readonly IProcessRunner _processRunner;
        private readonly IClock _clock;
        private readonly IReadinessProbe _readinessProbe;

        public string Name => "AspNetCoreServerLauncher";

        public AspNetCoreServerLauncher(ILogger? logger = null, IProcessRunner? processRunner = null, IClock? clock = null, IReadinessProbe? readinessProbe = null)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _commandBuilder = new AspNetCoreCommandBuilder();
            _envVarProvider = new AspNetEnvVarProvider();
            _processRunner = processRunner ?? new ProcessRunner();
            _clock = clock ?? new SystemClock();
            _readinessProbe = readinessProbe ?? new HttpReadinessProbe(null, _clock);
        }

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.AspNetCore;
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
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(configuration.ProjectPath),
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

            return new LaunchPlan(startInfo, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(200));
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AspNetCoreServerLauncher));

            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            _logger?.LogInformation("Launching ASP.NET Core server for project: {ProjectPath}", configuration.ProjectPath);
            _logger?.LogInformation("Expected base URL: {BaseUrl}", configuration.BaseUrl);

            // Kill any existing processes on the same port
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port);

            var plan = PlanLaunch(configuration);

            _logger?.LogInformation("Starting ASP.NET Core server with command: {Command} {Arguments}",
                plan.StartInfo.FileName, plan.StartInfo.Arguments);

            try
            {
                _ = _processRunner.Start(plan.StartInfo);
                _webServerProcess = Process.Start(plan.StartInfo);
                if (_webServerProcess == null)
                {
                    throw new InvalidOperationException("Failed to start ASP.NET Core server process.");
                }

                _logger?.LogInformation("ASP.NET Core server process started with PID: {ProcessId}", _webServerProcess.Id);

                // Start capturing output asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (!_webServerProcess.HasExited)
                        {
                            var line = await _webServerProcess.StandardOutput.ReadLineAsync();
                            if (line != null)
                            {
                                _logger?.LogDebug("Server output: {Output}", line);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Error reading server output");
                    }
                });

                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (!_webServerProcess.HasExited)
                        {
                            var line = await _webServerProcess.StandardError.ReadLineAsync();
                            if (line != null)
                            {
                                _logger?.LogWarning("Server error: {Error}", line);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Error reading server error output");
                    }
                });

                // Wait for the server to be ready
                await _readinessProbe.WaitUntilReadyAsync(configuration, _logger, plan.InitialDelay, plan.PollInterval);

                _logger?.LogInformation("ASP.NET Core server is ready and responding at {BaseUrl}", configuration.BaseUrl);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to start ASP.NET Core server");
                throw;
            }
        }

        private static string BuildCommandArguments(ServerConfiguration configuration)
        {
            // Use the exact same command format as the old WebServerLauncher
            return $"run --configuration Release --framework net8.0 --urls \"{configuration.BaseUrl}\" --no-launch-profile";
        }

        // readiness logic centralized in HttpReadinessProbe

        private async Task KillProcessesOnPortAsync(int port)
        {
            try
            {
                // Use netstat to find processes using the port
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

                    // Parse the output to find PIDs
                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 5 && int.TryParse(parts[4], out var pid))
                        {
                            try
                            {
                                var targetProcess = Process.GetProcessById(pid);
                                if (targetProcess.ProcessName.Contains("dotnet") || targetProcess.ProcessName.Contains("SampleApp"))
                                {
                                    _logger?.LogInformation("Killing existing process {ProcessName} (PID: {PID}) on port {Port}",
                                        targetProcess.ProcessName, pid, port);
                                    targetProcess.Kill();
                                    await targetProcess.WaitForExitAsync();
                                }
                            }
                            catch (ArgumentException)
                            {
                                // Process already terminated
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Failed to kill processes on port {Port}: {Message}", port, ex.Message);
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
                if (_webServerProcess != null && !_webServerProcess.HasExited)
                {
                    try
                    {
                        _logger?.LogInformation("Stopping ASP.NET Core server process");
                        _webServerProcess.Kill();
                        _webServerProcess.WaitForExit(5000); // Wait up to 5 seconds
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("Failed to stop ASP.NET Core server process: {Message}", ex.Message);
                    }
                    finally
                    {
                        _webServerProcess.Dispose();
                        _webServerProcess = null;
                    }
                }

                _httpClient?.Dispose();
                _disposed = true;
            }
        }
    }
}
