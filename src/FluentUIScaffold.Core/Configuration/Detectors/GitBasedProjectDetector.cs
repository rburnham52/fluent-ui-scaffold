using System;
using System.Collections.Generic; // Added for List
using System.IO;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Detectors
{
    /// <summary>
    /// Project detector that uses git repository structure for project detection.
    /// Falls back to alternative detection methods when git is not available.
    /// </summary>
    public class GitBasedProjectDetector : IProjectDetector
    {
        private readonly ILogger? _logger;

        public string Name => "GitBasedProjectDetector";
        public int Priority => 100; // High priority for git-based detection

        public GitBasedProjectDetector(ILogger? logger = null)
        {
            _logger = logger;
        }

        public string? DetectProjectPath(ProjectDetectionContext context)
        {
            try
            {
                // Try to find git repository root
                var gitRoot = GitBasedProjectDetector.FindGitRepositoryRoot(context.CurrentDirectory);
                if (gitRoot != null)
                {
                    return DetectProjectInGitRepository(gitRoot, context);
                }

                // Fallback to non-git detection
                return DetectProjectInDirectory(context.CurrentDirectory, context);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error detecting project path");
                return null;
            }
        }

        private static string? FindGitRepositoryRoot(string startDirectory)
        {
            var currentDir = startDirectory;
            var maxDepth = 10; // Prevent infinite loops
            var depth = 0;

            while (currentDir != null && depth < maxDepth)
            {
                var gitDir = Path.Combine(currentDir, ".git");
                if (Directory.Exists(gitDir))
                {
                    return currentDir;
                }

                currentDir = Directory.GetParent(currentDir)?.FullName;
                depth++;
            }

            return null;
        }

        private string? DetectProjectInGitRepository(string gitRoot, ProjectDetectionContext context)
        {
            // Use additional search paths from context, or fall back to common project structures
            var searchPaths = new List<string>();

            // Add additional search paths from context
            if (context.AdditionalSearchPaths.Count > 0)
            {
                foreach (var path in context.AdditionalSearchPaths)
                {
                    var fullPath = Path.Combine(gitRoot, path);
                    searchPaths.Add(fullPath);
                }
            }
            else
            {
                // Fall back to common project structures in git repositories
                var fallbackPaths = new[]
                {
                    Path.Combine(gitRoot, "src"),
                    Path.Combine(gitRoot, "samples"),
                    Path.Combine(gitRoot, "examples"),
                    Path.Combine(gitRoot, "apps"),
                    Path.Combine(gitRoot, "web"),
                    Path.Combine(gitRoot, "client"),
                    Path.Combine(gitRoot, "server")
                };
                searchPaths.AddRange(fallbackPaths);
            }

            // Always check the root directory as well
            searchPaths.Add(gitRoot);

            foreach (var searchPath in searchPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    var projectPath = FindProjectInDirectory(searchPath, context);
                    if (projectPath != null)
                    {
                        return projectPath;
                    }
                }
            }

            return null;
        }

        private string? DetectProjectInDirectory(string directory, ProjectDetectionContext context)
        {
            // Search in the current directory and immediate subdirectories
            var projectPath = FindProjectInDirectory(directory, context);
            if (projectPath != null)
            {
                return projectPath;
            }

            // Search in subdirectories
            try
            {
                var subdirectories = Directory.GetDirectories(directory);
                foreach (var subdir in subdirectories)
                {
                    projectPath = FindProjectInDirectory(subdir, context);
                    if (projectPath != null)
                    {
                        return projectPath;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Failed to search subdirectories in {Directory}", directory);
            }

            return null;
        }

        private string? FindProjectInDirectory(string directory, ProjectDetectionContext context)
        {
            try
            {
                // If a specific project name is provided, look for it
                if (!string.IsNullOrEmpty(context.ProjectName))
                {
                    var namedProjectPath = Path.Combine(directory, context.ProjectName);
                    if (File.Exists(namedProjectPath))
                    {
                        return namedProjectPath;
                    }

                    // Also check for project name with extension
                    var projectExtensions = GitBasedProjectDetector.GetProjectExtensions(context.ProjectType);
                    foreach (var extension in projectExtensions)
                    {
                        // Treat entries with dots as exact filenames (e.g., package.json)
                        if (extension.Contains('.'))
                        {
                            var exact = Path.Combine(directory, extension);
                            if (File.Exists(exact))
                            {
                                return exact;
                            }
                        }
                        else
                        {
                            var projectPath = Path.Combine(directory, $"{context.ProjectName}.{extension}");
                            if (File.Exists(projectPath))
                            {
                                return projectPath;
                            }
                        }
                    }
                }

                // Search for project files by type (recursively)
                var extensions = GitBasedProjectDetector.GetProjectExtensions(context.ProjectType);
                foreach (var extension in extensions)
                {
                    if (extension.Contains('.'))
                    {
                        // Exact filename match (e.g., package.json, requirements.txt)
                        var matches = Directory.GetFiles(directory, extension, SearchOption.AllDirectories);
                        if (matches.Length > 0)
                        {
                            return matches[0];
                        }
                    }
                    else
                    {
                        var projectFiles = Directory.GetFiles(directory, $"*.{extension}", SearchOption.AllDirectories);
                        if (projectFiles.Length > 0)
                        {
                            var projectPath = projectFiles[0]; // Take the first one found
                            return projectPath;
                        }
                    }
                }

                // If no specific type is specified, search for common project files (recursively)
                if (string.IsNullOrEmpty(context.ProjectType))
                {
                    var commonEntries = new[] { "csproj", "fsproj", "vbproj", "package.json", "requirements.txt" };
                    foreach (var entry in commonEntries)
                    {
                        string[] projectFiles;
                        if (entry.Contains('.'))
                        {
                            projectFiles = Directory.GetFiles(directory, entry, SearchOption.AllDirectories);
                        }
                        else
                        {
                            projectFiles = Directory.GetFiles(directory, $"*.{entry}", SearchOption.AllDirectories);
                        }
                        if (projectFiles.Length > 0)
                        {
                            var projectPath = projectFiles[0];
                            return projectPath;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error searching in directory {Directory}", directory);
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
