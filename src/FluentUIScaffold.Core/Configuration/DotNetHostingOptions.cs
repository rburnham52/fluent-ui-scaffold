using System;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Configuration options specific to .NET application hosting via 'dotnet run'.
    /// </summary>
    public class DotNetHostingOptions
    {
        /// <summary>
        /// Path to the .csproj file.
        /// </summary>
        public string ProjectPath { get; set; } = string.Empty;

        /// <summary>
        /// The base URL where the application will be accessible.
        /// </summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// The target framework moniker (e.g., "net8.0").
        /// </summary>
        public string Framework { get; set; } = "net8.0";

        /// <summary>
        /// The build configuration (e.g., "Release", "Debug").
        /// </summary>
        public string Configuration { get; set; } = "Release";

        /// <summary>
        /// Maximum time to wait for the application to become ready.
        /// </summary>
        public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Health check endpoints to verify after startup.
        /// </summary>
        public string[] HealthCheckEndpoints { get; set; } = new[] { "/" };

        /// <summary>
        /// Working directory for the process. Defaults to the project directory.
        /// </summary>
        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// Optional process name for identification.
        /// </summary>
        public string? ProcessName { get; set; }
    }
}
