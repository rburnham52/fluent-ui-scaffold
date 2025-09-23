using System;
using System.Threading.Tasks;
namespace FluentUIScaffold.Core.Configuration.Launchers.Abstractions
{

    public interface IClock
    {
        DateTime UtcNow { get; }
        Task Delay(TimeSpan delay);
    }

    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public Task Delay(TimeSpan delay) => Task.Delay(delay);
    }
}
