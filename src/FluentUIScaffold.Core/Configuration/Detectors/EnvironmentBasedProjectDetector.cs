using System;
using System.IO;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Detectors
{
    /// <summary>
    /// Project detector that uses environment variables and configuration files for project detection.
    /// Useful for CI/CD environments where project paths are explicitly configured.
    /// </summary>
    public class EnvironmentBasedProjectDetector : IProjectDetector
    {
        private readonly ILogger? _logger;

        public string Name => "EnvironmentBasedProjectDetector";
        public int Priority => 200; // Higher priority than git-based detection

        public EnvironmentBasedProjectDetector(ILogger? logger = null)
        {
            _logger = logger;
        }

        public string? DetectProjectPath(ProjectDetectionContext context)
        {
            try
            {
                // Check environment variables first
                var projectPath = DetectFromEnvironmentVariables();
                if (!string.IsNullOrEmpty(projectPath) && File.Exists(projectPath))
                {
                    _logger?.LogInformation("Found project path from environment variable: {ProjectPath}", projectPath);
                    return projectPath;
                }

                // Check configuration files
                projectPath = DetectFromConfigurationFiles(context);
                if (!string.IsNullOrEmpty(projectPath) && File.Exists(projectPath))
                {
                    _logger?.LogInformation("Found project path from configuration file: {ProjectPath}", projectPath);
                    return projectPath;
                }

                // Check additional search paths
                projectPath = DetectFromAdditionalPaths(context);
                if (!string.IsNullOrEmpty(projectPath) && File.Exists(projectPath))
                {
                    _logger?.LogInformation("Found project path from additional search paths: {ProjectPath}", projectPath);
                    return projectPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Environment-based project detection failed");
                return null;
            }
        }

        private string? DetectFromEnvironmentVariables()
        {
            // Check common environment variables for project paths
            var envVars = new[]
            {
                "FLUENTUI_PROJECT_PATH",
                "TEST_PROJECT_PATH",
                "WEB_PROJECT_PATH",
                "APP_PROJECT_PATH",
                "PROJECT_PATH"
            };

            foreach (var envVar in envVars)
            {
                var projectPath = Environment.GetEnvironmentVariable(envVar);
                if (!string.IsNullOrEmpty(projectPath))
                {
                    _logger?.LogDebug("Found project path in environment variable {EnvVar}: {ProjectPath}", envVar, projectPath);
                    return projectPath;
                }
            }

            return null;
        }

        private string? DetectFromConfigurationFiles(ProjectDetectionContext context)
        {
            // Look for configuration files in the test assembly directory and current directory
            var searchDirectories = new[]
            {
                context.TestAssemblyDirectory,
                context.CurrentDirectory,
                Path.GetDirectoryName(context.TestAssemblyDirectory) ?? context.CurrentDirectory
            };

            foreach (var directory in searchDirectories)
            {
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                    continue;

                // Look for common configuration files
                var configFiles = new[]
                {
                    "fluentui.config.json",
                    "test.config.json",
                    "project.config.json",
                    ".fluentui",
                    "fluentui.config"
                };

                foreach (var configFile in configFiles)
                {
                    var configPath = Path.Combine(directory, configFile);
                    if (File.Exists(configPath))
                    {
                        var projectPath = ReadProjectPathFromConfig(configPath, context);
                        if (!string.IsNullOrEmpty(projectPath))
                        {
                            return projectPath;
                        }
                    }
                }
            }

            return null;
        }

        private string? DetectFromAdditionalPaths(ProjectDetectionContext context)
        {
            foreach (var searchPath in context.AdditionalSearchPaths)
            {
                if (string.IsNullOrEmpty(searchPath) || !Directory.Exists(searchPath))
                    continue;

                // Look for project files in the additional search path
                var projectExtensions = EnvironmentBasedProjectDetector.GetProjectExtensions(context.ProjectType);
                foreach (var extension in projectExtensions)
                {
                    var projectFiles = Directory.GetFiles(searchPath, $"*.{extension}", SearchOption.TopDirectoryOnly);
                    if (projectFiles.Length > 0)
                    {
                        var projectPath = projectFiles[0];
                        _logger?.LogDebug("Found project in additional search path: {ProjectPath}", projectPath);
                        return projectPath;
                    }
                }
            }

            return null;
        }

        private string? ReadProjectPathFromConfig(string configPath, ProjectDetectionContext context)
        {
            try
            {
                // Simple JSON-like config file parsing
                var configContent = File.ReadAllText(configPath);

                // Look for project path patterns
                var patterns = new[]
                {
                    "\"projectPath\":",
                    "\"project_path\":",
                    "\"webProjectPath\":",
                    "\"testProjectPath\":"
                };

                foreach (var pattern in patterns)
                {
                    var index = configContent.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        var startIndex = configContent.IndexOf('"', index + pattern.Length);
                        if (startIndex >= 0)
                        {
                            var endIndex = configContent.IndexOf('"', startIndex + 1);
                            if (endIndex >= 0)
                            {
                                var projectPath = configContent.Substring(startIndex + 1, endIndex - startIndex - 1);
                                if (!string.IsNullOrEmpty(projectPath))
                                {
                                    // Resolve relative paths
                                    var configDir = Path.GetDirectoryName(configPath);
                                    if (!string.IsNullOrEmpty(configDir) && !Path.IsPathRooted(projectPath))
                                    {
                                        projectPath = Path.Combine(configDir, projectPath);
                                    }

                                    return projectPath;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to read project path from config file: {ConfigPath}", configPath);
            }

            return null;
        }

        private static string[] GetProjectExtensions(string? projectType)
        {
            return projectType?.ToLowerInvariant() switch
            {
                "csproj" => new[] { "csproj" },
                "fsproj" => new[] { "fsproj" },
                "vbproj" => new[] { "vbproj" },
                "package.json" => new[] { "package.json" },
                "requirements.txt" => new[] { "requirements.txt", "pyproject.toml", "setup.py" },
                _ => new[] { "csproj", "fsproj", "vbproj", "package.json", "requirements.txt" }
            };
        }
    }
}
