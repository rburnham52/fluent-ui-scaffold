namespace FluentUIScaffold.Core.Configuration.Launchers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public interface IReadinessProbe
    {
        Task WaitUntilReadyAsync(ServerConfiguration configuration,
                                  ILogger? logger,
                                  TimeSpan initialDelay,
                                  TimeSpan pollInterval,
                                  CancellationToken cancellationToken = default);
    }
}