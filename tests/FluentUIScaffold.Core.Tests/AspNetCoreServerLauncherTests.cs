using System;
using System.Diagnostics;
using System.Reflection;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetCoreServerLauncherTests
    {
        private static string InvokeBuildArgs(ServerConfiguration config)
        {
            var launcherType = typeof(AspNetCoreServerLauncher);
            var method = launcherType.GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Could not reflect BuildCommandArguments on AspNetCoreServerLauncher");
            return (string)method!.Invoke(null, new object[] { config })!;
        }

        [Test]
        public void BuildCommandArguments_ReturnsExpectedFormat()
        {
            var baseUrl = new Uri("http://localhost:5151");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

            var args = InvokeBuildArgs(config);

            Assert.That(args, Is.EqualTo($"run --configuration Release --framework net8.0 --urls \"{baseUrl}\" --no-launch-profile"));
        }

        [Test]
        public void SpaProxyEnv_PropagatesToStartInfo()
        {
            var baseUrl = new Uri("http://localhost:5252");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .WithSpaProxy(true)
                .Build();

            var startInfo = new ProcessStartInfo();
            foreach (var kv in config.EnvironmentVariables)
            {
                startInfo.EnvironmentVariables[kv.Key] = kv.Value;
            }

            // Simulate the propagation path in launcher
            if (config.EnvironmentVariables.TryGetValue("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", out var val))
            {
                startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = val ?? string.Empty;
            }

            Assert.That(startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo("Microsoft.AspNetCore.SpaProxy"));
        }

        [Test]
        public void CanHandle_AspNetCore_ReturnsTrue()
        {
            var launcher = new AspNetCoreServerLauncher();
            var config = new ServerConfiguration { ServerType = ServerType.AspNetCore };
            Assert.That(launcher.CanHandle(config), Is.True);
        }

        [Test]
        public void LaunchAsync_Throws_OnDisposed()
        {
            var launcher = new AspNetCoreServerLauncher();
            launcher.Dispose();
            var cfg = ServerConfiguration.CreateDotNetServer(new Uri("http://localhost:7301"), "/path/to/app.csproj").Build();
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception.TypeOf<ObjectDisposedException>());
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingProjectPath()
        {
            var launcher = new AspNetCoreServerLauncher();
            var cfg = new ServerConfiguration { ServerType = ServerType.AspNetCore, BaseUrl = new Uri("http://localhost:7302") };
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception);
        }

        [Test]
        public void LaunchAsync_Throws_OnMissingBaseUrl()
        {
            var launcher = new AspNetCoreServerLauncher();
            var cfg = new ServerConfiguration { ServerType = ServerType.AspNetCore, ProjectPath = "/path/to/app.csproj" };
            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception);
        }

        [Test]
        public async System.Threading.Tasks.Task LaunchAsync_Uses_ProcessRunner_And_CommandArgs()
        {
            var baseUrl = new Uri("http://localhost:7144");
            var cfg = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/MyApp.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Debug")
                .WithStartupTimeout(TimeSpan.FromMilliseconds(10))
                .Build();

            var fakeRunner = new FluentUIScaffold.Core.Tests.Mocks.FakeProcessRunner();
            var fakeClock = new FluentUIScaffold.Core.Tests.Mocks.FakeClock();
            var launcher = new AspNetCoreServerLauncher(null, fakeRunner, fakeClock);

            Assert.That(async () => await launcher.LaunchAsync(cfg), Throws.Exception);

            Assert.That(fakeRunner.LastStartInfo, Is.Not.Null);
            Assert.That(fakeRunner.LastStartInfo!.FileName, Is.EqualTo("dotnet"));
            Assert.That(fakeRunner.LastStartInfo!.Arguments, Does.Contain("run"));
            Assert.That(fakeRunner.LastStartInfo!.Arguments, Does.Contain("--framework"));
            Assert.That(fakeRunner.LastStartInfo!.Arguments, Does.Contain("--configuration"));
        }

        [Test]
        public void PlanLaunch_BuildsExpectedStartInfo()
        {
            var baseUrl = new Uri("http://localhost:7171");
            var cfg = ServerConfiguration.CreateDotNetServer(baseUrl, "/path/to/MyApp.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .Build();

            var launcher = new AspNetCoreServerLauncher();
            var plan = launcher.PlanLaunch(cfg);
            Assert.That(plan.StartInfo.FileName, Is.EqualTo("dotnet"));
            Assert.That(plan.StartInfo.Arguments, Does.Contain("run"));
            Assert.That(plan.PollInterval, Is.EqualTo(TimeSpan.FromMilliseconds(200)));
        }
    }
}