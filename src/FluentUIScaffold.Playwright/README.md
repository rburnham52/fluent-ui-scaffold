# FluentUIScaffold.Playwright

Playwright plugin for FluentUIScaffold. Provides browser automation powered by Microsoft Playwright with per-test session isolation.

## Installation

```bash
dotnet add package FluentUIScaffold.Playwright
```

Requires [FluentUIScaffold.Core](https://www.nuget.org/packages/FluentUIScaffold.Core).

## Usage

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlaywright()
    .Web<WebApp>(options => { })
    .Build<WebApp>();
```

## Documentation

For full documentation, examples, and guides, visit the [GitHub repository](https://github.com/rburnham52/fluent-ui-scaffold).

## License

This project is licensed under the MIT License. See the [LICENSE.md](https://github.com/rburnham52/fluent-ui-scaffold/blob/main/LICENSE.md) file for details.
