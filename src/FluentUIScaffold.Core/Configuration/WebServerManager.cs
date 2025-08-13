using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FluentUIScaffold.Core.Configuration.Launchers;

namespace FluentUIScaffold.Core.Configuration
{
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

        private readonly ILogger? _logger;
        private ProcessLauncher? _currentProcessLauncher;

        public WebServerManager(ILogger? logger = null)
        {
            _logger = logger;
        }

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

        public static async Task StartServerAsync(LaunchPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            var port = plan.BaseUrl.Port;
            var mutexName = $"FluentUIScaffold_WebServer_{port}";
            _startupMutex = new System.Threading.Mutex(initiallyOwned: false, name: mutexName, createdNew: out _);

            lock (_lockObject)
            {
                if (_serverStarted) return;
            }

            await Task.Delay(100);

            WebServerManager? instance = null;
            try
            {
                instance = GetInstance();
                var acquired = _startupMutex!.WaitOne(TimeSpan.FromSeconds(1));
                if (!acquired)
                {
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

                _isServerOwner = true;

                if (await IsServerRunningOnPortAsync(port))
                {
                    instance._logger?.LogInformation("Server is already running on port {Port}, skipping startup", port);
                    lock (_lockObject)
                    {
                        _serverStarted = true;
                        _isServerOwner = false;
                    }
                    try { _startupMutex.ReleaseMutex(); } catch { }
                    return;
                }

                instance._currentProcessLauncher = new ProcessLauncher(instance._logger);

                // Defensive: if the plan's working directory is invalid (common in unit tests),
                // prefer to wait briefly for an already-running server instead of attempting to start.
                try
                {
                    var workingDir = plan.StartInfo.WorkingDirectory;
                    if (!string.IsNullOrWhiteSpace(workingDir) && !System.IO.Directory.Exists(workingDir))
                    {
                        instance._logger?.LogInformation("Working directory '{WorkingDirectory}' does not exist. Waiting briefly for an already running server on port {Port}...", workingDir, port);
                        var waitStart = DateTime.UtcNow;
                        while (DateTime.UtcNow - waitStart < TimeSpan.FromSeconds(3))
                        {
                            if (await IsServerRunningOnPortAsync(port))
                            {
                                lock (_lockObject)
                                {
                                    _serverStarted = true;
                                    _isServerOwner = false;
                                }
                                try { _startupMutex.ReleaseMutex(); } catch { }
                                instance._logger?.LogInformation("Detected running server on port {Port}. Skipping process launch due to invalid working directory.", port);
                                return;
                            }
                            await Task.Delay(100);
                        }
                        throw new InvalidOperationException($"The working directory '{workingDir}' does not exist, and no server was detected on port {port}.");
                    }
                }
                catch (Exception ex)
                {
                    instance._logger?.LogDebug(ex, "Pre-launch validation warning");
                }
                await instance._currentProcessLauncher.StartAsync(plan);

                lock (_lockObject)
                {
                    _serverStarted = true;
                }

                instance._logger?.LogInformation("Web server started successfully via WebServerManager for framework: {Framework}", _frameworkIdentifier);
            }
            catch (Exception ex)
            {
                lock (_lockObject)
                {
                    _serverStarted = false;
                    _isServerOwner = false;
                }
                instance?._logger?.LogError(ex, "Failed to start web server via WebServerManager for framework: {Framework}", _frameworkIdentifier);
                throw new InvalidOperationException($"Failed to start web server via WebServerManager for framework: {_frameworkIdentifier}", ex);
            }
        }

        public static void StopServer()
        {
            lock (_lockObject)
            {
                if (!_serverStarted || _instance == null) return;
                try
                {
                    if (_isServerOwner)
                    {
                        _instance._currentProcessLauncher?.Dispose();
                    }
                    _instance = null;
                    _serverStarted = false;
                    _isServerOwner = false;
                    try { _startupMutex?.ReleaseMutex(); } catch { }
                }
                catch (Exception ex)
                {
                    _instance?._logger?.LogError(ex, "Failed to stop web server via WebServerManager for framework: {Framework}", _frameworkIdentifier);
                }
            }
        }

        public static bool IsServerRunning()
        {
            lock (_lockObject)
            {
                return _serverStarted && _instance != null;
            }
        }

        private static async Task<bool> IsServerRunningOnPortAsync(int port)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(2) };
                // Prefer IPv4 loopback to avoid IPv6 (::1) resolution issues on Windows
                var urlsToTry = new[] { $"http://127.0.0.1:{port}", $"http://localhost:{port}" };
                foreach (var url in urlsToTry)
                {
                    try
                    {
                        var response = await httpClient.GetAsync(url);
                        if (response.IsSuccessStatusCode) return true;
                    }
                    catch { /* try next */ }
                }
                return false;
            }
            catch { return false; }
        }

        public void Dispose()
        {
            _currentProcessLauncher?.Dispose();
        }
    }
}
