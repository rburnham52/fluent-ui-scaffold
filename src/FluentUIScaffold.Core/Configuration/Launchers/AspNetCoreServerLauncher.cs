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

        public string Name => "AspNetCoreServerLauncher";

        public AspNetCoreServerLauncher(ILogger? logger = null)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _commandBuilder = new AspNetCoreCommandBuilder();
            _envVarProvider = new AspNetEnvVarProvider();
        }

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.AspNetCore;
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            _logger?.LogInformation("Launching ASP.NET Core server for project: {ProjectPath}", configuration.ProjectPath);
            _logger?.LogInformation("Expected base URL: {BaseUrl}", configuration.BaseUrl);

            // Kill any existing processes on the same port
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port);

            // Build the command arguments
            var arguments = _commandBuilder.BuildCommand(configuration);

            // Create the process start info
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = Path.GetDirectoryName(configuration.ProjectPath), // Use project directory like old launcher
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

            _logger?.LogInformation("Starting ASP.NET Core server with command: {Command} {Arguments}",
                startInfo.FileName, startInfo.Arguments);

            try
            {
                _webServerProcess = Process.Start(startInfo);
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
                await WaitForServerReadyAsync(configuration);

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

        private async Task WaitForServerReadyAsync(ServerConfiguration configuration)
        {
            _logger?.LogInformation("Waiting for ASP.NET Core server to be ready at {BaseUrl}", configuration.BaseUrl);

            var startTime = DateTime.UtcNow;
            var attempt = 0;
            var maxAttempts = (int)(configuration.StartupTimeout.TotalMilliseconds / 200); // Check every 200ms instead of 100ms

            // Build test URLs from health check endpoints
            var testUrls = new List<Uri> { configuration.BaseUrl };
            foreach (var endpoint in configuration.HealthCheckEndpoints)
            {
                if (!string.IsNullOrEmpty(endpoint))
                {
                    var uri = endpoint.StartsWith("/")
                        ? new Uri(configuration.BaseUrl, endpoint)
                        : new Uri($"{configuration.BaseUrl}{endpoint}");
                    testUrls.Add(uri);
                }
            }

            // Add a small delay before starting health checks to give the server time to start
            await Task.Delay(2000);

            while (DateTime.UtcNow - startTime < configuration.StartupTimeout)
            {
                attempt++;

                // Check if the process is still running
                if (_webServerProcess?.HasExited == true)
                {
                    var exitCode = _webServerProcess.ExitCode;
                    _logger?.LogError("ASP.NET Core server process has exited with code: {ExitCode}", exitCode);
                    throw new InvalidOperationException($"ASP.NET Core server process exited with code {exitCode}");
                }

                bool serverReady = false;

                foreach (var testUrl in testUrls)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5 second timeout per request
                        var response = await _httpClient.GetAsync(testUrl, cts.Token);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger?.LogInformation("ASP.NET Core server is ready after {Attempts} attempts at {TestUrl}",
                                attempt, testUrl);
                            return;
                        }
                        else
                        {
                            _logger?.LogDebug("Server responded with status {StatusCode} on attempt {Attempt} at {TestUrl}",
                                response.StatusCode, attempt, testUrl);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger?.LogDebug("Attempt {Attempt}: Server not ready yet at {TestUrl} ({Message})",
                            attempt, testUrl, ex.Message);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger?.LogDebug("Attempt {Attempt}: Request timeout at {TestUrl}", attempt, testUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug("Attempt {Attempt}: Unexpected error checking {TestUrl}: {Message}",
                            attempt, testUrl, ex.Message);
                    }
                }

                // Log progress every 5 attempts
                if (attempt % 5 == 0)
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    _logger?.LogInformation("Still waiting for ASP.NET Core server... Attempt {Attempt}/{MaxAttempts}, Elapsed: {Elapsed:F1}s",
                        attempt, maxAttempts, elapsed.TotalSeconds);
                }

                await Task.Delay(200); // Wait 200ms between attempts
            }

            // Check if the process is still running
            if (_webServerProcess != null && !_webServerProcess.HasExited)
            {
                _logger?.LogWarning("ASP.NET Core server process is still running but not responding. Process ID: {PID}", _webServerProcess.Id);
            }
            else if (_webServerProcess != null && _webServerProcess.HasExited)
            {
                _logger?.LogError("ASP.NET Core server process has exited with code: {ExitCode}", _webServerProcess.ExitCode);
            }

            throw new TimeoutException($"ASP.NET Core server failed to start within {configuration.StartupTimeout.TotalSeconds} seconds after {attempt} attempts.");
        }

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
