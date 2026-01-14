# Headless and SlowMo Configuration

## Overview

FluentUIScaffold provides flexible control over browser headless mode and SlowMo settings, with sensible defaults that automatically adapt based on your environment and debug mode.

## Configuration Options

### HeadlessMode Property

The `HeadlessMode` property allows explicit control over whether the browser runs in headless mode:

- **`null`** (default): Automatic determination based on debug mode and CI environment
- **`true`**: Force headless mode
- **`false`**: Force visible browser

### SlowMo Property

The `SlowMo` property controls the delay between browser operations in milliseconds:

- **`null`** (default): Automatic determination based on debug mode
- **`0`**: No delay (fastest execution)
- **`> 0`**: Custom delay in milliseconds

## Automatic Behavior

When `HeadlessMode` and `SlowMo` are not explicitly set (`null`), the framework automatically determines appropriate values:

### Headless Mode Logic

1. **Explicit Setting**: If `HeadlessMode` is set, use that value
2. Defaults when debugging: if a debugger is attached, drivers default to `HeadlessMode = false`
3. **CI Environment**: If `CI` environment variable is set, use headless mode
4. **Development**: Otherwise, use visible browser for easier debugging

### SlowMo Logic

1. **Explicit Setting**: If `SlowMo` is set, use that value
2. Defaults when debugging: if a debugger is attached, drivers default to a slight `SlowMo` (e.g., `250ms`)
3. **Normal Mode**: Otherwise, set `SlowMo = 0` for fastest execution

## Usage Examples

### Default Configuration (Automatic)

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        // HeadlessMode = null (automatic)
        // SlowMo = null (automatic)
    })
    .Build<WebApp>();
```

### Explicit Headless Control

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = true;  // Force headless
        opts.SlowMo = 500;         // Custom SlowMo
    })
    .Build<WebApp>();
```

### Defaults While Debugging (Automatic Non-Headless + SlowMo)

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = false;  // Visible browser
        opts.SlowMo = 250;          // 250ms delay during debugging
    })
    .Build<WebApp>();
```

### CI/CD Configuration

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = true;  // Force headless for CI
        opts.SlowMo = 0;           // No delay for speed
    })
    .Build<WebApp>();
```

### Development with Custom SlowMo

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = false;  // Visible browser
        opts.SlowMo = 2000;         // 2 second delay for debugging
    })
    .Build<WebApp>();
```

## Environment Variables

The framework respects the `CI` environment variable for automatic headless mode determination:

- **CI Environment**: `CI=true` → Headless mode enabled
- **Development**: `CI` not set → Visible browser (unless explicitly overridden)

## Best Practices

### For Development
```csharp
// Let the framework handle it automatically (optional explicit override shown)
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = false;
        opts.SlowMo = 250;
    })
    .Build<WebApp>();
```

### For CI/CD
```csharp
// Explicit control for consistent behavior
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = true;  // Force headless
        opts.SlowMo = 0;           // No delay for speed
    })
    .Build<WebApp>();
```

### For Manual Testing
```csharp
// Custom configuration for specific needs
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = false;  // Visible browser
        opts.SlowMo = 1500;         // 1.5 second delay
    })
    .Build<WebApp>();
```

## Configuration via FluentUIScaffoldBuilder

The recommended approach is to use the `FluentUIScaffoldBuilder` with the `Web<TApp>()` method:

```csharp
var app = new FluentUIScaffoldBuilder()
    .UsePlugin(new PlaywrightPlugin())
    .Web<WebApp>(opts =>
    {
        opts.BaseUrl = new Uri("http://localhost:5000");
        opts.HeadlessMode = true;   // null for auto-detect
        opts.SlowMo = 0;            // null for auto-detect
    })
    .WithAutoPageDiscovery()
    .Build<WebApp>();

await app.StartAsync();
```

## Logging

The framework provides detailed logging about headless and SlowMo decisions:

```
[Information] Using explicit headless mode setting: True
[Information] Using explicit SlowMo setting: 500ms
[Information] Debug mode enabled: automatically setting headless mode to false
[Information] Debug mode enabled: automatically setting SlowMo to 1000ms
[Information] Automatic headless mode determination: True (CI: True)
[Information] Normal mode: setting SlowMo to 0ms for faster execution
```

This helps you understand how the framework is making its automatic decisions.
