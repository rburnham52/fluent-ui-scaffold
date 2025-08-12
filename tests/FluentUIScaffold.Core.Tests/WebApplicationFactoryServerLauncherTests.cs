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
        [Test]
        public void CanHandle_WebApplicationFactoryType_ReturnsTrue()
        {
            var launcher = new WebApplicationFactoryServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.WebApplicationFactory };
            Assert.That(launcher.CanHandle(config), Is.True);
        }
    }
}