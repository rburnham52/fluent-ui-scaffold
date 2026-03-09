using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Helper class to detect test environment capabilities and determine which tests can run.
    /// </summary>
    public static class TestEnvironmentHelper
    {
        private static readonly Lazy<bool> _isDockerAvailable = new Lazy<bool>(CheckDockerAvailability);

        /// <summary>
        /// Gets a value indicating whether Docker is available in the current environment.
        /// </summary>
        public static bool IsDockerAvailable => _isDockerAvailable.Value;

        /// <summary>
        /// Gets a value indicating whether Aspire tests can run.
        /// Only requires Docker — Aspire is consumed via NuGet packages (the workload is deprecated).
        /// </summary>
        public static bool CanRunAspireTests => IsDockerAvailable;

        /// <summary>
        /// Gets a descriptive message about the current test environment capabilities.
        /// </summary>
        public static string GetEnvironmentStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine("=== Test Environment Status ===");
            status.AppendLine($"Docker Available: {IsDockerAvailable}");
            status.AppendLine($"Can Run Aspire Tests: {CanRunAspireTests}");
            status.AppendLine($"OS: {RuntimeInformation.OSDescription}");
            status.AppendLine($"Architecture: {RuntimeInformation.OSArchitecture}");
            status.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");

            if (!IsDockerAvailable)
            {
                status.AppendLine();
                status.AppendLine("Docker is not available. Aspire tests will be skipped.");
                status.AppendLine("   To enable Aspire tests:");
                status.AppendLine("   - Install Docker Desktop, Rancher Desktop, or Docker Engine");
                status.AppendLine("   - Ensure Docker daemon is running");
                status.AppendLine("   - Verify 'docker info' command works");
            }

            return status.ToString();
        }

        private static bool CheckDockerAvailability()
        {
            try
            {
                // Try to run 'docker info' command
                var startInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null) return false;

                process.WaitForExit(5000); // 5 second timeout

                // Docker info returns 0 on success
                return process.ExitCode == 0;
            }
            catch
            {
                // If we can't start the process or get an exception, Docker is not available
                return false;
            }
        }

    }
}
