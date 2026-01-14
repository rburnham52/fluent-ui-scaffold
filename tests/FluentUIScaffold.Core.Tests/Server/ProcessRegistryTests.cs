using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Server;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Server
{
    public class ProcessRegistryTests : IDisposable
    {
        private readonly ProcessRegistry _registry;
        private readonly ILogger _logger;
        private readonly string _testConfigHash;
        private readonly List<Process> _testProcesses = new();

        public ProcessRegistryTests()
        {
            _registry = new ProcessRegistry();
            _logger = NullLogger.Instance;
            _testConfigHash = "test-config-hash-" + Guid.NewGuid().ToString("N")[..8];
        }

        public void Dispose()
        {
            // Clean up all test processes first
            foreach (var process in _testProcesses)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (InvalidOperationException)
                {
                    // Process already disposed/exited, ignore
                }
                finally
                {
                    try
                    {
                        process.Dispose();
                    }
                    catch (InvalidOperationException)
                    {
                        // Process already disposed, ignore
                    }
                }
            }
            _testProcesses.Clear();

            // Clean up test data
            _registry.Delete(_testConfigHash);
        }

        [Test]
        public void TryLoad_WithNonExistentConfig_ReturnsNull()
        {
            // Act
            var result = _registry.TryLoad("non-existent-config");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void Save_ThenLoad_ReturnsCorrectStatus()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var process = CreateDummyProcess();

            // Act
            _registry.Save(_testConfigHash, process, baseUrl);
            var result = _registry.TryLoad(_testConfigHash);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Pid, Is.EqualTo(process.Id));
            Assert.That(result.BaseUrl, Is.EqualTo(baseUrl));
            Assert.That(result.ConfigHash, Is.EqualTo(_testConfigHash));
            Assert.That(result.IsHealthy, Is.False); // Should be false initially
        }

        [Test]
        public void UpdateWithReady_UpdatesHealthStatus()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var process = CreateDummyProcess();
            _registry.Save(_testConfigHash, process, baseUrl);

            // Act
            var result = _registry.UpdateWithReady(_testConfigHash, baseUrl);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsHealthy, Is.True);
            Assert.That(result.Pid, Is.EqualTo(process.Id));
            Assert.That(result.BaseUrl, Is.EqualTo(baseUrl));
        }

        [Test]
        public void UpdateWithReady_WithNonExistentConfig_ThrowsException()
        {
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(
                () => _registry.UpdateWithReady("non-existent", new Uri("http://localhost:5000")));

            Assert.That(exception.Message, Does.Contain("No saved state found"));
        }

        [Test]
        public void TryKill_WithInvalidPid_ReturnsTrue()
        {
            // Act
            var result = _registry.TryKill(-1, _logger);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void TryKill_WithZeroPid_ReturnsTrue()
        {
            // Act
            var result = _registry.TryKill(0, _logger);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Delete_WithExistingConfig_RemovesState()
        {
            // Arrange
            var baseUrl = new Uri("http://localhost:5000");
            var process = CreateDummyProcess();
            _registry.Save(_testConfigHash, process, baseUrl);

            // Verify it exists
            var beforeDelete = _registry.TryLoad(_testConfigHash);
            Assert.That(beforeDelete, Is.Not.Null);

            // Act
            _registry.Delete(_testConfigHash);

            // Assert
            var afterDelete = _registry.TryLoad(_testConfigHash);
            Assert.That(afterDelete, Is.Null);
        }

        [Test]
        public void Delete_WithNonExistentConfig_DoesNotThrow()
        {
            // Act & Assert (should not throw)
            _registry.Delete("non-existent-config");
        }

        [Test]
        public void KillOrphans_ReturnsCountOfKilledProcesses()
        {
            // Act
            var result = _registry.KillOrphans(_logger);

            // Assert
            Assert.That(result >= 0, Is.True);
        }

        private Process CreateDummyProcess()
        {
            // Create a long-running, stable process for testing
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            if (OperatingSystem.IsWindows())
            {
                // Use powershell Start-Sleep which is more reliable than timeout
                startInfo.FileName = "powershell.exe";
                startInfo.Arguments = "-Command \"Start-Sleep -Seconds 300\""; // 5 minute sleep, will terminate naturally
            }
            else
            {
                // Use sleep command on Unix systems
                startInfo.FileName = "sleep";
                startInfo.Arguments = "300"; // Sleep for 5 minutes
            }

            var process = Process.Start(startInfo);
            Assert.That(process, Is.Not.Null, "Failed to start test process");

            // Track this process for cleanup
            _testProcesses.Add(process);

            // Give the process a moment to fully start
            System.Threading.Thread.Sleep(100);

            Assert.That(process.HasExited, Is.False, "Test process should not have exited immediately");

            return process;
        }
    }
}
