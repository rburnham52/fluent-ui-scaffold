---
date: 2026-02-15
topic: browser-interaction-apis
---

# Browser Interaction APIs

## What We're Building

Add `ExecuteScript` and `TakeScreenshot` methods to `IUIDriver` so that BDD tests can interact with browser state (localStorage, sessionStorage) and capture screenshots for debugging. These are framework-agnostic operations supported by all major browser automation frameworks (Playwright, Selenium, Cypress).

`GetCurrentUrl` is already covered by the existing `IUIDriver.CurrentUrl` property.

## Why This Approach

### Approaches Considered

**A: Add to `PlaywrightDriver` only** - Would couple test code to Playwright and prevent portability to other drivers (e.g., Selenium).

**B: Wire up existing `PlaywrightAdvancedFeatures` into DI** - Already has async implementations but is Playwright-specific and includes many advanced features (network interception, geolocation) that aren't cross-framework.

**C: Add to `IUIDriver` (chosen)** - `ExecuteScript` and `TakeScreenshot` are genuinely cross-framework concepts. Every browser automation framework supports them. Adding to `IUIDriver` keeps test code portable while the Playwright-specific advanced features stay in `PlaywrightAdvancedFeatures`.

**D: Hybrid** - Considered but unnecessary since both features are truly framework-agnostic.

### Why C

- `ExecuteScript` maps to Playwright's `IPage.EvaluateAsync`, Selenium's `IJavaScriptExecutor.ExecuteScript`, etc.
- `TakeScreenshot` maps to Playwright's `IPage.ScreenshotAsync`, Selenium's `ITakesScreenshot.GetScreenshot`, etc.
- Test code stays portable across driver implementations
- Keeps the boundary clean: universal browser ops on `IUIDriver`, Playwright-specific features on `PlaywrightAdvancedFeatures`

## Key Decisions

- **Async API**: New methods will be async (`ExecuteScriptAsync`, `TakeScreenshotAsync`) rather than matching the current sync-over-async pattern on `IUIDriver`. This is a deliberate improvement. Existing sync methods remain unchanged for backwards compatibility.
- **No `GetCurrentUrl` method**: `IUIDriver.CurrentUrl` already exists as a property.
- **`PlaywrightAdvancedFeatures` unchanged**: Network interception, geolocation, PDF generation, etc. remain Playwright-specific and outside `IUIDriver`.
- **Documentation**: Update [api-reference.md](../api-reference.md), [playwright-integration.md](../playwright-integration.md), and [getting-started.md](../getting-started.md) with usage examples for the new methods.

## API Design

### IUIDriver additions

```csharp
// Execute JavaScript and return a typed result
Task<T> ExecuteScriptAsync<T>(string script);

// Execute JavaScript with no return value
Task ExecuteScriptAsync(string script);

// Save a screenshot to the specified file path
Task TakeScreenshotAsync(string filePath);
```

### PlaywrightDriver implementation

```csharp
public async Task<T> ExecuteScriptAsync<T>(string script)
    => await _page.EvaluateAsync<T>(script);

public async Task ExecuteScriptAsync(string script)
    => await _page.EvaluateAsync(script);

public async Task TakeScreenshotAsync(string filePath)
    => await _page.ScreenshotAsync(new PageScreenshotOptions { Path = filePath });
```

### Usage examples

```csharp
var driver = app.GetService<IUIDriver>();

// Clear browser storage between tests
await driver.ExecuteScriptAsync("localStorage.clear(); sessionStorage.clear()");

// Read a value from the browser
var href = await driver.ExecuteScriptAsync<string>("window.location.href");

// Check DOM state
var count = await driver.ExecuteScriptAsync<int>("document.querySelectorAll('h1').length");

// Capture screenshot for debugging
await driver.TakeScreenshotAsync("/tmp/debug-screenshot.png");

// Use CurrentUrl property (already exists)
var currentUrl = driver.CurrentUrl;
```

## Scope

### In scope
- Add `ExecuteScriptAsync<T>`, `ExecuteScriptAsync`, `TakeScreenshotAsync` to `IUIDriver`
- Implement in `PlaywrightDriver`
- Unit tests for new methods
- Update docs: api-reference.md, playwright-integration.md, getting-started.md

### Out of scope (documented separately)
- Refactoring `IUIDriver` to reduce the existing 19 sync methods
- The `Framework<T>(action)` escape hatch pattern
- Wiring `PlaywrightAdvancedFeatures` into DI
- See: [future-enhancement-framework-escape-hatch.md](../future-enhancement-framework-escape-hatch.md)

## Open Questions

- Should `TakeScreenshotAsync` return `byte[]` in addition to saving to file (matching `PlaywrightAdvancedFeatures`)? Or keep it simple with file-path-only for now?
- Should `ExecuteScriptAsync` accept arguments (like Playwright's `EvaluateAsync(expression, arg)`)? Or keep the initial API minimal?

## Next Steps

-> `/workflows:plan` for implementation details
