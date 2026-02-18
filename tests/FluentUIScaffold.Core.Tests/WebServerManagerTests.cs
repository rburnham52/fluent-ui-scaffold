using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Configuration.Launchers.Defaults;
using FluentUIScaffold.Core.Tests.Helpers;

using NUnit.Framework;

#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class WebServerManagerTests
    {
        private static LaunchPlan CreateTestLaunchPlan(Uri baseUrl)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --no-launch-profile --framework net8.0 --configuration Release",
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            return new LaunchPlan(
                startInfo,
                baseUrl,
                TimeSpan.FromSeconds(60),
                new HttpReadinessProbe(),
                new[] { "/" },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(200));
        }

        [Test]
#if NET8_0_OR_GREATER
        [Platform(Exclude="Win")] // Avoid process start path issues on Windows
#endif
        public async Task StartServerAsync_ServerAlreadyRunning_SetsRunningState()
        {
            await using var server = await TestHttpServer.StartAsync();
            var baseUrl = new Uri($"http://localhost:{server.Port}");

            var plan = CreateTestLaunchPlan(baseUrl);

            Assert.That(WebServerManager.IsServerRunning(), Is.False);
            await WebServerManager.StartServerAsync(plan);
            Assert.That(WebServerManager.IsServerRunning(), Is.True);

            WebServerManager.StopServer();
            Assert.That(WebServerManager.IsServerRunning(), Is.False);
        }

        [Test]
#if NET8_0_OR_GREATER
        [Platform(Exclude="Win")] // Avoid process start path issues on Windows
#endif
        public async Task StartServerAsync_MutexHeldByOther_AndServerBecomesReady_Completes()
        {
            await using var server = await TestHttpServer.StartAsync();
            var baseUrl = new Uri($"http://localhost:{server.Port}");
            var mutexName = $"FluentUIScaffold_WebServer_{baseUrl.Port}";

            using var mutex = new Mutex(initiallyOwned: true, name: mutexName, createdNew: out _);

            var plan = CreateTestLaunchPlan(baseUrl);

            var startTask = WebServerManager.StartServerAsync(plan);
            // Release quickly so the StartServerAsync loop can proceed
            mutex.ReleaseMutex();
            await startTask;

            Assert.That(WebServerManager.IsServerRunning(), Is.True);
            WebServerManager.StopServer();
            Assert.That(WebServerManager.IsServerRunning(), Is.False);
        }

        [Test]
#if NET8_0_OR_GREATER
        [Platform(Include="Linux")] // Only meaningful on Linux CI, avoids Windows process path issues
#endif
        [Ignore("Long-running timeout path; ignored for CI runtime stability")]
        public void StartServerAsync_MutexHeldByOther_AndNoServer_AfterTimeout_Throws()
        {
            var baseUrl = new Uri("http://localhost:61"); // Unused test port
            var plan = CreateTestLaunchPlan(baseUrl);

            var mutexName = $"FluentUIScaffold_WebServer_{baseUrl.Port}";
            using var mutex = new System.Threading.Mutex(initiallyOwned: true, name: mutexName, createdNew: out _);

            // Because no server will ever become ready on port 61, the wait loop should eventually time out
            Assert.That(async () => await WebServerManager.StartServerAsync(plan), Throws.Exception);

            WebServerManager.StopServer();
        }

        [Test]
        public void StopServer_WhenNotStarted_IsNoOp()
        {
            // Ensure clean state
            WebServerManager.StopServer();
            Assert.That(WebServerManager.IsServerRunning(), Is.False);
        }

        [Test]
        public void IsServerRunning_ReturnsFalse_WhenNoInstance()
        {
            WebServerManager.StopServer();
            Assert.That(WebServerManager.IsServerRunning(), Is.False);
        }

        [Test]
        public void StartServerAsync_Throws_OnNullConfig()
        {
            Assert.That(async () => await FluentUIScaffold.Core.Configuration.WebServerManager.StartServerAsync(null!), Throws.Exception);
        }
    }
}
