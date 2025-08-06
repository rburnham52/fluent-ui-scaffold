using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Framework-agnostic test assembly hook for web server management.
    /// Provides a unified approach to starting and stopping web servers for test suites.
    /// </summary>
    public class TestAssemblyWebHook : IDisposable
    {
        private static TestAssemblyWebHook? _instance;
        private static readonly object _lockObject = new object();
        private static bool _serverStarted = false;
        private readonly WebServerLauncher _webServerLauncher;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the TestAssemblyWebHook.
        /// </summary>
        /// <param name="logger">Optional logger for debugging.</param>
        public TestAssemblyWebHook(ILogger? logger = null)
        {
            _logger = logger;
            _webServerLauncher = new WebServerLauncher(_logger);
        }

        /// <summary>
        /// Gets or creates the singleton instance of TestAssemblyWebHook.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>The singleton instance.</returns>
        public static TestAssemblyWebHook GetInstance(FluentUIScaffoldOptions options, ILogger? logger = null)
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = new TestAssemblyWebHook(logger);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Starts the web server if not already started.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns>A task that completes when the server is ready.</returns>
        public static async Task StartServerAsync(FluentUIScaffoldOptions options)
        {
            lock (_lockObject)
            {
                if (_serverStarted)
                    return;
            }

            try
            {
                var instance = GetInstance(options);
                await instance._webServerLauncher.LaunchWebServerAsync(
                    options.WebServerProjectPath!,
                    options.BaseUrl!,
                    options.DefaultWaitTimeout);

                lock (_lockObject)
                {
                    _serverStarted = true;
                }

                instance._logger?.LogInformation("Web server started successfully via TestAssemblyWebHook");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to start web server via TestAssemblyWebHook", ex);
            }
        }

        /// <summary>
        /// Stops the web server and cleans up resources.
        /// </summary>
        public static void StopServer()
        {
            lock (_lockObject)
            {
                if (!_serverStarted || _instance == null)
                    return;

                try
                {
                    _instance._webServerLauncher.Dispose();
                    _instance = null;
                    _serverStarted = false;
                }
                catch (Exception ex)
                {
                    _instance?._logger?.LogError(ex, "Failed to stop web server via TestAssemblyWebHook");
                }
            }
        }

        /// <summary>
        /// Disposes the web server launcher.
        /// </summary>
        public void Dispose()
        {
            _webServerLauncher?.Dispose();
        }
    }
}
