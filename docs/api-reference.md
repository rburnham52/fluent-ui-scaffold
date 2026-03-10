# API Reference

> **Scope note:** Infrastructure types that are public but not intended for direct consumer use (e.g., Server/ namespace internals, Launcher abstractions, project detectors) are intentionally excluded.

This reference covers all public types organized by NuGet package. For usage examples, see the [Getting Started Guide](getting-started.md) and [Page Object Pattern](page-object-pattern.md).

---

## FluentUIScaffold.Core

### `AppScaffold<TWebApp>`

Central orchestrator for the test lifecycle. Implements `IAsyncDisposable`.

**Constructor:**

```csharp
AppScaffold(IServiceProvider serviceProvider, Func<IServiceProvider, Task> startAction, IUITestingPlugin plugin)
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `ServiceProvider` | `IServiceProvider` | Root service provider |

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `StartAsync(CancellationToken cancellationToken = default)` | `Task` | Starts hosting and initializes the plugin. Idempotent. |
| `CreateSessionAsync()` | `Task<IBrowserSession>` | Creates an isolated browser session, stores as instance field |
| `DisposeSessionAsync()` | `Task` | Disposes the current session |
| `NavigateTo<TPage>()` | `TPage` | Creates page and enqueues navigation. `where TPage : Page<TPage>` |
| `NavigateTo<TPage>(object routeParams)` | `TPage` | Creates page with route parameter substitution. `where TPage : Page<TPage>` |
| `On<TPage>()` | `TPage` | Resolves a page without navigating. `where TPage : Page<TPage>` |
| `GetService<T>()` | `T` | Resolves a service from root DI. `where T : notnull` |

---

### `FluentUIScaffoldBuilder`

Instance-based fluent configuration builder.

**Constructor:**

```csharp
FluentUIScaffoldBuilder()  // parameterless
```

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `ConfigureServices(Action<IServiceCollection> configure)` | `FluentUIScaffoldBuilder` | Register additional services into DI |
| `AddStartupAction(Func<IServiceProvider, Task> action)` | `FluentUIScaffoldBuilder` | Add an async action to run at startup |
| `Web<TWebApp>(Action<FluentUIScaffoldOptions> configureOptions)` | `FluentUIScaffoldBuilder` | Configure web application options |
| `WithEnvironmentName(string environmentName)` | `FluentUIScaffoldBuilder` | Set environment name. Rejects "Production". |
| `WithSpaProxy(bool enabled)` | `FluentUIScaffoldBuilder` | Toggle SPA dev server proxy |
| `WithHeadlessMode(bool? headless)` | `FluentUIScaffoldBuilder` | Set headless mode. `null` = auto-detect (headless unless debugger attached). |
| `WithEnvironmentVariable(string key, string value)` | `FluentUIScaffoldBuilder` | Set an environment variable. Blocks dangerous keys (PATH, LD_PRELOAD, etc.). |
| `UseDotNetHosting(Action<DotNetHostingOptions> configure)` | `FluentUIScaffoldBuilder` | Configure .NET hosting. Requires BaseUrl + ProjectPath. |
| `UseNodeHosting(Action<NodeHostingOptions> configure)` | `FluentUIScaffoldBuilder` | Configure Node.js hosting. Requires BaseUrl + ProjectPath. |
| `UseExternalServer(Uri baseUrl, params string[] healthCheckEndpoints)` | `FluentUIScaffoldBuilder` | Configure an external (pre-started) server |
| `UsePlugin(IUITestingPlugin plugin)` | `FluentUIScaffoldBuilder` | Register a UI testing plugin. Only one plugin allowed. |
| `SetHostingStrategyRegistered()` | `void` | For extension methods that register hosting strategies externally |
| `Build<TWebApp>()` | `AppScaffold<TWebApp>` | Build the scaffold. Requires a plugin to be registered. Resolves HeadlessMode. |

---

### `FluentUIScaffoldOptions`

Configuration options record.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `Uri?` | `null` | Application under test URL |
| `HeadlessMode` | `bool?` | `null` | Headless browser mode. `null` = auto-detect. |
| `SlowMo` | `int?` | `null` | Browser slow-motion delay in milliseconds |
| `EnvironmentVariables` | `Dictionary<string, string>` | empty (case-insensitive) | Custom env vars for hosted applications |
| `EnvironmentName` | `string` | `"Testing"` | Logical environment name |
| `SpaProxyEnabled` | `bool` | `false` | ASP.NET SPA dev server proxy toggle |

---

### `DotNetHostingOptions`

Configuration for .NET-hosted applications.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProjectPath` | `string` | — | Path to .csproj file |
| `BaseUrl` | `Uri?` | `null` | Application base URL |
| `Framework` | `string` | `"net8.0"` | Target framework |
| `Configuration` | `string` | `"Release"` | Build configuration |
| `StartupTimeout` | `TimeSpan` | 60 seconds | Maximum time to wait for startup |
| `HealthCheckEndpoints` | `string[]` | `["/"]` | Endpoints to poll for readiness |
| `WorkingDirectory` | `string?` | `null` | Working directory override |
| `ProcessName` | `string?` | `null` | Process name override |

---

### `NodeHostingOptions`

Configuration for Node.js-hosted applications.

**Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProjectPath` | `string` | — | Path to directory containing package.json |
| `BaseUrl` | `Uri?` | `null` | Application base URL |
| `Script` | `string` | `"start"` | npm script to run |
| `StartupTimeout` | `TimeSpan` | 60 seconds | Maximum time to wait for startup |
| `HealthCheckEndpoints` | `string[]` | `["/"]` | Endpoints to poll for readiness |
| `WorkingDirectory` | `string?` | `null` | Working directory override |

---

### `Page<TSelf>`

Abstract page object base class. Custom awaitable -- chains execute when awaited via `GetAwaiter()`. Constraint: `where TSelf : Page<TSelf>`.

**Constructors:**

```csharp
protected Page(IServiceProvider serviceProvider)                                              // fresh action list
internal Page(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)  // shared action list (cross-page navigation)
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `ServiceProvider` | `IServiceProvider` | (protected) Scoped service provider |
| `Self` | `TSelf` | (protected) Typed self-reference for fluent returns |

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `Enqueue(Func<Task> action)` | `TSelf` | (protected) Queue a deferred action with no DI |
| `Enqueue<T>(Func<T, Task> action)` | `TSelf` | (protected) Queue an action with DI-resolved `T`. `where T : notnull` |
| `NavigateTo<TTarget>()` | `TTarget` | Freeze this page and create target sharing the action list. `where TTarget : Page<TTarget>` |
| `GetAwaiter()` | `TaskAwaiter` | Makes the page awaitable. Executes all queued actions sequentially. |

**Behavioral notes:**
- Frozen pages throw `FrozenPageException` if you attempt to enqueue more actions after `NavigateTo`.
- In DEBUG builds, a finalizer warns via `Trace.TraceWarning` if a chain with actions is never awaited.

---

### `RouteAttribute`

Marks a page class with its URL route pattern.

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RouteAttribute : Attribute
```

**Constructor:**

```csharp
RouteAttribute(string path)
```

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Path` | `string` | Route template (e.g., `"/users/{userId}"`) |

---

### `IBrowserSession`

Per-test browser session. Implements `IAsyncDisposable`.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `ServiceProvider` | `IServiceProvider` | Session-scoped service provider |

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `NavigateToUrlAsync(Uri url)` | `Task` | Navigate the session's page to a URL |

---

### `IUITestingPlugin`

Plugin contract for UI testing frameworks. Implements `IAsyncDisposable`.

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `ConfigureServices(IServiceCollection services)` | `void` | Register shared services into DI |
| `InitializeAsync(FluentUIScaffoldOptions options, CancellationToken cancellationToken = default)` | `Task` | One-time initialization (e.g., launch browser) |
| `CreateSessionAsync(IServiceProvider rootProvider)` | `Task<IBrowserSession>` | Create an isolated browser session |

---

### `IHostingStrategy`

Pluggable hosting contract. Implements `IAsyncDisposable`.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `ConfigurationHash` | `string` | Hash identifying the current configuration |
| `BaseUrl` | `Uri?` | Resolved base URL (available after start) |

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `StartAsync(ILogger logger, CancellationToken cancellationToken = default)` | `Task<HostingResult>` | Start the hosted application |
| `StopAsync(CancellationToken cancellationToken = default)` | `Task` | Stop the hosted application |
| `GetStatus()` | `HostingStatus` | Get current hosting status |

---

### `HostingResult`

```csharp
public sealed record HostingResult(Uri BaseUrl, bool WasReused);
```

---

### `HostingStatus`

```csharp
public sealed record HostingStatus(bool IsRunning, Uri? BaseUrl, int? ProcessId);
```

---

### Hosting Strategy Implementations

| Type | Description |
|------|-------------|
| `DotNetHostingStrategy` | Sealed. Manages .NET application lifecycle via `dotnet run`. |
| `NodeHostingStrategy` | Sealed. Manages Node.js application lifecycle via `npm run`. |
| `ExternalHostingStrategy` | Sealed. For pre-started servers (CI environments, staging). |

---

### Exceptions

| Exception | Base Class | Key Properties |
|-----------|-----------|----------------|
| `FluentUIScaffoldException` | `Exception` | `string? ScreenshotPath`, `string? DOMState`, `Uri? CurrentUrl`, `Dictionary<string, object> Context` |
| `InvalidPageException` | `FluentUIScaffoldException` | (inherited) |
| `ElementNotFoundException` | `FluentUIScaffoldException` | (inherited) |
| `TimeoutException` | `FluentUIScaffoldException` | (inherited) |
| `FluentUIScaffoldValidationException` | `FluentUIScaffoldException` | `string? Property` |
| `FluentUIScaffoldPluginException` | `FluentUIScaffoldException` | (inherited) |
| `FrozenPageException` | `InvalidOperationException` | `Type PageType` |

> **Note:** `FrozenPageException` extends `InvalidOperationException`, not `FluentUIScaffoldException`.

---

## FluentUIScaffold.Playwright

### `PlaywrightPlugin`

Implements `IUITestingPlugin`. Uses Chromium with `DOMContentLoaded` wait strategy.

**SlowMo behavior:** Uses the explicit value from options if set; otherwise defaults to 0 in headless mode, 50ms in headed mode.

---

### `PlaywrightBrowserSession`

Implements `IBrowserSession`. Each session owns its own `IBrowserContext` and `IPage`. Navigation uses `WaitUntilState.DOMContentLoaded`.

---

### `SessionServiceProvider`

Lightweight `IServiceProvider` wrapper for session-scoped resolution.

- Checks session-local dictionary (`IPage`, `IBrowserContext`, `IBrowser`) first
- Falls back to root provider for all other services
- Implements `IServiceProviderIsService` for `ActivatorUtilities` compatibility

---

### `FluentUIScaffoldPlaywrightBuilder`

Static extension class.

```csharp
public static FluentUIScaffoldBuilder UsePlaywright(this FluentUIScaffoldBuilder builder)
```

Convenience method that registers `PlaywrightPlugin` as the UI testing plugin.

---

## FluentUIScaffold.AspireHosting

For Aspire integration details and usage patterns, see [Aspire Integration](aspire-integration.md).

### `AspireHostingExtensions`

Static extension class.

```csharp
public static FluentUIScaffoldBuilder UseAspireHosting<TEntryPoint>(
    this FluentUIScaffoldBuilder builder,
    Action<IDistributedApplicationTestingBuilder> configure,
    string? baseUrlResourceName = null,
    string? baseUrlPrefix = null)
    where TEntryPoint : class
```

Configures the builder for Aspire-based hosting. Auto-discovers base URL from the named resource. The optional `baseUrlPrefix` supports hash-based SPA routing.

---

### `AspireHostingStrategy<TEntryPoint>`

Sealed. Implements `IHostingStrategy`. Constraint: `where TEntryPoint : class`.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Application` | `DistributedApplication?` | The running Aspire application instance |

**Behavioral notes:** Manages process-level environment variable mutation serialized via semaphore with snapshot/restore.

---

### `AspireResourceExtensions`

Static class.

```csharp
public static HttpClient CreateHttpClient<T>(this AppScaffold<T> app, string resourceName)
```

Creates an `HttpClient` configured to communicate with a named Aspire resource.

---

### `DistributedApplicationHolder`

Backward-compatibility DI accessor for the Aspire application instance.

**Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `DistributedApplication?` | The current application instance (get/set) |
