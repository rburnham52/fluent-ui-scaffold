# FluentUIScaffold Initialization Pattern

## Overview

The FluentUIScaffold framework uses a builder-based initialization pattern with `AppScaffold<TApp>` as the central orchestrator. This provides an async-first design with pluggable hosting strategies.

## Builder Pattern

The `FluentUIScaffoldBuilder` provides a fluent API for configuring and building the application scaffold:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
        opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
        opts.HeadlessMode = true;
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

## Hosting Strategies

The framework supports pluggable hosting strategies for different application types:

### DotNetHostingStrategy

For .NET applications started with `dotnet run`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .UseDotNetHosting(new Uri("https://localhost:5001"), "path/to/project.csproj", opts =>
    {
        opts.WithFramework("net8.0");
        opts.WithConfiguration("Release");
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

### NodeHostingStrategy

For Node.js applications started with `npm run`:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .UseNodeHosting(new Uri("http://localhost:3000"), "path/to/client-app", opts =>
    {
        opts.WithScript("dev");
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

### ExternalHostingStrategy

For pre-started servers (CI environments, staging):

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .UseExternalServer(new Uri("https://staging.example.com"))
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

### AspireHostingStrategy

For Aspire distributed applications:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UseAspireHosting<Projects.MyApp_AppHost>(
        appHost => { /* configure */ },
        "myapp")
    .Web<WebApp>(opts => { opts.UsePlaywright(); })
    .Build<WebApp>();

await app.StartAsync();
```

## Auto-Discovery Features

### Plugin Registration

Plugins must be explicitly registered:

```csharp
// Register Playwright plugin
builder.UsePlugin(new PlaywrightPlugin());
```

### Page Auto-Discovery

The framework can automatically discover and register page objects:

```csharp
builder.WithAutoPageDiscovery();
```

This finds all types inheriting from `Page<TSelf>` and registers them with the DI container.

### Manual Page Registration

Alternatively, register pages explicitly:

```csharp
builder.RegisterPage<HomePage>();
builder.RegisterPage<LoginPage>();
builder.RegisterPage<RegistrationPage>();
```

## Debug Mode

The framework supports automatic debug detection:

- **Non-Headless Mode**: Automatically disables headless mode when debugger is attached
- **SlowMo**: Sets SlowMo to 1000ms for better visibility during debugging
- **Detailed Logging**: Provides enhanced logging of browser actions

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("https://localhost:5001");
        // HeadlessMode and SlowMo auto-detect based on debugger attachment
    })
    .Build<WebApp>();
```

## Fluent API Usage

With the initialized app, use a clean fluent API:

```csharp
app.NavigateTo<HomePage>()
    .WaitForVisible(p => p.LoginButton)
    .Click(p => p.LoginButton)
    .NavigateTo<LoginPage>()
    .Type(p => p.UsernameInput, "admin")
    .Type(p => p.PasswordInput, "password")
    .Click(p => p.SubmitButton);
```

### WaitFor Methods

The framework provides convenient `WaitFor` methods:

```csharp
// Wait for a specific element on a page
app.WaitFor<HomePage>(p => p.CounterButton);

// Wait for a page to be ready
app.WaitFor<Dashboard>();
```

## Benefits

1. **Async-First**: Modern async lifecycle with `StartAsync()` and `DisposeAsync()`
2. **Pluggable Hosting**: Support for .NET, Node.js, External servers, and Aspire
3. **Auto-Discovery**: Optional automatic page object discovery
4. **Clean API**: Fluent API with type-safe element selectors
5. **Debug Support**: Automatic debug detection for development

## Usage Examples

### Basic Test Setup

```csharp
[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlugin(new PlaywrightPlugin())
            .Web<WebApp>(opts =>
            {
                opts.BaseUrl = new Uri("https://localhost:5001");
                opts.DefaultWaitTimeout = TimeSpan.FromSeconds(30);
            })
            .WithAutoPageDiscovery()
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }

    public static AppScaffold<WebApp> App => _app!;
}
```

### Writing Tests

```csharp
[TestMethod]
public void Can_Navigate_And_Interact()
{
    var homePage = TestAssemblyHooks.App.NavigateTo<HomePage>();

    homePage
        .WaitForVisible(p => p.CounterButton)
        .Click(p => p.CounterButton)
        .Verify.TextContains(p => p.CounterValue, "1");
}
```

### Aspire Testing Setup

```csharp
[TestClass]
public class AspireTestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UseAspireHosting<Projects.SampleApp_AppHost>(
                appHost => { /* configure */ },
                "sampleapp")
            .Web<WebApp>(opts => { opts.UsePlaywright(); })
            .Build<WebApp>();

        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null)
            await _app.DisposeAsync();
    }

    public static AppScaffold<WebApp> App => _app!;
}
```
