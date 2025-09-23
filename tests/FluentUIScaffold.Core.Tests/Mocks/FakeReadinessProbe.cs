using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Configuration.Launchers.Abstractions;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Tests.Mocks
{
    public sealed class FakeReadinessProbe : IReadinessProbe
    {
        private readonly bool _shouldSucceed;
        public int Calls { get; private set; }

        public FakeReadinessProbe(bool shouldSucceed)
        {
            _shouldSucceed = shouldSucceed;
        }

        public Task WaitUntilReadyAsync(LaunchPlan plan, ILogger? logger, CancellationToken cancellationToken = default)
        {
            Calls++;
            if (_shouldSucceed)
            {
                return Task.CompletedTask;
            }
            throw new TimeoutException("Fake readiness probe timeout");
        }
    }
}
