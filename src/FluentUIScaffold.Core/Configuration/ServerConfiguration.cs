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
            return new Launchers.AspireServerConfigurationBuilder(baseUrl)
                .WithBaseUrl(baseUrl)
                .WithProjectPath(projectPath)
                .WithAspNetCoreEnvironment("Development")
                .WithDotNetEnvironment("Development")
                .EnableSpaProxy(false)
                .WithStartupTimeout(TimeSpan.FromSeconds(90));
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
