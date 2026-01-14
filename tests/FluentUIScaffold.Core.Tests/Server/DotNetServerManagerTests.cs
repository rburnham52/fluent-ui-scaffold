using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Configuration.Launchers.Defaults;
using FluentUIScaffold.Core.Server;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Server
{
    public class DotNetServerManagerTests : IDisposable
    {
        private readonly Mock<IProcessRegistry> _mockRegistry;
        private readonly Mock<IProcessLauncher> _mockLauncher;
        private readonly Mock<IHealthWaiter> _mockHealthWaiter;
        private readonly DotNetServerManager _serverManager;
        private readonly ILogger _logger;

        public DotNetServerManagerTests()
        {
            _mockRegistry = new Mock<IProcessRegistry>();
            _mockLauncher = new Mock<IProcessLauncher>();
            _mockHealthWaiter = new Mock<IHealthWaiter>();
            _logger = NullLogger.Instance;

            _serverManager = new DotNetServerManager(
                _mockRegistry.Object,
                _mockLauncher.Object,
                _mockHealthWaiter.Object);
        }

        public void Dispose()
        {
            // Don't dispose in the shared fixture - each test manages its own lifecycle
        }

        [Test]
        public async Task EnsureStartedAsync_WithHealthyExistingServer_ReusesServer()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);
            var plan = CreateTestLaunchPlan("healthy-test");
            var configHash = ConfigHasher.Compute(plan);
            var existingStatus = new ServerStatus(1234, DateTimeOffset.UtcNow, plan.BaseUrl, configHash, true);

            _mockRegistry.Setup(r => r.TryLoad(configHash))
                        .Returns(existingStatus);

            // Act
            var result = await serverManager.EnsureStartedAsync(plan, _logger);

            // Assert
            Assert.That(result, Is.EqualTo(existingStatus));
            _mockLauncher.Verify(l => l.StartProcess(It.IsAny<LaunchPlan>(), It.IsAny<ILogger>()), Times.Never);
            _mockHealthWaiter.Verify(h => h.WaitUntilHealthyAsync(It.IsAny<LaunchPlan>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task EnsureStartedAsync_WithUnhealthyExistingServer_StartsNewServer()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);
            var plan = CreateTestLaunchPlan("unhealthy-test");
            var configHash = ConfigHasher.Compute(plan);
            var existingStatus = new ServerStatus(1234, DateTimeOffset.UtcNow, plan.BaseUrl, configHash, false);

            // Create a real lightweight process for testing instead of mocking
            var testProcess = CreateTestProcess();

            _mockRegistry.Setup(r => r.TryLoad(configHash))
                        .Returns(existingStatus);
            _mockRegistry.Setup(r => r.TryKill(existingStatus.Pid, It.IsAny<ILogger>()))
                        .Returns(true);
            _mockLauncher.Setup(l => l.StartProcess(plan, _logger))
                        .Returns(testProcess);
            _mockRegistry.Setup(r => r.UpdateWithReady(configHash, plan.BaseUrl))
                        .Returns(new ServerStatus(testProcess.Id, DateTimeOffset.UtcNow, plan.BaseUrl, configHash, true));

            // Act
            var result = await serverManager.EnsureStartedAsync(plan, _logger);

            // Assert
            Assert.That(result.IsHealthy, Is.True);
            Assert.That(result.Pid, Is.EqualTo(testProcess.Id));
            _mockRegistry.Verify(r => r.TryKill(existingStatus.Pid, _logger), Times.Once);
            _mockLauncher.Verify(l => l.StartProcess(plan, _logger), Times.Once);
            _mockHealthWaiter.Verify(h => h.WaitUntilHealthyAsync(plan, _logger, It.IsAny<CancellationToken>()), Times.Once);

            try
            {
                if (!testProcess.HasExited)
                {
                    testProcess.Kill();
                }
            }
            catch (InvalidOperationException)
            {
                // Process already disposed/exited, ignore
            }
            finally
            {
                testProcess.Dispose();
            }
        }

        [Test]
        public async Task EnsureStartedAsync_WithNoExistingServer_StartsNewServer()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);
            var plan = CreateTestLaunchPlan("new-server-test");
            var configHash = ConfigHasher.Compute(plan);
            var testProcess = CreateTestProcess();

            _mockRegistry.Setup(r => r.TryLoad(configHash))
                        .Returns((ServerStatus?)null);
            _mockRegistry.Setup(r => r.KillOrphans(It.IsAny<ILogger>()))
                        .Returns(0);
            _mockLauncher.Setup(l => l.StartProcess(plan, _logger))
                        .Returns(testProcess);
            _mockRegistry.Setup(r => r.UpdateWithReady(configHash, plan.BaseUrl))
                        .Returns(new ServerStatus(testProcess.Id, DateTimeOffset.UtcNow, plan.BaseUrl, configHash, true));

            // Act
            var result = await serverManager.EnsureStartedAsync(plan, _logger);

            // Assert
            Assert.That(result.IsHealthy, Is.True);
            Assert.That(result.Pid, Is.EqualTo(testProcess.Id));
            _mockRegistry.Verify(r => r.KillOrphans(_logger), Times.Once);
            _mockLauncher.Verify(l => l.StartProcess(plan, _logger), Times.Once);
            _mockHealthWaiter.Verify(h => h.WaitUntilHealthyAsync(plan, _logger, It.IsAny<CancellationToken>()), Times.Once);

            try
            {
                if (!testProcess.HasExited)
                {
                    testProcess.Kill();
                }
            }
            catch (InvalidOperationException)
            {
                // Process already disposed/exited, ignore
            }
            finally
            {
                testProcess.Dispose();
            }
        }

        [Test]
        public void RestartAsync_WithoutPreviousConfiguration_ThrowsException()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);

            // Act & Assert
            var exception = Assert.ThrowsAsync<InvalidOperationException>(
                () => serverManager.RestartAsync());

            Assert.That(exception.Message, Does.Contain("No previous configuration available"));
        }

        [Test]
        public async Task StopAsync_WithRunningServer_StopsServer()
        {
            // Arrange - Create fresh mock for this test to avoid shared state
            var mockRegistry = new Mock<IProcessRegistry>();
            var mockLauncher = new Mock<IProcessLauncher>();
            var mockHealthWaiter = new Mock<IHealthWaiter>();

            using var serverManager = new DotNetServerManager(mockRegistry.Object, mockLauncher.Object, mockHealthWaiter.Object);
            var plan = CreateTestLaunchPlan("stop-test");
            var configHash = ConfigHasher.Compute(plan);
            var testProcess = CreateTestProcess();
            var serverStatus = new ServerStatus(testProcess.Id, DateTimeOffset.UtcNow, plan.BaseUrl, configHash, true);

            mockRegistry.Setup(r => r.TryLoad(configHash))
                        .Returns((ServerStatus?)null);
            mockRegistry.Setup(r => r.KillOrphans(It.IsAny<ILogger>()))
                        .Returns(0);
            mockLauncher.Setup(l => l.StartProcess(plan, _logger))
                        .Returns(testProcess);
            mockRegistry.Setup(r => r.UpdateWithReady(configHash, plan.BaseUrl))
                        .Returns(serverStatus);

            // Start a server first
            await serverManager.EnsureStartedAsync(plan, _logger);

            // Act
            await serverManager.StopAsync();

            // Assert
            mockRegistry.Verify(r => r.TryKill(serverStatus.Pid, It.IsAny<ILogger>()), Times.Once);
            mockRegistry.Verify(r => r.Delete(serverStatus.ConfigHash), Times.Once);

            var status = serverManager.GetStatus();
            Assert.That(status, Is.EqualTo(ServerStatus.None));

            try
            {
                if (!testProcess.HasExited)
                {
                    testProcess.Kill();
                }
            }
            catch (InvalidOperationException)
            {
                // Process already disposed/exited, ignore
            }
            finally
            {
                testProcess.Dispose();
            }
        }

        [Test]
        public void GetStatus_WithoutStartedServer_ReturnsNone()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);

            // Act
            var status = serverManager.GetStatus();

            // Assert
            Assert.That(status, Is.EqualTo(ServerStatus.None));
        }

        [Test]
        public void EnsureStartedAsync_WithNullPlan_ThrowsArgumentNullException()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                () => serverManager.EnsureStartedAsync(null!, _logger));
        }

        [Test]
        public void EnsureStartedAsync_WithNullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            using var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);
            var plan = CreateTestLaunchPlan("null-logger-test");

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                () => serverManager.EnsureStartedAsync(plan, null!));
        }

        [Test]
        public void Dispose_AfterDisposed_ThrowsObjectDisposedException()
        {
            // Arrange
            var serverManager = new DotNetServerManager(_mockRegistry.Object, _mockLauncher.Object, _mockHealthWaiter.Object);
            serverManager.Dispose();

            // Act & Assert
            Assert.Throws<ObjectDisposedException>(() => serverManager.GetStatus());
        }

        private static LaunchPlan CreateTestLaunchPlan(string uniqueId = null)
        {
            // Use unique configurations to avoid mock sharing issues
            uniqueId ??= Guid.NewGuid().ToString("N")[..8];
            var uniquePort = 5000 + Math.Abs(uniqueId.GetHashCode()) % 1000;

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project TestApp-{uniqueId}",
                WorkingDirectory = $"/test/path/{uniqueId}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            return new LaunchPlan(
                startInfo,
                new Uri($"http://localhost:{uniquePort}"),
                TimeSpan.FromSeconds(60),
                new HttpReadinessProbe(),
                new[] { "/" },
                TimeSpan.FromSeconds(2),
                TimeSpan.FromMilliseconds(200),
                true
            );
        }

        private static Process CreateTestProcess()
        {
            // Create a simple, short-lived process for testing
            var startInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "sleep",
                Arguments = OperatingSystem.IsWindows() ? "/c timeout /t 60" : "60",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var process = Process.Start(startInfo);
            if (process == null)
                throw new InvalidOperationException("Failed to create test process");
            return process;
        }
    }
}
