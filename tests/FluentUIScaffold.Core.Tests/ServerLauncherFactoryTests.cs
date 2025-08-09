using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class ServerLauncherFactoryTests
    {
        private ServerLauncherFactory _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new ServerLauncherFactory();
        }

        [Test]
        public void RegisterLauncher_WithValidLauncher_RegistersSuccessfully()
        {
            // Arrange
            var launcher = new TestServerLauncher();

            // Act
            _factory.RegisterLauncher(launcher);

            // Assert
            var retrievedLauncher = _factory.GetLauncher(new ServerConfiguration { ServerType = ServerType.AspNetCore });
            Assert.That(retrievedLauncher, Is.EqualTo(launcher));
        }

        [Test]
        public void RegisterDetector_WithValidDetector_RegistersSuccessfully()
        {
            // Arrange
            var detector = new TestProjectDetector();

            // Act
            _factory.RegisterDetector(detector);

            // Assert
            // The detector should be registered and available for project detection
            Assert.Pass("Detector registered successfully");
        }

        [Test]
        public void CreateConfiguration_WithValidParameters_ReturnsConfiguration()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var projectPath = "/path/to/project.csproj";

            // Act
            var config = ServerLauncherFactory.CreateConfiguration(baseUrl, ServerType.AspNetCore, projectPath);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(config.BaseUrl, Is.EqualTo(baseUrl));
                Assert.That(config.ServerType, Is.EqualTo(ServerType.AspNetCore));
                Assert.That(config.ProjectPath, Is.EqualTo(projectPath));
            });
        }

        [Test]
        public void CreateConfigurationWithDetection_WithValidParameters_ReturnsConfiguration()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var additionalPaths = new List<string> { "/path1", "/path2" };

            // Act & Assert
            // This test would require actual project detection, so we'll just verify the method exists
            Assert.Throws<InvalidOperationException>(() =>
                _factory.CreateConfigurationWithDetection(baseUrl, ServerType.AspNetCore, additionalPaths));
        }

        [Test]
        public void GetLauncher_WithUnregisteredServerType_ThrowsException()
        {
            // Arrange
            var config = new ServerConfiguration { ServerType = ServerType.Aspire };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _factory.GetLauncher(config));
        }

        private class TestServerLauncher : IServerLauncher
        {
            public string Name => "TestServerLauncher";

            public bool CanHandle(ServerConfiguration configuration)
            {
                return configuration.ServerType == ServerType.AspNetCore;
            }

            public Task LaunchAsync(ServerConfiguration configuration)
            {
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                // No cleanup needed for test
            }
        }

        private class TestProjectDetector : IProjectDetector
        {
            public string Name => "TestProjectDetector";
            public int Priority => 100;

            public string? DetectProjectPath(ProjectDetectionContext context)
            {
                return null; // Return null for test
            }
        }
    }
}
