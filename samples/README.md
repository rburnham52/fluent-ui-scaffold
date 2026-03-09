# FluentUIScaffold Sample Application

This directory contains a sample application demonstrating the FluentUIScaffold E2E testing framework. The sample includes an ASP.NET Core backend with a Svelte frontend and two test projects: one using standard hosting and one using .NET Aspire.

## Overview

The sample application showcases:

- **Modern Web Application**: ASP.NET Core backend with Svelte frontend (Vite)
- **Deferred Execution Page Objects**: `Page<TSelf>` chain builder with `Enqueue<IPage>()` for browser interactions
- **Awaitable Fluent Chains**: `GetAwaiter()` makes page action chains directly awaitable
- **Cross-Page Navigation**: Type-safe `NavigateTo<TTarget>()` transitions
- **Aspire Integration**: Distributed application testing via `.UseAspireHosting<T>()`

## Project Structure

```
samples/
├── SampleApp/                    # ASP.NET Core web application
│   ├── ClientApp/               # Svelte frontend with Vite
│   │   ├── src/
│   │   │   ├── App.svelte      # Main application component
│   │   │   └── lib/
│   │   │       ├── Counter.svelte      # Interactive counter component
│   │   │       ├── TodoList.svelte     # Todo management component
│   │   │       └── UserProfile.svelte  # User profile form component
│   │   └── package.json
│   ├── Controllers/             # API controllers
│   ├── Program.cs               # ASP.NET Core startup
│   └── SampleApp.csproj
├── SampleApp.AppHost/           # Aspire AppHost orchestrator
│   ├── Program.cs               # Aspire application setup
│   └── SampleApp.AppHost.csproj
├── SampleApp.Tests/             # Standard test project
│   ├── Pages/                   # Page objects (HomePage, TodosPage, LoginPage)
│   ├── TestBases/               # Abstract base test classes
│   ├── Examples/                # Concrete test wrappers
│   ├── SharedTestApp.cs         # Assembly hooks (AppScaffold lifecycle)
│   ├── TestConfiguration.cs     # Config (headless mode, base URL)
│   └── SampleApp.Tests.csproj
├── SampleApp.AspireTests/       # Aspire-hosted test project
│   ├── Examples/                # Concrete test wrappers (own namespace)
│   ├── TestAssemblyHooks.cs     # Aspire assembly hooks
│   └── SampleApp.AspireTests.csproj
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Node.js 16+ and npm
- Docker (required for Aspire tests)

### Running the Sample Application

1. **Install frontend dependencies**:
   ```bash
   cd samples/SampleApp/ClientApp
   npm install
   cd ..
   ```

2. **Run the application**:
   ```bash
   dotnet run --project samples/SampleApp/SampleApp.csproj
   ```

3. **Access the application** at `http://localhost:5001`. The application automatically proxies to the Vite dev server.

### Running the Tests

#### Standard Tests
```bash
dotnet test samples/SampleApp.Tests/SampleApp.Tests.csproj
```

#### Aspire Tests (requires Docker)
```bash
dotnet test samples/SampleApp.AspireTests/SampleApp.AspireTests.csproj
```

No Aspire workload installation is required. The Aspire hosting libraries are referenced as NuGet packages.

## Framework Usage

### Assembly Setup with AppScaffold

Tests share a single `AppScaffold<TApp>` instance configured via `FluentUIScaffoldBuilder`. The scaffold is started once at assembly initialization and disposed at cleanup.

#### Standard Setup (SharedTestApp.cs)

```csharp
[TestClass]
public class SharedTestApp
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UsePlaywright()
            .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost:5000"))
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

#### Aspire Setup (TestAssemblyHooks.cs)

```csharp
[TestClass]
public class TestAssemblyHooks
{
    private static AppScaffold<WebApp>? _app;

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext context)
    {
        _app = new FluentUIScaffoldBuilder()
            .UseAspireHosting<Projects.SampleApp_AppHost>(
                appHost => { /* configure distributed app */ },
                "sampleapp")
            .Web<WebApp>(options => { options.UsePlaywright(); })
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

### Per-Test Sessions

Each test (or test class) creates and disposes its own browser session:

```csharp
[TestInitialize]
public async Task TestInitialize()
{
    await SharedTestApp.App.CreateSessionAsync();
}

[TestCleanup]
public async Task TestCleanup()
{
    await SharedTestApp.App.DisposeSessionAsync();
}
```

Abstract base test classes in `TestBases/` encapsulate this session lifecycle. Concrete test classes in `Examples/` inherit from the base classes to avoid boilerplate.

### Page Object Pattern

Page objects extend `Page<TSelf>` and use the `[Route]` attribute for URL mapping. Browser interactions are enqueued as deferred async actions via `Enqueue<IPage>()`, building a chain that executes when awaited.

```csharp
[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public HomePage VerifyWelcomeVisible() => Enqueue<IPage>(async page =>
    {
        await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
    });

    public HomePage ClickCounter() => Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
    });

    public HomePage VerifyCounterValue(string expected) => Enqueue<IPage>(async page =>
    {
        var button = page.Locator("button:has-text('count is')");
        await Assertions.Expect(button).ToContainTextAsync(expected).ConfigureAwait(false);
    });
}
```

Key differences from the old API:

- No `ConfigureElements()` override or `IElement` fields.
- No `WaitStrategy` configuration. Waiting is handled directly in `Enqueue` lambdas using Playwright's built-in waiting (e.g., `WaitForAsync()`, `ToContainTextAsync()`).
- Constructor takes only `IServiceProvider`; no driver, options, or logger parameters.
- Methods return `this` (typed as `TSelf`) for chaining, but the chain is deferred until awaited via `GetAwaiter()`.

### Cross-Page Navigation

Navigate between pages using `NavigateTo<TTarget>()`:

```csharp
[Route("/todos")]
public class TodosPage : Page<TodosPage>
{
    protected TodosPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public TodosPage VerifyTodoCount(int expected) => Enqueue<IPage>(async page =>
    {
        var items = page.Locator("[data-testid='todo-item']");
        await Assertions.Expect(items).ToHaveCountAsync(expected).ConfigureAwait(false);
    });

    public HomePage NavigateToHome() => NavigateTo<HomePage>();
}
```

### Writing Tests

Tests await the fluent page chain directly. The `GetAwaiter()` on `Page<TSelf>` executes all enqueued actions in order.

```csharp
[TestMethod]
public async Task HomePage_DisplaysWelcomeMessage()
{
    await SharedTestApp.App.NavigateTo<HomePage>()
        .VerifyWelcomeVisible();
}

[TestMethod]
public async Task Counter_IncrementsOnClick()
{
    await SharedTestApp.App.NavigateTo<HomePage>()
        .ClickCounter()
        .ClickCounter()
        .ClickCounter()
        .VerifyCounterValue("3");
}

[TestMethod]
public async Task Navigation_BetweenPages()
{
    await SharedTestApp.App.NavigateTo<HomePage>()
        .VerifyWelcomeVisible();

    await SharedTestApp.App.NavigateTo<TodosPage>()
        .VerifyTodoCount(3);
}
```

### Test Organization

The sample tests use a layered structure:

- **`TestBases/`** -- Abstract base test classes that handle `CreateSessionAsync()` / `DisposeSessionAsync()` lifecycle and provide helper methods.
- **`Examples/`** -- Concrete test classes that inherit from the base classes and contain the actual test methods. Both `SampleApp.Tests` and `SampleApp.AspireTests` have their own `Examples/` folder with tests in their respective namespaces.

This separation keeps session management DRY while allowing each test project to define its own assembly hooks and configuration.

## Web Application Features

The Svelte frontend provides several UI components for testing:

1. **Multi-tab Navigation**: Home, Todos, and Profile tabs
2. **Interactive Counter**: Clickable counter with state management
3. **Todo Management**: Add, edit, filter, and delete todos
4. **User Profile Form**: Complex form with validation
5. **Weather Data**: Async data loading and display

## Best Practices

### Page Object Design

- **Use `[Route]` attributes** to declare page URLs declaratively.
- **Keep `Enqueue` lambdas focused** -- each method should perform one logical interaction or assertion.
- **Return `this`** (via `Enqueue`) for fluent chaining; avoid returning raw `Task`.
- **Use Playwright locators** directly inside `Enqueue<IPage>()` rather than abstracting element selectors into fields.

### Selectors

- Prefer `data-testid` attributes for stable selectors that survive UI refactors.
- Use Playwright's built-in pseudo-selectors like `:has-text()` for readable locators.

### Test Structure

- **One behavior per test**: Each test method should verify a single scenario.
- **Await the chain**: Always `await` the page chain to execute enqueued actions.
- **Session isolation**: Use `CreateSessionAsync()` / `DisposeSessionAsync()` per test for a clean browser state.
- **Descriptive names**: Test method names should describe the scenario and expected outcome.

### Aspire Tests

- Docker must be running before executing Aspire tests.
- Aspire tests share the same page objects and test base classes as standard tests; only the assembly hooks differ.
- No Aspire workload installation is needed -- the required packages are referenced via NuGet.

## Troubleshooting

### Common Issues

1. **Element not found / timeout**: Check your Playwright locator. Use the browser dev tools to verify the selector matches.
2. **Tests hang**: Ensure `await` is used on all page chains. A missing `await` means enqueued actions never execute.
3. **Aspire tests fail to start**: Verify Docker is running. Check that the AppHost project builds successfully.
4. **Session errors**: Confirm that `CreateSessionAsync()` is called before navigation and `DisposeSessionAsync()` is called in cleanup.

### Debugging Tips

1. **Run headed**: Set headless mode to `false` in `TestConfiguration.cs` to watch the browser.
2. **Playwright traces**: Enable Playwright tracing for detailed interaction logs.
3. **Inspect selectors**: Use `page.Locator().HighlightAsync()` inside an `Enqueue` lambda during development.
4. **Check logs**: Review test output for navigation URLs and session lifecycle events.
