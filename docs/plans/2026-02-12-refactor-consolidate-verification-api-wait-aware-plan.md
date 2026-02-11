---
title: Consolidate Verification API and Make All Assertions Wait-Aware
type: refactor
date: 2026-02-12
---

# Consolidate Verification API and Make All Assertions Wait-Aware

## Overview

**Two-part refactor** to eliminate test flakiness and API inconsistency in FluentUIScaffold's verification system:

1. **Make all `Verify.*` methods wait-aware** - Replace instant DOM checks with wait-before-assert behavior
2. **Consolidate two parallel verification systems** - Merge `Page.VerifyText()`, `VerifyValue()`, `VerifyProperty()` into `VerificationContext` as the single verification API

**Why now**: Users writing `NavigateTo<SomePage>().Verify.Visible(p => p.Element)` get flaky tests because the natural-reading pattern does no waiting. The dual verification APIs (`page.Verify.*` vs `page.VerifyText()`) create confusion with different exception types and return values.

**Source**: [Brainstorm Document](../brainstorms/2026-02-12-verify-wait-strategy-brainstorm.md)

## Problem Statement

### The Pit of Failure: No-Wait Assertions

Every `Verify.*` method performs instant checks with zero waiting:

```csharp
// Current behavior - throws VerificationException immediately if element hasn't rendered
NavigateTo<DashboardPage>().Verify.Visible(p => p.WelcomeMessage);

// Developers expect this to work, but it's racy - element might render 100ms later
```

**Root cause**: `VerificationContext.Visible()` calls `_driver.IsVisible(selector)` directly - a single boolean check with no timeout, no polling, no retry.

Meanwhile, `Page.WaitForVisible()` properly waits up to `DefaultWaitTimeout` (30s). Users must know to call:

```csharp
page.WaitForVisible(p => p.Element).Verify.Visible(p => p.Element); // Redundant!
```

### Two Parallel, Inconsistent Verification APIs

| Aspect | `page.Verify.*` (VerificationContext) | `page.VerifyText()` (Page methods) |
|--------|---------------------------------------|-----------------------------------|
| **Text check** | `Verify.TextContains(p => p.El, "text")` | `VerifyText(p => p.El, "text")` |
| **Returns** | `IVerificationContext<TSelf>` | `TSelf` |
| **Exception** | `VerificationException` | `ElementValidationException` |
| **Chains back** | via `.And` | directly |

Same concept, different names, different return types, different exceptions. No clear reason to prefer one over the other.

**Additional issues**:
- `Page.VerifyProperty()` uses fragile string-based dispatch (`propertyName.ToLower()` switch)
- `Page.GetElementValue<TValue>()` is generic but only supports `string` - false generality
- `Page.WaitForVisible()` becomes redundant once `Verify.Visible()` waits

**Evidence from codebase**:
- [RegistrationPage.cs:142-178](../../samples/SampleApp.Tests/Pages/RegistrationPage.cs#L142-L178) uses `WaitForVisible()`, `VerifyText()`, `VerifyValue()` extensively - all candidates for consolidation

## Proposed Solution

### Core Principles

1. **Verify method determines wait target, not element's WaitStrategy**
   - `Verify.Visible()` always calls `element.WaitForVisible()` (ignores element's configured `WaitStrategy.None` or `WaitStrategy.Hidden`)
   - `Verify.NotVisible()` always calls `element.WaitForHidden()`
   - Avoids contradictions (e.g., element with `WaitStrategy.Hidden` + `Verify.Visible()`)

2. **Poll for dynamic conditions**
   - `Verify.TextContains()` / `Verify.TextIs()` wait for visibility, then poll `GetText()` until match or timeout
   - `Verify.TitleContains()` / `Verify.UrlContains()` poll page state (SPA navigation is async)
   - Poll interval: 100ms (matches existing `PlaywrightWaitStrategy` conventions)

3. **Single verification system**
   - `page.Verify.*` is the **only** verification API
   - Remove `Page.VerifyText()`, `VerifyValue()`, `VerifyProperty()`, `WaitForVisible()`
   - All verification failures throw `VerificationException` (consistent exception type)

4. **Preserve synchronization primitives**
   - Keep `Page.WaitForElement()` and `WaitForHidden()` as wait-without-asserting utilities
   - Useful for synchronization before complex interactions (e.g., wait for animation to complete)

### Consolidated API Surface

After changes, the complete verification API:

```csharp
page.Verify
    .Visible(p => p.Element)                         // wait for visible, assert visible
    .NotVisible(p => p.Spinner)                      // wait for hidden, assert not visible
    .TextIs(p => p.Title, "Hello")                   // NEW: wait, poll for exact text
    .TextContains(p => p.Body, "world")              // wait, poll for substring
    .HasAttribute(p => p.Btn, "class", "active")     // NEW: wait, check attribute
    .TitleIs("Dashboard")                            // poll title
    .TitleContains("Dash")                           // poll title substring
    .UrlIs("http://localhost/dash")                  // poll URL
    .UrlContains("/dash")                            // poll URL substring
    .And                                             // return to page
    .Click(p => p.NextButton);
```

## Technical Approach

### Architecture

**Key decision**: Implement polling in `VerificationContext` rather than adding to `IUIDriver` to keep the driver interface minimal and framework-agnostic.

```
┌─────────────────────────────────────────────┐
│        VerificationContext<TPage>           │
│  - Owns polling logic (100ms interval)      │
│  - Wraps all exceptions → VerificationExc   │
│  - Uses DefaultWaitTimeout from options     │
└──────────────────┬──────────────────────────┘
                   │
                   ├─ element.WaitForVisible()
                   ├─ element.WaitForHidden()
                   ├─ _driver.GetText(selector)  ─┐
                   ├─ _driver.GetAttribute(...)   ├─ polled in while loop
                   ├─ _driver.GetPageTitle()      │
                   └─ _driver.CurrentUrl          ┘

┌─────────────────────────────────────────────┐
│              IUIDriver                      │
│  NO CHANGES - existing wait methods used   │
│  - WaitForElementToBeVisible(selector)      │
│  - WaitForElementToBeHidden(selector)       │
│  - IsVisible(selector)                      │
│  - GetText(selector)                        │
│  - GetAttribute(selector, name)             │
│  - GetPageTitle()                           │
│  - CurrentUrl { get; }                      │
└─────────────────────────────────────────────┘
```

**Critical decision**: Do NOT add timeout parameters to `IUIDriver` methods. Element timeout is already handled by `Element.WaitForVisible()` calling driver with configured timeout. Polling timeout uses `_options.DefaultWaitTimeout`.

### Implementation Phases

#### Phase 1: Foundation - Add Waiting to VerificationContext (Breaking Changes)

**Goal**: Make all existing `Verify.*` methods wait-aware

**Files modified**:
- [src/FluentUIScaffold.Core/Configuration/VerificationContext.cs:36-112](../../src/FluentUIScaffold.Core/Configuration/VerificationContext.cs#L36-L112)
- [src/FluentUIScaffold.Core/Interfaces/IVerificationContext.cs:17-47](../../src/FluentUIScaffold.Core/Interfaces/IVerificationContext.cs#L17-L47)

**Changes to `VerificationContext<TPage>`**:

##### 1.1 Add Private Polling Helper

```csharp
// VerificationContext.cs - new method
private void PollUntil(Func<bool> condition, string errorMessage, string description)
{
    _logger.LogInformation($"Polling for condition: {description}");
    var startTime = DateTime.UtcNow;
    while (DateTime.UtcNow - startTime < _options.DefaultWaitTimeout)
    {
        try
        {
            if (condition())
            {
                _logger.LogInformation($"Condition met: {description}");
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Polling condition check threw exception: {ex.Message}");
            // Continue polling - element might not exist yet
        }
        System.Threading.Thread.Sleep(100); // 100ms interval
    }

    throw new VerificationException(errorMessage);
}
```

##### 1.2 Update `Visible()`

```csharp
// VerificationContext.cs:92-101 - BEFORE
public IVerificationContext<TPage> Visible(Func<TPage, IElement> elementSelector)
{
    var element = elementSelector(_page);
    _logger.LogInformation($"Verifying element '{element.Selector}' is visible");
    if (!_driver.IsVisible(element.Selector))
    {
        throw new VerificationException($"Element '{element.Selector}' is not visible");
    }
    return this;
}

// VerificationContext.cs:92-106 - AFTER
public IVerificationContext<TPage> Visible(Func<TPage, IElement> elementSelector)
{
    var element = elementSelector(_page);
    _logger.LogInformation($"Verifying element '{element.Selector}' is visible");

    try
    {
        // Verify method determines wait target - always wait for visible
        element.WaitForVisible();

        // After wait, assert visible (TOCTOU window acceptable - see Known Limitations)
        if (!_driver.IsVisible(element.Selector))
        {
            throw new VerificationException($"Element '{element.Selector}' became invisible after wait completed");
        }
    }
    catch (Exception ex) when (!(ex is VerificationException))
    {
        throw new VerificationException(
            $"Element '{element.Selector}' did not become visible within {_options.DefaultWaitTimeout.TotalSeconds}s",
            ex);
    }

    return this;
}
```

##### 1.3 Update `NotVisible()`

```csharp
// VerificationContext.cs:103-112 - AFTER
public IVerificationContext<TPage> NotVisible(Func<TPage, IElement> elementSelector)
{
    var element = elementSelector(_page);
    _logger.LogInformation($"Verifying element '{element.Selector}' is not visible");

    try
    {
        // Wait for element to become hidden (or never exist)
        element.WaitForHidden();

        // After wait, assert not visible
        if (_driver.IsVisible(element.Selector))
        {
            throw new VerificationException($"Element '{element.Selector}' is still visible");
        }
    }
    catch (Exception ex) when (!(ex is VerificationException))
    {
        throw new VerificationException(
            $"Element '{element.Selector}' did not become hidden within {_options.DefaultWaitTimeout.TotalSeconds}s",
            ex);
    }

    return this;
}
```

**Note**: `NotVisible()` treats non-existent elements as "not visible" by calling `WaitForHidden()` which should succeed for elements that don't exist in the DOM.

##### 1.4 Update `TextContains()` with Polling

```csharp
// VerificationContext.cs:80-90 - AFTER
public IVerificationContext<TPage> TextContains(Func<TPage, IElement> elementSelector, string contains)
{
    var element = elementSelector(_page);
    _logger.LogInformation($"Verifying element '{element.Selector}' contains text '{contains}'");

    try
    {
        // First, wait for element to be visible
        element.WaitForVisible();

        // Then poll for text to contain expected value
        PollUntil(
            condition: () => _driver.GetText(element.Selector).Contains(contains, StringComparison.Ordinal),
            errorMessage: $"Element '{element.Selector}' text never contained '{contains}' within {_options.DefaultWaitTimeout.TotalSeconds}s",
            description: $"Text contains '{contains}'"
        );
    }
    catch (Exception ex) when (!(ex is VerificationException))
    {
        throw new VerificationException(
            $"Failed to verify text contains '{contains}' for element '{element.Selector}'",
            ex);
    }

    return this;
}
```

**Note**: Full `DefaultWaitTimeout` applies to each phase (visibility + polling) to avoid premature timeout when elements render slowly with placeholder text. This means worst-case is 60s (30s for visibility + 30s for polling), which is acceptable for E2E tests.

##### 1.5 Update Title/URL Methods with Polling

```csharp
// VerificationContext.cs:69-78 - AFTER (TitleContains example)
public IVerificationContext<TPage> TitleContains(string text)
{
    _logger.LogInformation($"Verifying title contains '{text}'");

    PollUntil(
        condition: () => _driver.GetPageTitle().Contains(text, StringComparison.Ordinal),
        errorMessage: $"Page title never contained '{text}' within {_options.DefaultWaitTimeout.TotalSeconds}s. Last title: '{_driver.GetPageTitle()}'",
        description: $"Title contains '{text}'"
    );

    return this;
}

// Similar updates for: TitleIs(), UrlContains(), UrlIs()
```

##### 1.6 Exception Wrapping Contract

**All `Verify.*` methods follow this pattern**:

```csharp
try {
    // Wait and/or poll
} catch (Exception ex) when (!(ex is VerificationException)) {
    throw new VerificationException(
        $"[Descriptive message with selector, condition, timeout]",
        ex
    );
}
```

**Exception types to catch**:
- `TimeoutException` - from driver wait methods
- `ElementTimeoutException` - from Element wait methods
- `AggregateException` - unwrap and inspect inner exception
- `InvalidOperationException`, `ArgumentException` - from driver methods
- Any other `Exception` - catch-all to ensure consistent VerificationException

**Do NOT catch `VerificationException`** - let it propagate as-is to avoid double-wrapping.

#### Phase 2: Expansion - Add New Methods and Remove Old Ones

**Goal**: Add `TextIs()` and `HasAttribute()` to VerificationContext, remove redundant Page methods

**Files modified**:
- [src/FluentUIScaffold.Core/Configuration/VerificationContext.cs](../../src/FluentUIScaffold.Core/Configuration/VerificationContext.cs)
- [src/FluentUIScaffold.Core/Interfaces/IVerificationContext.cs](../../src/FluentUIScaffold.Core/Interfaces/IVerificationContext.cs)
- [src/FluentUIScaffold.Core/Pages/Page.cs:141-202](../../src/FluentUIScaffold.Core/Pages/Page.cs#L141-L202)

##### 2.1 Add `TextIs()` to VerificationContext

```csharp
// IVerificationContext.cs - add to interface
/// <summary>
/// Verifies that the element's text exactly matches the specified text.
/// </summary>
IVerificationContext<TPage> TextIs(Func<TPage, IElement> elementSelector, string expectedText);

// VerificationContext.cs - implement
public IVerificationContext<TPage> TextIs(Func<TPage, IElement> elementSelector, string expectedText)
{
    var element = elementSelector(_page);
    _logger.LogInformation($"Verifying element '{element.Selector}' text is exactly '{expectedText}'");

    try
    {
        element.WaitForVisible();

        // Poll for exact text match (case-sensitive, no trimming - matches current TextContains behavior)
        PollUntil(
            condition: () => string.Equals(_driver.GetText(element.Selector), expectedText, StringComparison.Ordinal),
            errorMessage: $"Element '{element.Selector}' text never matched '{expectedText}' within {_options.DefaultWaitTimeout.TotalSeconds}s. Last text: '{_driver.GetText(element.Selector)}'",
            description: $"Text is '{expectedText}'"
        );
    }
    catch (Exception ex) when (!(ex is VerificationException))
    {
        throw new VerificationException(
            $"Failed to verify text is '{expectedText}' for element '{element.Selector}'",
            ex);
    }

    return this;
}
```

**Note**: `TextIs()` uses case-sensitive exact match with no trimming (matches existing `TextContains` behavior for consistency). Uses `driver.GetText()` which returns whatever the driver provides (Playwright's `textContent`).

##### 2.2 Add `HasAttribute()` to VerificationContext

```csharp
// IVerificationContext.cs - add to interface
/// <summary>
/// Verifies that the element has the specified attribute with the expected value.
/// </summary>
IVerificationContext<TPage> HasAttribute(Func<TPage, IElement> elementSelector, string attributeName, string expectedValue);

// VerificationContext.cs - implement
public IVerificationContext<TPage> HasAttribute(Func<TPage, IElement> elementSelector, string attributeName, string expectedValue)
{
    var element = elementSelector(_page);
    _logger.LogInformation($"Verifying element '{element.Selector}' has attribute '{attributeName}' with value '{expectedValue}'");

    try
    {
        element.WaitForVisible();

        // Check attribute value (exact match for consistency)
        var actualValue = _driver.GetAttribute(element.Selector, attributeName) ?? string.Empty;
        if (!string.Equals(actualValue, expectedValue, StringComparison.Ordinal))
        {
            throw new VerificationException(
                $"Element '{element.Selector}' attribute '{attributeName}' expected '{expectedValue}' but was '{actualValue}'"
            );
        }
    }
    catch (Exception ex) when (!(ex is VerificationException))
    {
        throw new VerificationException(
            $"Failed to verify attribute '{attributeName}' for element '{element.Selector}'",
            ex);
    }

    return this;
}
```

**Note**: `HasAttribute()` uses exact match (not substring). For CSS class checking where classes can be multiple values separated by spaces, consider adding `HasAttributeContaining()` in a future enhancement if needed (YAGNI for now).

Parameter order is `(elementSelector, attributeName, expectedValue)` following the natural reading order.

##### 2.3 Remove Page Methods

**Delete from `Page.cs`**:
- `VerifyValue<TValue>()` - lines 165-177
- `VerifyText()` - lines 182-185
- `VerifyProperty()` - lines 190-202
- `WaitForVisible()` - lines 141-146
- `GetElementValue<TValue>()` - lines 350-358 (dead code)
- `GetElementPropertyValue()` - lines 363-382 (dead code)

**Keep (do NOT remove)**:
- `WaitForElement()` - lines 131-136 (wait using element's WaitStrategy, no assertion)
- `WaitForHidden()` - lines 148-156 (wait for hidden, no assertion)

#### Phase 3: Test Updates and Migration

**Goal**: Update all tests to use new wait-aware API, create migration guide

**Files modified**:
- [tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs](../../tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs)
- [tests/FluentUIScaffold.Core.Tests/VerificationContextV2Tests.cs](../../tests/FluentUIScaffold.Core.Tests/VerificationContextV2Tests.cs)
- [tests/FluentUIScaffold.Core.Tests/Pages/PageTests.cs:119-131](../../tests/FluentUIScaffold.Core.Tests/Pages/PageTests.cs#L119-L131)
- [samples/SampleApp.Tests/Pages/RegistrationPage.cs:142-178](../../samples/SampleApp.Tests/Pages/RegistrationPage.cs#L142-L178)

##### 3.1 Create StatefulMockDriver for Testing Wait Behavior

**New file**: `tests/FluentUIScaffold.Core.Tests/Mocks/StatefulMockDriver.cs`

Create a mock driver that simulates time-based state transitions for testing wait/poll behavior.

**Key capabilities**:
- `WithElement(selector, visible, text)` - Initialize element state
- `TransitionTo(selector, visible, text, afterMs)` - Schedule state change after delay
- `ApplyTransitions()` - Auto-apply transitions based on elapsed time
- Implements `IUIDriver` with time-based behavior

**Usage example**:
```csharp
var driver = new StatefulMockDriver()
    .WithElement("#button", visible: false)
    .TransitionTo("#button", visible: true, afterMs: 200);

// IsVisible("#button") returns false initially, true after 200ms
```

##### 3.2 Add Wait Behavior Tests

**New tests in `VerificationContextTests.cs`**:

```csharp
[TestMethod]
public void Visible_WaitsForElement_ThenAsserts()
{
    // Arrange: Element starts invisible, becomes visible after 200ms
    var driver = new StatefulMockDriver()
        .WithElement("#button", visible: false)
        .TransitionTo("#button", visible: true, afterMs: 200);

    var options = new FluentUIScaffoldOptions { DefaultWaitTimeout = TimeSpan.FromSeconds(1) };
    var page = new TestPage(driver, options);

    // Act & Assert: Should succeed after waiting
    page.Verify.Visible(p => p.Button);
}

[TestMethod]
public void TextContains_PollsForText_UntilMatch()
{
    // Arrange: Element visible with placeholder, text updates after 300ms
    var driver = new StatefulMockDriver()
        .WithElement("#message", visible: true, text: "Loading...")
        .TransitionTo("#message", text: "Success!", afterMs: 300);

    var options = new FluentUIScaffoldOptions { DefaultWaitTimeout = TimeSpan.FromSeconds(1) };
    var page = new TestPage(driver, options);

    // Act & Assert: Should succeed after polling
    page.Verify.TextContains(p => p.Message, "Success");
}

[TestMethod]
public void Visible_ThrowsVerificationException_WhenTimeout()
{
    // Arrange: Element never becomes visible
    var driver = new StatefulMockDriver()
        .WithElement("#button", visible: false);

    var options = new FluentUIScaffoldOptions { DefaultWaitTimeout = TimeSpan.FromMilliseconds(500) };
    var page = new TestPage(driver, options);

    // Act & Assert
    var ex = Assert.ThrowsException<VerificationException>(() =>
        page.Verify.Visible(p => p.Button)
    );
    Assert.IsTrue(ex.Message.Contains("#button"));
    Assert.IsTrue(ex.Message.Contains("did not become visible"));
}
```

**Update existing tests**: Replace `DriverStub` expectations to account for wait calls. Example:

```csharp
// BEFORE
[TestMethod]
public void ElementIsVisible_Positive()
{
    var driver = new DriverStub(visible: true);
    var page = new TestPage(driver);

    page.Verify.Visible(p => p.TestElement); // Instant check
}

// AFTER
[TestMethod]
public void ElementIsVisible_Positive()
{
    var driver = new StatefulMockDriver()
        .WithElement("#test", visible: true);
    var options = new FluentUIScaffoldOptions { DefaultWaitTimeout = TimeSpan.FromMilliseconds(100) };
    var page = new TestPage(driver, options);

    page.Verify.Visible(p => p.TestElement); // Waits before asserting
}
```

**Note**: Use short timeout (100ms-1s) in test options to keep tests fast.

##### 3.3 Update Sample Code (RegistrationPage.cs)

**Migration patterns**:

```csharp
// BEFORE (lines 142-143)
WaitForVisible(e => e.SuccessMessage)
    .VerifyText(e => e.SuccessMessage, "Registration successful!");

// AFTER
Verify
    .Visible(e => e.SuccessMessage)
    .TextIs(e => e.SuccessMessage, "Registration successful!")
    .And;  // Return to page if needed

// BEFORE (lines 160-163)
VerifyValue(e => e.NameField, "John")
    .VerifyValue(e => e.EmailField, "john@example.com");

// AFTER
Verify
    .TextIs(e => e.NameField, "John")
    .TextIs(e => e.EmailField, "john@example.com")
    .And;

// BEFORE (using WaitForVisible for synchronization)
WaitForVisible(e => e.LoadingSpinner)
    .WaitForHidden(e => e.LoadingSpinner);

// AFTER (same - these methods are kept)
WaitForVisible(e => e.LoadingSpinner)
    .WaitForHidden(e => e.LoadingSpinner);

// OR use Verify.NotVisible (combines wait + assert)
Verify.NotVisible(e => e.LoadingSpinner);
```

##### 3.4 Remove Obsolete Tests

**Delete from `PageTests.cs`**:
- `WaitForVisible_CallsElementWaitForVisible` - lines 119-131 (method removed)
- Any tests for `VerifyText`, `VerifyValue`, `VerifyProperty` (methods removed)

## Acceptance Criteria

### Functional Requirements

#### Element-Based Verifications

- [ ] `Verify.Visible(p => p.Element)` waits for element via `element.WaitForVisible()` before asserting
- [ ] `Verify.Visible()` succeeds immediately if element already visible
- [ ] `Verify.Visible()` succeeds after wait if element becomes visible within timeout
- [ ] `Verify.Visible()` throws `VerificationException` if element never appears within `DefaultWaitTimeout`
- [ ] `Verify.Visible()` works regardless of element's configured `WaitStrategy` (method drives waiting, not element)
- [ ] `Verify.NotVisible(p => p.Element)` waits via `element.WaitForHidden()` then asserts not visible
- [ ] `Verify.NotVisible()` treats non-existent elements as "not visible" (succeeds)
- [ ] `Verify.NotVisible()` throws `VerificationException` if element remains visible after timeout

#### Text Verification with Polling

- [ ] `Verify.TextContains(p => p.El, "expected")` waits for visible, then polls `GetText()` until match
- [ ] `Verify.TextContains()` succeeds immediately if text already contains substring
- [ ] `Verify.TextContains()` succeeds after polling if text updates to contain substring
- [ ] `Verify.TextContains()` throws `VerificationException` if text never matches within timeout
- [ ] `Verify.TextContains()` includes last actual text in error message
- [ ] `Verify.TextIs(p => p.El, "exact")` waits for visible, then polls for exact match
- [ ] `Verify.TextIs()` uses case-sensitive comparison with no whitespace trimming
- [ ] `Verify.TextIs()` throws `VerificationException` with last actual text if never matches

#### Attribute Verification

- [ ] `Verify.HasAttribute(p => p.El, "class", "active")` waits for visible, then checks attribute
- [ ] `Verify.HasAttribute()` uses exact value match (not substring)
- [ ] `Verify.HasAttribute()` throws `VerificationException` with actual attribute value if mismatch
- [ ] `Verify.HasAttribute()` handles missing attributes (empty string)

#### Page-Level Verifications (Polling)

- [ ] `Verify.TitleContains("text")` polls `GetPageTitle()` every 100ms until match
- [ ] `Verify.TitleIs("exact")` polls for exact title match
- [ ] `Verify.UrlContains("/path")` polls `CurrentUrl` until match
- [ ] `Verify.UrlIs("http://...")` polls for exact URL match
- [ ] Title/URL polling throws `VerificationException` with last value if timeout
- [ ] Title/URL polling includes timeout duration in error message

#### Exception Handling

- [ ] All `Verify.*` methods throw `VerificationException` (never `ElementValidationException`)
- [ ] `VerificationException` message includes: selector (if applicable), expected condition, timeout duration
- [ ] `VerificationException` wraps inner exceptions (TimeoutException, ElementTimeoutException, etc.)
- [ ] `VerificationException` not double-wrapped (catches `when (!(ex is VerificationException))`)
- [ ] Error messages include last actual value for text/title/URL failures

#### Polling Behavior

- [ ] Polling interval is 100ms (matches existing `PlaywrightWaitStrategy` convention)
- [ ] Polling uses `Thread.Sleep(100)` (synchronous API)
- [ ] Polling timeout is `_options.DefaultWaitTimeout` (full timeout for each phase)
- [ ] Visibility wait + text polling can take up to 2× `DefaultWaitTimeout` (acceptable for E2E tests)
- [ ] Polling continues even if intermediate `GetText()` calls throw exceptions (element might not exist yet)

#### API Consolidation

- [ ] `Page.VerifyText()` removed (compilation error)
- [ ] `Page.VerifyValue()` removed (compilation error)
- [ ] `Page.VerifyProperty()` removed (compilation error)
- [ ] `Page.WaitForVisible()` removed (compilation error)
- [ ] `Page.GetElementValue<TValue>()` removed (dead code)
- [ ] `Page.GetElementPropertyValue()` removed (dead code)
- [ ] `Page.WaitForElement()` **kept** (synchronization primitive)
- [ ] `Page.WaitForHidden()` **kept** (synchronization primitive)
- [ ] `IVerificationContext<TPage>` has `TextIs()` method
- [ ] `IVerificationContext<TPage>` has `HasAttribute()` method

#### Chaining Behavior

- [ ] All `Verify.*` methods return `IVerificationContext<TPage>` for chaining
- [ ] `Verify.And` returns `TPage` for continued page interactions
- [ ] Chaining works: `Verify.Visible(...).TextIs(...).And.Click(...)`

### Non-Functional Requirements

#### Performance

- [ ] Unit tests use short `DefaultWaitTimeout` (100ms-1s) to avoid slow test runs
- [ ] Negative assertions fail within configured timeout (not instant, but not hanging)
- [ ] Test suite overhead from waiting is acceptable (<10% increase in total duration)
- [ ] Poll interval (100ms) is fast enough for typical E2E scenarios

#### Backward Compatibility (Breaking Changes)

- [ ] Migration guide documents all removed Page methods
- [ ] Migration guide shows before/after examples for common patterns
- [ ] Sample code (RegistrationPage.cs) updated to demonstrate new API
- [ ] All sample tests pass with new API
- [ ] Exception type change documented (`ElementValidationException` → `VerificationException`)

### Quality Gates

#### Test Coverage

- [ ] All existing VerificationContext tests updated to account for wait behavior
- [ ] New tests added for state transitions (invisible → visible, placeholder → final text)
- [ ] New tests added for timeout scenarios
- [ ] New tests for `TextIs()` and `HasAttribute()` methods
- [ ] StatefulMockDriver created and used in tests
- [ ] Sample tests (RegistrationPage) all pass after migration
- [ ] No test uses removed Page methods (VerifyText, VerifyValue, VerifyProperty, WaitForVisible)

#### Code Quality

- [ ] All `Verify.*` methods use consistent exception wrapping pattern
- [ ] All `Verify.*` methods log polling/wait activity
- [ ] Polling helper method (`PollUntil`) is reused across text/title/URL verifications
- [ ] No code duplication in verification implementations
- [ ] Clear error messages with actionable information (selector, condition, timeout, last value)

#### Documentation

- [ ] Known Limitations section updated in README or docs (TOCTOU window, thread safety, slower failure path)
- [ ] Migration guide created showing before/after patterns
- [ ] RegistrationPage.cs serves as migration example
- [ ] XML comments updated for all public methods
- [ ] Breaking changes listed in CHANGELOG

## Alternative Approaches Considered

### 1. Add `waitFirst` Parameter (Rejected)

```csharp
Verify.Visible(p => p.Element, waitFirst: true)
```

**Why rejected**: Leaks framework plumbing into API. If default is `true`, parameter is rarely used. If `false`, the pit-of-failure isn't fixed.

### 2. Add `WaitFor*` Methods to VerificationContext (Rejected)

```csharp
Verify.WaitForVisible(p => p.Element).Visible(p => p.Element)
```

**Why rejected**: Users still need to know which method to use. More API surface without making the default safe.

### 3. Dispatch Based on Element's WaitStrategy (Rejected)

```csharp
// If element has WaitStrategy.Hidden, call element.WaitFor() which waits for hidden
Verify.Visible(p => p.Element) // Then assert visible → always fails!
```

**Why rejected**: Creates contradictions. The Verify method's intent must override element's interaction strategy.

### 4. Keep Both Verification Systems (Rejected)

**Why rejected**: Two APIs for the same concept, different exception types, confusing for users. Consolidation opportunity is too good to pass up.

### 5. Deprecate Page Methods with `[Obsolete]` (Rejected)

```csharp
[Obsolete("Use Verify.TextIs instead")]
public TSelf VerifyText(...) { ... }
```

**Why rejected**: Adds transition complexity and leaves dead code. Clean break is simpler since this is already a behavioral change.

### 6. Add Timeout Parameters to IUIDriver (Rejected)

```csharp
void WaitForElementToBeVisible(string selector, TimeSpan timeout);
```

**Why rejected**: Element timeout is already handled by `Element.WaitForVisible()`. Polling timeout uses `DefaultWaitTimeout`. Adding timeout parameters bloats the interface without adding value.

### 7. Add `WaitForText()` to IUIDriver (Rejected)

```csharp
void WaitForElementText(string selector, string expectedText, TimeSpan timeout);
```

**Why rejected**: Keeps VerificationContext framework-agnostic by implementing polling in VerificationContext itself. Avoids bloating IUIDriver with verification-specific methods.

## Dependencies & Prerequisites

### Code Dependencies

- No new external dependencies
- `FluentUIScaffold.Core` projects (existing)
- `FluentUIScaffold.Playwright` projects (existing)
- Test frameworks: MSTest (existing)

### Technical Prerequisites

- Understanding of existing `Element` and `IUIDriver` architecture
- Understanding of `PlaywrightWaitStrategy` polling pattern ([PlaywrightWaitStrategy.cs:171-199](../../src/FluentUIScaffold.Playwright/PlaywrightWaitStrategy.cs#L171-L199))
- Familiarity with exception hierarchy (`VerificationException`, `ElementValidationException`, `ElementTimeoutException`)

### Blocking Issues

None identified. All technical gaps have clear resolutions.

## Risk Analysis & Mitigation

### High Risk: Breaking Changes Impact

**Risk**: Existing tests and sample code will break. Users upgrading will face compilation errors and behavioral changes.

**Impact**: High - affects all users

**Mitigation**:
1. Create comprehensive migration guide with before/after examples
2. Update all sample code (RegistrationPage.cs) to demonstrate new patterns
3. Clearly document breaking changes in CHANGELOG
4. Consider semantic versioning major bump (2.0.0 if currently 1.x)
5. Provide error messages that guide users to new API (e.g., "VerifyText removed, use Verify.TextIs instead")

### Medium Risk: Test Suite Performance Degradation

**Risk**: All assertions now wait before checking. Tests that previously failed instantly will wait up to `DefaultWaitTimeout` before failing.

**Impact**: Medium - could add significant time to test runs

**Mitigation**:
1. Use short `DefaultWaitTimeout` (100ms-1s) in unit test configuration
2. Document timeout configuration best practices
3. Monitor test suite duration before/after change
4. Consider adding fast-fail option in future if needed (out of scope for initial implementation)

**Quantified impact**: If 100 tests each fail after 30s instead of instantly, that's +50 minutes. Mitigation: short timeouts in unit tests, acceptance tests should have realistic timeouts.

### Medium Risk: TOCTOU Window Confusion

**Risk**: Element becomes invisible between `WaitForVisible()` completing and `IsVisible()` check. Could cause confusing failures.

**Impact**: Medium - could lead to flaky tests in edge cases

**Mitigation**:
1. Document TOCTOU window in Known Limitations
2. Provide escape hatch: users can access Playwright's native `Expect` API via `GetFrameworkDriver<IPage>()` for atomic wait-and-assert
3. Accept as inherent limitation of two-step approach (wait, then assert)
4. Error message should be clear: "Element became invisible after wait completed"

**Note**: This is an acceptable tradeoff for the framework-agnostic architecture.

### Low Risk: Polling Overhead

**Risk**: 100ms polling with 30s timeout = up to 300 poll attempts. For slow drivers, this could be expensive.

**Impact**: Low - `GetPageTitle()` and `GetText()` should be fast in Playwright

**Mitigation**:
1. Monitor performance in real-world scenarios
2. Document that polling is intended for E2E tests (not unit tests)
3. Consider making poll interval configurable in future if users report performance issues (YAGNI for now)

Accept this as reasonable overhead for E2E tests. Optimize only if users report actual performance problems.

### Low Risk: Framework-Specific Behavior Differences

**Risk**: Future drivers (Selenium, Cypress) may have different wait semantics than Playwright.

**Impact**: Low - currently only Playwright driver exists

**Mitigation**:
1. Document that VerificationContext relies on `IUIDriver.WaitForElementToBeVisible()` behavior
2. Each driver implementation must respect timeout and wait semantics
3. Driver contract should be clear about exception types thrown

This is inherent to multi-driver architecture. Each driver must implement wait methods correctly.

## Known Limitations

### 1. TOCTOU Window

After `WaitForVisible()` succeeds, there is a brief window where the element could become invisible before the `IsVisible()` assertion runs. This is inherent to the two-step wait-then-assert approach.

**Workaround**: For atomic wait-and-assert, use Playwright's native `Expect` API:
```csharp
var page = app.GetFrameworkDriver<IPage>();
await page.Locator("#element").ToBeVisibleAsync();
```

### 2. Thread Safety

`VerificationContext` is not thread-safe and should not be used concurrently on the same page instance. This aligns with the existing single-threaded test execution model.

**Workaround**: Each test should have its own page instance. Do not share pages across parallel tests.

### 3. Slower Failure Path

Tests that previously failed instantly will now wait up to `DefaultWaitTimeout` before failing. This is intentional and fixes flakiness, but can make tests slower.

**Workaround**: Use short `DefaultWaitTimeout` (100ms-1s) in unit test configuration. Reserve longer timeouts (10s-30s) for integration/E2E tests. This pattern is documented in the migration guide and test setup examples.

### 4. Text Polling Double-Timeout

For `TextContains`/`TextIs`, the worst-case wait time is 2× `DefaultWaitTimeout` (once for visibility, once for text polling). This is acceptable for E2E tests but can be slow.

**Workaround**: Use shorter timeouts for unit tests. For E2E tests, 60s total wait (30s visibility + 30s text) is reasonable for slow-loading pages.

### 5. Non-Existent Elements Are "Not Visible"

`Verify.NotVisible()` treats elements that don't exist in the DOM as "not visible" (succeeds). This might not catch bugs where an element was removed entirely instead of being hidden.

**Workaround**: If you need to verify an element exists but is hidden, use `page.WaitForElement(p => p.Element).Verify.NotVisible(p => p.Element)` (two separate calls - first waits for DOM attachment, second verifies hidden).

## Migration Guide

### Quick Reference Table

| Old API (Page methods) | New API (VerificationContext) | Notes |
|------------------------|-------------------------------|-------|
| `page.WaitForVisible(p => p.El)` | `page.Verify.Visible(p => p.El).And` | Combines wait + assert |
| `page.VerifyText(p => p.El, "text")` | `page.Verify.TextIs(p => p.El, "text").And` | Exact match, polls for text |
| `page.VerifyValue(p => p.El, "value")` | `page.Verify.TextIs(p => p.El, "value").And` | Same as VerifyText |
| `page.VerifyProperty(p => p.El, "active", "class")` | `page.Verify.HasAttribute(p => p.El, "class", "active").And` | Exact match |
| `page.WaitForVisible(p => p.El).VerifyText(...)` | `page.Verify.Visible(p => p.El).TextIs(...).And` | WaitForVisible redundant |

### Step-by-Step Migration

#### Step 1: Update Simple WaitForVisible Calls

```csharp
// BEFORE
page.WaitForVisible(p => p.Element);

// AFTER
page.Verify.Visible(p => p.Element).And;
// OR just omit if next line is another verification or action
```

#### Step 2: Migrate VerifyText to TextIs

```csharp
// BEFORE
page.VerifyText(p => p.Message, "Success!");

// AFTER
page.Verify.TextIs(p => p.Message, "Success!").And;
```

#### Step 3: Migrate VerifyValue to TextIs

```csharp
// BEFORE
page.VerifyValue(p => p.InputField, "expected value");

// AFTER
page.Verify.TextIs(p => p.InputField, "expected value").And;
```

#### Step 4: Migrate VerifyProperty to HasAttribute

```csharp
// BEFORE
page.VerifyProperty(p => p.Button, "active", "class");

// AFTER
page.Verify.HasAttribute(p => p.Button, "class", "active").And;
```

#### Step 5: Remove Redundant WaitForVisible Before Verify

```csharp
// BEFORE
page.WaitForVisible(p => p.Element)
    .VerifyText(p => p.Element, "text");

// AFTER (WaitForVisible is redundant - Verify.TextIs waits)
page.Verify.TextIs(p => p.Element, "text").And;
```

#### Step 6: Chain Multiple Verifications

```csharp
// BEFORE
page.VerifyText(p => p.Title, "Dashboard")
    .VerifyText(p => p.Subtitle, "Welcome")
    .VerifyValue(p => p.Username, "John");

// AFTER
page.Verify
    .TextIs(p => p.Title, "Dashboard")
    .TextIs(p => p.Subtitle, "Welcome")
    .TextIs(p => p.Username, "John")
    .And;
```

#### Step 7: Update Exception Handling

```csharp
// BEFORE
try {
    page.VerifyText(p => p.Message, "expected");
} catch (ElementValidationException ex) {
    // Handle
}

// AFTER
try {
    page.Verify.TextIs(p => p.Message, "expected");
} catch (VerificationException ex) {
    // Handle
}
```

#### Step 8: Keep Synchronization Primitives

```csharp
// BEFORE (wait for spinner to appear and disappear)
page.WaitForVisible(p => p.LoadingSpinner)
    .WaitForHidden(p => p.LoadingSpinner);

// AFTER (same - these methods are kept)
page.WaitForVisible(p => p.LoadingSpinner)
    .WaitForHidden(p => p.LoadingSpinner);

// OR use verification (combines wait + assert)
page.Verify.NotVisible(p => p.LoadingSpinner).And;
```

### Example: RegistrationPage.cs Full Migration

**Before** ([RegistrationPage.cs:142-178](../../samples/SampleApp.Tests/Pages/RegistrationPage.cs#L142-L178)):

```csharp
public RegistrationPage SubmitAndVerifySuccess()
{
    return Click(e => e.SubmitButton)
        .WaitForVisible(e => e.SuccessMessage)
        .VerifyText(e => e.SuccessMessage, "Registration successful!");
}

public RegistrationPage VerifyFormValues()
{
    return WaitForVisible(e => e.NameField)
        .VerifyValue(e => e.NameField, "John")
        .VerifyValue(e => e.EmailField, "john@example.com")
        .VerifyValue(e => e.PhoneField, "+1234567890");
}

public RegistrationPage VerifyValidationErrors()
{
    return VerifyText(e => e.NameError, "Name is required")
        .VerifyText(e => e.EmailError, "Invalid email format")
        .VerifyText(e => e.PhoneError, "Phone number is required");
}
```

**After**:

```csharp
public RegistrationPage SubmitAndVerifySuccess()
{
    return Click(e => e.SubmitButton)
        .Verify
            .Visible(e => e.SuccessMessage)
            .TextIs(e => e.SuccessMessage, "Registration successful!")
        .And;
}

public RegistrationPage VerifyFormValues()
{
    return Verify
        .Visible(e => e.NameField)
        .TextIs(e => e.NameField, "John")
        .TextIs(e => e.EmailField, "john@example.com")
        .TextIs(e => e.PhoneField, "+1234567890")
        .And;
}

public RegistrationPage VerifyValidationErrors()
{
    return Verify
        .TextIs(e => e.NameError, "Name is required")
        .TextIs(e => e.EmailError, "Invalid email format")
        .TextIs(e => e.PhoneError, "Phone number is required")
        .And;
}
```

## Success Metrics

### Verification API Usage

- 100% of sample tests use `page.Verify.*` pattern (no Page-level VerifyText/VerifyValue/VerifyProperty)
- 0 compilation errors from removed Page methods after migration complete
- All tests pass with new API

### Test Reliability

- Flaky tests due to instant assertions reduced to zero
- No regression in test reliability (no new flaky tests introduced)
- Test suite failure rate consistent or improved compared to baseline

### Performance Impact

- Test suite duration increase <10% for unit tests (using short timeouts)
- E2E test suite duration acceptable (longer timeouts expected)
- No individual test takes >2× as long as before (with short timeout configuration)

### Code Quality

- 100% of `Verify.*` methods have consistent exception wrapping
- 100% of `Verify.*` methods have descriptive error messages with context
- All public methods have XML documentation comments

## Future Considerations

### 1. Soft Assertions (Post-Release)

Allow collecting multiple verification failures before throwing:

```csharp
page.SoftVerify
    .Visible(p => p.Element1)
    .TextIs(p => p.Element2, "expected")
    .HasAttribute(p => p.Element3, "class", "active")
    .AssertAll(); // Throws with all collected failures
```

**Why deferred**: YAGNI - can add later if users request it.

### 2. Configurable Poll Interval (Post-Release)

```csharp
var options = new FluentUIScaffoldOptions
{
    DefaultWaitTimeout = TimeSpan.FromSeconds(30),
    PollInterval = TimeSpan.FromMilliseconds(50) // Faster polling
};
```

**Why deferred**: 100ms is reasonable default. Optimize only if users report issues.

### 3. Additional Verification Methods

```csharp
Verify.Enabled(p => p.Button)
Verify.Disabled(p => p.Button)
Verify.Exists(p => p.Element)
Verify.Selected(p => p.Checkbox)
```

**Why deferred**: Not in scope for this refactor. Can add incrementally based on user needs.

### 4. HasAttributeContaining for Partial Matches

```csharp
Verify.HasAttributeContaining(p => p.Button, "class", "active") // Matches "btn active primary"
```

**Why deferred**: Exact match is sufficient for most cases. Add if users need substring matching for CSS classes.

### 5. Custom Polling Predicates

```csharp
Verify.Custom(p => p.Element, el => el.GetText().Length > 10, "Text length > 10")
```

**Why deferred**: Advanced use case. Users can drop down to driver methods for custom logic.

## Documentation Plan

### README.md Updates

- Update "Verification Pattern" section to show only `page.Verify.*` API
- Remove examples using `Page.VerifyText()`, `VerifyValue()`, etc.
- Add "Known Limitations" section documenting TOCTOU, thread safety, slower failure path

### Migration Guide (New Document)

- Create `docs/migration/v2.0-verification-api.md`
- Include all migration patterns from this plan
- Add RegistrationPage.cs before/after examples
- Document exception type changes
- Link from CHANGELOG

### API Documentation Updates

- Update XML comments for all `IVerificationContext<TPage>` methods
- Document polling behavior for text/title/URL verifications
- Document wait-before-assert behavior for all methods
- Remove XML comments for deleted Page methods

### CHANGELOG.md

- Add "Breaking Changes" section for v2.0
- List all removed Page methods
- Link to migration guide
- Highlight new `TextIs()` and `HasAttribute()` methods

### Sample Tests

- Update all sample tests to use new API
- Add comments explaining patterns (e.g., "Verify.Visible waits before asserting")
- Serve as living documentation for best practices

## References & Research

### Internal References

**Core Implementation**:
- [VerificationContext.cs:36-112](../../src/FluentUIScaffold.Core/Configuration/VerificationContext.cs#L36-L112) - Current instant-check implementation
- [IVerificationContext.cs:17-47](../../src/FluentUIScaffold.Core/Interfaces/IVerificationContext.cs#L17-L47) - Interface to extend
- [Page.cs:141-202](../../src/FluentUIScaffold.Core/Pages/Page.cs#L141-L202) - Methods to remove
- [Element.cs:137-170](../../src/FluentUIScaffold.Core/Element.cs#L137-L170) - Wait methods to use

**Polling Pattern Reference**:
- [PlaywrightWaitStrategy.cs:171-199](../../src/FluentUIScaffold.Playwright/PlaywrightWaitStrategy.cs#L171-L199) - Established polling pattern
- [PlaywrightWaitStrategy.cs:208-236](../../src/FluentUIScaffold.Playwright/PlaywrightWaitStrategy.cs#L208-L236) - Attribute polling pattern

**Driver Interface**:
- [IUIDriver.cs:14-112](../../src/FluentUIScaffold.Core/Interfaces/IUIDriver.cs#L14-L112) - Available wait/query methods
- [PlaywrightDriver.cs:295-323](../../src/FluentUIScaffold.Playwright/PlaywrightDriver.cs#L295-L323) - Wait implementations

**Exception Hierarchy**:
- [VerificationException.cs](../../src/FluentUIScaffold.Core/Exceptions/VerificationException.cs)
- [ElementValidationException.cs](../../src/FluentUIScaffold.Core/Exceptions/ElementValidationException.cs)
- [ElementTimeoutException.cs](../../src/FluentUIScaffold.Core/Exceptions/ElementTimeoutException.cs)

**Configuration**:
- [FluentUIScaffoldOptions.cs:25](../../src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptions.cs#L25) - DefaultWaitTimeout
- [ElementBuilder.cs:39-41](../../src/FluentUIScaffold.Core/ElementBuilder.cs#L39-L41) - Default timeout/retry interval

**Tests to Update**:
- [VerificationContextTests.cs](../../tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs) - Core verification tests
- [VerificationContextV2Tests.cs](../../tests/FluentUIScaffold.Core.Tests/VerificationContextV2Tests.cs) - Chainable API tests
- [PageTests.cs:119-131](../../tests/FluentUIScaffold.Core.Tests/Pages/PageTests.cs#L119-L131) - WaitForVisible test to remove

**Sample Code to Migrate**:
- [RegistrationPage.cs:142-178](../../samples/SampleApp.Tests/Pages/RegistrationPage.cs#L142-L178) - Real-world usage examples

### Related Work

**Brainstorm Document**:
- [2026-02-12-verify-wait-strategy-brainstorm.md](../brainstorms/2026-02-12-verify-wait-strategy-brainstorm.md) - All decisions documented

**Project Documentation**:
- [CLAUDE.md](../../CLAUDE.md) - Project overview and patterns
- Page Object Pattern section - Verification best practices
- Element Configuration section - Wait strategies

### Research Findings

**From Repo Research**:
- All VerificationContext methods currently do instant checks (no waiting)
- PlaywrightWaitStrategy has established polling pattern: `while (DateTime.UtcNow - startTime < timeout) { /* check condition */ Thread.Sleep(100); }`
- MockUIDriver and DriverStub in tests need updates to simulate state transitions
- AppScaffold.WaitFor<TPage>() calls IElement.WaitForVisible (not Page method) - not affected by this change

**From Requirements Analysis**:
- All technical gaps identified and resolved in this plan
- Key decisions: No IUIDriver timeout parameters needed; polling implemented in VerificationContext; all exceptions wrapped in VerificationException
- Design choices: Element timeout for visibility wait, DefaultWaitTimeout for polling; HasAttribute uses exact match; comprehensive migration guide included

**Key Patterns Established**:
- 100ms poll interval (from PlaywrightWaitStrategy, ElementBuilder defaults)
- `Thread.Sleep` for synchronous polling (matches codebase conventions)
- Full DefaultWaitTimeout for each phase (visibility + polling) to avoid premature timeouts
- Exception wrapping pattern: catch all except VerificationException, wrap with context
- Short timeouts (100ms-1s) in unit tests, realistic timeouts (10s-30s) in E2E tests

**Breaking Change Impact**:
- RegistrationPage.cs uses removed methods 14 times (7 VerifyText, 4 VerifyValue, 3 WaitForVisible)
- Exception type change affects any `catch (ElementValidationException)` blocks
- All Page-level verification methods removed - clean break, no deprecation period
