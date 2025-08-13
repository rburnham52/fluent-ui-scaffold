namespace FluentUIScaffold.Core.Configuration.Launchers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public sealed class LaunchPlan
    {
        public ProcessStartInfo StartInfo { get; }
        public Uri BaseUrl { get; }
        public TimeSpan StartupTimeout { get; }
        public IReadOnlyList<string> HealthCheckEndpoints { get; }
        public IReadinessProbe ReadinessProbe { get; }
        public TimeSpan InitialDelay { get; }
        public TimeSpan PollInterval { get; }
        public bool StreamProcessOutput { get; }

        public LaunchPlan(
            ProcessStartInfo startInfo,
            Uri baseUrl,
            TimeSpan startupTimeout,
            IReadinessProbe readinessProbe,
            IReadOnlyList<string> healthCheckEndpoints,
            TimeSpan initialDelay,
            TimeSpan pollInterval,
            bool streamProcessOutput = true)
        {
            StartInfo = startInfo;
            BaseUrl = baseUrl;
            StartupTimeout = startupTimeout;
            ReadinessProbe = readinessProbe;
            HealthCheckEndpoints = healthCheckEndpoints;
            InitialDelay = initialDelay;
            PollInterval = pollInterval;
            StreamProcessOutput = streamProcessOutput;
        }
    }
}
