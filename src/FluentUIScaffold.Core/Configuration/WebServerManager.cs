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
        /// <param name="options">The configuration options.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>The singleton instance.</returns>
        public static WebServerManager GetInstance(FluentUIScaffoldOptions options, ILogger? logger = null)
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    // Create a logger with the specified log level from options
                    var configuredLogger = CreateConfiguredLogger(logger, options.WebServerLogLevel);
                    _instance = new WebServerManager(configuredLogger);
                }
                return _instance;
            }
        }

        /// <summary>
        /// Starts the web server using flexible configuration and automatic project detection.
        /// </summary>
        /// <param name="options">The configuration options.</param>
        /// <returns>A task that completes when the server is ready.</returns>
        public static async Task StartServerAsync(FluentUIScaffoldOptions options)
        {
            // Set shared options for consistency with FluentUIScaffoldApp
            SharedOptionsManager.SetSharedOptions(options);

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
                instance = GetInstance(options);

                // Check if server is already running on the expected port
                if (await IsServerRunningOnPortAsync(options.BaseUrl?.Port ?? 5000))
                {
                    instance._logger?.LogInformation("Server is already running on port {Port}, skipping startup", options.BaseUrl?.Port ?? 5000);
                    lock (_lockObject)
                    {
                        _serverStarted = true;
                    }
                    return;
                }

                await instance.StartServerInternalAsync(options);

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
                    _instance._currentLauncher?.Dispose();
                    _instance = null;
                    _serverStarted = false;
                    _instance?._logger?.LogInformation("Web server stopped successfully for framework: {Framework}", _frameworkIdentifier);
                }
                catch (Exception ex)
                {
                    _instance?._logger?.LogError(ex, "Failed to stop web server via WebServerManager for framework: {Framework}", _frameworkIdentifier);
                }
            }

            // Clear shared options when server stops
            SharedOptionsManager.ClearSharedOptions();
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

        private async Task StartServerInternalAsync(FluentUIScaffoldOptions options)
        {
            // Determine server configuration
            var serverConfig = await DetermineServerConfigurationAsync(options);

            // Get the appropriate launcher
            _currentLauncher = _factory.GetLauncher(serverConfig);

            // Launch the server
            await _currentLauncher.LaunchAsync(serverConfig);
        }

        private async Task<ServerConfiguration> DetermineServerConfigurationAsync(FluentUIScaffoldOptions options)
        {
            // If explicit server configuration is provided, use it
            if (options.ServerConfiguration != null)
            {
                _logger?.LogInformation("Using explicit server configuration");
                return options.ServerConfiguration;
            }

            // If explicit project path is provided, create configuration from it
            if (!string.IsNullOrEmpty(options.WebServerProjectPath))
            {
                _logger?.LogInformation("Using explicit project path: {ProjectPath}", options.WebServerProjectPath);
                return ServerConfiguration.CreateDotNetServer(options.BaseUrl!, options.WebServerProjectPath).Build();
            }

            // Use simplified project detection (just check if project path is provided)
            if (options.EnableProjectDetection)
            {
                _logger?.LogInformation("Using simplified project detection");
                // For now, we'll just throw an exception since we need a project path
                // In the future, this could be extended to search for common project patterns
                throw new InvalidOperationException(
                    "Project detection is enabled but no project path is provided. " +
                    "Please provide either ServerConfiguration, WebServerProjectPath, or disable EnableProjectDetection.");
            }

            throw new InvalidOperationException(
                "No server configuration provided. " +
                "Please provide either ServerConfiguration or WebServerProjectPath.");
        }



        private void RegisterDefaultComponents()
        {
            // Register default server launchers
            _factory.RegisterLauncher(new Launchers.AspNetServerLauncher(_logger));
            _factory.RegisterLauncher(new Launchers.NodeJsServerLauncher(_logger));

            // Register default project detectors
            _factory.RegisterDetector(new Detectors.EnvironmentBasedProjectDetector(_logger));
            _factory.RegisterDetector(new Detectors.GitBasedProjectDetector(_logger));
        }

        /// <summary>
        /// Creates a configured logger with the specified log level.
        /// </summary>
        /// <param name="logger">The base logger.</param>
        /// <param name="logLevel">The minimum log level to include.</param>
        /// <returns>A configured logger.</returns>
        private static ILogger? CreateConfiguredLogger(ILogger? logger, LogLevel logLevel)
        {
            // For now, just return the existing logger
            // The log level filtering should be handled by the logging infrastructure
            return logger;
        }

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
