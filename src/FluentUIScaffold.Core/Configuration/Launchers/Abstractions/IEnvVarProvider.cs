using System.Collections.Generic;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public interface IEnvVarProvider
    {
        void Apply(IDictionary<string, string> targetEnvironment, ServerConfiguration configuration);
    }
}