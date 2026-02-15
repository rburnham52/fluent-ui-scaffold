---
title: "feat: Add ExecuteScript and TakeScreenshot to IUIDriver"
type: feat
date: 2026-02-15
---

# feat: Add ExecuteScript and TakeScreenshot to IUIDriver

## Overview

Add `ExecuteScriptAsync<T>`, `ExecuteScriptAsync`, and `TakeScreenshotAsync` as async methods to the `IUIDriver` interface. Implement in `PlaywrightDriver`. Update mock drivers and documentation.

This unblocks BDD tests that need to interact with browser state (localStorage, sessionStorage) and capture screenshots for visual debugging.

## Problem Statement / Motivation

Tests using FluentUIScaffold need to:
1. **Clear browser storage** between test scenarios (`localStorage.clear()`)
2. **Diagnose rendering issues** by querying DOM state (`document.querySelectorAll('h1').length`)
3. **Capture screenshots** for debugging failed tests

The current API exposes element interactions (Click, Type, Verify) but not these browser-level operations. `PlaywrightAdvancedFeatures` has async implementations but isn't registered in DI and is Playwright-specific. These operations are universally supported across browser automation frameworks (Playwright, Selenium, Cypress) and belong on the framework-agnostic `IUIDriver` interface.

## Proposed Solution

Add three async methods to `IUIDriver` and implement them in `PlaywrightDriver`.

### IUIDriver Interface Changes

```csharp
// src/FluentUIScaffold.Core/Interfaces/IUIDriver.cs

/// <summary>
/// Executes JavaScript in the browser page context and returns a typed result.
/// </summary>
/// <typeparam name="T">The expected return type.</typeparam>
/// <param name="script">The JavaScript expression to evaluate.</param>
/// <returns>The result of the script evaluation, deserialized to type T.</returns>
Task<T> ExecuteScriptAsync<T>(string script);

/// <summary>
/// Executes JavaScript in the browser page context with no return value.
/// </summary>
/// <param name="script">The JavaScript expression to evaluate.</param>
Task ExecuteScriptAsync(string script);

/// <summary>
/// Saves a screenshot of the current page to the specified file path.
/// </summary>
/// <param name="filePath">The file path where the screenshot will be saved.</param>
/// <returns>The screenshot as a byte array.</returns>
Task<byte[]> TakeScreenshotAsync(string filePath);
```

### PlaywrightDriver Implementation

```csharp
// src/FluentUIScaffold.Playwright/PlaywrightDriver.cs

public async Task<T> ExecuteScriptAsync<T>(string script)
{
    if (string.IsNullOrEmpty(script))
        throw new ArgumentException("Script cannot be null or empty.", nameof(script));

    _logger?.LogDebug("Executing script with return type {Type}: {Script}", typeof(T).Name, script);
    return await _page!.EvaluateAsync<T>(script);
}

public async Task ExecuteScriptAsync(string script)
{
    if (string.IsNullOrEmpty(script))
        throw new ArgumentException("Script cannot be null or empty.", nameof(script));

    _logger?.LogDebug("Executing script: {Script}", script);
    await _page!.EvaluateAsync(script);
}

public async Task<byte[]> TakeScreenshotAsync(string filePath)
{
    if (string.IsNullOrEmpty(filePath))
        throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

    _logger?.LogDebug("Taking screenshot: {FilePath}", filePath);
    return await _page!.ScreenshotAsync(new PageScreenshotOptions { Path = filePath });
}
```

### MockUIDriver Updates

```csharp
// tests/FluentUIScaffold.Core.Tests/Mocks/MockUIDriver.cs

public Task<T> ExecuteScriptAsync<T>(string script) => Task.FromResult(default(T)!);
public Task ExecuteScriptAsync(string script) => Task.CompletedTask;
public Task<byte[]> TakeScreenshotAsync(string filePath) => Task.FromResult(Array.Empty<byte>());
```

### StatefulMockDriver Updates

```csharp
// tests/FluentUIScaffold.Core.Tests/Mocks/StatefulMockDriver.cs

private readonly Dictionary<string, Func<string, object?>> _scriptRules = new();

public Task<T> ExecuteScriptAsync<T>(string script)
{
    if (_scriptRules.TryGetValue(script, out var rule))
        return Task.FromResult((T)rule(script)!);
    return Task.FromResult(default(T)!);
}

public Task ExecuteScriptAsync(string script) => Task.CompletedTask;

public Task<byte[]> TakeScreenshotAsync(string filePath) => Task.FromResult(Array.Empty<byte>());

/// <summary>
/// Configure script execution results for testing.
/// </summary>
public void SetScriptRule(string script, Func<string, object?> rule)
{
    _scriptRules[script] = rule;
}
```

## Technical Considerations

### Breaking Change

Adding methods to `IUIDriver` is a **compile-time breaking change** for any external implementations. The following internal implementations must be updated:

| File | Action |
|------|--------|
| [MockUIDriver.cs](../../tests/FluentUIScaffold.Core.Tests/Mocks/MockUIDriver.cs) | Add no-op implementations |
| [StatefulMockDriver.cs](../../tests/FluentUIScaffold.Core.Tests/Mocks/StatefulMockDriver.cs) | Add configurable implementations |

For external consumers, this would require a **minor version bump** (since the project is pre-1.0, interface additions are expected).

### Async-First Design

These are the **first async methods on `IUIDriver`**. This is a deliberate improvement over the existing sync-over-async pattern. Existing sync methods remain unchanged for backwards compatibility.

### `TakeScreenshotAsync` Returns `byte[]`

Returning `Task<byte[]>` (not just `Task`) provides flexibility: callers can embed screenshots in test reports, do in-memory comparisons, or send to external services without reading back from disk. This matches the existing `PlaywrightAdvancedFeatures.TakeScreenshotAsync` signature.

### Error Handling

Follow the existing pattern: **let framework exceptions propagate unwrapped**. The current `PlaywrightDriver` does not catch or wrap Playwright exceptions. The new methods follow the same convention.

- Invalid JavaScript throws Playwright's evaluation exception
- Invalid file paths throw Playwright's I/O exception
- Type mismatches in `ExecuteScriptAsync<T>` throw `JsonException`

### Relationship to PlaywrightAdvancedFeatures

`PlaywrightAdvancedFeatures` continues to exist for Playwright-specific operations (network interception, geolocation, PDF generation, element screenshots). The new `IUIDriver` methods are the **framework-agnostic** API for script execution and page screenshots. Both APIs coexist without deprecation.

### Not Included (Deferred)

| Decision | Rationale |
|----------|-----------|
| `CancellationToken` parameter | Matches existing pattern (no IUIDriver method accepts one). Can be added later as an overload without breaking. |
| Script arguments (`params object[] args`) | Keep initial API minimal. String-only covers majority of use cases. |
| Screenshot options (format, full-page, quality) | Keep simple. Users needing options can use `PlaywrightAdvancedFeatures`. |
| Fluent API integration on `Page<TSelf>` | Async methods don't fit the sync fluent chain. Documented as future enhancement. |
| `_disposed` check | Pre-existing gap in all PlaywrightDriver methods. Separate cleanup task. |

## Acceptance Criteria

- [x] `await driver.ExecuteScriptAsync("localStorage.clear()")` runs JavaScript in the browser
- [x] `await driver.ExecuteScriptAsync<string>("window.location.href")` returns a typed result
- [x] `await driver.TakeScreenshotAsync("path.png")` saves a screenshot and returns the bytes
- [x] Null/empty arguments throw `ArgumentException` (not framework-specific errors)
- [x] Solution builds on all target frameworks (net6.0, net7.0, net8.0, net9.0) with no regressions
- [x] Docs show practical usage: storage clearing, DOM queries, debugging screenshots

## Implementation Checklist

Files to modify:

| # | File | Change |
|---|------|--------|
| 1 | [IUIDriver.cs](../../src/FluentUIScaffold.Core/Interfaces/IUIDriver.cs) | Add `using System.Threading.Tasks;` and 3 async method signatures with XML docs |
| 2 | [PlaywrightDriver.cs](../../src/FluentUIScaffold.Playwright/PlaywrightDriver.cs) | Implement 3 methods with validation + logging |
| 3 | [MockUIDriver.cs](../../tests/FluentUIScaffold.Core.Tests/Mocks/MockUIDriver.cs) | Add no-op implementations |
| 4 | [StatefulMockDriver.cs](../../tests/FluentUIScaffold.Core.Tests/Mocks/StatefulMockDriver.cs) | Add configurable implementations + `SetScriptRule` |
| 5 | New: `tests/FluentUIScaffold.Core.Tests/MockDriverScriptTests.cs` | Tests for `MockUIDriver` and `StatefulMockDriver` async method behavior |
| 6 | New: `tests/FluentUIScaffold.Playwright.Tests/PlaywrightDriverScriptTests.cs` | Tests for `PlaywrightDriver` input validation (`ArgumentException` for null/empty args) |
| 7 | [api-reference.md](../api-reference.md) | Document new IUIDriver methods |
| 8 | [playwright-integration.md](../playwright-integration.md) | Add script/screenshot usage examples |
| 9 | [getting-started.md](../getting-started.md) | Add "Browser Interaction" section |

## Dependencies & Risks

- **No new NuGet dependencies** — `Task` and `Task<T>` are in `System.Threading.Tasks`, available on all target frameworks
- **Low risk** — additive interface change, no modifications to existing method signatures
- **Breaking for external implementors** — anyone with a custom `IUIDriver` implementation will need to add three methods. Acceptable for a pre-1.0 library.

## References

- Brainstorm: [2026-02-15-browser-interaction-apis-brainstorm.md](../brainstorms/2026-02-15-browser-interaction-apis-brainstorm.md)
- Future enhancement: [future-enhancement-framework-escape-hatch.md](../future-enhancement-framework-escape-hatch.md)
- Playwright `IPage.EvaluateAsync`: https://playwright.dev/dotnet/docs/api/class-page#page-evaluate
- Playwright `IPage.ScreenshotAsync`: https://playwright.dev/dotnet/docs/api/class-page#page-screenshot
- Existing reference implementation: [PlaywrightAdvancedFeatures.cs](../../src/FluentUIScaffold.Playwright/PlaywrightAdvancedFeatures.cs)
