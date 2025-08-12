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

        private sealed class DisposableLauncherStub : FluentUIScaffold.Core.Configuration.IServerLauncher
        {
            public string Name => "DisposableStub";
            public bool Disposed { get; private set; }
            public bool CanHandle(FluentUIScaffold.Core.Configuration.ServerConfiguration configuration) => true;
            public System.Threading.Tasks.Task LaunchAsync(FluentUIScaffold.Core.Configuration.ServerConfiguration configuration) => System.Threading.Tasks.Task.CompletedTask;
            public void Dispose() { Disposed = true; }
        }

        [Test]
        public void StopServer_WhenOwner_Disposes_CurrentLauncher()
        {
            // Ensure instance exists
            var instance = FluentUIScaffold.Core.Configuration.WebServerManager.GetInstance();

            // Reflect to set private fields
            var type = typeof(FluentUIScaffold.Core.Configuration.WebServerManager);
            var currentLauncherField = type.GetField("_currentLauncher", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var serverStartedField = type.GetField("_serverStarted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var isOwnerField = type.GetField("_isServerOwner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            var stub = new DisposableLauncherStub();
            currentLauncherField!.SetValue(instance, stub);
            serverStartedField!.SetValue(null, true);
            isOwnerField!.SetValue(null, true);

            FluentUIScaffold.Core.Configuration.WebServerManager.StopServer();
            Assert.That(stub.Disposed, Is.True);
        }
    }
}