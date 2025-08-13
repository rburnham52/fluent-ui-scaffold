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

        [TestCase("net7.0")]
        [TestCase("net8.0")]
        [TestCase("net9.0")]
        public void BuildCommand_Framework_Variants_Are_Propagated(string framework)
        {
            var config = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5003"), "/path/to/app.csproj")
                .WithFramework(framework)
                .Build();

            var builder = new AspNetCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.That(args, Does.Contain($"--framework {framework}"));
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

        [Test]
        public void BuildCommand_Uses_Last_Framework_And_Config_When_Duplicates()
        {
            var baseUrl = new Uri("http://localhost:9091");
            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, "/path/app.csproj")
                .WithArguments("--framework", "net7.0", "--configuration", "Debug", "--framework", "net8.0", "--configuration", "Release")
                .Build();

            var builder = new AspNetCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.That(args, Does.Contain("--framework net8.0"));
            Assert.That(args, Does.Contain("--configuration Release"));
        }

        [Test]
        public void BuildCommand_Defaults_When_NoArgs()
        {
            var cfg = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:9100"), "/p.csproj")
                .Build();
            var args = new AspNetCommandBuilder().BuildCommand(cfg);
            Assert.Multiple(() =>
            {
                Assert.That(args, Does.Contain("--framework net8.0"));
                Assert.That(args, Does.Contain("--configuration Release"));
                Assert.That(args, Does.Contain("--no-launch-profile"));
            });
        }

        [Test]
        public void BuildCommand_Preserves_Custom_Args()
        {
            var cfg = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:9101"), "/p.csproj")
                .WithArguments("--no-restore", "--verbosity", "quiet")
                .Build();
            var args = new AspNetCommandBuilder().BuildCommand(cfg);
            Assert.Multiple(() =>
            {
                Assert.That(args, Does.Contain("--no-restore"));
                Assert.That(args, Does.Contain("--verbosity quiet"));
            });
        }
    }
}
