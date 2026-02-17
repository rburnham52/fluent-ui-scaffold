using System;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Configuration options specific to Node.js application hosting via 'npm run'.
    /// </summary>
    public class NodeHostingOptions
    {
        /// <summary>
        /// Path to the directory containing package.json.
        /// </summary>
        public string ProjectPath { get; set; } = string.Empty;

        /// <summary>
        /// The base URL where the application will be accessible.
        /// </summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// The npm script to run (e.g., "start", "dev").
        /// </summary>
        public string Script { get; set; } = "start";

        /// <summary>
        /// Maximum time to wait for the application to become ready.
        /// </summary>
        public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Health check endpoints to verify after startup.
        /// </summary>
        public string[] HealthCheckEndpoints { get; set; } = new[] { "/" };

        /// <summary>
        /// Working directory for the process.
        /// </summary>
        public string? WorkingDirectory { get; set; }
    }
}
