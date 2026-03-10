# FluentUIScaffold.Core

Core package for fluent E2E UI test automation in .NET. Provides the foundational building blocks: `FluentUIScaffoldBuilder`, `Page<TSelf>`, `AppScaffold<TWebApp>`, and pluggable hosting strategies.

## Installation

```bash
dotnet add package FluentUIScaffold.Core
```

## Usage

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(myPlugin)
    .UseDotNetHosting(opts => opts.ProjectPath = "path/to/App.csproj")
    .Web<WebApp>(options => { })
    .Build<WebApp>();
await app.StartAsync();
```

## Documentation

For full documentation, examples, and guides, visit the [GitHub repository](https://github.com/rburnham52/fluent-ui-scaffold).

## License

This project is licensed under the MIT License. See the [LICENSE.md](https://github.com/rburnham52/fluent-ui-scaffold/blob/main/LICENSE.md) file for details.
