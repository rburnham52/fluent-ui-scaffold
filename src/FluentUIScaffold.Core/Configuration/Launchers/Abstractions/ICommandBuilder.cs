using System;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public interface ICommandBuilder
    {
        string BuildCommand(ServerConfiguration configuration);
    }
}