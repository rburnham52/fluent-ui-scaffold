using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    /// <summary>
    /// Manages persistent state of server processes across test runs.
    /// Handles process discovery, state persistence, and orphan cleanup.
    /// </summary>
    public interface IProcessRegistry
    {
        /// <summary>
        /// Attempts to load a previously saved server state by configuration hash.
        /// </summary>
        /// <param name="configHash">The configuration hash to look up</param>
        /// <returns>The server status if found and still valid, otherwise null</returns>
        ServerStatus? TryLoad(string configHash);

        /// <summary>
        /// Saves the current server state to persistent storage.
        /// </summary>
        /// <param name="configHash">The configuration hash</param>
        /// <param name="process">The running process</param>
        /// <param name="baseUrl">The server's base URL</param>
        void Save(string configHash, Process process, Uri baseUrl);

        /// <summary>
        /// Updates an existing server state to mark it as healthy and ready.
        /// </summary>
        /// <param name="configHash">The configuration hash</param>
        /// <param name="baseUrl">The server's base URL</param>
        /// <returns>The updated server status</returns>
        ServerStatus UpdateWithReady(string configHash, Uri baseUrl);

        /// <summary>
        /// Attempts to gracefully kill a process by PID.
        /// </summary>
        /// <param name="pid">The process ID to kill</param>
        /// <param name="logger">Optional logger for diagnostics</param>
        /// <returns>True if the process was killed or was not running</returns>
        bool TryKill(int pid, ILogger? logger = null);

        /// <summary>
        /// Deletes the saved state for a configuration hash.
        /// </summary>
        /// <param name="configHash">The configuration hash to delete</param>
        void Delete(string configHash);

        /// <summary>
        /// Finds and kills orphaned processes matching previous configurations.
        /// </summary>
        /// <param name="logger">Optional logger for diagnostics</param>
        /// <returns>Number of orphaned processes cleaned up</returns>
        int KillOrphans(ILogger? logger = null);
    }

    /// <summary>
    /// Default implementation of IProcessRegistry using filesystem-based persistence.
    /// </summary>
    public sealed class ProcessRegistry : IProcessRegistry
    {
        private readonly string _stateDirectory;
        private readonly object _lock = new object();

        /// <summary>
        /// Represents the persistent state of a server process.
        /// </summary>
        private sealed record ProcessState(
            int Pid,
            DateTimeOffset StartTime,
            string Executable,
            string Arguments,
            string BaseUrl,
            string ConfigHash,
            bool IsHealthy
        );

        public ProcessRegistry()
        {
            // Determine platform-specific state directory
            _stateDirectory = GetStateDirectory();
            Directory.CreateDirectory(_stateDirectory);
        }

        public ServerStatus? TryLoad(string configHash)
        {
            if (string.IsNullOrEmpty(configHash)) return null;

            lock (_lock)
            {
                var stateFile = GetStateFile(configHash);
                if (!File.Exists(stateFile)) return null;

                try
                {
                    var json = File.ReadAllText(stateFile);
                    var state = JsonSerializer.Deserialize<ProcessState>(json);
                    if (state == null) return null;

                    // Verify the process is still running
                    if (!IsProcessRunning(state.Pid))
                    {
                        // Process died, clean up stale state
                        File.Delete(stateFile);
                        return null;
                    }

                    return new ServerStatus(
                        state.Pid,
                        state.StartTime,
                        new Uri(state.BaseUrl),
                        state.ConfigHash,
                        state.IsHealthy
                    );
                }
                catch
                {
                    // Corrupted state file, delete it
                    try { File.Delete(stateFile); } catch { }
                    return null;
                }
            }
        }

        public void Save(string configHash, Process process, Uri baseUrl)
        {
            if (string.IsNullOrEmpty(configHash)) throw new ArgumentException("Config hash cannot be null or empty", nameof(configHash));
            if (process == null) throw new ArgumentNullException(nameof(process));
            if (baseUrl == null) throw new ArgumentNullException(nameof(baseUrl));

            lock (_lock)
            {
                var state = new ProcessState(
                    process.Id,
                    DateTimeOffset.UtcNow,
                    process.StartInfo.FileName ?? "",
                    process.StartInfo.Arguments ?? "",
                    baseUrl.ToString(),
                    configHash,
                    false // Will be updated to true when health checks pass
                );

                var stateFile = GetStateFile(configHash);
                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(stateFile, json);
            }
        }

        public ServerStatus UpdateWithReady(string configHash, Uri baseUrl)
        {
            if (string.IsNullOrEmpty(configHash)) throw new ArgumentException("Config hash cannot be null or empty", nameof(configHash));
            if (baseUrl == null) throw new ArgumentNullException(nameof(baseUrl));

            lock (_lock)
            {
                var stateFile = GetStateFile(configHash);
                if (!File.Exists(stateFile))
                    throw new InvalidOperationException($"No saved state found for config hash: {configHash}");

                var json = File.ReadAllText(stateFile);
                var state = JsonSerializer.Deserialize<ProcessState>(json)
                    ?? throw new InvalidOperationException("Failed to deserialize process state");

                // Update to healthy
                var updatedState = state with { IsHealthy = true };
                var updatedJson = JsonSerializer.Serialize(updatedState, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(stateFile, updatedJson);

                return new ServerStatus(
                    updatedState.Pid,
                    updatedState.StartTime,
                    new Uri(updatedState.BaseUrl),
                    updatedState.ConfigHash,
                    updatedState.IsHealthy
                );
            }
        }

        public bool TryKill(int pid, ILogger? logger = null)
        {
            if (pid <= 0) return true;

            try
            {
                var process = Process.GetProcessById(pid);
                if (process.HasExited) return true;

                logger?.LogInformation("Terminating process {Pid} ({ProcessName})", pid, process.ProcessName);

                // Try graceful shutdown first
                if (OperatingSystem.IsWindows())
                {
                    process.CloseMainWindow();
                    if (process.WaitForExit(5000)) return true;
                }

                // Force kill if graceful shutdown failed
                process.Kill(entireProcessTree: true);
                process.WaitForExit(10000);
                return process.HasExited;
            }
            catch (ArgumentException)
            {
                // Process not found - already dead
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to kill process {Pid}", pid);
                return false;
            }
        }

        public void Delete(string configHash)
        {
            if (string.IsNullOrEmpty(configHash)) return;

            lock (_lock)
            {
                var stateFile = GetStateFile(configHash);
                try
                {
                    if (File.Exists(stateFile))
                    {
                        File.Delete(stateFile);
                    }
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        }

        public int KillOrphans(ILogger? logger = null)
        {
            var killed = 0;

            lock (_lock)
            {
                try
                {
                    var stateFiles = Directory.GetFiles(_stateDirectory, "*.json");

                    foreach (var stateFile in stateFiles)
                    {
                        try
                        {
                            var json = File.ReadAllText(stateFile);
                            var state = JsonSerializer.Deserialize<ProcessState>(json);
                            if (state == null) continue;

                            if (!IsProcessRunning(state.Pid))
                            {
                                // Process is dead, clean up state
                                File.Delete(stateFile);
                            }
                            else
                            {
                                // Process is running, check if it's stale (older than 1 hour)
                                if (DateTimeOffset.UtcNow - state.StartTime > TimeSpan.FromHours(1))
                                {
                                    logger?.LogInformation("Killing stale server process {Pid} (started {StartTime})", state.Pid, state.StartTime);
                                    if (TryKill(state.Pid, logger))
                                    {
                                        killed++;
                                        File.Delete(stateFile);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // Ignore errors with individual state files
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Failed to clean up orphaned processes");
                }
            }

            return killed;
        }

        private string GetStateFile(string configHash)
        {
            return Path.Combine(_stateDirectory, $"{configHash}.json");
        }

        private static bool IsProcessRunning(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                return !process.HasExited;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static string GetStateDirectory()
        {
            string baseDir;

            if (OperatingSystem.IsWindows())
            {
                baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(baseDir, "FluentUIScaffold", "servers");
            }
            else
            {
                // Linux/macOS
                var xdgRuntimeDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
                if (!string.IsNullOrEmpty(xdgRuntimeDir) && Directory.Exists(xdgRuntimeDir))
                {
                    return Path.Combine(xdgRuntimeDir, "fluentuiscaffold", "servers");
                }

                // Fallback to /tmp
                return Path.Combine("/tmp", "fluentuiscaffold", "servers");
            }
        }
    }
}
