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
2. **Debug Mode**: If `EnableDebugMode` is `true`, automatically set `HeadlessMode = false`
3. **CI Environment**: If `CI` environment variable is set, use headless mode
4. **Development**: Otherwise, use visible browser for easier debugging

### SlowMo Logic

1. **Explicit Setting**: If `SlowMo` is set, use that value
2. **Debug Mode**: If `EnableDebugMode` is `true`, automatically set `SlowMo = 1000`
3. **Normal Mode**: Otherwise, set `SlowMo = 0` for fastest execution

## Usage Examples

### Default Configuration (Automatic)

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .Build();

// HeadlessMode = null (automatic)
// SlowMo = null (automatic)
```

### Explicit Headless Control

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithHeadlessMode(true)  // Force headless
    .WithSlowMo(500)         // Custom SlowMo
    .Build();
```

### Debug Mode (Automatic Non-Headless + SlowMo)

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithDebugMode(true)     // Automatically sets HeadlessMode = false, SlowMo = 1000
    .Build();
```

### CI/CD Configuration

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithHeadlessMode(true)  // Force headless for CI
    .WithSlowMo(0)           // No delay for speed
    .Build();
```

### Development with Custom SlowMo

```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithHeadlessMode(false) // Visible browser
    .WithSlowMo(2000)        // 2 second delay for debugging
    .Build();
```

## Environment Variables

The framework respects the `CI` environment variable for automatic headless mode determination:

- **CI Environment**: `CI=true` → Headless mode enabled
- **Development**: `CI` not set → Visible browser (unless explicitly overridden)

## Best Practices

### For Development
```csharp
// Let the framework handle it automatically
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithDebugMode(true)  // Non-headless + SlowMo for debugging
    .Build();
```

### For CI/CD
```csharp
// Explicit control for consistent behavior
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithHeadlessMode(true)  // Force headless
    .WithSlowMo(0)           // No delay for speed
    .Build();
```

### For Manual Testing
```csharp
// Custom configuration for specific needs
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithHeadlessMode(false) // Visible browser
    .WithSlowMo(1500)        // 1.5 second delay
    .Build();
```

## Migration from Previous Versions

If you were previously using explicit `HeadlessMode` settings:

### Before
```csharp
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("http://localhost:5000"),
    HeadlessMode = true  // Old property
};
```

### After
```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithBaseUrl(new Uri("http://localhost:5000"))
    .WithHeadlessMode(true)  // New nullable property
    .Build();
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
