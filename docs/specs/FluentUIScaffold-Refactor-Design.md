### FluentUIScaffold Server Startup Refactor

## Problem statement

The current server startup uses multiple launcher types (ASP.NET Core, Aspire, Node.js) and a builder hierarchy where derived builder methods can accidentally upcast to the base builder, causing order-dependent behavior. As a result, late calls to base-only methods change the builder type before Build(), dropping .NET-specific settings (e.g., framework, configuration). Process stdout/stderr are not streamed into test logs, making startup issues hard to diagnose.

## Goals

- Single, generic process launcher that can start/stop/monitor any server (Aspire, ASP.NET Core, Node.js)
- Order-independent builder API: any sequence of WithX calls yields identical results
- Sensible defaults per profile that can be overridden (Aspire, ASP.NET Core, Node.js)
- Stream child process stdout/stderr into test logs
- Backwards-compatible static factories: CreateAspireServer, CreateDotNetServer, CreateNodeJsServer

## Non-goals

- Changing application code to alter ports/hosts
- Containerized launch support (future)

## High-level design

- Base builder: ServerProcessBuilder
  - Responsible for building a complete ProcessStartInfo + readiness/timeout into a LaunchPlan
  - Provides methods to set command, arguments (structured), environment variables, working directory, timeouts, health endpoints, and process name
  - Never loses builder type; all fluent calls return the same concrete builder type

- Profile builders extend the Process builder by seeding defaults and adding helpers:
  - DotNetServerConfigurationBuilder (command = "dotnet")
    - Seeds defaults for .NET apps (e.g., run, --no-launch-profile, configuration=Release, framework=net8.0)
    - Helpers: WithFramework, WithConfiguration, WithAspNetCoreUrls, WithAspNetCoreEnvironment, WithDotNetEnvironment, EnableSpaProxy
  - AspireServerConfigurationBuilder (inherits from DotNet)
    - Adds Aspire defaults (Aspire dashboard/resource env, ASNETCORE_URLS from BaseUrl, startup timeout ~90s)
    - Reuses DotNet helpers
  - NodeJsServerConfigurationBuilder (command = "npm")
    - Seeds defaults for Node (script=start, PORT from BaseUrl, NODE_ENV=development)
    - Helpers: WithNpmScript, WithNodeEnvironment, WithPort

- One launcher: ProcessLauncher
  - Consumes LaunchPlan (ProcessStartInfo + readiness + BaseUrl)
  - Starts process, streams stdout/stderr line-by-line to ILogger, runs readiness probe until healthy or timeout, handles stop/cleanup

## API (proposed)

### Entry points (backwards-compatible)

```csharp
public static class ServerConfiguration
{
	public static AspireServerConfigurationBuilder CreateAspireServer(Uri baseUrl, string projectPath);
	public static DotNetServerConfigurationBuilder CreateDotNetServer(Uri baseUrl, string projectPath);
	public static NodeJsServerConfigurationBuilder CreateNodeJsServer(Uri baseUrl, string projectPath);
}
```

### Base builder

```csharp
public class ServerProcessBuilder
{
	// Core inputs
	public ServerProcessBuilder WithBaseUrl(Uri baseUrl);
	public ServerProcessBuilder WithProjectPath(string projectPath);
	public ServerProcessBuilder WithWorkingDirectory(string workingDirectory);
	public ServerProcessBuilder WithProcessName(string processName);

	// Process command + args (order-independent)
	public ServerProcessBuilder WithExecutable(string executable);            // e.g., "dotnet", "npm"
	public ServerProcessBuilder WithArgument(string name, string? value = null);
	public ServerProcessBuilder WithArguments(params string[] args);

	// Environment & readiness
	public ServerProcessBuilder WithEnvironmentVariable(string key, string value);
	public ServerProcessBuilder WithEnvironmentVariables(IDictionary<string,string> env);
	public ServerProcessBuilder WithStartupTimeout(TimeSpan timeout);
	public ServerProcessBuilder WithHealthCheckEndpoints(params string[] endpoints);
	public ServerProcessBuilder WithReadiness(IReadinessProbe probe, TimeSpan? initialDelay = null, TimeSpan? pollInterval = null);
	public ServerProcessBuilder WithProcessOutputLogging(bool enabled = true);

	public LaunchPlan Build(); // Deterministically composes ProcessStartInfo + readiness
}
```

### DotNet builder (inherits Process)

```csharp
public class DotNetServerConfigurationBuilder : ServerProcessBuilder
{
	// Defaults applied in constructor
	// Executable = "dotnet"; args include: run, --no-launch-profile
	// Configuration = Release; Framework = net8.0

	public DotNetServerConfigurationBuilder WithFramework(string tfm);           // e.g., "net9.0"
	public DotNetServerConfigurationBuilder WithConfiguration(string config);     // Debug/Release
	public DotNetServerConfigurationBuilder WithAspNetCoreUrls(string urls);      // sets ASNETCORE_URLS
	public DotNetServerConfigurationBuilder WithAspNetCoreEnvironment(string env = "Development");
	public DotNetServerConfigurationBuilder WithDotNetEnvironment(string env = "Development");
	public DotNetServerConfigurationBuilder EnableSpaProxy(bool enabled = true);  // sets ASPNETCORE_HOSTINGSTARTUPASSEMBLIES
}
```

### Aspire builder (inherits DotNet)

```csharp
public class AspireServerConfigurationBuilder : DotNetServerConfigurationBuilder
{
	// Additional Aspire defaults in constructor:
	// DOTNET_DASHBOARD_OTLP_ENDPOINT_URL, DOTNET_RESOURCE_SERVICE_ENDPOINT_URL
	// ASNETCORE_URLS from BaseUrl, StartupTimeout ~ 90s
}
```

### Node builder (inherits Process)

```csharp
public class NodeJsServerConfigurationBuilder : ServerProcessBuilder
{
	// Defaults applied in constructor: Executable = "npm"; args = ["run", "start"]
	public NodeJsServerConfigurationBuilder WithNpmScript(string script = "start");
	public NodeJsServerConfigurationBuilder WithNodeEnvironment(string env = "development");
	public NodeJsServerConfigurationBuilder WithPort(int port); // sets PORT
}
```

### Launch plan consumed by the launcher

```csharp
public sealed class LaunchPlan
{
	public ProcessStartInfo StartInfo { get; init; }
	public Uri BaseUrl { get; init; }
	public TimeSpan StartupTimeout { get; init; }
	public string[] HealthCheckEndpoints { get; init; }
	public IReadinessProbe ReadinessProbe { get; init; }
	public TimeSpan InitialDelay { get; init; }
	public TimeSpan PollInterval { get; init; }
	// Process name can be derived from StartInfo.FileName (and optionally arguments) when needed
	public bool StreamProcessOutput { get; init; }
}
```

## ProcessLauncher changes

- After Start(process), spawn async readers that stream `StandardOutput` and `StandardError` line-by-line to ILogger (stdout=Information, stderr=Warning/Error)
- Use the LaunchPlan’s ReadinessProbe to poll BaseUrl and endpoints until healthy or timeout
- On Stop(), kill the process if we own it and dispose readers

## Order independence

- Builder stores structured options; `WithX` calls simply update fields (last-writer-wins)
- `Build()` serializes options to arguments/env in a deterministic order
- No reliance on parsing "Arguments" to infer framework/configuration

## Backwards compatibility

- Keep static factories in `ServerConfiguration` but return the new concrete builders
- Keep `WebServerManager.StartServerAsync(LaunchPlan)` signature unchanged
- Legacy launchers and command builders are deprecated but unused by default

## Defaults per profile (initial)

- Aspire: `dotnet run --no-launch-profile`, Framework=net8.0, Configuration=Release, ASNETCORE_URLS from BaseUrl, DOTNET/ASPNET envs, startup timeout=90s, health endpoints: ["/", "/health"]
- ASP.NET Core: `dotnet run --no-launch-profile`, Framework=net8.0, Configuration=Release, `--urls` from BaseUrl, startup timeout=60s, health endpoints: ["/"]
- Node.js: `npm run start`, `PORT` from BaseUrl, `NODE_ENV=development`, startup timeout=60s

## Examples

### Aspire (.NET 9, Debug)

```csharp
var plan = ServerConfiguration.CreateAspireServer(TestConfiguration.BaseUri, appHostCsproj)
	.WithFramework("net9.0")
	.WithConfiguration("Debug")
	.WithDotNetEnvironment("Development")
	.WithProcessName("KitchenChef.AppHost")
	.WithHealthCheckEndpoints("/", "/health")
	.Build();

await WebServerManager.StartServerAsync(plan);
```

### ASP.NET Core with SpaProxy

```csharp
var plan = ServerConfiguration.CreateDotNetServer(new Uri("http://localhost:5173"), webCsproj)
	.EnableSpaProxy()
	.WithFramework("net9.0")
	.WithConfiguration("Debug")
	.Build();
```

### Node.js dev

```csharp
var plan = ServerConfiguration.CreateNodeJsServer(new Uri("http://localhost:3000"), packageJson)
	.WithNpmScript("dev")
	.WithPort(3001)
	.WithNodeEnvironment("development")
	.Build();
```

## Acceptance criteria

- Any order of `WithX` calls leads to correct final args/env (tests assert permutations)
- Setting `WithFramework("net9.0")` anywhere results in `--framework net9.0` in StartInfo
- Process stdout/stderr appear in test logs by default
- Single ProcessLauncher handles Aspire, ASP.NET Core, and Node.js

## Implementation plan

1) Core models and builders
- Introduce `LaunchPlan`, refactor `WebServerManager` to use it
- Implement `ServerProcessBuilder` (renamed) and profile builders

2) ProcessLauncher streaming + readiness
- Add async readers for stdout/stderr
- Wire readiness probe from `LaunchPlan`

3) Factories and BC
- Update `ServerConfiguration.Create*` to return new builders
- Mark legacy launchers/command builders as `[Obsolete]`

4) Tests & docs
- Permutation tests for order independence
- Defaults tests per profile
- Stdout/stderr streaming tests with fake process
- Explicit serialization tests for `StartInfo.FileName`, `StartInfo.Arguments`, and `StartInfo.EnvironmentVariables` across helper variations (framework, configuration, proxy, urls, env merges, node script/port/env)
- Documentation and migration notes

## Open questions

- Keep `WebApplicationFactory` path? Recommendation: keep as a separate specialized path; default to ProcessLauncher
- For ASP.NET Core, prefer `--urls` argument or `ASPNETCORE_URLS` env? Current proposal: `--urls` for ASP.NET Core, env for Aspire


