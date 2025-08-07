using System;
using System.Threading.Tasks;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Strategy interface for launching different types of web servers.
    /// Allows framework-agnostic server launching with customizable startup behavior.
    /// </summary>
    public interface IServerLauncher : IDisposable
    {
        /// <summary>
        /// Launches a web server using the specified configuration.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>A task that completes when the server is ready.</returns>
        Task LaunchAsync(ServerConfiguration configuration);

        /// <summary>
        /// Checks if the server launcher can handle the specified configuration.
        /// </summary>
        /// <param name="configuration">The server configuration to check.</param>
        /// <returns>True if this launcher can handle the configuration.</returns>
        bool CanHandle(ServerConfiguration configuration);

        /// <summary>
        /// Gets the name of the server launcher for identification.
        /// </summary>
        string Name { get; }
    }
}
