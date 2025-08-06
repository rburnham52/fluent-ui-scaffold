using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.Tests.Examples
{
    [TestClass]
    public class WebServerLaunchTest
    {
        [TestMethod]
        public async Task Can_Launch_WebServer_Manually()
        {
            // Arrange
            string? repoRoot = AppContext.BaseDirectory;
            while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
            {
                repoRoot = Directory.GetParent(repoRoot)?.FullName;
            }

            if (repoRoot == null)
            {
                throw new InvalidOperationException("Could not find repo root (directory containing .git)");
            }

            var projectPath = Path.Combine(repoRoot, "samples", "SampleApp", "SampleApp.csproj");
            Console.WriteLine($"Repo root: {repoRoot}");
            Console.WriteLine($"Using project path: {projectPath}");
            Console.WriteLine($"Project exists: {File.Exists(projectPath)}");

            var baseUrl = new Uri("http://localhost:5000");
            var timeout = TimeSpan.FromSeconds(30);

            // Act & Assert
            using var launcher = new WebServerLauncher();

            try
            {
                Console.WriteLine("Starting web server...");
                await launcher.LaunchWebServerAsync(projectPath, baseUrl, timeout);
                Console.WriteLine("Web server started successfully!");

                // Test that the server is responding
                using var httpClient = new System.Net.Http.HttpClient();
                var response = await httpClient.GetAsync(baseUrl);
                Console.WriteLine($"Server response status: {response.StatusCode}");

                Assert.IsTrue(response.IsSuccessStatusCode, "Server should respond with success status code");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start web server: {ex.Message}");
                Console.WriteLine($"Exception type: {ex.GetType().Name}");
                throw;
            }
        }

        [TestMethod]
        public async Task Can_Test_WebServer_Process_Start()
        {
            // Arrange
            string? repoRoot = AppContext.BaseDirectory;
            while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
            {
                repoRoot = Directory.GetParent(repoRoot)?.FullName;
            }

            if (repoRoot == null)
            {
                throw new InvalidOperationException("Could not find repo root (directory containing .git)");
            }

            var projectPath = Path.Combine(repoRoot, "samples", "SampleApp", "SampleApp.csproj");
            Console.WriteLine($"Testing project path: {projectPath}");
            Console.WriteLine($"Project exists: {File.Exists(projectPath)}");

            // Test if we can run dotnet build
            try
            {
                var buildProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"build \"{projectPath}\"",
                        WorkingDirectory = Path.GetDirectoryName(projectPath),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                Console.WriteLine("Testing dotnet build...");
                buildProcess.Start();
                var buildOutput = await buildProcess.StandardOutput.ReadToEndAsync();
                var buildError = await buildProcess.StandardError.ReadToEndAsync();
                await buildProcess.WaitForExitAsync();

                Console.WriteLine($"Build exit code: {buildProcess.ExitCode}");
                Console.WriteLine($"Build output: {buildOutput}");
                if (!string.IsNullOrEmpty(buildError))
                {
                    Console.WriteLine($"Build errors: {buildError}");
                }

                Assert.AreEqual(0, buildProcess.ExitCode, "Project should build successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Build failed: {ex.Message}");
                throw;
            }
        }

        [TestMethod]
        public async Task Can_Test_WebServer_Process_Start_Detailed()
        {
            // Arrange
            string? repoRoot = AppContext.BaseDirectory;
            while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
            {
                repoRoot = Directory.GetParent(repoRoot)?.FullName;
            }

            if (repoRoot == null)
            {
                throw new InvalidOperationException("Could not find repo root (directory containing .git)");
            }

            var projectPath = Path.Combine(repoRoot, "samples", "SampleApp", "SampleApp.csproj");
            Console.WriteLine($"Testing project path: {projectPath}");
            Console.WriteLine($"Project exists: {File.Exists(projectPath)}");

            // Test if we can run dotnet run directly
            try
            {
                var runProcess = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "dotnet",
                        Arguments = $"run --project \"{projectPath}\" --urls \"http://localhost:5001\" --no-launch-profile --framework net8.0",
                        WorkingDirectory = Path.GetDirectoryName(projectPath),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        EnvironmentVariables =
                        {
                            ["ASPNETCORE_ENVIRONMENT"] = "Development",
                            ["ASPNETCORE_URLS"] = "http://localhost:5001",
                            ["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "Microsoft.AspNetCore.SpaProxy",
                            ["DOTNET_USE_POLLING_FILE_WATCHER"] = "1"
                        }
                    }
                };

                Console.WriteLine("Testing dotnet run...");
                runProcess.Start();

                // Set up real-time output capture
                var outputLines = new List<string>();
                var errorLines = new List<string>();

                // Start capturing output in background tasks
                var outputTask = Task.Run(async () =>
                {
                    while (!runProcess.StandardOutput.EndOfStream)
                    {
                        var line = await runProcess.StandardOutput.ReadLineAsync();
                        if (line != null)
                        {
                            outputLines.Add(line);
                            Console.WriteLine($"[SERVER OUTPUT] {line}");
                        }
                    }
                });

                var errorTask = Task.Run(async () =>
                {
                    while (!runProcess.StandardError.EndOfStream)
                    {
                        var line = await runProcess.StandardError.ReadLineAsync();
                        if (line != null)
                        {
                            errorLines.Add(line);
                            Console.WriteLine($"[SERVER ERROR] {line}");
                        }
                    }
                });

                // Wait a bit for the process to start and show initial output
                await Task.Delay(3000);

                // Check if process is still running
                if (!runProcess.HasExited)
                {
                    Console.WriteLine($"Process is running with PID: {runProcess.Id}");

                    // Try to connect to the server
                    using var httpClient = new System.Net.Http.HttpClient();
                    try
                    {
                        var response = await httpClient.GetAsync("http://localhost:5001/api/weather");
                        Console.WriteLine($"Server response status: {response.StatusCode}");
                        Assert.IsTrue(response.IsSuccessStatusCode, "Server should respond with success status code");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to connect to server: {ex.Message}");
                    }

                    // Kill the process
                    runProcess.Kill();
                    await runProcess.WaitForExitAsync();
                }
                else
                {
                    Console.WriteLine($"Process exited with code: {runProcess.ExitCode}");
                    Console.WriteLine($"Total output lines captured: {outputLines.Count}");
                    Console.WriteLine($"Total error lines captured: {errorLines.Count}");

                    if (outputLines.Count > 0)
                    {
                        Console.WriteLine("Final output summary:");
                        foreach (var line in outputLines.TakeLast(10)) // Show last 10 lines
                        {
                            Console.WriteLine($"  {line}");
                        }
                    }

                    if (errorLines.Count > 0)
                    {
                        Console.WriteLine("Final error summary:");
                        foreach (var line in errorLines.TakeLast(10)) // Show last 10 lines
                        {
                            Console.WriteLine($"  {line}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Run failed: {ex.Message}");
                throw;
            }
        }
    }
}
