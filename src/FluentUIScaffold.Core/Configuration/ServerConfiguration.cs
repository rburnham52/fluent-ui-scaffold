using System;

using FluentUIScaffold.Core.Server;

namespace FluentUIScaffold.Core.Configuration
{
    public static class ServerConfiguration
    {
        public static DotNetServerConfigurationBuilder CreateDotNetServer(Uri baseUrl, string projectPath)
        {
            return new DotNetServerConfigurationBuilder()
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath)
                .WithAspNetCoreEnvironment("Development")
                .EnableSpaProxy(false)
                .WithStartupTimeout(TimeSpan.FromSeconds(60))
                .WithAutoCI(); // Enable automatic CI detection and setup
        }

        public static AspireServerConfigurationBuilder CreateAspireServer(Uri baseUrl, string projectPath)
        {
            var builder = new AspireServerConfigurationBuilder(baseUrl);
            builder.WithBaseUrl(baseUrl);
            builder.WithProjectPath(projectPath);
            builder.WithAspNetCoreEnvironment("Development");
            builder.WithDotNetEnvironment("Development");
            builder.EnableSpaProxy(false);
            builder.WithStartupTimeout(TimeSpan.FromSeconds(60));
            builder.WithAutoCI(); // Enable automatic CI detection and setup
            return builder;
        }

        public static NodeJsServerConfigurationBuilder CreateNodeJsServer(Uri baseUrl, string projectPath)
        {
            return new NodeJsServerConfigurationBuilder(baseUrl)
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath)
                .WithNodeEnvironment("development")
                .WithStartupTimeout(TimeSpan.FromSeconds(60))
                .WithAutoCI(); // Enable automatic CI detection and setup
        }
    }
}
