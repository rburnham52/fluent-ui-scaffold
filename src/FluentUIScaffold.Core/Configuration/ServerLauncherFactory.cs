using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Factory for creating and managing server launchers and project detectors.
    /// </summary>
    public class ServerLauncherFactory
    {
        private readonly Dictionary<string, IServerLauncher> _launchers;
        private readonly Dictionary<string, IProjectDetector> _detectors;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the ServerLauncherFactory.
        /// </summary>
        /// <param name="logger">Optional logger for debugging.</param>
        public ServerLauncherFactory(ILogger? logger = null)
        {
            _logger = logger;
            _launchers = new Dictionary<string, IServerLauncher>();
            _detectors = new Dictionary<string, IProjectDetector>();
        }

        /// <summary>
        /// Registers a server launcher.
        /// </summary>
        /// <param name="launcher">The launcher to register.</param>
        public void RegisterLauncher(IServerLauncher launcher)
        {
            _launchers[launcher.Name] = launcher;
            _logger?.LogInformation("Registered server launcher: {LauncherName}", launcher.Name);
        }

        /// <summary>
        /// Registers a project detector.
        /// </summary>
        /// <param name="detector">The detector to register.</param>
        public void RegisterDetector(IProjectDetector detector)
        {
            _detectors[detector.Name] = detector;
            _logger?.LogInformation("Registered project detector: {DetectorName}", detector.Name);
        }

        /// <summary>
        /// Gets the appropriate launcher for the given configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>The appropriate launcher.</returns>
        public IServerLauncher GetLauncher(ServerConfiguration configuration)
        {
            var launcher = _launchers.Values.FirstOrDefault(l => l.CanHandle(configuration));
            if (launcher == null)
            {
                throw new InvalidOperationException($"No launcher found for server type: {configuration.ServerType}");
            }

            return launcher;
        }

        /// <summary>
        /// Detects the project path using registered detectors.
        /// </summary>
        /// <param name="context">The project detection context.</param>
        /// <returns>The detected project path, or null if not found.</returns>
        public string? DetectProjectPath(ProjectDetectionContext context)
        {
            var detectors = _detectors.Values.OrderByDescending(d => d.Priority).ToList();

            foreach (var detector in detectors)
            {
                try
                {
                    var projectPath = detector.DetectProjectPath(context);
                    if (!string.IsNullOrEmpty(projectPath))
                    {
                        _logger?.LogInformation("Project detected by {DetectorName}: {ProjectPath}", detector.Name, projectPath);
                        return projectPath;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Project detector {DetectorName} failed", detector.Name);
                }
            }

            _logger?.LogWarning("No project detected by any detector");
            return null;
        }

        /// <summary>
        /// Creates a server configuration with automatic project detection.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="serverType">The type of server to launch.</param>
        /// <param name="additionalSearchPaths">Additional search paths for project detection.</param>
        /// <returns>A server configuration with detected project path.</returns>
        public ServerConfiguration CreateConfigurationWithDetection(Uri baseUrl, ServerType serverType, IEnumerable<string>? additionalSearchPaths = null)
        {
            var context = new ProjectDetectionContext
            {
                CurrentDirectory = Environment.CurrentDirectory,
                ProjectType = GetProjectTypeForServerType(serverType),
                AdditionalSearchPaths = additionalSearchPaths?.ToList() ?? new List<string>()
            };

            var projectPath = DetectProjectPath(context);
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new InvalidOperationException($"Could not detect project path for server type '{serverType}'. Please specify the project path explicitly.");
            }

            return CreateConfiguration(baseUrl, serverType, projectPath);
        }

        /// <summary>
        /// Creates a server configuration with explicit project path.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="serverType">The type of server to launch.</param>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>A server configuration.</returns>
        public ServerConfiguration CreateConfiguration(Uri baseUrl, ServerType serverType, string projectPath)
        {
            return new ServerConfiguration
            {
                BaseUrl = baseUrl,
                ServerType = serverType,
                ProjectPath = projectPath,
                WorkingDirectory = Path.GetDirectoryName(projectPath)
            };
        }

        /// <summary>
        /// Gets the project type for a given server type.
        /// </summary>
        /// <param name="serverType">The server type.</param>
        /// <returns>The project type.</returns>
        private static string GetProjectTypeForServerType(ServerType serverType)
        {
            return serverType switch
            {
                ServerType.AspNetCore => "csproj",
                ServerType.Aspire => "csproj",
                _ => throw new ArgumentException($"Unknown server type: {serverType}")
            };
        }
    }
}
