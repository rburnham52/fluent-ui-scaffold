using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FluentUIScaffold.AspireHosting
{
    /// <summary>
    /// Performs a fast (≤2s) Docker daemon health check before Aspire bootstrap.
    /// Surfaces a clear, single-line user-facing error when the daemon is unreachable
    /// instead of letting Aspire hang and bury the failure ~20 stack frames deep.
    /// </summary>
    public static class DockerPreflightCheck
    {
        /// <summary>
        /// The user-facing error message thrown when the Docker daemon is not reachable.
        /// Exposed as a public constant so downstream tests can assert on the exact text.
        /// </summary>
        public const string DockerUnreachableMessage =
            "Docker daemon is not reachable. FluentUIScaffold.AspireHosting requires Docker " +
            "(or a compatible container runtime like Rancher Desktop, Podman with Docker compatibility) " +
            "to be running." +
            "\n\n" +
            "Common causes:\n" +
            "- Docker Desktop / Rancher Desktop is not started\n" +
            "- Docker Desktop is in Resource Saver mode (click the tray icon to wake it)\n" +
            "- The Docker daemon crashed (check the logs)" +
            "\n\n" +
            "Start your container runtime and try again.";

        /// <summary>
        /// Default timeout for the Docker health probe. Kept tight so a stuck daemon
        /// fails fast instead of compounding into Aspire's longer startup hang.
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// Runs <c>docker info --format "{{.ServerVersion}}"</c> with a short timeout.
        /// Throws <see cref="InvalidOperationException"/> with <see cref="DockerUnreachableMessage"/>
        /// and no inner exception (to keep the error single-line and not bury the cause)
        /// if the command fails, times out, or returns no server version.
        /// </summary>
        /// <param name="timeout">Optional override for the probe timeout. Defaults to <see cref="DefaultTimeout"/>.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        public static async Task EnsureDockerHealthyAsync(
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var effectiveTimeout = timeout ?? DefaultTimeout;

            if (!await IsDockerHealthyAsync(effectiveTimeout, cancellationToken).ConfigureAwait(false))
            {
                throw new InvalidOperationException(DockerUnreachableMessage);
            }
        }

        /// <summary>
        /// Probes the Docker daemon. Returns true if <c>docker info</c> exits with code 0
        /// and produces non-empty output within the timeout. Returns false otherwise
        /// (including when the docker binary is not on PATH at all).
        /// </summary>
        internal static async Task<bool> IsDockerHealthyAsync(
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Process? process = null;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "info --format \"{{.ServerVersion}}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                process = new Process { StartInfo = psi };

                try
                {
                    if (!process.Start())
                    {
                        return false;
                    }
                }
                catch
                {
                    // docker binary is not on PATH, or some other launch failure.
                    return false;
                }

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);

                try
                {
#if NET8_0_OR_GREATER
                    await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
#else
                    var exited = process.WaitForExit((int)timeout.TotalMilliseconds);
                    if (!exited)
                    {
                        TryKill(process);
                        return false;
                    }
                    await Task.CompletedTask.ConfigureAwait(false);
#endif
                }
                catch (OperationCanceledException)
                {
                    TryKill(process);
                    return false;
                }

                if (process.ExitCode != 0)
                {
                    return false;
                }

                var stdout = await process.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
                return !string.IsNullOrWhiteSpace(stdout);
            }
            catch
            {
                return false;
            }
            finally
            {
                process?.Dispose();
            }
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // best-effort
            }
        }
    }
}
