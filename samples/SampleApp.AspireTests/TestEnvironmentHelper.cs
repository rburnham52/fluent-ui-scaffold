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
        private static readonly Lazy<bool> _isAspireWorkloadInstalled = new Lazy<bool>(CheckAspireWorkloadInstalled);

        /// <summary>
        /// Gets a value indicating whether Docker is available in the current environment.
        /// </summary>
        public static bool IsDockerAvailable => _isDockerAvailable.Value;

        /// <summary>
        /// Gets a value indicating whether the Aspire workload is installed.
        /// </summary>
        public static bool IsAspireWorkloadInstalled => _isAspireWorkloadInstalled.Value;

        /// <summary>
        /// Gets a value indicating whether Aspire tests can run (Docker + Aspire workload).
        /// </summary>
        public static bool CanRunAspireTests => IsDockerAvailable && IsAspireWorkloadInstalled;

        /// <summary>
        /// Gets a descriptive message about the current test environment capabilities.
        /// </summary>
        public static string GetEnvironmentStatus()
        {
            var status = new System.Text.StringBuilder();
            status.AppendLine("=== Test Environment Status ===");
            status.AppendLine($"Docker Available: {IsDockerAvailable}");
            status.AppendLine($"Aspire Workload Installed: {IsAspireWorkloadInstalled}");
            status.AppendLine($"Can Run Aspire Tests: {CanRunAspireTests}");
            status.AppendLine($"OS: {RuntimeInformation.OSDescription}");
            status.AppendLine($"Architecture: {RuntimeInformation.OSArchitecture}");
            status.AppendLine($"Framework: {RuntimeInformation.FrameworkDescription}");

            if (!IsDockerAvailable)
            {
                status.AppendLine();
                status.AppendLine("⚠️  Docker is not available. Aspire tests will be skipped.");
                status.AppendLine("   To enable Aspire tests:");
                status.AppendLine("   - Install Docker Desktop or Docker Engine");
                status.AppendLine("   - Ensure Docker daemon is running");
                status.AppendLine("   - Verify 'docker info' command works");
            }

            if (!IsAspireWorkloadInstalled)
            {
                status.AppendLine();
                status.AppendLine("⚠️  Aspire workload is not installed.");
                status.AppendLine("   To install: dotnet workload install aspire");
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

        private static bool CheckAspireWorkloadInstalled()
        {
            try
            {
                // Try to run 'dotnet workload list' and check for aspire
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "workload list",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null) return false;

                process.WaitForExit(10000); // 10 second timeout

                if (process.ExitCode != 0) return false;

                var output = process.StandardOutput.ReadToEnd();
                return output.Contains("aspire", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                // If we can't check workloads, assume it's not installed
                return false;
            }
        }
    }
}
