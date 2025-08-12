using System;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public sealed class AspNetCoreCommandBuilder : ICommandBuilder
    {
        public string BuildCommand(ServerConfiguration configuration)
        {
            var baseUrl = configuration.BaseUrl ?? new Uri("http://localhost:5000");
            var framework = "net8.0";
            var cfg = "Release";

            var frameworkIndex = configuration.Arguments.LastIndexOf("--framework");
            if (frameworkIndex >= 0 && frameworkIndex + 1 < configuration.Arguments.Count)
            {
                framework = configuration.Arguments[frameworkIndex + 1];
            }

            var configurationIndex = configuration.Arguments.LastIndexOf("--configuration");
            if (configurationIndex >= 0 && configurationIndex + 1 < configuration.Arguments.Count)
            {
                cfg = configuration.Arguments[configurationIndex + 1];
            }

            return $"run --configuration {cfg} --framework {framework} --urls \"{baseUrl}\" --no-launch-profile";
        }
    }
}