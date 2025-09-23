using System;
using System.Diagnostics;

using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Configuration.Launchers.Defaults;
using FluentUIScaffold.Core.Server;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Server
{
    public class ConfigHasherTests
    {
        [Test]
        public void Compute_WithSameConfiguration_ReturnsSameHash()
        {
            // Arrange
            var plan1 = CreateBasicLaunchPlan();
            var plan2 = CreateBasicLaunchPlan();

            // Act
            var hash1 = ConfigHasher.Compute(plan1);
            var hash2 = ConfigHasher.Compute(plan2);

            // Assert
            Assert.That(hash1, Is.EqualTo(hash2));
        }

        [Test]
        public void Compute_WithDifferentExecutable_ReturnsDifferentHash()
        {
            // Arrange
            var plan1 = CreateBasicLaunchPlan();
            var plan2 = CreateBasicLaunchPlan();
            plan2.StartInfo.FileName = "different-executable";

            // Act
            var hash1 = ConfigHasher.Compute(plan1);
            var hash2 = ConfigHasher.Compute(plan2);

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void Compute_WithDifferentArguments_ReturnsDifferentHash()
        {
            // Arrange
            var plan1 = CreateBasicLaunchPlan();
            var plan2 = CreateBasicLaunchPlan();
            plan2.StartInfo.Arguments = "different arguments";

            // Act
            var hash1 = ConfigHasher.Compute(plan1);
            var hash2 = ConfigHasher.Compute(plan2);

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void Compute_WithDifferentEnvironmentVariables_ReturnsDifferentHash()
        {
            // Arrange
            var plan1 = CreateBasicLaunchPlan();
            var plan2 = CreateBasicLaunchPlan();
            plan2.StartInfo.EnvironmentVariables["EXTRA_VAR"] = "extra_value";

            // Act
            var hash1 = ConfigHasher.Compute(plan1);
            var hash2 = ConfigHasher.Compute(plan2);

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void Compute_WithDifferentBaseUrl_ReturnsDifferentHash()
        {
            // Arrange
            var plan1 = CreateBasicLaunchPlan();
            var plan2 = new LaunchPlan(
                plan1.StartInfo,
                new Uri("http://localhost:5001"),
                plan1.StartupTimeout,
                plan1.ReadinessProbe,
                plan1.HealthCheckEndpoints,
                plan1.InitialDelay,
                plan1.PollInterval,
                plan1.StreamProcessOutput
            );

            // Act
            var hash1 = ConfigHasher.Compute(plan1);
            var hash2 = ConfigHasher.Compute(plan2);

            // Assert
            Assert.That(hash1, Is.Not.EqualTo(hash2));
        }

        [Test]
        public void Compute_WithNullPlan_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ConfigHasher.Compute(null!));
        }

        [Test]
        public void Equals_WithIdenticalHashes_ReturnsTrue()
        {
            // Arrange
            var hash = "abc123";

            // Act & Assert
            Assert.That(ConfigHasher.Equals(hash, hash), Is.True);
        }

        [Test]
        public void Equals_WithDifferentCase_ReturnsTrue()
        {
            // Arrange
            var hash1 = "ABC123";
            var hash2 = "abc123";

            // Act & Assert
            Assert.That(ConfigHasher.Equals(hash1, hash2), Is.True);
        }

        [Test]
        public void Equals_WithDifferentHashes_ReturnsFalse()
        {
            // Arrange
            var hash1 = "abc123";
            var hash2 = "def456";

            // Act & Assert
            Assert.That(ConfigHasher.Equals(hash1, hash2), Is.False);
        }

        private static LaunchPlan CreateBasicLaunchPlan()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project TestApp",
                WorkingDirectory = "/test/path",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            // Use indexer assignment instead of Add() to avoid conflicts with existing environment variables
            startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = "Development";
            startInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"] = "Development";

            return new LaunchPlan(
                startInfo,
                new Uri("http://localhost:5000"),
                TimeSpan.FromSeconds(60),
                new HttpReadinessProbe(),
                new[] { "/", "/health" },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(200),
                true
            );
        }
    }
}
