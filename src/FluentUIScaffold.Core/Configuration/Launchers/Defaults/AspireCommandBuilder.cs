namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public sealed class AspireCommandBuilder : ICommandBuilder
    {
        private readonly AspNetCommandBuilder _inner = new AspNetCommandBuilder();
        public string BuildCommand(ServerConfiguration configuration) => _inner.BuildCommand(configuration);
    }
}