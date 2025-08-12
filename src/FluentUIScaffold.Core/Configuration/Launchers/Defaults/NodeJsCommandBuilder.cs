namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public sealed class NodeJsCommandBuilder : ICommandBuilder
    {
        public string BuildCommand(ServerConfiguration configuration)
        {
            var arguments = new System.Collections.Generic.List<string> { "start" };
            arguments.AddRange(configuration.Arguments);
            return string.Join(" ", arguments);
        }
    }
}
