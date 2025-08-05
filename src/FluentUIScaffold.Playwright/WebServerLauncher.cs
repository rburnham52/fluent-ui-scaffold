using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// Handles launching and managing web servers for testing purposes.
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
        /// Launches a web server for the specified project path using Playwright-style configuration.
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

            // Start the web server process using Playwright-style configuration
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{projectPath}\" --urls \"{baseUrl.Scheme}://localhost:{baseUrl.Port}\" --no-launch-profile",
                WorkingDirectory = Path.GetDirectoryName(projectPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                EnvironmentVariables =
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "Development",
                    ["ASPNETCORE_URLS"] = $"{baseUrl.Scheme}://localhost:{baseUrl.Port}",
                    ["DOTNET_USE_POLLING_FILE_WATCHER"] = "1"
                }
            };

            _webServerProcess = new Process { StartInfo = startInfo };

            // Set up output handling
            _webServerProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger?.LogDebug("Web server output: {Output}", e.Data);
                }
            };

            _webServerProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger?.LogWarning("Web server error: {Error}", e.Data);
                }
            };

            // Start the process
            if (!_webServerProcess.Start())
            {
                throw new InvalidOperationException("Failed to start web server process.");
            }

            _webServerProcess.BeginOutputReadLine();
            _webServerProcess.BeginErrorReadLine();

            // Wait for the server to be ready
            await WaitForServerReadyAsync(baseUrl, timeout);
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

            while (DateTime.UtcNow - startTime < timeout)
            {
                attempt++;
                try
                {
                    var response = await _httpClient.GetAsync(baseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger?.LogInformation("Web server is ready after {Attempts} attempts", attempt);
                        return;
                    }
                    else
                    {
                        _logger?.LogDebug("Server responded with status {StatusCode} on attempt {Attempt}", response.StatusCode, attempt);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger?.LogDebug("Attempt {Attempt}: Server not ready yet ({Message})", attempt, ex.Message);
                }

                await Task.Delay(100); // Wait 100ms between attempts
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
