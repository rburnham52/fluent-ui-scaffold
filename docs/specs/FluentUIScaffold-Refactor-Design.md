### FluentUIScaffold Refactor: Unified Server Launch Design

This document describes the final design implemented to unify server startup under a single, test-friendly API.

### Goals
- One way to start servers for tests and samples
- Eliminate launcher/type explosion and hidden conventions
- Deterministic, order-independent builder API
- Cross-platform stability (Linux, macOS, Windows)
- Playwright-first testing integration

### Final API Surface
- `LaunchPlan` (immutable):
  - `ProcessStartInfo StartInfo`
  - `Uri BaseUrl`
  - `TimeSpan StartupTimeout`
  - `IReadOnlyList<string> HealthCheckEndpoints`
  - `IReadinessProbe ReadinessProbe`
  - `TimeSpan InitialDelay`, `TimeSpan PollInterval`
  - `bool StreamProcessOutput`
- `ServerProcessBuilder<TSelf>` (fluent, order-independent)
  - Concrete builders:
    - `DotNetServerConfigurationBuilder`
    - `AspireServerConfigurationBuilder`
    - `NodeJsServerConfigurationBuilder`
- `IReadinessProbe`
  - `Task WaitUntilReadyAsync(LaunchPlan plan, ILogger? logger, CancellationToken ct = default)`
  - Default: `HttpReadinessProbe`
- `ProcessLauncher` (single implementation to start processes and stream logs)
- `WebServerManager`
  - `static Task StartServerAsync(LaunchPlan plan)`
  - `static void StopServer()`
  - `static bool IsServerRunning()`

### Usage
```csharp
// Build a plan for a .NET app
var plan = ServerConfiguration
    .CreateDotNetServer(new Uri("http://localhost:5050"), "/abs/path/to/App.csproj")
    .WithFramework("net8.0")
    .WithConfiguration("Release")
    .WithAspNetCoreEnvironment("Development")
    .WithHealthCheckEndpoints("/", "/health")
    .Build();

await WebServerManager.StartServerAsync(plan);
// ... run tests ...
WebServerManager.StopServer();
```

```csharp
// Aspire
var plan = ServerConfiguration
    .CreateAspireServer(new Uri("http://localhost:6060"), "/abs/path/to/App.csproj")
    .WithAspireDashboardOtlpEndpoint("http://localhost:4317")
    .Build();
```

```csharp
// Node.js
var plan = ServerConfiguration
    .CreateNodeJsServer(new Uri("http://localhost:5173"), "/abs/path/to/package.json")
    .WithNpmScript("dev")
    .WithNodeEnvironment("test")
    .Build();
```

### Behavioral Details
- Mutex ownership across processes per port (`FluentUIScaffold_WebServer_{port}`)
  - If another runner is starting the server, wait for readiness instead of starting a second process
- Readiness
  - Default probe performs HTTP checks against `BaseUrl + endpoint`
  - Tries `http://127.0.0.1:{port}` first, then `http://localhost:{port}` (improves Windows stability)
  - `InitialDelay` and `PollInterval` are configurable via builder
- Process startup
  - Output streaming follows `LaunchPlan.StreamProcessOutput`
  - Builder sets `UseShellExecute = false`, redirects stdio, and `CreateNoWindow = true`
- Working directory
  - `WithProjectPath(path)` sets `WorkingDirectory` only if the directory exists
  - If not set, `ProcessStartInfo.WorkingDirectory` defaults to `Environment.CurrentDirectory`
  - Prevents invalid path errors on Windows when tests use dummy paths

### Environment Bootstrap for Background Agents
Add `.cursor/environment.json` to auto-install .NET and Playwright on agent startup:
```json
{
  "snapshot": "POPULATED_FROM_SETTINGS",
  "install": "bash -lc 'set -euxo pipefail; chmod +x ./dotnet-install.sh; ./dotnet-install.sh --channel 8.0 --install-dir $HOME/.dotnet; ./dotnet-install.sh --channel 7.0 --runtime dotnet --install-dir $HOME/.dotnet; ./dotnet-install.sh --channel 6.0 --runtime dotnet --install-dir $HOME/.dotnet; export DOTNET_ROOT=$HOME/.dotnet; export PATH=$HOME/.dotnet:$HOME/.dotnet/tools:$PATH; dotnet --info || true; dotnet tool install --global Microsoft.Playwright.CLI || true; playwright --version || true; playwright install chromium firefox webkit || true'",
  "terminals": []
}
```

### Testing
- Unit tests cover:
  - Builder serialization and env var projection: `LaunchPlanBuilderTests`
  - HTTP readiness behavior (success, non-200, timeout): `HttpReadinessProbeTests`
  - Manager lifecycle, mutex short-circuit, already-running detection: `WebServerManagerTests`
- Coverage target >= 70% achieved via `scripts/coverage.sh`

### Migration Guide
Removed legacy components (no backward compatibility as pre-release):
- `IServerLauncher`, `ServerLauncherFactory`
- `AspNet*ServerLauncher`, `AspireServerLauncher`, `NodeJsServerLauncher`
- Command builders and env var providers under `Launchers/Defaults`

Replace legacy flows:
```csharp
// Old (removed)
// var config = ServerConfiguration.CreateDotNetServer(...);
// await WebServerManager.StartServerAsync(config);

// New
var plan = ServerConfiguration.CreateDotNetServer(baseUrl, projectPath)
    .WithFramework("net8.0")
    .WithConfiguration("Release")
    .Build();
await WebServerManager.StartServerAsync(plan);
```

### FAQ
- Q: Can I supply a custom readiness probe?
  - Yes. Implement `IReadinessProbe` and pass via `WithReadiness(...)` on the builder.
- Q: How do I stream server logs during tests?
  - `WithProcessOutputLogging(true)` (default) enables piping stdout/stderr to the logger.
- Q: How do I avoid starting a process locally?
  - If a server is already running on `BaseUrl.Port`, the manager detects it and skips process start.