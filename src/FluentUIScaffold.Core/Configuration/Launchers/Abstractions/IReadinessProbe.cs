using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public interface IReadinessProbe
    {
        Task WaitUntilReadyAsync(
            ServerConfiguration configuration,
            ILogger? logger,
            TimeSpan initialDelay,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default);
    }
}