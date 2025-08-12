using System;
using System.Diagnostics;
using System.Reflection;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetServerLauncherTests
    {
        private static string InvokeBuildArgs(ServerConfiguration config)
        {
            var launcherType = typeof(AspNetServerLauncher);
            var method = launcherType.GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Could not reflect BuildCommandArguments on AspNetServerLauncher");
            return (string)method!.Invoke(null, new object[] { config })!;
        }

        private static void InvokeSetEnv(ProcessStartInfo startInfo, ServerConfiguration config)
        {
            var launcherType = typeof(AspNetServerLauncher);
            var method = launcherType.GetMethod("SetServerSpecificEnvironmentVariables", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Could not reflect SetServerSpecificEnvironmentVariables on AspNetServerLauncher");
            method!.Invoke(null, new object[] { startInfo, config });
        }

        [Test]
        public void BuildCommandArguments_UsesFrameworkAndConfiguration_FromBuilder()
        {
            var baseUrl = new Uri("http://localhost:5001");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .WithArguments("--no-restore")
                .Build();

            var args = InvokeBuildArgs(config);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.StartWith("run "));
                Assert.That(args, Does.Contain("--framework net8.0"));
                Assert.That(args, Does.Contain("--configuration Release"));
                Assert.That(args, Does.EndWith("--no-restore"));
                Assert.That(args, Does.Contain("--no-launch-profile"));
            });
        }

        [Test]
        public void SetServerSpecificEnvironmentVariables_AspNetCore_SetsAspNetCoreUrls()
        {
            var baseUrl = new Uri("http://localhost:5050");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Debug")
                .Build();

            var startInfo = new ProcessStartInfo();

            InvokeSetEnv(startInfo, config);

            Assert.That(startInfo.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
        }

        [Test]
        public void SetServerSpecificEnvironmentVariables_Aspire_DoesNotOverrideAspNetCoreUrls()
        {
            var baseUrl = new Uri("http://localhost:6060");
            var projectPath = "/path/to/AppHost.csproj";

            var config = ServerConfiguration
                .CreateAspireServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

            var startInfo = new ProcessStartInfo();
            startInfo.EnvironmentVariables["ASPNETCORE_URLS"] = "http://preset/";

            InvokeSetEnv(startInfo, config);

            // Should remain unchanged for Aspire
            Assert.That(startInfo.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo("http://preset/"));
        }

        [Test]
        public void CanHandle_AspNetCore_And_Aspire_ReturnsTrue()
        {
            var launcher = new AspNetServerLauncher();
            Assert.That(launcher.CanHandle(new ServerConfiguration { ServerType = ServerType.AspNetCore }), Is.True);
            Assert.That(launcher.CanHandle(new ServerConfiguration { ServerType = ServerType.Aspire }), Is.True);
            Assert.That(launcher.CanHandle(new ServerConfiguration { ServerType = ServerType.NodeJs }), Is.False);
        }

        [Test]
        public void LaunchAsync_Throws_OnDisposed()
        {
            var launcher = new AspNetServerLauncher();
            launcher.Dispose();
            var config = ServerConfiguration.CreateDotNetServer(new Uri("http://localhost:5005"), "/path/app.csproj").Build();
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.Exception.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingProjectPath()
        {
            var launcher = new AspNetServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.AspNetCore, BaseUrl = new Uri("http://localhost:5006"), ProjectPath = null };
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.ArgumentException);
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingBaseUrl()
        {
            var launcher = new AspNetServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.AspNetCore, ProjectPath = "/path/app.csproj", BaseUrl = null };
            Assert.That(async () => await launcher.LaunchAsync(config), Throws.ArgumentException);
        }

        [Test]
        public void BuildCommandArguments_UsesFrameworkAndConfiguration_OrDefaults()
        {
            var baseUrl = new Uri("http://localhost:7201");
            var cfgWithArgs = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/app.csproj")
                .WithFramework("net9.0")
                .WithConfiguration("Debug")
                .Build();

            var method = typeof(AspNetServerLauncher).GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            var argsWith = (string)method!.Invoke(null, new object[] { cfgWithArgs })!;
            Assert.That(argsWith, Does.Contain("--framework net9.0"));
            Assert.That(argsWith, Does.Contain("--configuration Debug"));

            var cfgDefault = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/app.csproj").Build();
            var argsDefault = (string)method!.Invoke(null, new object[] { cfgDefault })!;
            Assert.That(argsDefault, Does.Contain("--framework net8.0"));
            Assert.That(argsDefault, Does.Contain("--configuration Release"));
        }
    }
}