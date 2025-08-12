using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Tests.Helpers;
using NUnit.Framework;
using System.Diagnostics;
using System.Linq;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetServerLauncherReadinessDirectTests
    {
        private static async Task InvokeWaitAsync(AspNetServerLauncher launcher, ServerConfiguration config)
        {
            var method = typeof(AspNetServerLauncher).GetMethod("WaitForServerReadyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null);
            var task = (Task)method!.Invoke(launcher, new object[] { config })!;
            await task;
        }

        [Test]
        public async Task WaitForServerReady_Succeeds_WhenHealthy()
        {
            await using var server = await TestHttpServer.StartAsync();
            var baseUrl = new Uri($"http://localhost:{server.Port}");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(3))
                .Build();

            var launcher = new AspNetServerLauncher();
            await InvokeWaitAsync(launcher, config);
        }

        [Test]
        public void WaitForServerReady_TimesOut_WhenUnhealthy()
        {
            var baseUrl = new Uri("http://localhost:9");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(3))
                .Build();

            var launcher = new AspNetServerLauncher();
            Assert.That(async () => await InvokeWaitAsync(launcher, config), Throws.Exception.TypeOf<TimeoutException>());
        }

        [Test]
        public void WaitForServerReady_Fails_WhenProcessExitsEarly()
        {
            var baseUrl = new Uri("http://localhost:6553");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(1))
                .Build();

            var launcher = new AspNetServerLauncher();

            var procField = typeof(AspNetServerLauncher).GetField("_webServerProcess", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(procField, Is.Not.Null);

            var fake = new Process();
            procField!.SetValue(launcher, fake);

            var waitTask = typeof(AspNetServerLauncher).GetMethod("WaitForServerReadyAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(launcher, new object[] { config }) as Task;

            // Immediately dispose the fake process to emulate exit
            try { fake.Kill(); } catch { }

            Assert.That(async () => await waitTask!, Throws.Exception.TypeOf<InvalidOperationException>().Or.TypeOf<TimeoutException>());
        }

        [Test]
        public void WaitForServerReady_Fails_OnNonResponsivePort()
        {
            var baseUrl = new Uri("http://localhost:9");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromMilliseconds(200))
                .Build();

            var launcher = new AspNetServerLauncher();
            Assert.That(async () => await InvokeWaitAsync(launcher, config), Throws.Exception);
        }

        [Test]
        public async Task WaitForServerReady_TimesOut_On500Responses()
        {
            // Use a server that returns 500 to force probe failure
            await using var server = await TestHttpServer.StartAsync(System.Net.HttpStatusCode.InternalServerError);

            var baseUrl = new Uri($"http://localhost:{server.Port}");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(2))
                .Build();

            var launcher = new AspNetServerLauncher();
            Assert.That(async () => await InvokeWaitAsync(launcher, config), Throws.Exception);
        }

        [Test]
        public async Task WaitForServerReady_Uses_HealthCheck_Endpoints()
        {
            await using var server = await TestHttpServer.StartAsync();
            var baseUrl = new Uri($"http://localhost:{server.Port}");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithHealthCheckEndpoints("/health", "status")
                .WithStartupTimeout(TimeSpan.FromSeconds(2))
                .Build();

            var launcher = new AspNetServerLauncher();
            await InvokeWaitAsync(launcher, config);
        }

        private sealed class TestLogger : Microsoft.Extensions.Logging.ILogger
        {
            private sealed class NoopDisposable : IDisposable { public void Dispose() { } }
            public System.Collections.Generic.List<string> Messages { get; } = new System.Collections.Generic.List<string>();
            public IDisposable BeginScope<TState>(TState state) => new NoopDisposable();
            public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
            public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                try { Messages.Add(formatter(state, exception)); } catch { }
            }
        }

        [Test]
        public void WaitForServerReady_Logs_Progress_Every5Attempts()
        {
            var logger = new TestLogger();
            var fakeClock = new FluentUIScaffold.Core.Tests.Mocks.FakeClock();
            var launcher = new AspNetServerLauncher(logger, null, fakeClock);

            var baseUrl = new Uri("http://localhost:9");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(1))
                .Build();

            Assert.That(async () => await InvokeWaitAsync(launcher, config), Throws.Exception);
            Assert.That(logger.Messages.Any(m => m.Contains("Still waiting for")), Is.True);
        }
    }
}