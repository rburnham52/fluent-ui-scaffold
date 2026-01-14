using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    /// <summary>
    /// Handles launching and managing server processes.
    /// </summary>
    public interface IProcessLauncher
    {
        /// <summary>
        /// Starts a new server process using the provided launch plan.
        /// </summary>
        /// <param name="plan">The launch plan containing startup configuration</param>
        /// <param name="logger">Logger for process output and diagnostics</param>
        /// <returns>The started process</returns>
        Process StartProcess(LaunchPlan plan, ILogger logger);
    }

    /// <summary>
    /// Default implementation of IProcessLauncher with output streaming support.
    /// </summary>
    public sealed class ProcessLauncher : IProcessLauncher
    {
        private readonly object _outputLock = new object();

        public Process StartProcess(LaunchPlan plan, ILogger logger)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("Starting server process: {FileName} {Arguments}",
                plan.StartInfo.FileName, plan.StartInfo.Arguments);
            logger.LogInformation("Working directory: {WorkingDirectory}", plan.StartInfo.WorkingDirectory);

            var process = new Process { StartInfo = plan.StartInfo };

            if (plan.StreamProcessOutput)
            {
                SetupOutputStreaming(process, logger);
            }

            try
            {
                if (!process.Start())
                {
                    throw new InvalidOperationException("Failed to start the server process");
                }

                logger.LogInformation("Server process started with PID {Pid}", process.Id);

                if (plan.StreamProcessOutput)
                {
                    // Start async reading of output streams
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }

                return process;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to start server process");
                process?.Dispose();
                throw;
            }
        }

        private void SetupOutputStreaming(Process process, ILogger logger)
        {
            process.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    lock (_outputLock)
                    {
                        logger.LogInformation("[Server-OUT] {Message}", args.Data);
                    }
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    lock (_outputLock)
                    {
                        // Log errors as warnings to avoid test failures on normal stderr output
                        logger.LogWarning("[Server-ERR] {Message}", args.Data);
                    }
                }
            };
        }
    }
}
