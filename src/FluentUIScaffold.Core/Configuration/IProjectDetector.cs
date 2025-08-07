using System;
using System.Collections.Generic;
using System.IO;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Strategy interface for detecting project files and working directories.
    /// Allows flexible project detection without relying on specific repository structures.
    /// </summary>
    public interface IProjectDetector
    {
        /// <summary>
        /// Detects the project path based on the current context.
        /// </summary>
        /// <param name="context">The detection context.</param>
        /// <returns>The detected project path, or null if not found.</returns>
        string? DetectProjectPath(ProjectDetectionContext context);

        /// <summary>
        /// Gets the name of the detector for identification.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the priority of this detector (higher numbers are tried first).
        /// </summary>
        int Priority { get; }
    }

    /// <summary>
    /// Context for project detection.
    /// </summary>
    public class ProjectDetectionContext
    {
        /// <summary>
        /// The current working directory.
        /// </summary>
        public string CurrentDirectory { get; set; } = Directory.GetCurrentDirectory();

        /// <summary>
        /// The base directory for the test assembly.
        /// </summary>
        public string TestAssemblyDirectory { get; set; } = AppContext.BaseDirectory;

        /// <summary>
        /// The name of the project to look for.
        /// </summary>
        public string? ProjectName { get; set; }

        /// <summary>
        /// The type of project to look for (e.g., "csproj", "package.json", "requirements.txt").
        /// </summary>
        public string? ProjectType { get; set; }

        /// <summary>
        /// Additional search paths to consider.
        /// </summary>
        public List<string> AdditionalSearchPaths { get; set; } = new();

        /// <summary>
        /// Whether to search recursively in parent directories.
        /// </summary>
        public bool SearchRecursively { get; set; } = true;

        /// <summary>
        /// The maximum depth to search when searching recursively.
        /// </summary>
        public int MaxSearchDepth { get; set; } = 10;
    }
}
