using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    /// <summary>
    /// Unified server launcher for ASP.NET Core and Aspire applications.
    /// Handles both server types with different configuration options.
    /// </summary>
    public class AspNetServerLauncher : IServerLauncher
    {
        private readonly ILogger? _logger;
        private Process? _webServerProcess;
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public string Name => "AspNetServerLauncher";
        private static readonly string[] collection = new[] { "--framework", "net8.0" };

        public AspNetServerLauncher(ILogger? logger = null)
        {
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.AspNetCore ||
                   configuration.ServerType == ServerType.Aspire;
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AspNetServerLauncher));

            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            _logger?.LogInformation("Launching {ServerType} server for project: {ProjectPath}",
                configuration.ServerType, configuration.ProjectPath);
            _logger?.LogInformation("Expected base URL: {BaseUrl}", configuration.BaseUrl);

            // Kill any existing processes on the same port
            // Be conservative: only terminate processes that match the configured process name, avoid killing generic dotnet hosts
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port, configuration.ProcessName);

            // Build the command arguments
            var arguments = AspNetServerLauncher.BuildCommandArguments(configuration);

            // Create the process start info
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

            // Set environment variables
            foreach (var envVar in configuration.EnvironmentVariables)
            {
                startInfo.EnvironmentVariables[envVar.Key] = envVar.Value;
            }

            // Set server-specific environment variables
            AspNetServerLauncher.SetServerSpecificEnvironmentVariables(startInfo, configuration);

            _logger?.LogInformation("Starting {ServerType} server with command: {Command} {Arguments}",
                configuration.ServerType, startInfo.FileName, startInfo.Arguments);

            try
            {
                _webServerProcess = Process.Start(startInfo);
                if (_webServerProcess == null)
                {
                    throw new InvalidOperationException($"Failed to start {configuration.ServerType} server process.");
                }

                _logger?.LogInformation("{ServerType} server process started with PID: {ProcessId}",
                    configuration.ServerType, _webServerProcess.Id);

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

                _logger?.LogInformation("{ServerType} server is ready and responding at {BaseUrl}",
                    configuration.ServerType, configuration.BaseUrl);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to start {ServerType} server", configuration.ServerType);
                throw;
            }
        }

        private static void SetServerSpecificEnvironmentVariables(ProcessStartInfo startInfo, ServerConfiguration configuration)
        {
            // The builder defaults handle all standard environment variables.
            // This method is now only for server-specific overrides that can't be handled by the builder.

            switch (configuration.ServerType)
            {
                case ServerType.AspNetCore:
                    // Ensure the app binds to the configured base URL
                    if (configuration.BaseUrl != null)
                    {
                        startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = configuration.BaseUrl.ToString();
                    }
                    break;

                case ServerType.Aspire:
                    // Aspire-specific overrides can be added here if needed
                    // Currently, all environment variables are handled by the builder defaults
                    break;
            }
        }

        private static string BuildCommandArguments(ServerConfiguration configuration)
        {
            // Build the base command
            var arguments = new List<string> { "run" };

            // Add framework and configuration from arguments (set by the builder)
            var frameworkIndex = configuration.Arguments.IndexOf("--framework");
            var configurationIndex = configuration.Arguments.IndexOf("--configuration");

            if (frameworkIndex >= 0 && frameworkIndex + 1 < configuration.Arguments.Count)
            {
                arguments.AddRange(new[] { "--framework", configuration.Arguments[frameworkIndex + 1] });
            }
            else
            {
                arguments.AddRange(collection); // Default
            }

            if (configurationIndex >= 0 && configurationIndex + 1 < configuration.Arguments.Count)
            {
                arguments.AddRange(new[] { "--configuration", configuration.Arguments[configurationIndex + 1] });
            }
            else
            {
                arguments.AddRange(new[] { "--configuration", "Release" }); // Default
            }

            // URL configuration is provided via ASPNETCORE_URLS env var

            arguments.Add("--no-launch-profile");

            // Add any additional custom arguments (excluding the ones we already processed)
            var customArguments = new List<string>();
            for (int i = 0; i < configuration.Arguments.Count; i++)
            {
                if (configuration.Arguments[i] == "--framework" || configuration.Arguments[i] == "--configuration")
                {
                    i++; // Skip the value too
                    continue;
                }
                customArguments.Add(configuration.Arguments[i]);
            }
            arguments.AddRange(customArguments);

            return string.Join(" ", arguments);
        }

        private async Task WaitForServerReadyAsync(ServerConfiguration configuration)
        {
            _logger?.LogInformation("Waiting for {ServerType} server to be ready at {BaseUrl}",
                configuration.ServerType, configuration.BaseUrl);

            var startTime = DateTime.UtcNow;
            var attempt = 0;
            var maxAttempts = (int)(configuration.StartupTimeout.TotalMilliseconds / 200);

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
                    _logger?.LogError("{ServerType} server process has exited with code: {ExitCode}",
                        configuration.ServerType, exitCode);
                    throw new InvalidOperationException($"{configuration.ServerType} server process exited with code {exitCode}");
                }

                foreach (var testUrl in testUrls)
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        var response = await _httpClient.GetAsync(testUrl, cts.Token);

                        if (response.IsSuccessStatusCode)
                        {
                            _logger?.LogInformation("{ServerType} server is ready after {Attempts} attempts at {TestUrl}",
                                configuration.ServerType, attempt, testUrl);
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
                    _logger?.LogInformation("Still waiting for {ServerType} server... Attempt {Attempt}/{MaxAttempts}, Elapsed: {Elapsed:F1}s",
                        configuration.ServerType, attempt, maxAttempts, elapsed.TotalSeconds);
                }

                await Task.Delay(200);
            }

            // Check if the process is still running
            if (_webServerProcess != null && !_webServerProcess.HasExited)
            {
                _logger?.LogWarning("{ServerType} server process is still running but not responding. Process ID: {PID}",
                    configuration.ServerType, _webServerProcess.Id);
            }
            else if (_webServerProcess != null && _webServerProcess.HasExited)
            {
                _logger?.LogError("{ServerType} server process has exited with code: {ExitCode}",
                    configuration.ServerType, _webServerProcess.ExitCode);
            }

            throw new TimeoutException($"{configuration.ServerType} server failed to start within {configuration.StartupTimeout.TotalSeconds} seconds after {attempt} attempts.");
        }

        private async Task KillProcessesOnPortAsync(int port, string processName)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "netstat",
                    Arguments = $"-ano | findstr :{port}",
                    UseShellExecute = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                var result = await PortProcessFinder.FindProcessesOnPortAsync(port);
                // using var process = Process.Start(startInfo);
                if (result != null)
                {
                    // var output = await process.StandardOutput.ReadToEndAsync();
                    var output = result; // Use the output from PortProcessFinder
                    // await process.WaitForExitAsync();

                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 5 && int.TryParse(parts[4], out var pid))
                        {
                            try
                            {
                                var targetProcess = Process.GetProcessById(pid);
                                if (!string.IsNullOrWhiteSpace(processName) && targetProcess.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase))
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
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_webServerProcess != null && !_webServerProcess.HasExited)
                {
                    try
                    {
                        _logger?.LogInformation("Stopping ASP.NET server process");
                        _webServerProcess.Kill();
                        _webServerProcess.WaitForExit(5000);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("Failed to stop ASP.NET server process: {Message}", ex.Message);
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
