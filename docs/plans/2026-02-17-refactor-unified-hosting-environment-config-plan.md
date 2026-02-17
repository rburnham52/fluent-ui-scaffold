---
title: "refactor: Unified hosting environment configuration"
type: refactor
status: active
date: 2026-02-17
deepened: 2026-02-17
---

# Unified Hosting Environment Configuration

## Enhancement Summary

**Deepened on:** 2026-02-17
**Sections enhanced:** 10 phases + acceptance criteria + file manifest
**Review agents used:** architecture-strategist, code-simplicity-reviewer, pattern-recognition-specialist, performance-oracle, security-sentinel, best-practices-researcher, Context7 (Aspire docs)

### Key Improvements

1. **Thread safety for Aspire env vars** — Add `SemaphoreSlim` guard around process-level `Environment.SetEnvironmentVariable()` calls to prevent concurrent test run corruption
2. **Store `FluentUIScaffoldOptions` as field** — Current `Options` property performs LINQ scan of `IServiceCollection` on every access; store as field at construction for O(1) access
3. **Exception-safe snapshot/restore** — Aspire env var cleanup must use `try/finally` to guarantee restoration even on test failure
4. **Expanded file manifest** — 6 additional files identified that reference deleted types (tests, Playwright builder) were missing from the original plan
5. **Simplified headless resolution** — CI detection branch is dead code (both paths assign `true`); simplify to `!Debugger.IsAttached`
6. **Eager validation** — Validate `BaseUrl` and `ProjectPath` at `UseDotNetHosting()`/`UseNodeHosting()` call time, not deferred to `StartAsync()`
7. **Security: redact env var values in process logs** — `ProcessLauncher` currently logs all env var values in plain text

### New Considerations Discovered

- `FluentUIScaffoldPlaywrightBuilder.cs` has 3 `UsePlaywright()` overloads referencing deleted types — must be updated
- Test files `HttpReadinessProbeTests.cs`, `WebServerManagerTests.cs`, `FluentUIScaffoldTests.cs`, `VerificationContextTests.cs` reference deleted code
- Aspire registration path bypasses the single-strategy guard — must be covered
- Guard against `"Production"` environment name as a safety measure
- `WithEnvironmentVariables(IDictionary)` has no callers — remove for simplicity
- `ConfigurationHash` returns empty string before `StartAsync` — document this contract

---

## Overview

Refactor the hosting configuration system to provide a single, consistent API on `FluentUIScaffoldBuilder` for environment settings, SPA proxy control, headless mode, and custom environment variables — working identically across DotNet, Node, and Aspire hosting strategies. Remove all legacy builder classes. Default to test-friendly settings.

## Problem Statement

Three intertwined problems exist in the current hosting architecture:

1. **Port mismatch in Aspire mode**: Aspire runs the app in Development mode, which can trigger the SPA dev server proxy via `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=Microsoft.AspNetCore.SpaProxy`. Auth tokens bound to one port fail on the other.

2. **No environment control in Aspire mode**: `AspireHostingStrategy` has no mechanism to set `ASPNETCORE_ENVIRONMENT` or disable SPA proxy on hosted apps. The old `AspireServerConfigurationBuilder` has these capabilities but produces a `LaunchPlan` that Aspire never consumes.

3. **Inconsistent configuration surface**: DotNet/Node strategies use `ServerProcessBuilder` methods. Aspire has nothing. Each strategy has its own duplicated builder API with no shared abstraction.

## Proposed Solution

Lift all environment configuration to `FluentUIScaffoldBuilder` with opinionated defaults. Each `IHostingStrategy` reads shared config from `FluentUIScaffoldOptions` and applies it in its strategy-specific way.

## Technical Approach

### Architecture

```
FluentUIScaffoldBuilder
  .WithEnvironment("Testing")           ─┐
  .WithEnvironmentVariable("K", "V")     │  Stored on FluentUIScaffoldOptions
  .WithSpaProxy(false)                   │  (EnvironmentVariables dict,
  .WithHeadless(true/false/null)         ─┘   EnvironmentName, SpaProxyEnabled)
                                          │
                    ┌─────────────────────┼─────────────────────┐
                    ▼                     ▼                     ▼
           DotNetHostingStrategy  NodeHostingStrategy   AspireHostingStrategy
           Sets env vars on       Maps to NODE_ENV,     Sets process-level env
           ProcessStartInfo       sets on ProcessStart  vars before CreateAsync
```

### Implementation Phases

#### Phase 1: Add environment properties to FluentUIScaffoldOptions

Add new properties to `FluentUIScaffoldOptions`:

```csharp
// src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptions.cs

/// <summary>
/// Custom environment variables applied to hosted applications.
/// </summary>
public Dictionary<string, string> EnvironmentVariables { get; } = new(StringComparer.OrdinalIgnoreCase);

/// <summary>
/// The logical environment name (e.g., "Testing", "Development").
/// Default: "Testing" — the framework assumes test execution.
/// </summary>
public string EnvironmentName { get; set; } = "Testing";

/// <summary>
/// Whether to enable the ASP.NET SPA dev server proxy.
/// Default: false — tests use prebuilt static assets.
/// </summary>
public bool SpaProxyEnabled { get; set; } = false;
```

Also remove the legacy `ServerConfiguration` property and `Plugins` list (plugins are registered via builder).

**File:** [FluentUIScaffoldOptions.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptions.cs)

**Success criteria:**
- [ ] `EnvironmentVariables` dictionary exists with `OrdinalIgnoreCase` comparer
- [ ] `EnvironmentName` defaults to `"Testing"`
- [ ] `SpaProxyEnabled` defaults to `false`
- [ ] `ServerConfiguration` property removed
- [ ] `Plugins` list removed (plugins registered via `UsePlugin()` on builder)
- [ ] `ServiceProvider` property removed (service locator anti-pattern)

### Research Insights (Phase 1)

**Best Practices (Options Pattern):**
- The `Action<T>` configure pattern is the correct .NET convention for this use case — matches `AddDbContext`, `AddAuthentication`, etc.
- Mutable properties with sensible defaults is standard for options types; no need for immutability here since the builder owns the mutation window

**Security Consideration:**
- Guard against `EnvironmentName = "Production"` — add a runtime warning or throw. Running E2E tests against production config is almost always a mistake.

**Simplification:**
- Remove `ServiceProvider` property from options — storing a service provider on an options object is the service locator anti-pattern. Pages and strategies should receive dependencies through constructor injection.

---

#### Phase 2: Add unified builder methods to FluentUIScaffoldBuilder

Add these methods to `FluentUIScaffoldBuilder`:

```csharp
// src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs

public FluentUIScaffoldBuilder WithEnvironment(string environmentName)
{
    Options.EnvironmentName = environmentName;
    return this;
}

public FluentUIScaffoldBuilder WithSpaProxy(bool enabled)
{
    Options.SpaProxyEnabled = enabled;
    return this;
}

public FluentUIScaffoldBuilder WithHeadless(bool? headless)
{
    Options.HeadlessMode = headless;
    return this;
}

public FluentUIScaffoldBuilder WithEnvironmentVariable(string key, string value)
{
    Options.EnvironmentVariables[key] = value;
    return this;
}

public FluentUIScaffoldBuilder WithEnvironmentVariables(IDictionary<string, string> variables)
{
    foreach (var kv in variables)
        Options.EnvironmentVariables[kv.Key] = kv.Value;
    return this;
}
```

**Headless resolution in `Build()`:** Resolve `HeadlessMode` to a concrete value if left null:

```csharp
// In Build<TWebApp>():
if (_options.HeadlessMode == null)
{
    _options.HeadlessMode = !System.Diagnostics.Debugger.IsAttached;
}
```

> **Simplification note:** The original plan had separate CI detection (`isCI` check) but both the `else if (isCI)` and `else` branches assign `true`. Since `!Debugger.IsAttached` is `true` whenever no debugger is attached (which covers CI), the CI detection is dead code. Simplified to: debugger attached = visible, otherwise headless.

**Single-strategy guard:** Throw if a hosting strategy is already registered when a second is added. This guard **must also cover the Aspire registration path** in `AspireHostingExtensions` — currently Aspire bypasses `RegisterHostingStrategy` and goes directly to `_services.AddSingleton<IHostingStrategy>(...)`.

**Store Options as field:** The current `Options` property performs a LINQ scan of `IServiceCollection` on every access:
```csharp
// BAD — current implementation (O(n) per access):
public FluentUIScaffoldOptions Options =>
    _services.Where(d => d.ServiceType == typeof(FluentUIScaffoldOptions))...

// GOOD — store as field at construction (O(1)):
private readonly FluentUIScaffoldOptions _options = new();
```

**Remove `WithEnvironmentVariables(IDictionary)`:** No callers exist in the codebase. Users can call `.WithEnvironmentVariable()` per key. Remove to keep the API surface minimal.

**File:** [FluentUIScaffoldBuilder.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs)

**Success criteria:**
- [ ] `.WithEnvironment()`, `.WithSpaProxy()`, `.WithHeadless()`, `.WithEnvironmentVariable()` methods exist
- [ ] Headless resolution runs in `Build()`: explicit > debugger attached > default headless
- [ ] `InvalidOperationException` thrown if two hosting strategies registered (including Aspire path)
- [ ] `Options` stored as a field, not LINQ-scanned from `IServiceCollection`
- [ ] `PlaywrightDriver.InitializeBrowser()` simplified — `HeadlessMode` is always concrete by the time it runs

### Research Insights (Phase 2)

**Architecture:**
- Mutable builder returning `this` is standard .NET convention (e.g., `WebApplicationBuilder`, `IHostBuilder`)
- Validation at setter time + `Build()` time is best practice — fail fast for obvious mistakes, comprehensive check at build

**Performance:**
- The `Options` property LINQ scan was flagged by all review agents as the #1 performance concern. At O(n) per access with multiple accesses per `Build()`, this adds measurable overhead. Storing as a field is O(1).

**Pattern Compliance:**
- Single-strategy guard must cover ALL registration paths. Add a `SetHostingStrategyRegistered()` method that Aspire extensions call, or route through the same guard mechanism.

---

#### Phase 3: Create DotNetHostingOptions and NodeHostingOptions

Replace the old builder classes with simple options types:

```csharp
// src/FluentUIScaffold.Core/Configuration/DotNetHostingOptions.cs
public class DotNetHostingOptions
{
    public string ProjectPath { get; set; } = string.Empty;
    public Uri? BaseUrl { get; set; }
    public string Framework { get; set; } = "net8.0";
    public string Configuration { get; set; } = "Release";
    public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(60);
    public string[] HealthCheckEndpoints { get; set; } = new[] { "/" };
    public string? WorkingDirectory { get; set; }
    public string? ProcessName { get; set; }
    public bool StreamProcessOutput { get; set; } = true;
}
```

```csharp
// src/FluentUIScaffold.Core/Configuration/NodeHostingOptions.cs
public class NodeHostingOptions
{
    public string ProjectPath { get; set; } = string.Empty;
    public Uri? BaseUrl { get; set; }
    public string Script { get; set; } = "start";
    public TimeSpan StartupTimeout { get; set; } = TimeSpan.FromSeconds(60);
    public string[] HealthCheckEndpoints { get; set; } = new[] { "/" };
    public string? WorkingDirectory { get; set; }
    public bool StreamProcessOutput { get; set; } = true;
}
```

**Update `UseDotNetHosting()` and `UseNodeHosting()` signatures:**

```csharp
public FluentUIScaffoldBuilder UseDotNetHosting(Action<DotNetHostingOptions> configure)
{
    var hostingOptions = new DotNetHostingOptions();
    configure(hostingOptions);
    // Store options; strategy reads them + FluentUIScaffoldOptions at StartAsync time
    return RegisterHostingStrategy(hostingOptions);
}

public FluentUIScaffoldBuilder UseNodeHosting(Action<NodeHostingOptions> configure)
{
    var hostingOptions = new NodeHostingOptions();
    configure(hostingOptions);
    return RegisterHostingStrategy(hostingOptions);
}
```

**Files:**
- New: `src/FluentUIScaffold.Core/Configuration/DotNetHostingOptions.cs`
- New: `src/FluentUIScaffold.Core/Configuration/NodeHostingOptions.cs`
- Modified: [FluentUIScaffoldBuilder.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs)

**Success criteria:**
- [ ] `DotNetHostingOptions` has all properties currently on `DotNetServerConfigurationBuilder` (ProjectPath, BaseUrl, Framework, Configuration, StartupTimeout, HealthCheckEndpoints, WorkingDirectory, ProcessName, StreamProcessOutput)
- [ ] `NodeHostingOptions` has all properties currently on `NodeJsServerConfigurationBuilder` (ProjectPath, BaseUrl, Script, StartupTimeout, HealthCheckEndpoints, WorkingDirectory, StreamProcessOutput)
- [ ] `UseDotNetHosting(Action<DotNetHostingOptions>)` and `UseNodeHosting(Action<NodeHostingOptions>)` replace the old signatures
- [ ] `BaseUrl` is required (validated non-null after `configure` runs)
- [ ] `ProjectPath` is required (validated non-empty after `configure` runs)

### Research Insights (Phase 3)

**Eager Validation:**
Add validation inside `UseDotNetHosting()` and `UseNodeHosting()` immediately after the `configure` callback runs:
```csharp
public FluentUIScaffoldBuilder UseDotNetHosting(Action<DotNetHostingOptions> configure)
{
    var opts = new DotNetHostingOptions();
    configure(opts);
    if (opts.BaseUrl == null) throw new ArgumentException("BaseUrl is required.", nameof(configure));
    if (string.IsNullOrWhiteSpace(opts.ProjectPath)) throw new ArgumentException("ProjectPath is required.", nameof(configure));
    return RegisterHostingStrategy(opts);
}
```
This fails fast at configuration time rather than deferring to `StartAsync()` where the error is harder to diagnose.

**Simplification:**
- Consider removing `StreamProcessOutput` from options — it's never set to `false` in any production or test code. If needed later, it can be added back.

---

#### Phase 4: Refactor DotNetHostingStrategy and NodeHostingStrategy to build LaunchPlan internally

The strategies currently receive a pre-built `LaunchPlan`. Refactor them to receive their options + `FluentUIScaffoldOptions` and build the `LaunchPlan` internally at `StartAsync` time.

```csharp
// src/FluentUIScaffold.Core/Hosting/DotNetHostingStrategy.cs
public sealed class DotNetHostingStrategy : IHostingStrategy
{
    private readonly DotNetHostingOptions _hostingOptions;
    private readonly FluentUIScaffoldOptions _scaffoldOptions;
    private readonly IServerManager _serverManager;
    // ...

    public DotNetHostingStrategy(
        DotNetHostingOptions hostingOptions,
        FluentUIScaffoldOptions scaffoldOptions,
        IServerManager? serverManager = null)
    {
        _hostingOptions = hostingOptions;
        _scaffoldOptions = scaffoldOptions;
        _serverManager = serverManager ?? new DotNetServerManager();
        // Config hash deferred to StartAsync or computed from options
    }

    public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken ct = default)
    {
        var launchPlan = BuildLaunchPlan();
        _configHash = ConfigHasher.Compute(launchPlan);
        // ... same as before
    }

    private LaunchPlan BuildLaunchPlan()
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = BuildArguments(),
            WorkingDirectory = _hostingOptions.WorkingDirectory ?? Environment.CurrentDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Apply unified environment config
        startInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"] = _scaffoldOptions.EnvironmentName;
        startInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"] = _scaffoldOptions.EnvironmentName;
        startInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"] =
            _scaffoldOptions.SpaProxyEnabled ? "Microsoft.AspNetCore.SpaProxy" : "";

        // Apply custom env vars (user overrides win — last-write-wins)
        foreach (var kv in _scaffoldOptions.EnvironmentVariables)
            startInfo.EnvironmentVariables[kv.Key] = kv.Value;

        return new LaunchPlan(startInfo, _hostingOptions.BaseUrl!, ...);
    }
}
```

**NodeHostingStrategy** does the same but maps environment name to `NODE_ENV`:

```csharp
private static string MapToNodeEnv(string environmentName)
{
    // ASP.NET naming convention -> Node.js convention (lowercase)
    return environmentName.ToLowerInvariant() switch
    {
        "testing" => "test",
        _ => environmentName.ToLowerInvariant()
    };
}
```

And sets `NODE_ENV` and `PORT` instead of ASP.NET-specific vars.

**Merge order:** Framework defaults (EnvironmentName, SpaProxy) are applied first as env vars on ProcessStartInfo. Then `FluentUIScaffoldOptions.EnvironmentVariables` are applied, overriding any defaults. This means `.WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")` overrides the `Testing` default.

**ConfigurationHash:** Computed at `StartAsync` time from the built `LaunchPlan`, not at construction. The `IHostingStrategy.ConfigurationHash` property returns empty string until started.

**Files:**
- [DotNetHostingStrategy.cs](src/FluentUIScaffold.Core/Hosting/DotNetHostingStrategy.cs)
- [NodeHostingStrategy.cs](src/FluentUIScaffold.Core/Hosting/NodeHostingStrategy.cs)

**Success criteria:**
- [ ] Both strategies accept their options type + `FluentUIScaffoldOptions`
- [ ] `LaunchPlan` built internally at `StartAsync` time
- [ ] Framework defaults applied first, then user env vars override
- [ ] DotNet sets `ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT`, `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`
- [ ] Node sets `NODE_ENV` (mapped from EnvironmentName), `PORT`, ignores SPA proxy
- [ ] Config hash computed at `StartAsync` time

### Research Insights (Phase 4)

**Security — Env Var Logging:**
`ProcessLauncher` currently logs all environment variable values in plain text. This can expose secrets (connection strings, API keys) in CI logs. Redact values or log only keys:
```csharp
// BAD: logger.LogDebug("Env: {Key}={Value}", kv.Key, kv.Value);
// GOOD: logger.LogDebug("Env: {Key}=***", kv.Key);
```

**Merge Order Documentation:**
The env var merge order is a critical contract. Document it clearly in code comments:
1. Strategy defaults (`ASPNETCORE_ENVIRONMENT`, `DOTNET_ENVIRONMENT`, `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`)
2. `FluentUIScaffoldOptions.EnvironmentVariables` (user overrides — last-write-wins)

This means `.WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development")` intentionally overrides the `Testing` default.

**ConfigurationHash Contract:**
Document that `ConfigurationHash` returns empty string before `StartAsync()` is called. Callers (like server reuse logic) must check for this state.

---

#### Phase 5: Update AspireHostingStrategy to apply environment variables

Modify `AspireHostingStrategy.StartAsync()` to read from `FluentUIScaffoldOptions` and apply env vars before `DistributedApplicationTestingBuilder.CreateAsync<T>()`.

```csharp
// src/FluentUIScaffold.AspireHosting/AspireHostingStrategy.cs

public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken ct = default)
{
    // Snapshot current env vars for cleanup
    _envVarSnapshot = CaptureEnvironmentSnapshot();

    // Apply unified environment config as process-level env vars
    // (Aspire's DistributedApplicationTestingBuilder inherits from test process)
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _scaffoldOptions.EnvironmentName);
    Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", _scaffoldOptions.EnvironmentName);
    Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES",
        _scaffoldOptions.SpaProxyEnabled ? "Microsoft.AspNetCore.SpaProxy" : "");

    // Aspire-specific defaults
    Environment.SetEnvironmentVariable("DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true");
    Environment.SetEnvironmentVariable("ASPIRE_ALLOW_UNSECURED_TRANSPORT", "true");

    // Apply custom env vars (user overrides win)
    foreach (var kv in _scaffoldOptions.EnvironmentVariables)
        Environment.SetEnvironmentVariable(kv.Key, kv.Value);

    // Now create and start the Aspire builder
    var appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>(ct);
    // ...
}
```

**Env var cleanup in `DisposeAsync()`:** Restore original values from snapshot.

```csharp
public async ValueTask DisposeAsync()
{
    // Restore environment variables
    RestoreEnvironmentSnapshot(_envVarSnapshot);
    // ... existing dispose logic
}
```

**Update `AspireHostingExtensions.UseAspireHosting()`** to pass `FluentUIScaffoldOptions` to the strategy. Since the strategy is created at registration time but options may be modified later, the strategy should receive the options reference (singleton), not a copy.

**File:** [AspireHostingStrategy.cs](src/FluentUIScaffold.AspireHosting/AspireHostingStrategy.cs), [AspireHostingExtensions.cs](src/FluentUIScaffold.AspireHosting/AspireHostingExtensions.cs)

**Success criteria:**
- [ ] Aspire strategy receives `FluentUIScaffoldOptions` reference
- [ ] Process-level env vars set before `CreateAsync<T>()`
- [ ] Env var snapshot captured before modification
- [ ] Env vars restored in `DisposeAsync()` using `try/finally`
- [ ] Aspire-specific defaults (dashboard, unsecured transport) auto-applied
- [ ] Custom env vars from `FluentUIScaffoldOptions.EnvironmentVariables` applied last (user wins)
- [ ] Aspire registration path triggers the same single-strategy guard as DotNet/Node

### Research Insights (Phase 5)

**Thread Safety (CRITICAL):**
`Environment.SetEnvironmentVariable()` is process-global. If multiple test classes run concurrently (parallel test execution), they can corrupt each other's env vars. Add a `SemaphoreSlim` guard:
```csharp
private static readonly SemaphoreSlim _envVarLock = new(1, 1);

public async Task<HostingResult> StartAsync(ILogger logger, CancellationToken ct)
{
    await _envVarLock.WaitAsync(ct);
    try
    {
        _envVarSnapshot = CaptureSnapshot();
        ApplyEnvironmentVariables();
        var appBuilder = await DistributedApplicationTestingBuilder.CreateAsync<TEntryPoint>(ct);
        // ...
    }
    finally
    {
        // Note: Do NOT release here — release in DisposeAsync after restore
    }
}

public async ValueTask DisposeAsync()
{
    try { RestoreSnapshot(); }
    finally { _envVarLock.Release(); }
}
```

**Exception Safety (CRITICAL):**
The snapshot/restore pattern MUST use `try/finally` to guarantee restoration even when tests throw. Without this, a failing test can leave corrupted env vars for all subsequent tests:
```csharp
public async ValueTask DisposeAsync()
{
    try
    {
        // ... dispose Aspire resources
    }
    finally
    {
        RestoreEnvironmentSnapshot(_envVarSnapshot);
    }
}
```

**Aspire Env Var Ordering (from Context7 docs):**
Env vars must be set BEFORE `DistributedApplicationTestingBuilder.CreateAsync<T>()` because the Aspire builder reads them during configuration. The builder's `configureBuilder` callback runs inside `CreateAsync`, so env vars set after that point are too late.

**Single-Strategy Guard:**
`AspireHostingExtensions.UseAspireHosting()` currently registers `IHostingStrategy` directly on the service collection, bypassing the builder's `RegisterHostingStrategy` guard. Route through the same mechanism or call a shared `SetHostingStrategyRegistered()` method.

---

#### Phase 6: Update RegisterHostingStrategy and builder wiring

Refactor `RegisterHostingStrategy` in `FluentUIScaffoldBuilder` to create strategies from options at registration time but defer `LaunchPlan` building to `StartAsync`.

For DotNet/Node, the builder creates the strategy with both the hosting-specific options and the shared `FluentUIScaffoldOptions`:

```csharp
private FluentUIScaffoldBuilder RegisterHostingStrategy(DotNetHostingOptions hostingOptions)
{
    if (_hostingStrategyRegistered)
        throw new InvalidOperationException("A hosting strategy is already registered. Only one hosting strategy can be configured.");
    _hostingStrategyRegistered = true;

    _services.AddSingleton<IHostingStrategy>(sp =>
    {
        var scaffoldOptions = sp.GetRequiredService<FluentUIScaffoldOptions>();
        return new DotNetHostingStrategy(hostingOptions, scaffoldOptions);
    });

    AddStartupAction(async (provider) =>
    {
        var logger = provider.GetRequiredService<ILogger<FluentUIScaffoldBuilder>>();
        var strategy = provider.GetRequiredService<IHostingStrategy>();
        var result = await strategy.StartAsync(logger);
        var options = provider.GetRequiredService<FluentUIScaffoldOptions>();
        options.BaseUrl = result.BaseUrl;
    });

    return this;
}
```

Using a factory delegate (`AddSingleton<IHostingStrategy>(sp => ...)`) ensures the strategy gets the final `FluentUIScaffoldOptions` after all builder configuration is done.

**File:** [FluentUIScaffoldBuilder.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs)

**Success criteria:**
- [ ] Strategies created via factory delegates that receive resolved `FluentUIScaffoldOptions`
- [ ] Single-strategy guard enforced via `_hostingStrategyRegistered` flag
- [ ] `UseExternalServer()` still works (env vars silently ignored for external hosts)
- [ ] Aspire wiring updated in `AspireHostingExtensions` to pass options reference

---

#### Phase 7: Simplify PlaywrightDriver headless resolution

Since `Build()` now resolves `HeadlessMode` to a concrete value, `PlaywrightDriver.InitializeBrowser()` simplifies:

```csharp
// Before:
bool isHeadless = _options.HeadlessMode ?? !debuggerAttached;
int slowMo = _options.SlowMo ?? (debuggerAttached ? 250 : 0);

// After:
bool isHeadless = _options.HeadlessMode ?? true; // fallback shouldn't happen but safe default
int slowMo = _options.SlowMo ?? (isHeadless ? 0 : 250);
```

The driver no longer needs its own CI detection or debugger-based logic for headless — the builder already handled it.

**File:** [PlaywrightDriver.cs](src/FluentUIScaffold.Playwright/PlaywrightDriver.cs)

**Success criteria:**
- [ ] `PlaywrightDriver` reads `HeadlessMode` directly (always concrete)
- [ ] No CI detection in driver code
- [ ] SlowMo auto-set: 0 for headless, 250 for visible

---

#### Phase 8: Delete legacy code

Remove these files/classes entirely:

| File | What's removed |
|------|----------------|
| [ServerConfigurationBuilder.cs](src/FluentUIScaffold.Core/Configuration/ServerConfigurationBuilder.cs) | `ServerProcessBuilder<TSelf>`, `DotNetServerConfigurationBuilder`, `NodeJsServerConfigurationBuilder`, `AspireServerConfigurationBuilder` — entire file |
| [ServerConfiguration.cs](src/FluentUIScaffold.Core/Configuration/ServerConfiguration.cs) | `ServerConfiguration` static class — entire file |
| [FluentUIScaffoldOptionsBuilder.Hosting.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptionsBuilder.Hosting.cs) | `FluentUIScaffoldOptionsBuilderHostingExtensions` — entire file |
| [FluentUIScaffoldOptionsBuilder.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptionsBuilder.cs) | `FluentUIScaffoldOptionsBuilder` — entire file (all its concerns now on `FluentUIScaffoldBuilder`) |

Remove these test files that test deleted code:

| File | Why |
|------|-----|
| [DotNetServerConfigurationBuilderTests.cs](tests/FluentUIScaffold.Core.Tests/DotNetServerConfigurationBuilderTests.cs) | Tests `DotNetServerConfigurationBuilder` which is removed |
| [LaunchPlanBuilderTests.cs](tests/FluentUIScaffold.Core.Tests/LaunchPlanBuilderTests.cs) | Tests `ServerProcessBuilder.Build()` which is removed |
| [FluentUIScaffoldOptionsBuilderTests.cs](tests/FluentUIScaffold.Core.Tests/FluentUIScaffoldOptionsBuilderTests.cs) | Tests `FluentUIScaffoldOptionsBuilder` which is removed |

Remove dead Aspire example code:

| File | What's removed |
|------|----------------|
| [AspireServerLifecycleExample.cs](samples/SampleApp.AspireTests/AspireServerLifecycleExample.cs) | `ShowCIConfiguration` test uses `ServerConfiguration.CreateAspireServer()` — remove or rewrite |

**Success criteria:**
- [ ] All listed files deleted
- [ ] Solution builds with no references to deleted types
- [ ] No orphaned `using` statements

### Research Insights (Phase 8)

**Expanded File List — Agent-Discovered Gaps:**

The following files were identified by review agents as referencing deleted types but were **missing from the original plan**:

| File | References to fix/delete | Action |
|------|-------------------------|--------|
| [FluentUIScaffoldPlaywrightBuilder.cs](src/FluentUIScaffold.Playwright/FluentUIScaffoldPlaywrightBuilder.cs) | Has 3 `UsePlaywright()` overloads — the `FluentUIScaffoldOptionsBuilder` and `FluentUIScaffoldOptions` overloads reference deleted types | **Modify**: Keep only the `FluentUIScaffoldBuilder` overload, delete the other two |
| [HttpReadinessProbeTests.cs](tests/FluentUIScaffold.Core.Tests/HttpReadinessProbeTests.cs) | Uses `ServerConfiguration.CreateDotNetServer()` | **Modify**: Update to use new `DotNetHostingOptions` directly |
| [WebServerManagerTests.cs](tests/FluentUIScaffold.Core.Tests/WebServerManagerTests.cs) | Uses `ServerConfiguration.CreateDotNetServer()` | **Modify**: Update to use new `DotNetHostingOptions` directly |
| [FluentUIScaffoldTests.cs](tests/FluentUIScaffold.Core.Tests/FluentUIScaffoldTests.cs) | Uses `FluentUIScaffoldOptionsBuilder` | **Modify**: Update to use `FluentUIScaffoldBuilder` |
| [VerificationContextTests.cs](tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs) | Uses `FluentUIScaffoldOptionsBuilder` | **Modify**: Update to use `FluentUIScaffoldBuilder` |

These files MUST be updated in Phase 8 to avoid build failures.

---

#### Phase 9: Update sample tests to new API

**SampleApp.Tests/TestAssemblyHooks.cs** — before:
```csharp
.UseDotNetHosting(
    TestConfiguration.BaseUri,
    projectPath,
    config => config
        .WithFramework("net8.0")
        .WithConfiguration("Release")
        .EnableSpaProxy(false)
        .WithAspNetCoreEnvironment("Development")
        .WithHealthCheckEndpoints("/", "/index.html")
        .WithStartupTimeout(TimeSpan.FromSeconds(120))
        .WithProcessName("SampleApp")
        .WithWorkingDirectory(...))
```

After:
```csharp
.UseDotNetHosting(opts =>
{
    opts.ProjectPath = projectPath;
    opts.BaseUrl = TestConfiguration.BaseUri;
    opts.Framework = "net8.0";
    opts.Configuration = "Release";
    opts.HealthCheckEndpoints = new[] { "/", "/index.html" };
    opts.StartupTimeout = TimeSpan.FromSeconds(120);
    opts.ProcessName = "SampleApp";
    opts.WorkingDirectory = Path.Combine(projectRoot, "samples", "SampleApp");
})
// No .EnableSpaProxy(false) needed — default
// No .WithAspNetCoreEnvironment() needed — default "Testing"
```

**SampleApp.AspireTests/TestAssemblyHooks.cs** — before:
```csharp
.UseAspireHosting<Projects.SampleApp_AppHost>(
    appHost => { },
    "sampleapp")
.Web<WebApp>(options =>
{
    options.HeadlessMode = false;
    options.UsePlaywright();
})
```

After:
```csharp
.UseAspireHosting<Projects.SampleApp_AppHost>(
    appHost => { },
    "sampleapp")
.UsePlaywright()
.WithHeadless(false)  // explicit: show browser
```

**Files:**
- [SampleApp.Tests/TestAssemblyHooks.cs](samples/SampleApp.Tests/TestAssemblyHooks.cs)
- [SampleApp.AspireTests/TestAssemblyHooks.cs](samples/SampleApp.AspireTests/TestAssemblyHooks.cs)
- [SampleApp.AspireTests/AspireServerLifecycleExample.cs](samples/SampleApp.AspireTests/AspireServerLifecycleExample.cs) — rewrite or remove

**Success criteria:**
- [ ] Both sample test projects compile and use new API
- [ ] No references to removed types
- [ ] Zero-config for Testing env and SPA proxy off

---

#### Phase 10: Write new tests

Replace deleted test files with tests for the new unified API:

**New test: `FluentUIScaffoldBuilderEnvironmentTests.cs`**
- [ ] `.WithEnvironment("Staging")` sets `EnvironmentName`
- [ ] `.WithEnvironmentVariable("K", "V")` adds to dictionary
- [ ] `.WithEnvironmentVariables(dict)` bulk adds
- [ ] `.WithSpaProxy(true)` overrides default
- [ ] `.WithHeadless(false)` overrides resolution
- [ ] Headless resolution: debugger > CI > default
- [ ] Duplicate hosting strategy throws `InvalidOperationException`
- [ ] Default `EnvironmentName` is `"Testing"`
- [ ] Default `SpaProxyEnabled` is `false`
- [ ] User env vars override framework defaults (last-write-wins)

**Update: `HostingStrategyTests.cs`**
- [ ] `DotNetHostingStrategy` accepts `DotNetHostingOptions` + `FluentUIScaffoldOptions`
- [ ] Env vars appear on built `LaunchPlan`
- [ ] `NodeHostingStrategy` maps `Testing` -> `NODE_ENV=test`
- [ ] `NodeHostingStrategy` maps `Development` -> `NODE_ENV=development`
- [ ] External strategy ignores env vars

**Files:**
- New: `tests/FluentUIScaffold.Core.Tests/FluentUIScaffoldBuilderEnvironmentTests.cs`
- Modified: [HostingStrategyTests.cs](tests/FluentUIScaffold.Core.Tests/Hosting/HostingStrategyTests.cs)

---

## Acceptance Criteria

### Functional Requirements

- [ ] All three hosting strategies (DotNet, Node, Aspire) use the same builder API for environment config
- [ ] Default `ASPNETCORE_ENVIRONMENT=Testing` — zero config required
- [ ] Default SPA proxy disabled — zero config required
- [ ] `.WithEnvironmentVariable()` works on any hosting strategy
- [ ] Headless resolution: explicit > debugger attached > CI detected > default (headless on)
- [ ] Node hosting maps `Testing` -> `NODE_ENV=test` (lowercase convention)
- [ ] Aspire env vars set before `DistributedApplicationTestingBuilder.CreateAsync<T>()`
- [ ] Aspire env vars cleaned up in `DisposeAsync()`
- [ ] External hosting strategy ignores environment variables silently
- [ ] Only one hosting strategy can be registered per builder

### Non-Functional Requirements

- [ ] Solution builds clean across all target frameworks (net6.0, net7.0, net8.0, net9.0)
- [ ] All existing tests pass (updated for new API)
- [ ] No backward-compatible shims — clean break
- [ ] Aspire env var snapshot/restore is exception-safe (`try/finally`)
- [ ] Aspire env var mutation is thread-safe (`SemaphoreSlim`)
- [ ] No env var values logged in plain text (security)

### Quality Gates

- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes all test projects
- [ ] `dotnet format` clean
- [ ] No references to deleted types anywhere in solution
- [ ] Grep for `ServerConfiguration`, `ServerProcessBuilder`, `FluentUIScaffoldOptionsBuilder` returns zero hits

## Files Changed Summary

### New Files
- `src/FluentUIScaffold.Core/Configuration/DotNetHostingOptions.cs`
- `src/FluentUIScaffold.Core/Configuration/NodeHostingOptions.cs`
- `tests/FluentUIScaffold.Core.Tests/FluentUIScaffoldBuilderEnvironmentTests.cs`

### Modified Files
- `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptions.cs`
- `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs`
- `src/FluentUIScaffold.Core/Hosting/DotNetHostingStrategy.cs`
- `src/FluentUIScaffold.Core/Hosting/NodeHostingStrategy.cs`
- `src/FluentUIScaffold.AspireHosting/AspireHostingStrategy.cs`
- `src/FluentUIScaffold.AspireHosting/AspireHostingExtensions.cs`
- `src/FluentUIScaffold.Playwright/PlaywrightDriver.cs`
- `src/FluentUIScaffold.Playwright/FluentUIScaffoldPlaywrightBuilder.cs` **(agent-discovered)**
- `tests/FluentUIScaffold.Core.Tests/Hosting/HostingStrategyTests.cs`
- `tests/FluentUIScaffold.Core.Tests/HttpReadinessProbeTests.cs` **(agent-discovered)**
- `tests/FluentUIScaffold.Core.Tests/WebServerManagerTests.cs` **(agent-discovered)**
- `tests/FluentUIScaffold.Core.Tests/FluentUIScaffoldTests.cs` **(agent-discovered)**
- `tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs` **(agent-discovered)**
- `samples/SampleApp.Tests/TestAssemblyHooks.cs`
- `samples/SampleApp.AspireTests/TestAssemblyHooks.cs`

### Deleted Files
- `src/FluentUIScaffold.Core/Configuration/ServerConfigurationBuilder.cs`
- `src/FluentUIScaffold.Core/Configuration/ServerConfiguration.cs`
- `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptionsBuilder.cs`
- `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptionsBuilder.Hosting.cs`
- `tests/FluentUIScaffold.Core.Tests/DotNetServerConfigurationBuilderTests.cs`
- `tests/FluentUIScaffold.Core.Tests/LaunchPlanBuilderTests.cs`
- `tests/FluentUIScaffold.Core.Tests/FluentUIScaffoldOptionsBuilderTests.cs`
- `samples/SampleApp.AspireTests/AspireServerLifecycleExample.cs` (or rewrite)

## References & Research

### Internal References
- Brainstorm: [2026-02-17-unified-hosting-environment-brainstorm.md](../../kitchen-chef/docs/brainstorms/2026-02-17-unified-hosting-environment-brainstorm.md)
- Current builder: [FluentUIScaffoldBuilder.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs)
- Current options: [FluentUIScaffoldOptions.cs](src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptions.cs)
- Headless docs: [headless-slowmo-configuration.md](docs/headless-slowmo-configuration.md)
- Hosting docs: [flexible-server-startup-framework.md](docs/flexible-server-startup-framework.md)
- Init pattern docs: [new-initialization-pattern.md](docs/new-initialization-pattern.md)

### External References
- SPA proxy mechanism: https://www.infoq.com/articles/dotnet-spa-templates-proxy/
- Aspire testing builder: https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/testing
