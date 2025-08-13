using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    /// <summary>
    /// Generic process-based server launcher. It composes an executable, a command builder,
    /// an environment variable provider, and a readiness probe to start and validate a server
    /// regardless of underlying platform (ASP.NET Core, Aspire, Node.js, etc.).
    /// </summary>
    public sealed class ProcessLauncher : IServerLauncher
    {
        private readonly ILogger? _logger;
        private readonly string _name;
        private readonly string _executable;
        private readonly ServerType[] _supportedTypes;
        private readonly ICommandBuilder _commandBuilder;
        private readonly IEnvVarProvider _envVarProvider;
        private readonly IReadinessProbe _readinessProbe;
        private readonly IProcessRunner _processRunner;
        private readonly IClock _clock;
        private bool _disposed;

        public string Name => _name;

        public ProcessLauncher(
            string name,
            string executable,
            ServerType[] supportedTypes,
            ICommandBuilder commandBuilder,
            IEnvVarProvider envVarProvider,
            IReadinessProbe readinessProbe,
            IProcessRunner? processRunner = null,
            IClock? clock = null,
            ILogger? logger = null)
        {
            _name = name;
            _executable = executable;
            _supportedTypes = supportedTypes;
            _commandBuilder = commandBuilder;
            _envVarProvider = envVarProvider;
            _readinessProbe = readinessProbe;
            _processRunner = processRunner ?? new ProcessRunner();
            _clock = clock ?? new SystemClock();
            _logger = logger;
        }

        public bool CanHandle(ServerConfiguration configuration)
        {
            foreach (var t in _supportedTypes)
            {
                if (configuration.ServerType == t) return true;
            }
            return false;
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ProcessLauncher));

            if (string.IsNullOrEmpty(configuration.ProjectPath))
                throw new ArgumentException("Project path cannot be null or empty.", nameof(configuration));

            if (configuration.BaseUrl == null)
                throw new ArgumentException("Base URL cannot be null.", nameof(configuration));

            _logger?.LogInformation("Launching {Launcher} for {ServerType}: {ProjectPath}", Name, configuration.ServerType, configuration.ProjectPath);

            // Attempt to clear any existing process on the target port
            await KillProcessesOnPortAsync(configuration.BaseUrl.Port, configuration.ProcessName);

            var arguments = _commandBuilder.BuildCommand(configuration);
            var startInfo = new ProcessStartInfo
            {
                FileName = _executable,
                Arguments = arguments,
                WorkingDirectory = configuration.WorkingDirectory ?? Path.GetDirectoryName(configuration.ProjectPath),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Merge and apply environment variables
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (System.Collections.DictionaryEntry kv in startInfo.EnvironmentVariables)
            {
                var key = kv.Key?.ToString();
                var value = kv.Value?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(key)) env[key] = value;
            }
            _envVarProvider.Apply(env, configuration);
            foreach (var kv in env)
            {
                startInfo.EnvironmentVariables[kv.Key] = kv.Value;
            }

            _logger?.LogInformation("Starting process: {File} {Arguments}", startInfo.FileName, startInfo.Arguments);

            _ = _processRunner.Start(startInfo);

            // Wait for readiness using the shared probe
            await _readinessProbe.WaitUntilReadyAsync(configuration, _logger, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
            _logger?.LogInformation("{Launcher} reports server ready at {Url}", Name, configuration.BaseUrl);
        }

        private async Task KillProcessesOnPortAsync(int port, string processName)
        {
            try
            {
                var result = await PortProcessFinder.FindProcessesOnPortAsync(port);
                if (string.IsNullOrWhiteSpace(result)) return;

                var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    // On Windows netstat output lines, PID is usually the last token
                    if (parts.Length == 0) continue;
                    if (!int.TryParse(parts[^1], out var pid)) continue;

                    try
                    {
                        var proc = Process.GetProcessById(pid);
                        if (!string.IsNullOrEmpty(processName) && !proc.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Skip unrelated processes to be conservative
                            continue;
                        }

                        proc.Kill();
                        _logger?.LogInformation("Killed process {PID} on port {Port}", pid, port);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to kill process {PID}", pid);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to kill processes on port {Port}", port);
            }
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}


