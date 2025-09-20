using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    public interface IServerManager
    {
        Task<ServerStatus> EnsureStartedAsync(LaunchPlan plan, ILogger logger, CancellationToken ct);
        Task RestartAsync(CancellationToken ct);
        Task StopAsync(CancellationToken ct);
        ServerStatus GetStatus();
    }
}

