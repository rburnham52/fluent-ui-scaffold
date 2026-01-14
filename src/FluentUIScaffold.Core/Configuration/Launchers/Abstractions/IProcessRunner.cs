using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
namespace FluentUIScaffold.Core.Configuration.Launchers.Abstractions
{

    public interface IProcess
    {
        bool HasExited { get; }
        int ExitCode { get; }
        int Id { get; }
        TextReader StandardOutputReader { get; }
        TextReader StandardErrorReader { get; }
        void Kill();
        Task WaitForExitAsync();
        void Dispose();
    }

    public interface IProcessRunner
    {
        IProcess Start(ProcessStartInfo startInfo);
    }
}
