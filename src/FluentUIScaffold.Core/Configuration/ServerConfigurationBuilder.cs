using System;
using System.Collections.Generic;
using System.IO;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Builder for creating server configurations with fluent API.
    /// </summary>
    public class ServerConfigurationBuilder
    {
        protected readonly ServerConfiguration _configuration;

        public ServerConfigurationBuilder(ServerType serverType, Uri baseUrl, string projectPath)
        {
            _configuration = new ServerConfiguration
            {
                ServerType = serverType,
                BaseUrl = baseUrl,
                ProjectPath = projectPath,
                WorkingDirectory = Path.GetDirectoryName(projectPath)
            };
        }

        /// <summary>
        /// Sets the working directory for the server process.
        /// </summary>
        /// <param name="workingDirectory">The working directory.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ServerConfigurationBuilder WithWorkingDirectory(string workingDirectory)
        {
            _configuration.WorkingDirectory = workingDirectory;
            return this;
        }

        public ServerConfigurationBuilder WithProcessName(string processName)
        {
            _configuration.ProcessName = processName;
            return this;
        }


        /// <summary>
        /// Sets the startup timeout.
        /// </summary>
        /// <param name="timeout">The startup timeout.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ServerConfigurationBuilder WithStartupTimeout(TimeSpan timeout)
        {
            _configuration.StartupTimeout = timeout;
            return this;
        }

        /// <summary>
        /// Sets the health check endpoints.
        /// </summary>
        /// <param name="endpoints">The health check endpoints.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ServerConfigurationBuilder WithHealthCheckEndpoints(params string[] endpoints)
        {
            _configuration.HealthCheckEndpoints = new List<string>(endpoints);
            return this;
        }

        /// <summary>
        /// Adds environment variables.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ServerConfigurationBuilder WithEnvironmentVariable(string key, string value)
        {
            _configuration.EnvironmentVariables[key] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple environment variables.
        /// </summary>
        /// <param name="environmentVariables">Dictionary of environment variables.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ServerConfigurationBuilder WithEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            foreach (var kvp in environmentVariables)
            {
                _configuration.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Adds command line arguments.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public ServerConfigurationBuilder WithArguments(params string[] arguments)
        {
            _configuration.Arguments.AddRange(arguments);
            return this;
        }

        /// <summary>
        /// Builds the server configuration.
        /// </summary>
        /// <returns>The configured server configuration.</returns>
        public ServerConfiguration Build()
        {
            return _configuration;
        }
    }

    /// <summary>
    /// Builder for .NET-specific server configurations.
    /// </summary>
    public class DotNetServerConfigurationBuilder : ServerConfigurationBuilder
    {
        private readonly DotNetServerConfiguration _dotNetConfig;

        public DotNetServerConfigurationBuilder(ServerType serverType, Uri baseUrl, string projectPath)
            : base(serverType, baseUrl, projectPath)
        {
            _dotNetConfig = new DotNetServerConfiguration();
        }

        /// <summary>
        /// Sets the .NET framework version.
        /// </summary>
        /// <param name="framework">The framework version (e.g., "net8.0").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithFramework(string framework)
        {
            _dotNetConfig.Framework = framework;
            return this;
        }

        /// <summary>
        /// Sets the build configuration.
        /// </summary>
        /// <param name="configuration">The build configuration (e.g., "Debug", "Release").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithConfiguration(string configuration)
        {
            _dotNetConfig.Configuration = configuration;
            return this;
        }

        /// <summary>
        /// Enables SPA proxy for ASP.NET Core applications.
        /// </summary>
        /// <param name="enabled">Whether to enable SPA proxy (default: true).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithSpaProxy(bool enabled = true)
        {
            _dotNetConfig.EnableSpaProxy = enabled;
            return this;
        }

        /// <summary>
        /// Sets the ASP.NET Core environment.
        /// </summary>
        /// <param name="environment">The environment (default: "Development").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithAspNetCoreEnvironment(string environment = "Development")
        {
            _configuration.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = environment;
            return this;
        }

        /// <summary>
        /// Sets the .NET environment (for Aspire applications).
        /// </summary>
        /// <param name="environment">The environment (default: "Development").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithDotNetEnvironment(string environment = "Development")
        {
            _configuration.EnvironmentVariables["DOTNET_ENVIRONMENT"] = environment;
            return this;
        }

        /// <summary>
        /// Sets the Aspire dashboard OTLP endpoint URL.
        /// </summary>
        /// <param name="url">The OTLP endpoint URL (default: "https://localhost:21097").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithAspireDashboardOtlpEndpoint(string url = "https://localhost:21097")
        {
            _configuration.EnvironmentVariables["DOTNET_DASHBOARD_OTLP_ENDPOINT_URL"] = url;
            return this;
        }

        /// <summary>
        /// Sets the Aspire resource service endpoint URL.
        /// </summary>
        /// <param name="url">The resource service endpoint URL (default: "https://localhost:22268").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithAspireResourceServiceEndpoint(string url = "https://localhost:22268")
        {
            _configuration.EnvironmentVariables["DOTNET_RESOURCE_SERVICE_ENDPOINT_URL"] = url;
            return this;
        }

        /// <summary>
        /// Sets the ASP.NET Core hosting startup assemblies.
        /// </summary>
        /// <param name="assemblies">The hosting startup assemblies (default: "" for disabled, "Microsoft.AspNetCore.SpaProxy" for enabled).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithAspNetCoreHostingStartupAssemblies(string assemblies = "Microsoft.AspNetCore.SpaProxy")
        {
            _configuration.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = assemblies;
            return this;
        }

        /// <summary>
        /// Sets the ASP.NET Core URLs (for Aspire applications).
        /// </summary>
        /// <param name="urls">The URLs to bind to.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithAspNetCoreUrls(string urls)
        {
            _configuration.EnvironmentVariables["ASPNETCORE_URLS"] = urls;
            return this;
        }

        /// <summary>
        /// Sets the ASP.NET Core forwarded headers enabled flag.
        /// </summary>
        /// <param name="enabled">Whether to enable forwarded headers (default: false).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public DotNetServerConfigurationBuilder WithAspNetCoreForwardedHeaders(bool enabled = false)
        {
            _configuration.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] = enabled.ToString().ToLower();
            return this;
        }

        /// <summary>
        /// Sets the startup timeout.
        /// </summary>
        /// <param name="timeout">The startup timeout.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new DotNetServerConfigurationBuilder WithStartupTimeout(TimeSpan timeout)
        {
            base.WithStartupTimeout(timeout);
            return this;
        }

        /// <summary>
        /// Sets the health check endpoints.
        /// </summary>
        /// <param name="endpoints">The health check endpoints.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new DotNetServerConfigurationBuilder WithHealthCheckEndpoints(params string[] endpoints)
        {
            base.WithHealthCheckEndpoints(endpoints);
            return this;
        }

        /// <summary>
        /// Adds environment variables.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new DotNetServerConfigurationBuilder WithEnvironmentVariable(string key, string value)
        {
            base.WithEnvironmentVariable(key, value);
            return this;
        }

        /// <summary>
        /// Adds multiple environment variables.
        /// </summary>
        /// <param name="environmentVariables">Dictionary of environment variables.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new DotNetServerConfigurationBuilder WithEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            base.WithEnvironmentVariables(environmentVariables);
            return this;
        }

        /// <summary>
        /// Adds command line arguments.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new DotNetServerConfigurationBuilder WithArguments(params string[] arguments)
        {
            base.WithArguments(arguments);
            return this;
        }

        /// <summary>
        /// Builds the server configuration with .NET-specific settings.
        /// </summary>
        /// <returns>The configured server configuration.</returns>
        public new ServerConfiguration Build()
        {
            var config = base.Build();
            // Note: We'll need to modify the launchers to accept DotNetServerConfiguration separately
            // For now, we'll store it in the Arguments or EnvironmentVariables
            config.Arguments.AddRange(new[] { "--framework", _dotNetConfig.Framework });
            config.Arguments.AddRange(new[] { "--configuration", _dotNetConfig.Configuration });

            if (_dotNetConfig.EnableSpaProxy)
            {
                config.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "Microsoft.AspNetCore.SpaProxy";
            }
            else
            {
                config.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] = "";
            }

            return config;
        }
    }

    /// <summary>
    /// Builder for Node.js-specific server configurations.
    /// </summary>
    public class NodeJsServerConfigurationBuilder : ServerConfigurationBuilder
    {
        private string _npmScript = "start";
        private string _nodeEnv = "development";

        public NodeJsServerConfigurationBuilder(Uri baseUrl, string projectPath)
            : base(ServerType.NodeJs, baseUrl, projectPath)
        {
            // Automatically set the port from the URL
            _configuration.EnvironmentVariables["PORT"] = baseUrl.Port.ToString();
        }

        /// <summary>
        /// Sets the npm script to run (default: "start").
        /// </summary>
        /// <param name="script">The npm script name (e.g., "dev", "start", "serve").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NodeJsServerConfigurationBuilder WithNpmScript(string script)
        {
            _npmScript = script;
            return this;
        }

        /// <summary>
        /// Sets the Node.js environment.
        /// </summary>
        /// <param name="environment">The environment (default: "development").</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NodeJsServerConfigurationBuilder WithNodeEnvironment(string environment = "development")
        {
            _nodeEnv = environment;
            _configuration.EnvironmentVariables["NODE_ENV"] = environment;
            return this;
        }

        /// <summary>
        /// Sets the port for the Node.js application.
        /// </summary>
        /// <param name="port">The port number.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NodeJsServerConfigurationBuilder WithPort(int port)
        {
            _configuration.EnvironmentVariables["PORT"] = port.ToString();
            return this;
        }

        /// <summary>
        /// Sets the startup timeout.
        /// </summary>
        /// <param name="timeout">The startup timeout.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new NodeJsServerConfigurationBuilder WithStartupTimeout(TimeSpan timeout)
        {
            base.WithStartupTimeout(timeout);
            return this;
        }

        /// <summary>
        /// Sets the health check endpoints.
        /// </summary>
        /// <param name="endpoints">The health check endpoints.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new NodeJsServerConfigurationBuilder WithHealthCheckEndpoints(params string[] endpoints)
        {
            base.WithHealthCheckEndpoints(endpoints);
            return this;
        }

        /// <summary>
        /// Adds environment variables.
        /// </summary>
        /// <param name="key">The environment variable key.</param>
        /// <param name="value">The environment variable value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new NodeJsServerConfigurationBuilder WithEnvironmentVariable(string key, string value)
        {
            base.WithEnvironmentVariable(key, value);
            return this;
        }

        /// <summary>
        /// Adds multiple environment variables.
        /// </summary>
        /// <param name="environmentVariables">Dictionary of environment variables.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new NodeJsServerConfigurationBuilder WithEnvironmentVariables(Dictionary<string, string> environmentVariables)
        {
            base.WithEnvironmentVariables(environmentVariables);
            return this;
        }

        /// <summary>
        /// Adds command line arguments for npm.
        /// </summary>
        /// <param name="arguments">The command line arguments.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public new NodeJsServerConfigurationBuilder WithArguments(params string[] arguments)
        {
            base.WithArguments(arguments);
            return this;
        }

        /// <summary>
        /// Builds the server configuration with Node.js-specific settings.
        /// </summary>
        /// <returns>The configured server configuration.</returns>
        public new ServerConfiguration Build()
        {
            var config = base.Build();

            // Set default Node.js environment variables if not already set
            if (!config.EnvironmentVariables.ContainsKey("NODE_ENV"))
            {
                config.EnvironmentVariables["NODE_ENV"] = _nodeEnv;
            }

            // Ensure PORT is set from the URL if not explicitly overridden
            if (!config.EnvironmentVariables.ContainsKey("PORT"))
            {
                config.EnvironmentVariables["PORT"] = _configuration.BaseUrl!.Port.ToString();
            }

            // Add the npm script as the first argument
            config.Arguments.Insert(0, _npmScript);

            return config;
        }
    }
}
