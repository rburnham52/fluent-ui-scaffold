using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    /// <summary>
    /// Launcher that is intended to host an ASP.NET Core app in-process via WebApplicationFactory.
    /// For generic, cross-repo usage where the entry assembly is not directly referenced,
    /// this implementation gracefully falls back to the standard ASP.NET Core process launcher.
    /// </summary>
    public sealed class WebApplicationFactoryServerLauncher : IServerLauncher
    {
        private readonly ILogger? _logger;
        private readonly IServerLauncher _fallbackLauncher;
        private bool _disposed;

        public string Name => "WebApplicationFactoryServerLauncher";

        public WebApplicationFactoryServerLauncher(ILogger? logger = null, IServerLauncher? fallbackLauncher = null)
        {
            _logger = logger;
            _fallbackLauncher = fallbackLauncher ?? new ProcessLauncher(
                name: "AspNetProcessLauncher",
                executable: "dotnet",
                supportedTypes: new[] { ServerType.AspNetCore, ServerType.Aspire },
                commandBuilder: new AspNetCommandBuilder(),
                envVarProvider: new AspNetEnvVarProvider(),
                readinessProbe: new HttpReadinessProbe(null, new SystemClock()),
                logger: logger
            );
        }

        public bool CanHandle(ServerConfiguration configuration)
        {
            return configuration.ServerType == ServerType.WebApplicationFactory;
        }

        public async Task LaunchAsync(ServerConfiguration configuration)
        {
            _logger?.LogInformation("{Launcher} selected. Falling back to {Fallback} for cross-platform startup.",
                Name, _fallbackLauncher.Name);

            await _fallbackLauncher.LaunchAsync(configuration);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _fallbackLauncher.Dispose();
            _disposed = true;
        }
    }
}
