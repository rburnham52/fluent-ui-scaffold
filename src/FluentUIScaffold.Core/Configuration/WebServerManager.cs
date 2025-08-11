using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Web server manager that supports multiple server types and automatic project detection
    /// without relying on git repositories. Designed for test scenarios.
    /// </summary>
    public class WebServerManager : IDisposable
    {
        private static WebServerManager? _instance;
        private static readonly object _lockObject = new object();
        private static bool _serverStarted = false;
        private static System.Threading.Mutex? _startupMutex;
        private static bool _isServerOwner = false;
        private static readonly string _frameworkIdentifier = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            ? "container"
            : Environment.GetEnvironmentVariable("DOTNET_FRAMEWORK") ?? "unknown";
        private readonly ServerLauncherFactory _factory;
        private readonly ILogger? _logger;
        private IServerLauncher? _currentLauncher;

        /// <summary>
        /// Initializes a new instance of the WebServerManager.
        /// </summary>
        /// <param name="logger">Optional logger for debugging.</param>
        public WebServerManager(ILogger? logger = null)
        {
            _logger = logger;
            _factory = new ServerLauncherFactory(logger);

            // Register default launchers and detectors
            RegisterDefaultComponents();
        }

        /// <summary>
        /// Gets or creates the singleton instance of WebServerManager.
        /// </summary>
        /// <param name="logger">Optional logger.</param>
        /// <returns>The singleton instance.</returns>
        public static WebServerManager GetInstance(ILogger? logger = null)
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = new WebServerManager(logger);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Starts the web server using flexible configuration and automatic project detection.
        /// </summary>
        /// <param name="serverConfig">The server configuration to use for startup.</param>
        /// <returns>A task that completes when the server is ready.</returns>
        public static async Task StartServerAsync(ServerConfiguration serverConfig)
        {
            if (serverConfig == null) throw new ArgumentNullException(nameof(serverConfig));

            // No shared state: server lifecycle is decoupled from framework options

            // Initialize a cross-process mutex to avoid multiple runners racing to start/stop the same server
            var port = serverConfig.BaseUrl?.Port ?? 5000;
            var mutexName = $"FluentUIScaffold_WebServer_{port}"; // cross-platform safe name
            bool createdNew;
            _startupMutex = new System.Threading.Mutex(initiallyOwned: false, name: mutexName, createdNew: out _);

            lock (_lockObject)
            {
                if (_serverStarted)
                {
                    // Server is already started, just return
                    return;
                }
            }

            // Add a small delay to avoid conflicts during multi-framework testing
            await Task.Delay(100);

            WebServerManager? instance = null;
            try
            {
                instance = GetInstance();

                // Try to acquire ownership to start the server. If another process owns it, wait until the server is up.
                var acquired = _startupMutex!.WaitOne(TimeSpan.FromSeconds(1));
                if (!acquired)
                {
                    // Another process is starting/owns the server. Wait until it's up.
                    instance._logger?.LogInformation("Another test runner is starting the server. Waiting for readiness on port {Port}...", port);
                    var waitStart = DateTime.UtcNow;
                    while (DateTime.UtcNow - waitStart < TimeSpan.FromSeconds(120))
                    {
                        if (await IsServerRunningOnPortAsync(port))
                        {
                            lock (_lockObject)
                            {
                                _serverStarted = true;
                                _isServerOwner = false;
                            }
                            instance._logger?.LogInformation("Detected running server on port {Port}. Skipping startup.", port);
                            return;
                        }
                        await Task.Delay(250);
                    }
                    throw new TimeoutException($"Timed out waiting for existing server to start on port {port}.");
                }

                // We own the mutex; we'll be responsible for starting and stopping the server
                _isServerOwner = true;

                // Check if server is already running on the expected port
                if (await IsServerRunningOnPortAsync(port))
                {
                    instance._logger?.LogInformation("Server is already running on port {Port}, skipping startup", port);
                    lock (_lockObject)
                    {
                        _serverStarted = true;
                        _isServerOwner = false; // We didn't start it
                    }
                    // Release mutex since we didn't start the server
                    try { _startupMutex.ReleaseMutex(); } catch { }
                    return;
                }

                await instance.StartServerInternalAsync(serverConfig);

                lock (_lockObject)
                {
                    _serverStarted = true;
                }

                instance._logger?.LogInformation("Web server started successfully via WebServerManager for framework: {Framework}", _frameworkIdentifier);
            }
            catch (Exception ex)
            {
                // Reset the flag if startup fails
                lock (_lockObject)
                {
                    _serverStarted = false;
                    _isServerOwner = false;
                }

                instance?._logger?.LogError(ex, "Failed to start web server via WebServerManager for framework: {Framework}", _frameworkIdentifier);
                throw new InvalidOperationException($"Failed to start web server via WebServerManager for framework: {_frameworkIdentifier}", ex);
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
                    // Only the owner of the startup mutex should stop the server
                    if (_isServerOwner)
                    {
                        _instance._currentLauncher?.Dispose();
                    }
                    _instance = null;
                    _serverStarted = false;
                    _isServerOwner = false;
                    try { _startupMutex?.ReleaseMutex(); } catch { }
                    _instance?._logger?.LogInformation("Web server stopped successfully for framework: {Framework}", _frameworkIdentifier);
                }
                catch (Exception ex)
                {
                    _instance?._logger?.LogError(ex, "Failed to stop web server via WebServerManager for framework: {Framework}", _frameworkIdentifier);
                }
            }

            // Nothing to clear; no shared state maintained
        }

        /// <summary>
        /// Checks if the web server is currently running.
        /// </summary>
        /// <returns>True if the server is running, false otherwise.</returns>
        public static bool IsServerRunning()
        {
            lock (_lockObject)
            {
                return _serverStarted && _instance != null;
            }
        }

        private async Task StartServerInternalAsync(ServerConfiguration serverConfig)
        {
            // Get the appropriate launcher
            _currentLauncher = _factory.GetLauncher(serverConfig);

            // Launch the server
            await _currentLauncher.LaunchAsync(serverConfig);
        }



        private void RegisterDefaultComponents()
        {
            // Register default server launchers
            _factory.RegisterLauncher(new Launchers.AspNetServerLauncher(_logger));
            _factory.RegisterLauncher(new Launchers.NodeJsServerLauncher(_logger));
            _factory.RegisterLauncher(new Launchers.WebApplicationFactoryServerLauncher(_logger));

            // Register default project detectors
            _factory.RegisterDetector(new Detectors.EnvironmentBasedProjectDetector(_logger));
            _factory.RegisterDetector(new Detectors.GitBasedProjectDetector(_logger));
        }

        // Removed log level-based configuration; rely on host logging configuration

        public void Dispose()
        {
            _currentLauncher?.Dispose();
        }

        /// <summary>
        /// Checks if a server is already running on the specified port.
        /// </summary>
        /// <param name="port">The port to check.</param>
        /// <returns>True if a server is running on the port, false otherwise.</returns>
        private static async Task<bool> IsServerRunningOnPortAsync(int port)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(2);
                var response = await httpClient.GetAsync($"http://localhost:{port}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
