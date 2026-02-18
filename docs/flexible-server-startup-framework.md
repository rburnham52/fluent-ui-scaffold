# Hosting Strategies

## Overview

FluentUIScaffold uses pluggable hosting strategies to manage the lifecycle of the application under test. Each strategy handles starting, health-checking, and stopping a specific type of server. Configure hosting via `FluentUIScaffoldBuilder` methods.

## Available Strategies

| Strategy | Builder Method | Use Case |
|----------|---------------|----------|
| `DotNetHostingStrategy` | `.UseDotNetHosting()` | .NET applications via `dotnet run` |
| `NodeHostingStrategy` | `.UseNodeHosting()` | Node.js applications via `npm run` |
| `ExternalHostingStrategy` | `.UseExternalServer()` | Pre-started servers (CI, staging) |
| `AspireHostingStrategy` | `.UseAspireHosting<T>()` | Aspire distributed applications |

Only one hosting strategy can be registered per builder.

## DotNet Hosting

For .NET applications started with `dotnet run`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseDotNetHosting(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.ProjectPath = "/path/to/project.csproj";
        opts.Framework = "net8.0";
        opts.Configuration = "Release";
        opts.HealthCheckEndpoints = new[] { "/", "/index.html" };
        opts.StartupTimeout = TimeSpan.FromSeconds(120);
        opts.WorkingDirectory = "/path/to/project";
        opts.ProcessName = "MyApp";
    })
    .Web<WebApp>(options =>
    {
        options.BaseUrl = new Uri("http://localhost:5000");
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

### DotNetHostingOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProjectPath` | `string` | `""` | Path to the `.csproj` file (required) |
| `BaseUrl` | `Uri?` | `null` | URL where the app will be accessible (required) |
| `Framework` | `string` | `"net8.0"` | Target framework moniker |
| `Configuration` | `string` | `"Release"` | Build configuration |
| `StartupTimeout` | `TimeSpan` | `60s` | Max time to wait for readiness |
| `HealthCheckEndpoints` | `string[]` | `["/"]` | Endpoints to probe for readiness |
| `WorkingDirectory` | `string?` | `null` | Working directory (defaults to project dir) |
| `ProcessName` | `string?` | `null` | Optional process name for identification |

## Node Hosting

For Node.js applications started with `npm run`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseNodeHosting(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:3000");
        opts.ProjectPath = "/path/to/client-app";
        opts.Script = "dev";
        opts.StartupTimeout = TimeSpan.FromSeconds(60);
    })
    .Web<WebApp>(options =>
    {
        options.BaseUrl = new Uri("http://localhost:3000");
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

### NodeHostingOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProjectPath` | `string` | `""` | Path to the directory containing `package.json` (required) |
| `BaseUrl` | `Uri?` | `null` | URL where the app will be accessible (required) |
| `Script` | `string` | `"start"` | npm script to run |
| `StartupTimeout` | `TimeSpan` | `60s` | Max time to wait for readiness |
| `HealthCheckEndpoints` | `string[]` | `["/"]` | Endpoints to probe for readiness |
| `WorkingDirectory` | `string?` | `null` | Working directory for the process |

## External Server

For pre-started servers in CI environments or staging:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseExternalServer(new Uri("https://staging.example.com"), "/health")
    .Web<WebApp>(options =>
    {
        options.BaseUrl = new Uri("https://staging.example.com");
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

No process management is performed — only health check probing to verify the server is reachable.

## Aspire Hosting

For Aspire distributed applications (requires the `FluentUIScaffold.AspireHosting` package):

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.SampleApp_AppHost>(
        appHost => { /* configure distributed app */ },
        "sampleapp")
    .Web<WebApp>(options => { options.UsePlaywright(); })
    .Build<WebApp>();

await app.StartAsync();
```

The base URL is auto-discovered from the named Aspire resource. See [Architecture](architecture.md) for more details on Aspire integration.

## Environment Configuration

The builder provides environment settings that are applied to all hosting strategies:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .WithEnvironmentName("Development")      // Sets ASPNETCORE_ENVIRONMENT
    .WithSpaProxy(false)                      // Disables SPA dev proxy
    .WithEnvironmentVariable("MY_VAR", "x")  // Custom env vars
    .UseDotNetHosting(opts => { /* ... */ })
    .Web<WebApp>(options => { /* ... */ })
    .Build<WebApp>();
```

## Readiness Probes

All hosting strategies use `HttpReadinessProbe` to verify the application is ready. The probe polls the configured `BaseUrl` and `HealthCheckEndpoints` until the server responds with a success status or the `StartupTimeout` is reached.

## Best Practices

1. **Use explicit project paths in CI/CD** — don't rely on relative path resolution
2. **Set appropriate timeouts** — increase `StartupTimeout` for slow-starting applications
3. **Configure health check endpoints** — use endpoints that verify full readiness, not just TCP connectivity
4. **Always dispose** — use `await app.DisposeAsync()` in test cleanup to stop hosted processes
