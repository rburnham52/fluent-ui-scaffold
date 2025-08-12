using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class ProcessRunnerTests
    {
        private static ProcessStartInfo BuildNoopCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c echo hello",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }
            else
            {
                return new ProcessStartInfo
                {
                    FileName = "/bin/echo",
                    Arguments = "hello",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }
        }

        [Test]
        public async Task Start_Launches_Process_And_Adapter_Exposes_Basics()
        {
            var runner = new ProcessRunner();
            var psi = BuildNoopCommand();
            var proc = runner.Start(psi);
            Assert.That(proc.Id, Is.GreaterThan(0));
            await proc.WaitForExitAsync();
            Assert.That(proc.HasExited, Is.True);
        }

        [Test]
        public void Start_Throws_When_Cannot_Start()
        {
            var runner = new ProcessRunner();
            var psi = new ProcessStartInfo
            {
                FileName = "definitely-not-a-real-exe",
                UseShellExecute = false
            };
            Assert.That(() => runner.Start(psi), Throws.Exception);
        }

        [Test]
        public async Task Start_Captures_StandardOutput_And_Error()
        {
            var runner = new ProcessRunner();

            // stdout
            var outPsi = BuildNoopCommand();
            var outProc = runner.Start(outPsi);
            await outProc.WaitForExitAsync();
            var stdout = outProc.StandardOutputReader.ReadToEnd().Trim();
            Assert.That(stdout, Is.EqualTo("hello"));

            // stderr
            ProcessStartInfo errPsi;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                errPsi = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c echo err 1>&2",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }
            else
            {
                errPsi = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = "-c \"echo err 1>&2\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }
            var errProc = runner.Start(errPsi);
            await errProc.WaitForExitAsync();
            var stderr = errProc.StandardErrorReader.ReadToEnd().Trim();
            Assert.That(stderr, Is.EqualTo("err"));
        }

        [Test]
        public async Task Kill_Stops_LongRunning_Process()
        {
            var runner = new ProcessRunner();
            ProcessStartInfo psi;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                psi = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/c ping 127.0.0.1 -n 5 > nul",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }
            else
            {
                psi = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = "-c 'sleep 5'",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
            }

            var proc = runner.Start(psi);
            proc.Kill();
            await proc.WaitForExitAsync();
            Assert.That(proc.HasExited, Is.True);
        }
    }
}