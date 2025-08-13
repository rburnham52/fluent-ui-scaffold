using System;
using System.Collections.Generic;
using System.IO;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Generic configuration for web server startup and management.
    /// Contains only common properties needed to launch any server type.
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
        /// Gets or sets the timeout for server startup.
        /// </summary>
        public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Gets or sets the endpoints to check for server readiness.
        /// </summary>
        public List<string> HealthCheckEndpoints { get; set; } = new List<string> { "/" };

        /// <summary>
        /// The name of the process to launch/kill.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// Creates a .NET server configuration builder for ASP.NET Core applications.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>A .NET server configuration builder.</returns>
        public static DotNetServerConfigurationBuilder CreateDotNetServer(Uri baseUrl, string projectPath)
        {
            return new DotNetServerConfigurationBuilder(ServerType.AspNetCore, baseUrl, projectPath)
                .WithAspNetCoreEnvironment("Development")
                .WithAspNetCoreHostingStartupAssemblies("") // Disabled by default
                .WithStartupTimeout(TimeSpan.FromSeconds(60));
        }

        /// <summary>
        /// Creates a .NET server configuration builder for Aspire applications.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="projectPath">The path to the project file.</param>
        /// <returns>A .NET server configuration builder.</returns>
        public static DotNetServerConfigurationBuilder CreateAspireServer(Uri baseUrl, string projectPath)
        {
            return new DotNetServerConfigurationBuilder(ServerType.Aspire, baseUrl, projectPath)
                .WithAspNetCoreEnvironment("Development")
                .WithDotNetEnvironment("Development")
                .WithAspireDashboardOtlpEndpoint("https://localhost:21097")
                .WithAspireResourceServiceEndpoint("https://localhost:22268")
                .WithAspNetCoreHostingStartupAssemblies("") // Disabled by default
                .WithAspNetCoreUrls(baseUrl.ToString())
                .WithAspNetCoreForwardedHeaders(false) // Disabled by default
                .WithStartupTimeout(TimeSpan.FromSeconds(90));
        }

        /// <summary>
        /// Creates a Node.js server configuration builder.
        /// </summary>
        /// <param name="baseUrl">The base URL for the server.</param>
        /// <param name="projectPath">The path to the package.json file.</param>
        /// <returns>A Node.js server configuration builder.</returns>
        public static NodeJsServerConfigurationBuilder CreateNodeJsServer(Uri baseUrl, string projectPath)
        {
            return new NodeJsServerConfigurationBuilder(baseUrl, projectPath)
                .WithNodeEnvironment("development")
                .WithStartupTimeout(TimeSpan.FromSeconds(60));
        }

        public static FluentUIScaffold.Core.Configuration.Launchers.AspireServerConfigurationBuilder2 CreateAspireServerV2(Uri baseUrl, string projectPath)
        {
            var builder = new FluentUIScaffold.Core.Configuration.Launchers.AspireServerConfigurationBuilder2(baseUrl);
            builder.WithBaseUrl(baseUrl);
            builder.WithProjectPath(projectPath);
            builder.WithHealthCheckEndpoints("/", "/health");
            return builder;
        }

        public static FluentUIScaffold.Core.Configuration.Launchers.DotNetServerConfigurationBuilder2 CreateDotNetServerV2(Uri baseUrl, string projectPath)
        {
            return new FluentUIScaffold.Core.Configuration.Launchers.DotNetServerConfigurationBuilder2()
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath);
        }

        public static FluentUIScaffold.Core.Configuration.Launchers.NodeJsServerConfigurationBuilder2 CreateNodeJsServerV2(Uri baseUrl, string projectPath)
        {
            return new FluentUIScaffold.Core.Configuration.Launchers.NodeJsServerConfigurationBuilder2(baseUrl)
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath);
        }
    }

    /// <summary>
    /// .NET-specific configuration for .NET-based servers.
    /// </summary>
    public class DotNetServerConfiguration
    {
        /// <summary>
        /// Gets or sets the .NET framework to target (e.g., "net6.0", "net7.0", "net8.0").
        /// </summary>
        public string Framework { get; set; } = "net8.0";

        /// <summary>
        /// Gets or sets the build configuration (e.g., "Debug", "Release").
        /// </summary>
        public string Configuration { get; set; } = "Release";

        /// <summary>
        /// Gets or sets whether to enable SPA proxy for ASP.NET Core applications.
        /// </summary>
        public bool EnableSpaProxy { get; set; } = false;
    }
}
