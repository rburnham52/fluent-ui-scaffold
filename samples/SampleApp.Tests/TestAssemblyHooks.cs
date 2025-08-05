using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests
{
    /// <summary>
    /// Handles assembly-level initialization and cleanup for the test suite.
    /// Starts and stops the sample app server before and after all tests.
    /// </summary>
    [TestClass]
    public class TestAssemblyHooks
    {
        private static Process? _serverProcess;
        private static readonly object _lockObject = new object();
        private static bool _serverStarted = false;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            StartServerAsync().Wait();
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            StopServerAsync().Wait();
        }

        private static async Task StartServerAsync()
        {
            lock (_lockObject)
            {
                if (_serverStarted)
                    return;
            }

            try
            {
                // Print the current working directory for debugging
                Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");

                // Find the repo root by walking up from AppContext.BaseDirectory
                string? repoRoot = AppContext.BaseDirectory;
                while (repoRoot != null && !System.IO.Directory.Exists(System.IO.Path.Combine(repoRoot, ".git")))
                {
                    repoRoot = System.IO.Directory.GetParent(repoRoot)?.FullName;
                }
                if (repoRoot == null)
                {
                    throw new InvalidOperationException("Could not find repo root (directory containing .git)");
                }
                var projectPath = System.IO.Path.Combine(repoRoot, "samples", "SampleApp", "SampleApp.csproj");
                Console.WriteLine($"Repo root: {repoRoot}");
                Console.WriteLine($"Using project path: {projectPath}");

                // Start the ASP.NET Core server which will also start the Vite dev server
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project \"{projectPath}\" --configuration Release --framework net8.0",
                    WorkingDirectory = Environment.CurrentDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                lock (_lockObject)
                {
                    _serverProcess = new Process { StartInfo = startInfo };

                    // Add event handlers to capture output
                    _serverProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Console.WriteLine($"Server Output: {e.Data}");
                    };
                    _serverProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Console.WriteLine($"Server Error: {e.Data}");
                    };

                    _serverProcess.Start();
                    _serverProcess.BeginOutputReadLine();
                    _serverProcess.BeginErrorReadLine();
                }

                // Wait for the server to be ready
                var maxWaitTime = TimeSpan.FromSeconds(30); // Reduced timeout to 60 seconds
                var startTime = DateTime.UtcNow;
                var serverReady = false;
                var attempts = 0;

                Console.WriteLine("Waiting for sample app server to start...");
                Console.WriteLine($"Server process started with PID: {_serverProcess?.Id}");

                while (!serverReady && DateTime.UtcNow - startTime < maxWaitTime)
                {
                    attempts++;
                    try
                    {
                        using var client = new System.Net.Http.HttpClient();
                        client.Timeout = TimeSpan.FromSeconds(5);

                        // Handle SSL certificate validation for development
                        var handler = new System.Net.Http.HttpClientHandler();
                        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                        using var httpClient = new System.Net.Http.HttpClient(handler);
                        httpClient.Timeout = TimeSpan.FromSeconds(5);

                        var response = await httpClient.GetAsync(TestConfiguration.BaseUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            serverReady = true;
                            Console.WriteLine($"Server is ready after {attempts} attempts");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Server responded with status {response.StatusCode} on attempt {attempts}");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Server not ready yet, wait a bit more
                        Console.WriteLine($"Attempt {attempts}: Server not ready yet ({ex.Message})");
                        await Task.Delay(3000); // Increased delay to 3 seconds
                    }
                }

                if (!serverReady)
                {
                    throw new InvalidOperationException("Sample app server failed to start within the expected time.");
                }

                lock (_lockObject)
                {
                    _serverStarted = true;
                }
                Console.WriteLine("Sample app server started successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start sample app server: {ex.Message}");
                throw;
            }
        }

        private static Task StopServerAsync()
        {
            lock (_lockObject)
            {
                if (!_serverStarted || _serverProcess == null)
                    return Task.CompletedTask;

                try
                {
                    if (!_serverProcess.HasExited)
                    {
                        _serverProcess.Kill();
                        _serverProcess.WaitForExit(5000);
                    }
                    _serverProcess.Dispose();
                    _serverProcess = null;
                    _serverStarted = false;
                    Console.WriteLine("Sample app server stopped successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to stop sample app server: {ex.Message}");
                }
            }
            return Task.CompletedTask;
        }
    }
}
