using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

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
        public void LaunchAsync_Throws_OnDisposed()
        {
            var launcher = new NodeJsServerLauncher();
            launcher.Dispose();
            var cfg = ServerConfiguration.CreateNodeJsServer(new Uri("http://localhost:7102"), "/path/to/package.json").Build();
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingProjectPath()
        {
            var launcher = new NodeJsServerLauncher();
            var cfg = new ServerConfiguration { ServerType = ServerType.NodeJs, BaseUrl = new Uri("http://localhost:7103") };
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingBaseUrl()
        {
            var launcher = new NodeJsServerLauncher();
            var cfg = new ServerConfiguration { ServerType = ServerType.NodeJs, ProjectPath = "/path/to/package.json" };
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public async Task LaunchAsync_Uses_Command_And_Env_Vars_With_ProcessRunner()
        {
            var baseUrl = new Uri("http://localhost:7123");
            var cfg = ServerConfiguration.CreateNodeJsServer(baseUrl, "/path/to/package.json")
                .WithNodeEnvironment("production")
                .WithPort(7123)
                .WithArguments("--", "--open")
                .WithStartupTimeout(TimeSpan.FromMilliseconds(10))
                .Build();

            var fakeRunner = new Mocks.FakeProcessRunner();
            var fakeClock = new Mocks.FakeClock();
            var launcher = new NodeJsServerLauncher(null, fakeRunner, fakeClock);

            // We expect readiness to timeout quickly. Ensure it throws, then assert start info.
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception);

            Assert.That(fakeRunner.LastStartInfo, Is.Not.Null);
            Assert.That(fakeRunner.LastStartInfo!.FileName, Is.EqualTo("npm"));
            Assert.That(fakeRunner.LastStartInfo!.Arguments, Does.Contain("start"));
            Assert.That(fakeRunner.LastStartInfo!.Arguments, Does.Contain("--open"));
            Assert.That(fakeRunner.LastStartInfo!.WorkingDirectory, Is.EqualTo(System.IO.Path.GetDirectoryName(cfg.ProjectPath)));
        }

        [Test]
        public void PlanLaunch_BuildsExpectedStartInfo()
        {
            var baseUrl = new Uri("http://localhost:7161");
            var cfg = ServerConfiguration.CreateNodeJsServer(baseUrl, "/path/to/package.json")
                .WithArguments("--", "--open")
                .Build();

            var launcher = new NodeJsServerLauncher();
            var plan = launcher.PlanLaunch(cfg);
            Assert.That(plan.StartInfo.FileName, Is.EqualTo("npm"));
            Assert.That(plan.StartInfo.Arguments, Does.Contain("start"));
            Assert.That(plan.PollInterval, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }
    }
}