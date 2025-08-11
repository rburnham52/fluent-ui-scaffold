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
    public class AspireServerLauncherReadinessTests
    {
        private static async Task InvokeWaitAsync(AspireServerLauncher launcher, ServerConfiguration config)
        {
            var method = typeof(AspireServerLauncher).GetMethod("WaitForServerReadyAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null);
            var task = (Task)method!.Invoke(launcher, new object[] { config })!;
            await task;
        }

        [Test]
        public async Task WaitForServerReady_Succeeds_WhenHealthy()
        {
            await using var server = await TestHttpServer.StartAsync();
            var baseUrl = new Uri($"http://localhost:{server.Port}");
            var config = ServerConfiguration.CreateAspireServer(baseUrl, "/path/to/Aspire.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(2))
                .Build();

            var launcher = new AspireServerLauncher();
            await InvokeWaitAsync(launcher, config);
        }

        [Test]
        public void WaitForServerReady_TimesOut_WhenUnhealthy()
        {
            var baseUrl = new Uri("http://localhost:9");
            var config = ServerConfiguration.CreateAspireServer(baseUrl, "/path/to/Aspire.csproj")
                .WithStartupTimeout(TimeSpan.FromMilliseconds(50))
                .Build();

            var launcher = new AspireServerLauncher();
            Assert.That(async () => await InvokeWaitAsync(launcher, config), Throws.Exception.TypeOf<TimeoutException>());
        }
    }
}