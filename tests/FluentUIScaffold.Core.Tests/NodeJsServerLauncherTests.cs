using System;
using System.Diagnostics;
using System.Reflection;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class NodeJsServerLauncherTests
    {
        private static string InvokeBuildArgs(ServerConfiguration config)
        {
            var launcherType = typeof(NodeJsServerLauncher);
            var method = launcherType.GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Could not reflect BuildCommandArguments on NodeJsServerLauncher");
            return (string)method!.Invoke(null, new object[] { config })!;
        }

        [Test]
        public void BuildCommandArguments_PrependsScript_AndAppendsArgs()
        {
            var baseUrl = new Uri("http://localhost:7777");
            var projectPath = "/path/to/package.json";

            var config = ServerConfiguration
                .CreateNodeJsServer(baseUrl, projectPath)
                .WithNpmScript("dev")
                .WithArguments("--", "--open")
                .Build();

            var args = InvokeBuildArgs(config);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.StartWith("start "));
                Assert.That(args, Does.Contain("dev"));
                Assert.That(args, Does.Contain("-- --open"));
            });
        }

        [Test]
        public void Builder_SetsNodeEnvAndPort_FromFluentMethods()
        {
            var baseUrl = new Uri("http://localhost:8081");
            var projectPath = "/path/to/package.json";

            var config = ServerConfiguration
                .CreateNodeJsServer(baseUrl, projectPath)
                .WithNodeEnvironment("production")
                .WithPort(8081)
                .Build();

            Assert.That(config.EnvironmentVariables["NODE_ENV"], Is.EqualTo("production"));
            Assert.That(config.EnvironmentVariables["PORT"], Is.EqualTo("8081"));
        }

        [Test]
        public void CanHandle_NodeJs_ReturnsTrue()
        {
            var launcher = new NodeJsServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.NodeJs };
            Assert.That(launcher.CanHandle(config), Is.True);
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingProjectPath()
        {
            var launcher = new NodeJsServerLauncher();
            var config = new ServerConfiguration
            {
                ServerType = ServerType.NodeJs,
                BaseUrl = new Uri("http://localhost:7000"),
                ProjectPath = null
            };

            Assert.That(async () => await launcher.LaunchAsync(config), Throws.ArgumentException);
        }
    }
}