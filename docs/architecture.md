# FluentUIScaffold Architecture Diagram

This diagram illustrates the high-level architecture of the FluentUIScaffold framework, showing the relationships between the core components, plugins, drivers, and hosting strategies.

```mermaid
graph TD
    subgraph "Core Framework (FluentUIScaffold.Core)"
        App["AppScaffold&lt;TApp&gt;"]
        Builder["FluentUIScaffoldBuilder"]
        PM["PluginManager"]
        PR["PluginRegistry"]
        HS["IHostingStrategy"]
        Driver["IUIDriver (Abstract)"]
        Page["Page&lt;TSelf&gt;"]
    end

    subgraph "Plugins & Drivers"
        PP["PlaywrightPlugin"]
        PD["PlaywrightDriver"]
    end

    subgraph "Hosting Strategies"
        DHS["DotNetHostingStrategy"]
        NHS["NodeHostingStrategy"]
        EHS["ExternalHostingStrategy"]
        AHS["AspireHostingStrategy"]
    end

    subgraph "Aspire Integration"
        AH["AspireHostingExtensions"]
        DASH["DistributedApplicationHolder"]
    end

    %% Relationships
    Builder -->|Creates| App
    App -->|Uses| PM
    PM -->|Discovers via| PR
    PM -->|Creates| Driver
    App -->|Manages| HS
    App -->|Discovers| Page

    PP -.->|Implements| PR
    PD -.->|Implements| Driver
    PP -->|Creates| PD

    DHS -.->|Implements| HS
    NHS -.->|Implements| HS
    EHS -.->|Implements| HS
    AHS -.->|Implements| HS

    AH -->|Injects| DASH
    AH -->|Configures| Builder
    AH -->|Creates| AHS

    Page -->|Uses| Driver
```

## Key Components

### 1. AppScaffold<TApp>
The main entry point and async-first orchestrator for the testing framework. It manages the lifecycle of the driver, hosting strategy, and provides methods for navigation and page interaction.

Key Features:
- `StartAsync()` - Starts hosting strategy and initializes framework
- `DisposeAsync()` - Cleans up resources
- `NavigateTo<TPage>()` - Navigates to a page object
- `On<TPage>()` - Attaches to current page without navigation
- `WaitFor<TPage>()` - Waits for a page to be ready

### 2. Plugin Architecture
- **IUITestingFrameworkPlugin**: Interface for adding new UI testing frameworks (e.g., Playwright).
- **PluginManager / PluginRegistry**: Handle the registration and activation of plugins.
- **IUIDriver**: An abstraction layer over specific UI testing tools, allowing the core framework to remain tool-agnostic.

### 3. Hosting Strategies

The `IHostingStrategy` interface provides a unified abstraction for managing application servers:

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `DotNetHostingStrategy` | Manages .NET app via `dotnet run` | Standard .NET apps |
| `NodeHostingStrategy` | Manages Node.js app via `npm run` | Node.js/SPA apps |
| `ExternalHostingStrategy` | Health check only, no process management | CI/staging environments |
| `AspireHostingStrategy` | Wraps Aspire testing builder | Aspire distributed apps |

### 4. Page Object Model

The `Page<TSelf>` base class implements the Page Object Pattern with a self-referencing generic for fluent API support:

```csharp
public class HomePage : Page<HomePage>
{
    public IElement Button { get; private set; } = null!;

    public HomePage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        Button = Element("#button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();
    }

    public HomePage ClickButton()
    {
        return Click(p => p.Button);
    }
}
```

Key Features:
- **Auto-Discovery**: The framework can automatically discover and register page components via `WithAutoPageDiscovery()`.
- **Fluent Interactions**: `Click()`, `Type()`, `Select()`, `WaitForVisible()` all return `TSelf` for chaining.
- **Fluent Verification**: `page.Verify.Visible(p => p.Element)` for type-safe assertions.

### 5. Aspire Integration
- **AspireHostingExtensions**: Provides fluent methods to integrate Aspire's distributed application testing with FluentUIScaffold.
- **AspireHostingStrategy**: Wraps `DistributedApplicationTestingBuilder` from Aspire.Hosting.Testing.
- **DistributedApplicationHolder**: Stores the `DistributedApplication` instance in DI for test access.

## Initialization Flow

```mermaid
sequenceDiagram
    participant Test
    participant Builder as FluentUIScaffoldBuilder
    participant App as AppScaffold
    participant HS as IHostingStrategy
    participant PM as PluginManager
    participant Driver as IUIDriver

    Test->>Builder: new FluentUIScaffoldBuilder()
    Test->>Builder: UsePlugin(new PlaywrightPlugin())
    Test->>Builder: Web<WebApp>(opts => ...)
    Test->>Builder: Build<WebApp>()
    Builder->>App: Create AppScaffold
    Test->>App: StartAsync()
    App->>HS: StartAsync()
    HS-->>App: HostingResult (BaseUrl)
    App->>PM: CreateDriver()
    PM-->>App: IUIDriver
    Test->>App: NavigateTo<HomePage>()
    App->>Driver: Navigate and return page
```

## Configuration Example

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

// Use the app
app.NavigateTo<HomePage>()
    .Click(p => p.LoginButton)
    .NavigateTo<LoginPage>()
    .Type(p => p.Username, "admin")
    .Type(p => p.Password, "password")
    .Click(p => p.SubmitButton)
    .Verify.Visible(p => p.WelcomeMessage);

await app.DisposeAsync();
```
