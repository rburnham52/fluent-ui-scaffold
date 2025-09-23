using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using FluentUIScaffold.Core.Configuration.Launchers;

namespace FluentUIScaffold.Core.Server
{
    /// <summary>
    /// Computes deterministic hashes of server configurations to detect changes
    /// and determine if a server needs to be restarted.
    /// </summary>
    public static class ConfigHasher
    {
        /// <summary>
        /// Computes a deterministic hash of a LaunchPlan configuration.
        /// The hash includes all parameters that would affect server behavior.
        /// </summary>
        /// <param name="plan">The launch plan to hash</param>
        /// <returns>A deterministic string hash of the configuration</returns>
        public static string Compute(LaunchPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            var components = new List<string>
            {
                // Core startup info
                plan.StartInfo.FileName ?? "",
                plan.StartInfo.Arguments ?? "",
                plan.StartInfo.WorkingDirectory ?? "",
                
                // URL and timeout settings
                plan.BaseUrl.ToString(),
                plan.StartupTimeout.TotalMilliseconds.ToString("F0"),
                plan.InitialDelay.TotalMilliseconds.ToString("F0"),
                plan.PollInterval.TotalMilliseconds.ToString("F0"),
                
                // Health check configuration
                string.Join(",", plan.HealthCheckEndpoints.OrderBy(x => x)),
                plan.ReadinessProbe.GetType().FullName ?? "",
                plan.StreamProcessOutput.ToString()
            };

            // Environment variables (sorted for determinism)
            var envVars = plan.StartInfo.EnvironmentVariables.Cast<System.Collections.DictionaryEntry>()
                .OrderBy(kv => kv.Key.ToString())
                .Select(kv => $"{kv.Key}={kv.Value}");
            components.AddRange(envVars);

            // Combine all components with a separator
            var combined = string.Join("|", components);

            // Compute SHA256 hash and return as hex string
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Compares two configuration hashes for equality.
        /// </summary>
        /// <param name="hash1">First hash</param>
        /// <param name="hash2">Second hash</param>
        /// <returns>True if the hashes are equal</returns>
        public static bool Equals(string hash1, string hash2)
        {
            return string.Equals(hash1, hash2, StringComparison.OrdinalIgnoreCase);
        }
    }
}
