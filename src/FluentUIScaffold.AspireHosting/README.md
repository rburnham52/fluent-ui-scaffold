# FluentUIScaffold.AspireHosting

.NET Aspire hosting integration for FluentUIScaffold. Enables distributed application testing using Aspire's `DistributedApplicationTestingBuilder`.

## Installation

```bash
dotnet add package FluentUIScaffold.AspireHosting
```

Requires [FluentUIScaffold.Core](https://www.nuget.org/packages/FluentUIScaffold.Core), Docker, the Aspire workload, and .NET 8+.

## Usage

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseAspireHosting<Projects.MyApp_AppHost>(
        appHost => { },
        "resourcename")
    .Web<WebApp>(options => { })
    .Build<WebApp>();
```

## Common gotchas

### Docker daemon must be running

`UseAspireHosting<TAppHost>()` requires Docker (or a compatible runtime such as
Rancher Desktop or Podman with Docker compatibility). Before booting the Aspire
AppHost, FluentUIScaffold runs a 2-second `docker info` probe and throws a
clear, single-line `InvalidOperationException` if the daemon is unreachable —
instead of letting Aspire hang and bury the real cause ~20 stack frames deep
behind `DistributedApplicationFactory → DcpHost → DcpDependencyCheck`.

Common reasons the probe fails:

- Docker Desktop / Rancher Desktop is not started.
- Docker Desktop is in **Resource Saver mode** — click the tray icon to wake
  it. (After a host crash, Docker Desktop can land in Resource Saver and stay
  there silently, which previously turned every `dotnet test` invocation into
  an indefinite hang.)
- The Docker daemon crashed.

If you genuinely want to bypass the local probe — for example because Aspire
is talking to a remote daemon via `DOCKER_HOST` — opt out with
`.SkipDockerPreflightCheck()`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseAspireHosting<Projects.MyApp_AppHost>(appHost => { }, "web")
    .SkipDockerPreflightCheck()
    .Web<WebApp>(options => { })
    .Build<WebApp>();
```

### HTTPS redirect + YARP + SPA CSP

Many ASP.NET Core templates ship with `app.UseHttpsRedirection()` enabled.
In the `Testing` environment it issues a `307 Temporary Redirect` to the
absolute HTTPS upstream URL (e.g. `https://localhost:7039/api/...`). When the
API sits behind a YARP reverse proxy serving an SPA, the browser sees the
absolute redirect and the SPA's CSP `connect-src 'self'` blocks the follow-up
request, producing a cryptic `"Failed to fetch"` in every test.

Opt in to HTTP-only mode to short-circuit this:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .WithHttpOnlyMode()                // <-- inject ASPNETCORE_URLS=http://+:0 + clear ASPNETCORE_HTTPS_PORT
    .UseAspireHosting<Projects.MyApp_AppHost>(appHost => { }, "web")
    .Web<WebApp>(options => { })
    .Build<WebApp>();
```

This injects two env vars into every Aspire-hosted process:

- `ASPNETCORE_URLS=http://+:0` — Aspire picks the port; no HTTPS endpoint is advertised.
- `ASPNETCORE_HTTPS_PORT=` (empty) — disables `UseHttpsRedirection`'s port resolution.

Off by default — opt-in only, since it changes wire-protocol expectations.

### Configurable startup timeout + heartbeat

Aspire AppHost startup is bounded to **90 seconds** by default. If startup
doesn't complete in that window, FluentUIScaffold throws a `TimeoutException`
naming the timeout duration, instead of hanging indefinitely with no signal.

While startup is in progress, FluentUIScaffold emits an
`ILogger.LogInformation` line every 10 seconds —
`"Aspire host starting... ({elapsed}s elapsed)"` — so you can tell whether
startup is making progress or stuck.

If your AppHost genuinely needs longer (e.g., a heavy database seed):

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .UseAspireHosting<Projects.MyApp_AppHost>(appHost => { }, "web")
    .WithAspireStartupTimeout(TimeSpan.FromMinutes(3))
    .Web<WebApp>(options => { })
    .Build<WebApp>();
```

## Documentation

For full documentation, examples, and guides, visit the [GitHub repository](https://github.com/rburnham52/fluent-ui-scaffold).

## License

This project is licensed under the MIT License. See the [LICENSE.md](https://github.com/rburnham52/fluent-ui-scaffold/blob/main/LICENSE.md) file for details.
