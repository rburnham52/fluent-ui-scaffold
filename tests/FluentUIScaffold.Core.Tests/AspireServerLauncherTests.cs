using System;
using System.Reflection;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspireServerLauncherTests
    {
        private static string InvokeSharedBuildArgs(ServerConfiguration config)
        {
            var launcherType = typeof(AspNetServerLauncher);
            var method = launcherType.GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Could not reflect BuildCommandArguments on AspNetServerLauncher");
            return (string)method!.Invoke(null, new object[] { config })!;
        }

        [Test]
        public void EnvironmentDefaults_AreApplied_WhenNotProvided()
        {
            var baseUrl = new Uri("http://localhost:7070");
            var projectPath = "/path/to/AppHost.csproj";

            var config = ServerConfiguration.CreateAspireServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

            Assert.That(config.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Development"));
            Assert.That(config.EnvironmentVariables["DOTNET_ENVIRONMENT"], Is.EqualTo("Development"));
            Assert.That(config.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));

            var args = InvokeSharedBuildArgs(config);
            Assert.That(args, Does.Contain("--framework net8.0"));
            Assert.That(args, Does.Contain("--configuration Release"));
            Assert.That(args, Does.Contain("--no-launch-profile"));
        }

        [Test]
        public void AspNetServerLauncher_CanHandle_AspNetCore_And_Aspire()
        {
            var launcher = new AspNetServerLauncher();
            Assert.That(launcher.CanHandle(new ServerConfiguration { ServerType = ServerType.AspNetCore }), Is.True);
            Assert.That(launcher.CanHandle(new ServerConfiguration { ServerType = ServerType.Aspire }), Is.True);
        }

        [Test]
        public void CanHandle_Aspire_ReturnsTrue()
        {
            var launcher = new AspireServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.Aspire };
            Assert.That(launcher.CanHandle(config), Is.True);
        }
    }
}