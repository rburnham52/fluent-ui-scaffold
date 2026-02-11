# Brainstorm: Make Verify Methods Wait-Aware + Consolidate Verification API

**Date:** 2026-02-12
**Status:** Ready for planning

## What We're Building

Two things, done together:

1. **Make all `Verify.*` methods wait before asserting**, so they behave consistently with how element interaction methods (`Click`, `Type`, etc.) already work.
2. **Consolidate the two parallel verification systems** into one: `VerificationContext<TSelf>` becomes the single place for all assertions. Remove the redundant `Page.VerifyText()`, `Page.VerifyValue()`, `Page.VerifyProperty()`, and `Page.WaitForVisible()`.

### The Problem

**Flaky tests from no-wait assertions:**

`Verify.Visible(p => p.Element)` performs an immediate DOM check with no waiting. If the element hasn't rendered yet (after navigation, SPA routing, async data loading), it throws `VerificationException` instantly. In contrast, `WaitForVisible(p => p.Element)` on `Page<TSelf>` properly waits up to `DefaultWaitTimeout` (30s).

Users writing `NavigateTo<SomePage>().Verify.Visible(p => p.Element)` get flaky tests, even though the pattern reads naturally and feels correct.

The gap is broader than just `Visible()` -- **every** `Verify.*` method is an instant check.

**Two parallel, inconsistent verification APIs:**

| | `page.Verify.*` (VerificationContext) | `page.Verify*()` (Page methods) |
|---|---|---|
| Text check | `Verify.TextContains(p => p.El, "text")` | `VerifyText(p => p.El, "text")` |
| Returns | `IVerificationContext<TSelf>` | `TSelf` |
| Exception | `VerificationException` | `ElementValidationException` |
| Chains back to page | via `.And` | directly |

Same concept, different names, different return types, different exception types. No clear reason for a user to prefer one over the other.

**Additional issues:**
- `Page.WaitForVisible()` becomes redundant once `Verify.Visible()` waits
- `Page.VerifyProperty()` uses fragile string-based dispatch (`propertyName.ToLower()`)
- `Page.GetElementValue<TValue>()` is generic but only supports `string` -- false generality

## Why This Approach

**Chosen: Consolidate into VerificationContext + make all methods wait-aware.**

### Alternatives Considered

1. **Add `waitFirst` parameter** -- Rejected. Leaks framework plumbing into the API.

2. **Add `WaitFor*` methods to VerificationContext** -- Rejected. Doesn't fix the pit-of-failure.

3. **Dispatch based on element's WaitStrategy** -- Rejected. Creates contradictions: `WaitStrategy.Hidden` element + `Verify.Visible()` would wait-for-hidden then assert-visible, always failing.

4. **Keep both verification systems** -- Rejected. Two APIs for the same thing, different exception types, confusing for users. The consolidation opportunity is too good to pass up.

5. **Deprecate Page methods with `[Obsolete]`** -- Rejected. Adds transition complexity. Clean break is simpler since this is already a behavioral change.

### Why This Approach Wins

- **One verification system**: `page.Verify.*` is the single entry point for all assertions
- **Pit of success**: The natural API does the right thing by default
- **Consistent exceptions**: Everything throws `VerificationException`
- **Smaller API surface**: Remove redundant Page methods, no new parameters

## Key Decisions

### 1. The Verify method determines the wait target, not the element's WaitStrategy

- `Verify.Visible()` always calls `element.WaitForVisible()`
- `Verify.NotVisible()` always calls `element.WaitForHidden()`
- `Verify.TextContains()` / `Verify.TextIs()` wait for visibility, then poll for expected text

This avoids contradictions (e.g., `WaitStrategy.Hidden` element + `Verify.Visible()`) and provides predictable behavior. Elements with `WaitStrategy.None` still wait — the Verify method drives waiting, not the element's interaction strategy.

### 2. TextContains and TextIs poll for the expected text

`Verify.TextContains(p => p.Element, "Hello")` will:
1. Wait for the element to be visible
2. Poll `GetText()` until it contains the expected string, or timeout

This handles async data loading where the element renders immediately with placeholder text.

### 3. Non-element Verify methods get polling

`TitleContains`, `TitleIs`, `UrlContains`, `UrlIs` will use a polling loop with `DefaultWaitTimeout`.

**Polling specification:**
- **Poll interval**: 100ms (matches existing conventions)
- **Timing**: `Thread.Sleep` (matches the synchronous API)
- **Location**: Private helper method within `VerificationContext`

### 4. All failures throw VerificationException

Exceptions from the underlying driver will be caught and wrapped in `VerificationException` with descriptive context (selector, expected condition, timeout duration).

**Exception types to catch:** `TimeoutException`, `ElementTimeoutException`, `AggregateException` (with inner exception inspection), and framework-specific exceptions.

### 5. New methods added to VerificationContext

| Method | Replaces | Behavior |
|---|---|---|
| `TextIs(selector, text)` | `Page.VerifyText()` | Wait for visible, poll until exact text match or timeout |
| `HasAttribute(selector, name, value)` | `Page.VerifyProperty()` | Wait for visible, then check attribute value |

These follow the existing naming pattern (`TitleIs`, `UrlIs` → `TextIs`).

### 6. Remove redundant Page methods

**Remove from `Page<TSelf>`:**
- `VerifyText()` → replaced by `Verify.TextIs()` / `Verify.TextContains()`
- `VerifyValue<TValue>()` → replaced by `Verify.TextIs()` (the generic only supported string anyway)
- `VerifyProperty()` → replaced by `Verify.HasAttribute()` / `Verify.Visible()` etc.
- `WaitForVisible()` → replaced by `Verify.Visible()`

**Also remove supporting methods that become dead code:**
- `GetElementValue<TValue>()`
- `GetElementPropertyValue()`

**Keep on `Page<TSelf>`:**
- `WaitForElement()` — waits using the element's configured WaitStrategy without asserting. Still useful as a synchronization primitive (e.g., wait for an element to be "ready" before a complex interaction sequence).
- `WaitForHidden()` — kept for symmetry with WaitForElement, and for cases where you want to wait-without-asserting.

### 7. Consolidated API surface

After changes, the complete verification API is:

```csharp
// All through page.Verify.*
page.Verify
    .Visible(p => p.Element)           // wait for visible, assert visible
    .NotVisible(p => p.Spinner)        // wait for hidden, assert not visible
    .TextIs(p => p.Title, "Hello")     // wait, poll for exact text
    .TextContains(p => p.Body, "world") // wait, poll for substring
    .HasAttribute(p => p.Btn, "class", "active") // wait, check attribute
    .TitleIs("Dashboard")              // poll title
    .TitleContains("Dash")             // poll title substring
    .UrlIs("http://localhost/dash")    // poll URL
    .UrlContains("/dash")              // poll URL substring
    .And                               // return to page
    .Click(p => p.NextButton);
```

## Scope of Changes

### Files to modify:
- `src/FluentUIScaffold.Core/Configuration/VerificationContext.cs` — Add waiting to all methods, add `TextIs`, `HasAttribute`
- `src/FluentUIScaffold.Core/Interfaces/IVerificationContext.cs` — Add `TextIs`, `HasAttribute` to interface
- `src/FluentUIScaffold.Core/Pages/Page.cs` — Remove `VerifyText`, `VerifyValue`, `VerifyProperty`, `WaitForVisible`, `GetElementValue`, `GetElementPropertyValue`
- `tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs` — Update for waiting behavior, add new method tests
- `tests/FluentUIScaffold.Core.Tests/VerificationContextV2Tests.cs` — Update for waiting behavior
- `tests/FluentUIScaffold.Core.Tests/Pages/PageTests.cs` — Remove tests for deleted methods

### Breaking changes:
- `Page.VerifyText()`, `VerifyValue()`, `VerifyProperty()` — removed (use `Verify.TextIs()`, `Verify.HasAttribute()`)
- `Page.WaitForVisible()` — removed (use `Verify.Visible()`)
- `ElementValidationException` no longer thrown from verification (now `VerificationException`)
- All `Verify.*` methods now wait before asserting (slower failure path on timeout)

## Known Limitations

- **TOCTOU window**: After waiting completes, there is a brief window where state could change before the assertion runs. For atomic wait-and-assert, users can access Playwright's native `Expect` API via `GetFrameworkDriver<IPage>()`.
- **Not thread-safe**: `VerificationContext` should not be used concurrently on the same page instance. This aligns with the existing single-threaded test execution model.
- **Slower failure path**: Tests that previously failed instantly will now wait up to `DefaultWaitTimeout` before failing. Set shorter timeouts in test configuration if fast failure is desired.

## Testing Strategy

- Update existing tests to account for wait calls on mock drivers
- Add state-transitioning mock tests (element starts invisible, becomes visible after delay) to prove waiting works
- Add timeout tests (element never appears) to verify `VerificationException` is thrown within expected time
- Add tests for new `TextIs` and `HasAttribute` methods
- Use short `DefaultWaitTimeout` (e.g., 1 second) in unit tests to avoid slow test runs

## Open Questions

- Should there be a way to opt out of waiting for specific assertions (e.g., a `NoWait` fluent modifier)? Deferred — can add later if needed (YAGNI).
- Should `Verify.Enabled()`, `Verify.Disabled()`, `Verify.Exists()` be added? Deferred — separate enhancement to avoid scope creep.
