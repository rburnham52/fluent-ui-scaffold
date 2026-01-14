using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Hosting;
using FluentUIScaffold.Core.Server;

using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Hosting
{
    [TestFixture]
    public class HostingStrategyTests
    {
        private Mock<ILogger> _mockLogger = null!;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        }

        #region ExternalHostingStrategy Tests

        [Test]
        public void ExternalHostingStrategy_Constructor_SetsProperties()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var endpoints = new[] { "/health", "/ready" };

            // Act
            var strategy = new ExternalHostingStrategy(baseUrl, endpoints);

            // Assert
            Assert.That(strategy.BaseUrl, Is.EqualTo(baseUrl));
            Assert.That(string.IsNullOrEmpty(strategy.ConfigurationHash), Is.False);
        }

        [Test]
        public void ExternalHostingStrategy_GetStatus_ReturnsNotRunning_BeforeStart()
        {
            // Arrange
            var strategy = new ExternalHostingStrategy(new Uri("http://localhost:5000"));

            // Act
            var status = strategy.GetStatus();

            // Assert
            Assert.That(status.IsRunning, Is.False);
            Assert.That(status.ProcessId, Is.Null); // External servers have no process ID
        }

        [Test]
        public async Task ExternalHostingStrategy_StopAsync_Succeeds_EvenIfNotStarted()
        {
            // Arrange
            var strategy = new ExternalHostingStrategy(new Uri("http://localhost:5000"));

            // Act & Assert - should not throw
            await strategy.StopAsync();
        }

        [Test]
        public async Task ExternalHostingStrategy_DisposeAsync_Succeeds()
        {
            // Arrange
            var strategy = new ExternalHostingStrategy(new Uri("http://localhost:5000"));

            // Act & Assert - should not throw
            await strategy.DisposeAsync();
        }

        [Test]
        public void ExternalHostingStrategy_ConfigurationHash_IsDeterministic()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var endpoints = new[] { "/health" };

            var strategy1 = new ExternalHostingStrategy(baseUrl, endpoints);
            var strategy2 = new ExternalHostingStrategy(baseUrl, endpoints);

            // Act & Assert
            Assert.That(strategy1.ConfigurationHash, Is.EqualTo(strategy2.ConfigurationHash));
        }

        [Test]
        public void ExternalHostingStrategy_ConfigurationHash_DiffersForDifferentUrls()
        {
            // Arrange
            var strategy1 = new ExternalHostingStrategy(new Uri("http://localhost:5000"));
            var strategy2 = new ExternalHostingStrategy(new Uri("http://localhost:5001"));

            // Act & Assert
            Assert.That(strategy1.ConfigurationHash, Is.Not.EqualTo(strategy2.ConfigurationHash));
        }

        #endregion

        #region DotNetHostingStrategy Tests

        [Test]
        public void DotNetHostingStrategy_Constructor_SetsProperties()
        {
            // Arrange
            var mockServerManager = new Mock<IServerManager>();
            var launchPlan = CreateTestLaunchPlan();

            // Act
            var strategy = new DotNetHostingStrategy(launchPlan, mockServerManager.Object);

            // Assert
            Assert.That(strategy.ConfigurationHash, Is.Not.Null);
            Assert.That(strategy.BaseUrl, Is.EqualTo(launchPlan.BaseUrl));
        }

        [Test]
        public async Task DotNetHostingStrategy_StartAsync_CallsServerManager()
        {
            // Arrange
            var launchPlan = CreateTestLaunchPlan();
            var mockServerManager = new Mock<IServerManager>();
            var expectedStatus = new ServerStatus(
                Pid: 1234,
                StartTime: DateTimeOffset.Now,
                BaseUrl: launchPlan.BaseUrl,
                ConfigHash: ConfigHasher.Compute(launchPlan),
                IsHealthy: true);

            mockServerManager
                .Setup(x => x.EnsureStartedAsync(launchPlan, _mockLogger.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStatus);

            var strategy = new DotNetHostingStrategy(launchPlan, mockServerManager.Object);

            // Act
            var result = await strategy.StartAsync(_mockLogger.Object);

            // Assert
            Assert.That(result.BaseUrl, Is.EqualTo(launchPlan.BaseUrl));
            mockServerManager.Verify(
                x => x.EnsureStartedAsync(launchPlan, _mockLogger.Object, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Test]
        public async Task DotNetHostingStrategy_StopAsync_CallsServerManager()
        {
            // Arrange
            var launchPlan = CreateTestLaunchPlan();
            var mockServerManager = new Mock<IServerManager>();
            var expectedStatus = new ServerStatus(
                Pid: 1234,
                StartTime: DateTimeOffset.Now,
                BaseUrl: launchPlan.BaseUrl,
                ConfigHash: ConfigHasher.Compute(launchPlan),
                IsHealthy: true);

            mockServerManager
                .Setup(x => x.EnsureStartedAsync(launchPlan, _mockLogger.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStatus);

            var strategy = new DotNetHostingStrategy(launchPlan, mockServerManager.Object);
            await strategy.StartAsync(_mockLogger.Object);

            // Act
            await strategy.StopAsync();

            // Assert
            mockServerManager.Verify(x => x.StopAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void DotNetHostingStrategy_GetStatus_ReturnsServerManagerStatus()
        {
            // Arrange
            var launchPlan = CreateTestLaunchPlan();
            var mockServerManager = new Mock<IServerManager>();
            var expectedStatus = new ServerStatus(
                Pid: 1234,
                StartTime: DateTimeOffset.Now,
                BaseUrl: launchPlan.BaseUrl,
                ConfigHash: ConfigHasher.Compute(launchPlan),
                IsHealthy: true);

            mockServerManager.Setup(x => x.GetStatus()).Returns(expectedStatus);

            var strategy = new DotNetHostingStrategy(launchPlan, mockServerManager.Object);

            // Act
            var status = strategy.GetStatus();

            // Assert
            Assert.That(status.IsRunning, Is.True);
            Assert.That(status.ProcessId, Is.EqualTo(1234));
            Assert.That(status.BaseUrl, Is.EqualTo(launchPlan.BaseUrl));
        }

        #endregion

        #region NodeHostingStrategy Tests

        [Test]
        public void NodeHostingStrategy_Constructor_SetsProperties()
        {
            // Arrange
            var mockServerManager = new Mock<IServerManager>();
            var launchPlan = CreateNodeTestLaunchPlan();

            // Act
            var strategy = new NodeHostingStrategy(launchPlan, mockServerManager.Object);

            // Assert
            Assert.That(strategy.ConfigurationHash, Is.Not.Null);
            Assert.That(strategy.BaseUrl, Is.EqualTo(launchPlan.BaseUrl));
        }

        [Test]
        public async Task NodeHostingStrategy_StartAsync_CallsServerManager()
        {
            // Arrange
            var launchPlan = CreateNodeTestLaunchPlan();
            var mockServerManager = new Mock<IServerManager>();
            var expectedStatus = new ServerStatus(
                Pid: 5678,
                StartTime: DateTimeOffset.Now,
                BaseUrl: launchPlan.BaseUrl,
                ConfigHash: ConfigHasher.Compute(launchPlan),
                IsHealthy: true);

            mockServerManager
                .Setup(x => x.EnsureStartedAsync(launchPlan, _mockLogger.Object, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedStatus);

            var strategy = new NodeHostingStrategy(launchPlan, mockServerManager.Object);

            // Act
            var result = await strategy.StartAsync(_mockLogger.Object);

            // Assert
            Assert.That(result.BaseUrl, Is.EqualTo(launchPlan.BaseUrl));
            mockServerManager.Verify(
                x => x.EnsureStartedAsync(launchPlan, _mockLogger.Object, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        #endregion

        #region Helper Methods

        private static LaunchPlan CreateTestLaunchPlan()
        {
            return ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5000"), "test.csproj")
                .Build();
        }

        private static LaunchPlan CreateNodeTestLaunchPlan()
        {
            return ServerConfiguration
                .CreateNodeJsServer(new Uri("http://localhost:3000"), ".")
                .Build();
        }

        #endregion
    }
}
