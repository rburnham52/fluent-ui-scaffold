using System;

using FluentUIScaffold.Core.Configuration.Launchers;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Convenience helpers to configure server hosting via the options builder.
    /// NOTE: Aspire AppHost hosting should use the official DistributedApplicationTestingBuilder
    /// path (see FluentUIScaffold.AspireHosting.UseAspireHosting). These helpers are intended
    /// for plain dotnet and Node.js servers.
    /// </summary>
    public static class FluentUIScaffoldOptionsBuilderHostingExtensions
    {
        /// <summary>
        /// Configure a plain .NET app (dotnet run) for hosting.
        /// </summary>
        public static FluentUIScaffoldOptionsBuilder UseDotNetServer(this FluentUIScaffoldOptionsBuilder builder,
            Uri baseUrl,
            string projectPath,
            Action<DotNetServerConfigurationBuilder>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(baseUrl);
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentNullException(nameof(projectPath));

            var serverConfig = ServerConfiguration.CreateDotNetServer(baseUrl, projectPath);
            configure?.Invoke(serverConfig);
            builder.WithServerConfiguration(serverConfig.Build());
            return builder;
        }

        /// <summary>
        /// Configure a Node.js app for hosting.
        /// </summary>
        public static FluentUIScaffoldOptionsBuilder UseNodeServer(this FluentUIScaffoldOptionsBuilder builder,
            Uri baseUrl,
            string projectPath,
            Action<NodeJsServerConfigurationBuilder>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(baseUrl);
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentNullException(nameof(projectPath));

            var serverConfig = ServerConfiguration.CreateNodeJsServer(baseUrl, projectPath);
            configure?.Invoke(serverConfig);
            builder.WithServerConfiguration(serverConfig.Build());
            return builder;
        }
    }
}



