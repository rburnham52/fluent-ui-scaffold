using System;

namespace FluentUIScaffold.Core.Hosting
{
    /// <summary>
    /// Current status of a hosted application.
    /// </summary>
    /// <param name="IsRunning">True if the hosted application is currently running.</param>
    /// <param name="BaseUrl">The base URL if running, null otherwise.</param>
    /// <param name="ProcessId">The process ID if applicable (for process-based hosts), null otherwise.</param>
    public sealed record HostingStatus(bool IsRunning, Uri? BaseUrl, int? ProcessId);
}
