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
                    FileName = "/bin/sh",
                    Arguments = "-lc 'echo hello'",
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
    }
}