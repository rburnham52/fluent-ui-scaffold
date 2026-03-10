# FluentUIScaffold

A fluent API for building maintainable E2E UI tests in .NET.

## Features

- **Fluent page object model** -- define pages with `Page<TSelf>` and chain actions with a readable, strongly-typed API
- **Deferred execution chains** -- actions queue up and execute only when awaited, preventing sync-over-async pitfalls
- **Pluggable hosting strategies** -- run your app with .NET, Node, external servers, or .NET Aspire
- **Playwright integration** -- first-class Playwright plugin with per-test browser session isolation
- **Aspire support** -- distributed application testing via `DistributedApplicationTestingBuilder`

## Installation

```bash
dotnet add package FluentUIScaffold.Core
dotnet add package FluentUIScaffold.Playwright
dotnet add package FluentUIScaffold.AspireHosting  # optional, for Aspire hosting
```

## Quick Start

### 1. Configure the scaffold (once per test assembly)

```csharp
[TestClass]
public static class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .UseDotNetHosting(opts =>
            {
                opts.BaseUrl = new Uri("http://localhost:5000");
                opts.ProjectPath = "path/to/MyApp.csproj";
            })
            .Web<WebApp>(options => { })
            .Build<WebApp>();
        await _app.StartAsync();
    }

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (_app != null) await _app.DisposeAsync();
    }

    public static AppScaffold<WebApp> App => _app!;
}
```

### 2. Define a page object

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public HomePage VerifyWelcomeVisible()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
        });
    }
}
```

### 3. Write a test

```csharp
[TestMethod]
public async Task HomePage_ShowsWelcome()
{
    await TestAssemblyHooks.App.CreateSessionAsync();
    try
    {
        await TestAssemblyHooks.App.NavigateTo<HomePage>()
            .VerifyWelcomeVisible();
    }
    finally
    {
        await TestAssemblyHooks.App.DisposeSessionAsync();
    }
}
```

## Documentation

- [Getting Started](docs/getting-started.md)
- [Page Object Pattern](docs/page-object-pattern.md)
- [API Reference](docs/api-reference.md)
- [Aspire Integration](docs/aspire-integration.md)

## License

This project is licensed under the MIT License. See the [LICENSE.md](LICENSE.md) file for details.
