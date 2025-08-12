using System.Collections.Generic;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public sealed class AspNetCommandBuilder : ICommandBuilder
    {
        public string BuildCommand(ServerConfiguration configuration)
        {
            var arguments = new List<string> { "run" };

            var frameworkIndex = configuration.Arguments.LastIndexOf("--framework");
            var configurationIndex = configuration.Arguments.LastIndexOf("--configuration");

            if (frameworkIndex >= 0 && frameworkIndex + 1 < configuration.Arguments.Count)
            {
                arguments.AddRange(new[] { "--framework", configuration.Arguments[frameworkIndex + 1] });
            }
            else
            {
                arguments.AddRange(new[] { "--framework", "net8.0" });
            }

            if (configurationIndex >= 0 && configurationIndex + 1 < configuration.Arguments.Count)
            {
                arguments.AddRange(new[] { "--configuration", configuration.Arguments[configurationIndex + 1] });
            }
            else
            {
                arguments.AddRange(new[] { "--configuration", "Release" });
            }

            arguments.Add("--no-launch-profile");

            var customArguments = new List<string>();
            for (int i = 0; i < configuration.Arguments.Count; i++)
            {
                if (configuration.Arguments[i] == "--framework" || configuration.Arguments[i] == "--configuration")
                {
                    i++;
                    continue;
                }
                customArguments.Add(configuration.Arguments[i]);
            }
            arguments.AddRange(customArguments);

            return string.Join(" ", arguments);
        }
    }
}
