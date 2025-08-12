namespace FluentUIScaffold.Core.Configuration.Launchers
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;

    internal sealed class DiagnosticsProcessAdapter : IProcess
    {
        private readonly Process _inner;

        public DiagnosticsProcessAdapter(Process process)
        {
            _inner = process ?? throw new ArgumentNullException(nameof(process));
        }

        public bool HasExited => _inner.HasExited;
        public int ExitCode => _inner.ExitCode;
        public int Id => _inner.Id;
        public TextReader StandardOutputReader => _inner.StandardOutput;
        public TextReader StandardErrorReader => _inner.StandardError;
        public void Kill() => _inner.Kill();
        public Task WaitForExitAsync() => _inner.WaitForExitAsync();
        public void Dispose() => _inner.Dispose();
    }

    public class ProcessRunner : IProcessRunner
    {
        public IProcess Start(ProcessStartInfo startInfo)
        {
            var process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start process");
            }
            return new DiagnosticsProcessAdapter(process);
        }
    }
}