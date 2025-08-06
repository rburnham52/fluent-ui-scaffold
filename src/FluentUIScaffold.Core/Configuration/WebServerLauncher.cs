using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Handles launching and managing web servers for testing purposes.
    /// This is a framework-agnostic web server launcher that can be used with any UI testing framework.
    /// </summary>
    public class WebServerLauncher : IDisposable
    {
        private readonly ILogger<WebServerLauncher>? _logger;
        private Process? _webServerProcess;
        private readonly HttpClient _httpClient;
        private bool _disposed;

        public WebServerLauncher(ILogger? logger = null)
        {
            _logger = logger as ILogger<WebServerLauncher>;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Launches a web server for the specified project path.
        /// </summary>
        /// <param name="projectPath">The path to the ASP.NET Core project.</param>
        /// <param name="baseUrl">The base URL where the server should be accessible.</param>
        /// <param name="timeout">The timeout for waiting for the server to start.</param>
        /// <returns>A task that completes when the server is ready.</returns>
        public async Task LaunchWebServerAsync(string projectPath, Uri baseUrl, TimeSpan timeout)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(projectPath));
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl));

            _logger?.LogInformation("Launching web server for project: {ProjectPath}", projectPath);
            _logger?.LogInformation("Expected base URL: {BaseUrl}", baseUrl);

            // Kill any existing processes on the same port
            await KillProcessesOnPortAsync(baseUrl.Port);

            // Start the web server process in release mode to avoid SPA proxy
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --configuration Release --framework net8.0 --urls \"{baseUrl}\" --no-launch-profile",
                WorkingDirectory = Path.GetDirectoryName(projectPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Disable SPA proxy for testing by setting environment variable to empty
            startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "";

            _logger?.LogInformation("Starting web server with command: {Command} {Arguments}", startInfo.FileName, startInfo.Arguments);

            try
            {
                _webServerProcess = Process.Start(startInfo);
                if (_webServerProcess == null)
                {
                    throw new InvalidOperationException("Failed to start web server process.");
                }

                _logger?.LogInformation("Web server process started with PID: {ProcessId}", _webServerProcess.Id);

                // Wait for the server to be ready
                await WaitForServerReadyAsync(baseUrl, timeout);

                _logger?.LogInformation("Web server is ready and responding at {BaseUrl}", baseUrl);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to start web server");
                throw;
            }
        }

        /// <summary>
        /// Waits for the web server to be ready by polling the base URL.
        /// </summary>
        /// <param name="baseUrl">The base URL to check.</param>
        /// <param name="timeout">The timeout for waiting.</param>
        /// <returns>A task that completes when the server is ready.</returns>
        private async Task WaitForServerReadyAsync(Uri baseUrl, TimeSpan timeout)
        {
            _logger?.LogInformation("Waiting for web server to be ready at {BaseUrl}", baseUrl);

            var startTime = DateTime.UtcNow;
            var attempt = 0;
            var maxAttempts = (int)(timeout.TotalMilliseconds / 100);

            // Try the base URL and the weather API endpoint that we know exists
            var testUrls = new[]
            {
                baseUrl,
                new Uri($"{baseUrl.Scheme}://{baseUrl.Host}:{baseUrl.Port}/api/weather")
            };

            while (DateTime.UtcNow - startTime < timeout)
            {
                attempt++;
                bool serverReady = false;

                foreach (var testUrl in testUrls)
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(testUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            _logger?.LogInformation("Web server is ready after {Attempts} attempts at {TestUrl}", attempt, testUrl);
                            return;
                        }
                        else
                        {
                            _logger?.LogDebug("Server responded with status {StatusCode} on attempt {Attempt} at {TestUrl}", response.StatusCode, attempt, testUrl);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logger?.LogDebug("Attempt {Attempt}: Server not ready yet at {TestUrl} ({Message})", attempt, testUrl, ex.Message);
                    }
                }

                // Log progress every 10 attempts
                if (attempt % 10 == 0)
                {
                    var elapsed = DateTime.UtcNow - startTime;
                    _logger?.LogInformation("Still waiting for server... Attempt {Attempt}/{MaxAttempts}, Elapsed: {Elapsed:F1}s",
                        attempt, maxAttempts, elapsed.TotalSeconds);
                }

                await Task.Delay(100); // Wait 100ms between attempts
            }

            // Check if the process is still running
            if (_webServerProcess != null && !_webServerProcess.HasExited)
            {
                _logger?.LogWarning("Web server process is still running but not responding. Process ID: {PID}", _webServerProcess.Id);
            }
            else if (_webServerProcess != null && _webServerProcess.HasExited)
            {
                _logger?.LogError("Web server process has exited with code: {ExitCode}", _webServerProcess.ExitCode);
            }

            throw new TimeoutException($"Web server failed to start within {timeout.TotalSeconds} seconds after {attempt} attempts.");
        }

        /// <summary>
        /// Kills any processes running on the specified port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <returns>A task that completes when the processes are killed.</returns>
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

        /// <summary>
        /// Disposes the web server launcher and stops the web server process.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the web server launcher and stops the web server process.
        /// </summary>
        /// <param name="disposing">True if disposing; false if finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_webServerProcess != null && !_webServerProcess.HasExited)
                {
                    try
                    {
                        _logger?.LogInformation("Stopping web server process");
                        _webServerProcess.Kill();
                        _webServerProcess.WaitForExit(5000); // Wait up to 5 seconds
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning("Failed to stop web server process: {Message}", ex.Message);
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
