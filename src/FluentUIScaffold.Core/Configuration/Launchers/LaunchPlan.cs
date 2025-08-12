namespace FluentUIScaffold.Core.Configuration.Launchers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public sealed class LaunchPlan
    {
        public ProcessStartInfo StartInfo { get; }
        public TimeSpan InitialDelay { get; }
        public TimeSpan PollInterval { get; }

        public LaunchPlan(ProcessStartInfo startInfo, TimeSpan initialDelay, TimeSpan pollInterval)
        {
            StartInfo = startInfo;
            InitialDelay = initialDelay;
            PollInterval = pollInterval;
        }
    }
}
