using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Tests.Helpers;
using NUnit.Framework;

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
    }
}