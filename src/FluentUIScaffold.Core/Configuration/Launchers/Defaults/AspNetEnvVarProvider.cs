using System.Collections.Generic;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public sealed class AspNetEnvVarProvider : IEnvVarProvider
    {
        public void Apply(IDictionary<string, string> targetEnvironment, ServerConfiguration configuration)
        {
            foreach (var kv in configuration.EnvironmentVariables)
            {
                targetEnvironment[kv.Key] = kv.Value;
            }

            if (configuration.BaseUrl != null)
            {
                targetEnvironment["ASPNETCORE_URLS"] = configuration.BaseUrl.ToString();
            }
        }
    }
}