using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentUIScaffold.AspireHosting.Tests
{
    /// <summary>
    /// Tests for Aspire startup timeout + heartbeat (Fix 2).
    /// </summary>
    [TestClass]
    public class AspireStartupTimeoutTests
    {
        [TestMethod]
        public async Task StartWithTimeoutAndHeartbeat_FiresTimeoutException_WhenStartupNeverCompletes()
        {
            // Simulate Aspire hanging indefinitely by handing back a never-completing task.
            var hangingTask = new TaskCompletionSource<object?>();

            var ex = await Assert.ThrowsExceptionAsync<TimeoutException>(() =>
                AspireHostingStrategy<DummyEntryPoint>.StartWithTimeoutAndHeartbeatAsync(
                    startupFactory: _ => hangingTask.Task,
                    timeout: TimeSpan.FromMilliseconds(150),
                    heartbeatInterval: TimeSpan.FromMilliseconds(50),
                    logger: NullLogger.Instance,
                    cancellationToken: CancellationToken.None));

            // The message must name the timeout duration so users can act on it
            // (i.e., know they need to bump WithAspireStartupTimeout).
            StringAssert.Contains(ex.Message, "0s", "Timeout message should include elapsed-style timing.");
            StringAssert.Contains(ex.Message, "WithAspireStartupTimeout");
        }

        [TestMethod]
        public async Task StartWithTimeoutAndHeartbeat_EmitsHeartbeatLogs_DuringStartup()
        {
            var slowTask = Task.Delay(TimeSpan.FromMilliseconds(250));
            var capturedLogger = new ListLogger();

            await AspireHostingStrategy<DummyEntryPoint>.StartWithTimeoutAndHeartbeatAsync(
                startupFactory: _ => slowTask,
                timeout: TimeSpan.FromSeconds(5),
                heartbeatInterval: TimeSpan.FromMilliseconds(75),
                logger: capturedLogger,
                cancellationToken: CancellationToken.None);

            Assert.IsTrue(
                capturedLogger.Messages.Exists(m => m.Contains("Aspire host starting...")),
                "Expected at least one 'Aspire host starting...' heartbeat log line during a slow startup. Got:\n"
                + string.Join("\n", capturedLogger.Messages));
        }

        [TestMethod]
        public async Task StartWithTimeoutAndHeartbeat_SucceedsAndStopsHeartbeat_WhenStartupCompletes()
        {
            // Should not throw, and should return promptly once the inner task completes.
            await AspireHostingStrategy<DummyEntryPoint>.StartWithTimeoutAndHeartbeatAsync(
                startupFactory: _ => Task.CompletedTask,
                timeout: TimeSpan.FromSeconds(5),
                heartbeatInterval: TimeSpan.FromSeconds(1),
                logger: NullLogger.Instance,
                cancellationToken: CancellationToken.None);
        }

        [TestMethod]
        public async Task StartWithTimeoutAndHeartbeat_PropagatesStartupExceptions()
        {
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                AspireHostingStrategy<DummyEntryPoint>.StartWithTimeoutAndHeartbeatAsync(
                    startupFactory: _ => Task.FromException(new InvalidOperationException("boom")),
                    timeout: TimeSpan.FromSeconds(5),
                    heartbeatInterval: TimeSpan.FromSeconds(1),
                    logger: NullLogger.Instance,
                    cancellationToken: CancellationToken.None));

            Assert.AreEqual("boom", ex.Message);
        }

        [TestMethod]
        public void WithAspireStartupTimeout_StoresValueOnBuilderConfig()
        {
            var builder = new FluentUIScaffoldBuilder();
            var returned = builder.WithAspireStartupTimeout(TimeSpan.FromMinutes(2));

            Assert.AreSame(builder, returned);
            Assert.AreEqual(TimeSpan.FromMinutes(2), AspireHostingExtensions.GetOrCreateConfig(builder).AspireStartupTimeout);
        }

        [TestMethod]
        public void WithAspireStartupTimeout_RejectsNonPositiveTimeout()
        {
            var builder = new FluentUIScaffoldBuilder();
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => builder.WithAspireStartupTimeout(TimeSpan.Zero));
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => builder.WithAspireStartupTimeout(TimeSpan.FromSeconds(-1)));
        }

        /// <summary>Placeholder for the generic type parameter — the timeout helper is static and never touches it.</summary>
        private sealed class DummyEntryPoint { }

        private sealed class ListLogger : ILogger
        {
            public List<string> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Messages.Add(formatter(state, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }
    }
}
