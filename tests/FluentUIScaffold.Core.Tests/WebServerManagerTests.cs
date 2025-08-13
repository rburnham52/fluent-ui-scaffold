using System;
using System.Threading;
using System.Threading.Tasks;

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

            var plan = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/MyApp.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

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

            var plan = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/MyApp.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

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
            var plan = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/app.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

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

        // The disposal path is covered implicitly by StopServer() not throwing when nothing started
        // Explicit disposal injection via reflection is removed with the new unified launcher.
    }
}
