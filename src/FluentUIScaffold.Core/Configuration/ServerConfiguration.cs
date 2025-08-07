using System;
using System.Collections.Generic;
using System.IO;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Configuration for web server startup and management.
    /// </summary>
    public class ServerConfiguration
    {
        /// <summary>
        /// Gets or sets the type of server to launch.
        /// </summary>
        public ServerType ServerType { get; set; } = ServerType.AspNetCore;

        /// <summary>
        /// Gets or sets the path to the project file.
        /// </summary>
        public string? ProjectPath { get; set; }

        /// <summary>
        /// Gets or sets the working directory for the server process.
        /// </summary>
        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the server.
        /// </summary>
        public Uri? BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets additional command line arguments.
        /// </summary>
        public List<string> Arguments { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets environment variables to set for the server process.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets whether to enable SPA proxy for ASP.NET Core applications.
        /// </summary>
        public bool EnableSpaProxy { get; set; } = false;

        /// <summary>
        /// Gets or sets the timeout for server startup.
        /// </summary>
        public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the endpoints to check for server readiness.
        /// </summary>
        public List<string> HealthCheckEndpoints { get; set; } = new List<string> { "/" };

        /// <summary>
        /// Creates a new ASP.NET Core server configuration.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>A new ASP.NET Core server configuration.</returns>
        public static ServerConfiguration CreateAspNetCore(Uri baseUrl, string projectPath)
        {
            return new ServerConfiguration
            {
                ServerType = ServerType.AspNetCore,
                BaseUrl = baseUrl,
                ProjectPath = projectPath,
                WorkingDirectory = Path.GetDirectoryName(projectPath),
                EnableSpaProxy = false,
                StartupTimeout = TimeSpan.FromSeconds(60),
                HealthCheckEndpoints = new List<string> { "/" }
            };
        }

        /// <summary>
        /// Creates a new Aspire App Host server configuration.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>A new Aspire App Host server configuration.</returns>
        public static ServerConfiguration CreateAspire(Uri baseUrl, string projectPath)
        {
            return new ServerConfiguration
            {
                ServerType = ServerType.Aspire,
                BaseUrl = baseUrl,
                ProjectPath = projectPath,
                WorkingDirectory = Path.GetDirectoryName(projectPath),
                EnableSpaProxy = false,
                StartupTimeout = TimeSpan.FromSeconds(90),
                HealthCheckEndpoints = new List<string> { "/" }
            };
        }
    }
}
