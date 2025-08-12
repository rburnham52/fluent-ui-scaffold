using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class ServerLauncherFactoryTests
    {
        private sealed class DummyLauncher : IServerLauncher
        {
            public string Name => "Dummy";
            private readonly ServerType _type;
            public DummyLauncher(ServerType type) { _type = type; }
            public bool CanHandle(ServerConfiguration configuration) => configuration.ServerType == _type;
            public System.Threading.Tasks.Task LaunchAsync(ServerConfiguration configuration) => System.Threading.Tasks.Task.CompletedTask;
            public void Dispose() { }
        }

        private sealed class NoopDetector : IProjectDetector
        {
            public string Name => "Noop";
            public int Priority => 0;
            public string? DetectProjectPath(ProjectDetectionContext context) => null;
        }

        [Test]
        public void RegisterLauncher_And_GetLauncher_Selects_By_CanHandle()
        {
            var factory = new ServerLauncherFactory();
            factory.RegisterLauncher(new DummyLauncher(ServerType.NodeJs));
            factory.RegisterLauncher(new DummyLauncher(ServerType.AspNetCore));

            var cfg = new ServerConfiguration { ServerType = ServerType.AspNetCore };
            var selected = factory.GetLauncher(cfg);
            Assert.That(selected.Name, Is.EqualTo("Dummy"));
        }

        [Test]
        public void GetLauncher_Throws_When_No_Match()
        {
            var factory = new ServerLauncherFactory();
            Assert.That(() => factory.GetLauncher(new ServerConfiguration { ServerType = ServerType.NodeJs }), Throws.Exception);
        }

        [Test]
        public void CreateConfigurationWithDetection_Throws_When_No_Project_Detected()
        {
            var factory = new ServerLauncherFactory();
            factory.RegisterDetector(new NoopDetector());
            Assert.That(() => factory.CreateConfigurationWithDetection(new Uri("http://localhost:5007"), ServerType.NodeJs), Throws.Exception);
        }
    }
}
