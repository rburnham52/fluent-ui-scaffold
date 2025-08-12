using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

namespace FluentUIScaffold.Core.Tests.Mocks
{
    public sealed class FakeProcess : IProcess
    {
        public bool HasExited { get; set; }
        public int ExitCode { get; set; }
        public int Id { get; set; } = 12345;
        public TextReader StandardOutputReader { get; set; } = new StringReader(string.Empty);
        public TextReader StandardErrorReader { get; set; } = new StringReader(string.Empty);
        public void Kill() { HasExited = true; }
        public Task WaitForExitAsync() => Task.CompletedTask;
        public void Dispose() { }
    }

    public sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessStartInfo? LastStartInfo { get; private set; }
        public FakeProcess Process { get; } = new FakeProcess();
        public IProcess Start(ProcessStartInfo startInfo)
        {
            LastStartInfo = startInfo;
            return Process;
        }
    }

    public sealed class FakeClock : IClock
    {
        public DateTime UtcNow { get; private set; } = DateTime.UtcNow;
        public List<TimeSpan> Delays { get; } = new List<TimeSpan>();
        public Task Delay(TimeSpan delay)
        {
            Delays.Add(delay);
            UtcNow = UtcNow.Add(delay);
            return Task.CompletedTask;
        }
    }
}
