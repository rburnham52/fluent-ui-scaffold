using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
namespace FluentUIScaffold.Core.Configuration.Launchers.Abstractions
{

    public interface IReadinessProbe
    {
        Task WaitUntilReadyAsync(LaunchPlan plan, ILogger? logger, CancellationToken cancellationToken = default);
    }
}
