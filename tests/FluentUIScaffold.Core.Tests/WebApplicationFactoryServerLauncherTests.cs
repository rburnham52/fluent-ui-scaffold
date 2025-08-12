using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class WebApplicationFactoryServerLauncherTests
    {
        private sealed class FakeFallbackLauncher : IServerLauncher
        {
            public string Name => "FakeFallback";
            public int LaunchCalls { get; private set; }
            public bool Disposed { get; private set; }
            public bool CanHandle(ServerConfiguration configuration) => true;
            public Task LaunchAsync(ServerConfiguration configuration) { LaunchCalls++; return Task.CompletedTask; }
            public void Dispose() { Disposed = true; }
        }

        [Test]
        public void CanHandle_WebApplicationFactoryType_ReturnsTrue()
        {
            var launcher = new WebApplicationFactoryServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.WebApplicationFactory };
            Assert.That(launcher.CanHandle(config), Is.True);
        }

        [Test]
        public async Task LaunchAsync_Delegates_To_Fallback()
        {
            var fallback = new FakeFallbackLauncher();
            var launcher = new WebApplicationFactoryServerLauncher(null, fallback);
            var config = new ServerConfiguration { ServerType = ServerType.WebApplicationFactory };
            await launcher.LaunchAsync(config);
            Assert.That(fallback.LaunchCalls, Is.EqualTo(1));
        }

        [Test]
        public void Dispose_Disposes_Fallback()
        {
            var fallback = new FakeFallbackLauncher();
            var launcher = new WebApplicationFactoryServerLauncher(null, fallback);
            launcher.Dispose();
            Assert.That(fallback.Disposed, Is.True);
        }
    }
}