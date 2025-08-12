using System;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetCoreCommandBuilderTests
    {
        [Test]
        public void BuildCommand_IncludesUrls_Framework_Config()
        {
            var baseUrl = new Uri("http://localhost:6001");
            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, "/path/app.csproj")
                .WithFramework("net7.0")
                .WithConfiguration("Debug")
                .Build();

            var builder = new AspNetCoreCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.That(args, Is.EqualTo($"run --configuration Debug --framework net7.0 --urls \"{baseUrl}\" --no-launch-profile"));
        }

        [Test]
        public void BuildCommand_Defaults_WhenMissingArgs()
        {
            var baseUrl = new Uri("http://localhost:6002");
            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, "/path/app.csproj")
                .Build();

            var builder = new AspNetCoreCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.That(args, Is.EqualTo($"run --configuration Release --framework net8.0 --urls \"{baseUrl}\" --no-launch-profile"));
        }
    }
}