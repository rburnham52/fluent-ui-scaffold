using System;

namespace FluentUIScaffold.Core.Configuration
{
    public static class ServerConfiguration
    {
        public static Launchers.DotNetServerConfigurationBuilder CreateDotNetServer(Uri baseUrl, string projectPath)
        {
            return new Launchers.DotNetServerConfigurationBuilder()
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath)
                .WithAspNetCoreEnvironment("Development")
                .EnableSpaProxy(false)
                .WithStartupTimeout(TimeSpan.FromSeconds(60));
        }

        public static Launchers.AspireServerConfigurationBuilder CreateAspireServer(Uri baseUrl, string projectPath)
        {
            var builder = new Launchers.AspireServerConfigurationBuilder(baseUrl);
            builder.WithBaseUrl(baseUrl);
            builder.WithProjectPath(projectPath);
            builder.WithAspNetCoreEnvironment("Development");
            builder.WithDotNetEnvironment("Development");
            builder.EnableSpaProxy(false);
            builder.WithStartupTimeout(TimeSpan.FromSeconds(90));
            return builder;
        }

        public static Launchers.NodeJsServerConfigurationBuilder CreateNodeJsServer(Uri baseUrl, string projectPath)
        {
            return new Launchers.NodeJsServerConfigurationBuilder(baseUrl)
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath)
                .WithNodeEnvironment("development")
                .WithStartupTimeout(TimeSpan.FromSeconds(60));
        }
    }
}
