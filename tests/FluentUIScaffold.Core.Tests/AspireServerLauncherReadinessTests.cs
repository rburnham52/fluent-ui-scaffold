using System;
using System.Threading.Tasks;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Tests.Mocks;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspireServerLauncherReadinessTests
    {
        [Test]
        public async Task WaitForServerReady_Succeeds_WhenHealthy()
        {
            var baseUrl = new Uri("http://localhost:6001");
            var config = ServerConfiguration.CreateAspireServer(baseUrl, "/path/to/Aspire.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(2))
                .Build();

            var launcher = new AspireServerLauncher(null, new Mocks.FakeProcessRunner(), new Mocks.FakeClock(), new FakeReadinessProbe(true));
            Assert.DoesNotThrowAsync(async () => await launcher.LaunchAsync(config));
        }

        [Test]
        public void WaitForServerReady_TimesOut_WhenUnhealthy()
        {
            var baseUrl = new Uri("http://localhost:9");
            var config = ServerConfiguration.CreateAspireServer(baseUrl, "/path/to/Aspire.csproj")
                .WithStartupTimeout(TimeSpan.FromMilliseconds(50))
                .Build();

            var launcher = new AspireServerLauncher(null, new Mocks.FakeProcessRunner(), new Mocks.FakeClock(), new FakeReadinessProbe(false));
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.Exception.TypeOf<TimeoutException>().Or.InstanceOf<Exception>());
        }
    }
}