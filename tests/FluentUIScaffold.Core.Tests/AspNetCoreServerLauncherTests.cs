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
    }
}