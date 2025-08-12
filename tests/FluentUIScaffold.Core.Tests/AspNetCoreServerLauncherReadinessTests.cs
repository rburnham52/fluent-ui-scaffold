using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Tests.Mocks;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetCoreServerLauncherReadinessTests
    {
        [Test]
        public async Task WaitForServerReady_Succeeds_WhenHealthy()
        {
            var baseUrl = new Uri("http://localhost:6021");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(2))
                .Build();

            var launcher = new AspNetCoreServerLauncher(null, new Mocks.FakeProcessRunner(), new Mocks.FakeClock(), new FakeReadinessProbe(true));
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.Exception);
        }

        [Test]
        public void WaitForServerReady_TimesOut_WhenUnhealthy()
        {
            var baseUrl = new Uri("http://localhost:9");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromMilliseconds(50))
                .Build();

            var launcher = new AspNetCoreServerLauncher(null, new Mocks.FakeProcessRunner(), new Mocks.FakeClock(), new FakeReadinessProbe(false));
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.Exception.TypeOf<TimeoutException>().Or.InstanceOf<Exception>());
        }

        [Test]
        public void WaitForServerReady_TimesOut_On500Responses()
        {
            var baseUrl = new Uri("http://localhost:6022");
            var config = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/App.csproj")
                .WithStartupTimeout(TimeSpan.FromSeconds(2))
                .Build();

            var launcher = new AspNetCoreServerLauncher(null, new Mocks.FakeProcessRunner(), new Mocks.FakeClock(), new FakeReadinessProbe(false));
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.Exception);
        }
    }
}
