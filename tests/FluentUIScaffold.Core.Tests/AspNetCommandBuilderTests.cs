using System;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetCommandBuilderTests
    {
        [Test]
        public void BuildCommand_Defaults_ToNet8ReleaseAndNoLaunchProfile()
        {
            var config = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5001"), "/path/to/app.csproj")
                .Build();

            var builder = new AspNetCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.StartWith("run "));
                Assert.That(args, Does.Contain("--framework net8.0"));
                Assert.That(args, Does.Contain("--configuration Release"));
                Assert.That(args, Does.Contain("--no-launch-profile"));
            });
        }

        [Test]
        public void BuildCommand_Honors_Framework_And_Configuration_And_CustomArgs()
        {
            var config = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5002"), "/path/to/app.csproj")
                .WithFramework("net9.0")
                .WithConfiguration("Debug")
                .WithArguments("--verbosity", "minimal")
                .Build();

            var builder = new AspNetCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.Contain("--framework net9.0"));
                Assert.That(args, Does.Contain("--configuration Debug"));
                Assert.That(args, Does.Contain("--verbosity minimal"));
                Assert.That(args, Does.Not.Contain("--urls"));
            });
        }

        [Test]
        public void BuildCommand_Ignores_Framework_Config_Duplicates_In_CustomArgs()
        {
            var config = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5003"), "/path/to/app.csproj")
                .WithFramework("net7.0")
                .WithConfiguration("Release")
                .WithArguments("--framework", "ignoreMe", "--configuration", "AlsoIgnore")
                .Build();

            var builder = new AspNetCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.Contain("--framework net7.0"));
                Assert.That(args, Does.Contain("--configuration Release"));
                Assert.That(args, Does.Not.Contain("--framework ignoreMe"));
                Assert.That(args, Does.Not.Contain("--configuration AlsoIgnore"));
            });
        }
    }
}