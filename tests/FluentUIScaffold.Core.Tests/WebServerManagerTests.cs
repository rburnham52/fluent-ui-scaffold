using System;
using System.Threading.Tasks;
using System.Threading;

using FluentUIScaffold.Core.Configuration;
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
        [Test]
#if NET8_0_OR_GREATER
        [Platform(Exclude="Win")] // Avoid process start path issues on Windows
#endif
        public async Task StartServerAsync_ServerAlreadyRunning_SetsRunningState()
        {
            await using var server = await TestHttpServer.StartAsync();
            var baseUrl = new Uri($"http://localhost:{server.Port}");

            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/MyApp.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

            Assert.That(WebServerManager.IsServerRunning(), Is.False);
            await WebServerManager.StartServerAsync(config);
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

            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/MyApp.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

            var startTask = WebServerManager.StartServerAsync(config);
            // Release quickly so the StartServerAsync loop can proceed
            mutex.ReleaseMutex();
            await startTask;

            Assert.That(WebServerManager.IsServerRunning(), Is.True);
            WebServerManager.StopServer();
            Assert.That(WebServerManager.IsServerRunning(), Is.False);
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
    }
}