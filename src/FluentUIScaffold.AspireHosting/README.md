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

## Documentation

For full documentation, examples, and guides, visit the [GitHub repository](https://github.com/rburnham52/fluent-ui-scaffold).

## License

This project is licensed under the MIT License. See the [LICENSE.md](https://github.com/rburnham52/fluent-ui-scaffold/blob/main/LICENSE.md) file for details.
