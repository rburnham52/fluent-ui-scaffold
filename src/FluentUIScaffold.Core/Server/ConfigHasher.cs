using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using FluentUIScaffold.Core.Configuration.Launchers;

namespace FluentUIScaffold.Core.Server
{
    public interface IConfigHasher
    {
        string Compute(LaunchPlan plan);
    }

    public sealed class ConfigHasher : IConfigHasher
    {
        public string Compute(LaunchPlan plan)
        {
            var sb = new StringBuilder();
            // Process identity
            sb.AppendLine(plan.StartInfo.FileName ?? string.Empty);
            sb.AppendLine(plan.StartInfo.Arguments ?? string.Empty);
            sb.AppendLine(plan.StartInfo.WorkingDirectory ?? string.Empty);

            // Environment variables (sorted)
            var keys = plan.StartInfo.EnvironmentVariables.Keys.Cast<string>().OrderBy(k => k, StringComparer.Ordinal);
            foreach (var k in keys)
            {
                sb.Append(k);
                sb.Append('=');
                sb.AppendLine(plan.StartInfo.EnvironmentVariables[k] ?? string.Empty);
            }

            // Readiness and timing
            sb.AppendLine(plan.BaseUrl.ToString());
            foreach (var ep in plan.HealthCheckEndpoints)
            {
                sb.AppendLine(ep ?? string.Empty);
            }
            sb.AppendLine(plan.StartupTimeout.ToString());
            sb.AppendLine(plan.InitialDelay.ToString());
            sb.AppendLine(plan.PollInterval.ToString());
            sb.AppendLine(plan.StreamProcessOutput ? "1" : "0");
            sb.AppendLine(plan.ForceRestartOnConfigChange ? "1" : "0");
            sb.AppendLine(plan.KillOrphansOnStart ? "1" : "0");

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}

