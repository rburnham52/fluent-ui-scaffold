using System;
using NUnit.Framework;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class LaunchPlanBuilderTests
    {
        [Test]
        public void DotNet_Builder_BuildsExpectedStartInfo()
        {
            var plan = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5050"), "/path/to/App.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .WithAspNetCoreEnvironment("Development")
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(plan.StartInfo.FileName, Is.EqualTo("dotnet"));
                Assert.That(plan.StartInfo.Arguments, Does.StartWith("run "));
                Assert.That(plan.StartInfo.Arguments, Does.Contain("--framework net8.0"));
                Assert.That(plan.StartInfo.Arguments, Does.Contain("--configuration Release"));
                Assert.That(plan.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Development"));
            });
        }

        [Test]
        public void Aspire_DefaultHealthEndpoints_IncludeRootAndHealth()
        {
            var plan = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:6060"), "/path/to/Host.csproj")
                .Build();

            Assert.That(plan.HealthCheckEndpoints, Is.EquivalentTo(new[] { "/", "/health" }));
        }

        [Test]
        public void WithReadiness_Settings_AreApplied()
        {
            var customProbe = new Tests.Mocks.FakeReadinessProbe(true);
            var initialDelay = TimeSpan.FromSeconds(3);
            var poll = TimeSpan.FromMilliseconds(500);

            var plan = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5051"), "/path/to/App.csproj")
                .WithReadiness(customProbe, initialDelay, poll)
                .WithProcessOutputLogging(false)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(plan.ReadinessProbe, Is.SameAs(customProbe));
                Assert.That(plan.InitialDelay, Is.EqualTo(initialDelay));
                Assert.That(plan.PollInterval, Is.EqualTo(poll));
                Assert.That(plan.StreamProcessOutput, Is.False);
            });
        }

        [Test]
        public void WithEnvironmentVariables_MergeAndOverride()
        {
            var plan = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5052"), "/path/to/App.csproj")
                .WithEnvironmentVariable("FOO", "one")
                .WithEnvironmentVariables(new System.Collections.Generic.Dictionary<string, string>
                {
                    ["FOO"] = "two",
                    ["BAR"] = "bar"
                })
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(plan.StartInfo.EnvironmentVariables["FOO"], Is.EqualTo("two"));
                Assert.That(plan.StartInfo.EnvironmentVariables["BAR"], Is.EqualTo("bar"));
            });
        }
    }
}