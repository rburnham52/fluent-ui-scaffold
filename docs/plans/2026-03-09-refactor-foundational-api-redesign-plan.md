---
title: "refactor: Foundational API Redesign"
type: refactor
status: completed
date: 2026-03-09
deepened: 2026-03-09
brainstorm: docs/brainstorms/2026-03-09-foundational-api-redesign-brainstorm.md
---

# refactor: Foundational API Redesign

## Enhancement Summary

**Deepened on:** 2026-03-09
**Agents used:** architecture-strategist, code-simplicity-reviewer, performance-oracle, pattern-recognition-specialist, security-sentinel, best-practices-researcher (x2), Context7 Playwright docs

### Key Improvements

1. **Simplified architecture** — eliminated 5 unnecessary classes/interfaces per simplicity review (ActionQueue inlined, IBrowserSessionFactory merged into plugin, PlaywrightBrowserManager merged into plugin, PlaywrightSessionFactory merged, SessionLifecycle deferred)
2. **Clarified DI scoping model** — specified wrapper IServiceProvider pattern for sessions, resolving the most critical under-specification in the original plan
3. **Added runtime unawaited-chain detection** — moved from Phase 6 to Phase 1, addressing the highest-severity risk identified by all reviewers
4. **Added session lifecycle hooks** — explicit CreateSessionAsync/DisposeSessionAsync on AppScaffold with MSTest pattern
5. **Performance-validated** — all overhead is negligible vs browser I/O; DOMContentLoaded recommended over NetworkIdle

### New Considerations Discovered

- Freeze state should live on Page<TSelf>, not ActionQueue (supports multi-page chain correctly)
- `Enqueue<T1,T2>` through `Enqueue<T1,T2,T3,T4>` are YAGNI — ship with 2 overloads only
- Session scoped DI should use a wrapper provider (not build new container per session)
- `IUITestingPlugin.InitializeAsync` should accept `CancellationToken`
- Page resolution should use `ActivatorUtilities.CreateInstance` with session provider
- `AsyncLocal<IBrowserSession>` needed for parallel test execution support

---

## Overview

Ground-up redesign of FluentUIScaffold's core API. Removes the `IUIDriver` / `Element` / `IElement` abstraction layer, makes `Page<TSelf>` an async deferred execution chain builder, and introduces a plugin + session architecture for test isolation.

This is a **major breaking change** — all existing page objects and tests must be rewritten. No backward compatibility shims.

## Problem Statement

The current API has 10 identified pain points (see [brainstorm](../brainstorms/2026-03-09-foundational-api-redesign-brainstorm.md)):

- `IUIDriver` reimplements 22 members that mirror Playwright's native API
- Sync-over-async throughout (`PlaywrightDriver` blocks on every async call)
- Three disconnected wait systems (`Element.WaitFor()`, `PlaywrightWaitStrategy`, `VerificationContext`)
- Inconsistent delegation (some `Page` methods bypass `IElement`)
- Dead code (`PluginManager`, `PluginRegistry`, most wait strategies)
- No test isolation (single browser context shared across all tests)

The framework's real value is **hosting orchestration + structured page objects + fluent chaining** — not wrapping Playwright's API.

## Proposed Solution

Replace the driver/element abstraction with a thin chain builder on `Page<TSelf>` that queues actions for deferred async execution. Page objects interact with Playwright directly inside `Enqueue<T>()` lambdas. A new plugin + session architecture provides per-test browser isolation.

## Technical Approach

### Architecture

```
FluentUIScaffoldBuilder
    .UsePlugin(new PlaywrightPlugin())       -> configures DI
    .UseAspireHosting<T>(...)                -> configures hosting
    .Build<WebApp>()                         -> creates AppScaffold

AppScaffold<TWebApp>
    StartAsync()          -> starts hosting + plugin.InitializeAsync()
    CreateSessionAsync()  -> creates isolated IBrowserSession (per-test)
    NavigateTo<TPage>()   -> resolves page with session provider, enqueues navigation, returns TPage
    DisposeAsync()        -> dispose sessions, hosting, plugin

IUITestingPlugin (e.g., PlaywrightPlugin)
    ConfigureServices()   -> registers shared browser services
    InitializeAsync()     -> launches browser (singleton)
    CreateSessionAsync()  -> creates IBrowserContext + IPage (isolated per test)

IBrowserSession
    NavigateToUrlAsync()  -> session-scoped navigation
    ServiceProvider       -> wrapper provider (session-specific IPage + root fallback)

Page<TSelf>  (Core -- framework-agnostic, IS the chain builder)
    [Route("/path")]      -> URL template with {param} placeholders
    Enqueue(Func<Task>)   -> deferred action, returns TSelf
    Enqueue<T>(...)       -> deferred action with DI injection, returns TSelf
    NavigateTo<T>()       -> scope switch, freezes self, returns TTarget
    GetAwaiter()          -> executes all queued actions sequentially
```

### Research Insights: Architecture

**Pattern compliance (architecture-strategist):**
- The layering is sound: Core defines execution model, Plugin provides concrete sessions, page objects are framework-coupled by design
- SOLID principles are upheld — ISP improved (22-member `IUIDriver` eliminated), DIP maintained via `Enqueue<T>` generic resolution
- The deferred execution model is conceptually closest to Cypress's command queue, but with developer-managed `await` instead of a custom test runner

**Precedent analysis (pattern-recognition):**
- CRTP (`Page<TSelf>`) — well-established in .NET (FluentAssertions, Atata, NBuilder)
- Custom awaitable — unprecedented in .NET testing ecosystem; documentation burden is high
- Freeze pattern — matches WPF `Freezable` and ASP.NET Core pipeline `Build()` semantics
- `Enqueue<T>` pattern — closely mirrors ASP.NET Core Minimal APIs lambda DI, but with compile-time generic safety instead of reflection

**Performance validation (performance-oracle):**
- DI resolution overhead: ~100ns per `GetRequiredService<T>()` call — negligible vs 5-50ms per browser action
- Closure allocations: ~300-500 bytes per 5-action chain — trivial for Gen0 GC
- Browser context creation: ~50-130ms per test (the dominant cost) — deliberate trade-off for isolation
- All framework overhead is <0.001% of actual browser interaction time

### Implementation Phases

#### Phase 1: Core Chain Engine

Build the deferred execution engine in `FluentUIScaffold.Core`. No Playwright dependency.

**Deliverables:**

- [x] `Page<TSelf>` rewrite — becomes the chain builder with inlined action queue
  - **Action queue inlined as fields** (not a separate class — per simplicity review):
    - `private readonly List<Func<IServiceProvider, Task>> _actions = new(capacity: 8);`
    - `private bool _isFrozen;` — freeze state per-page (not on queue)
    - `private bool _isConsumed;` — tracks whether chain was executed
  - Implements `GetAwaiter()` returning `TaskAwaiter` (void return — page is already the reference)
  - `Enqueue(Func<Task>)` — adds action, returns `(TSelf)this`
  - `Enqueue<T1>(Func<T1, Task>)` — resolves T1 from `IServiceProvider`, returns `(TSelf)this`
  - **Only 2 Enqueue overloads shipped** (per simplicity review). For 2+ services, use `IServiceProvider` directly:
    ```csharp
    protected TSelf Enqueue(Func<Task> action);          // no DI
    protected TSelf Enqueue<T>(Func<T, Task> action);    // 1 service (covers 99% of cases)
    ```
  - `NavigateTo<TTarget>()` — sets `_isFrozen = true` on self, creates target page with shared `_actions` list reference, enqueues navigation action, returns TTarget
  - **Shared actions list mechanism:** Two constructors:
    ```csharp
    // Public — used by AppScaffold.NavigateTo<TPage>() for initial page creation
    protected Page(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _actions = new List<Func<IServiceProvider, Task>>(capacity: 8);
    }

    // Internal — used by Page.NavigateTo<TTarget>() to share the action list
    internal Page(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)
    {
        _serviceProvider = serviceProvider;
        _actions = sharedActions; // Same list reference — appending to target appends to shared chain
    }
    ```
    `NavigateTo<TTarget>()` calls `ActivatorUtilities.CreateInstance` with the internal constructor, passing `_actions` by reference. Target page appends to the same list. When any page in the chain is awaited, the full shared action list executes.
  - Constructor: `(IServiceProvider serviceProvider)` — minimal, no `IUIDriver`
  - Internal `ExecuteAllAsync()`:
    - Uses `ConfigureAwait(false)` on all internal awaits
    - Clears `_actions` list after execution (releases closure references for GC)
    - Sets `_isConsumed = true` to make double-await a no-op
  - `GetAwaiter()` implementation:
    ```csharp
    public TaskAwaiter GetAwaiter()
    {
        return ExecuteAllAsync().GetAwaiter();
        // Note: do NOT ConfigureAwait the returned Task — let caller's
        // SynchronizationContext control the final continuation
    }
    ```
  - Fail-fast: first exception stops chain, propagates to await site
  - File: `src/FluentUIScaffold.Core/Pages/Page.cs` (rewrite in place)

- [x] **Runtime unawaited-chain detection** (moved from Phase 6 — critical safety net)
  - In DEBUG builds, add a finalizer that warns if actions were queued but never executed:
    ```csharp
    #if DEBUG
    ~Page()
    {
        if (_actions.Count > 0 && !_isConsumed)
        {
            // Use Trace.TraceWarning, NOT Debug.Fail — finalizers run on GC thread
            // where Debug.Fail would crash the process with no actionable stack trace.
            Trace.TraceWarning($"Page<{typeof(TSelf).Name}> chain with {_actions.Count} " +
                               $"actions was never awaited. Add 'await' before the chain.");
        }
    }
    #endif
    ```
  - **`GC.SuppressFinalize(this)`** must be called at the start of `ExecuteAllAsync()` to prevent false positives when chain is properly awaited:
    ```csharp
    private async Task ExecuteAllAsync()
    {
        #if DEBUG
        GC.SuppressFinalize(this);
        #endif
        if (_isConsumed) return;
        _isConsumed = true;
        // ... execute actions ...
    }
    ```
  - Roslyn analyzer remains a follow-up issue (tracked separately, not in this plan)

- [x] `FrozenPageException` — thrown when enqueuing on a frozen page
  - Message includes source page type, target page type
  - Also thrown when calling `NavigateTo<T>()` on an already-frozen page
  - File: `src/FluentUIScaffold.Core/Exceptions/FrozenPageException.cs`

- [x] `RouteAttribute` — kept as-is, no changes
  - File: `src/FluentUIScaffold.Core/Pages/RouteAttribute.cs`

- [x] Unit tests for chain engine
  - Empty chain await (should be a successful no-op)
  - Single action execution
  - Multiple actions execute in order
  - Fail-fast on exception (subsequent actions skipped)
  - Frozen page throws on enqueue
  - Frozen page throws on NavigateTo (double-freeze)
  - NavigateTo transfers queue and freezes source
  - NavigateTo chaining depth (A -> B -> C, A and B frozen)
  - Double-await behavior (second await is no-op — queue consumed)
  - Enqueue<T> resolves from IServiceProvider
  - Enqueue with missing service throws at execution time (clear error message including service type + page type)
  - GetAwaiter on frozen page still executes the shared queue
  - File: `tests/FluentUIScaffold.Core.Tests/Pages/PageChainTests.cs`

**Design decisions for gaps identified in analysis:**

| Gap | Decision | Research backing |
|-----|----------|-----------------|
| Empty chain await | No-op success (not an error) | Consistent with `Task.CompletedTask` |
| Double-await | Second await is no-op (queue consumed, not an error) | Consistent with `Task` behavior |
| Unawaited chains | **Runtime DEBUG detection in Phase 1** + Roslyn analyzer as follow-up | All 5 reviewers flagged this as critical |
| GetAwaiter return | `TaskAwaiter` (void). Page reference already held by caller. | Stephen Toub "Await Anything" pattern |
| Freeze timing | Immediate at enqueue time (when `NavigateTo<T>()` is called) | Matches WPF Freezable semantics |
| Freeze state location | **Per-page `_isFrozen` boolean** (not on queue) | Supports A->B->C chain where A and B are frozen but queue is still appendable via C |
| Conditional branching | Not supported. Break chain with `await`, use C# `if`, start new chain. | — |
| Action queue storage | Inline on `Page<TSelf>` (no separate `ActionQueue` class) | Simplicity review: eliminates 1 file, ~50 LOC, 1 namespace |
| Enqueue overload count | 2 overloads (0 and 1 type param). For 2+ services, use `ServiceProvider` directly. | Simplicity review: all plan examples use exactly 1 type param |
| Queue ownership | `AppScaffold.NavigateTo<TPage>()` creates first page (public constructor → fresh list). `Page.NavigateTo<TTarget>()` passes `_actions` via internal constructor → shared list reference. | Architecture review: keeps queue lifecycle tied to interaction boundary |

#### Phase 2: Plugin + Session Architecture

New plugin contract and browser session abstraction in Core.

**Deliverables:**

- [x] `IUITestingPlugin` — replaces `IUITestingFrameworkPlugin`
  - `void ConfigureServices(IServiceCollection services)`
  - `Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken cancellationToken = default)` — added CancellationToken per pattern review
  - `Task<IBrowserSession> CreateSessionAsync()` — **factory method directly on plugin** (no separate IBrowserSessionFactory — per simplicity review)
  - Extends `IAsyncDisposable`
  - File: `src/FluentUIScaffold.Core/Interfaces/IUITestingPlugin.cs`

- [x] `IBrowserSession` — per-test isolation boundary
  - `Task NavigateToUrlAsync(Uri url)`
  - `IServiceProvider ServiceProvider { get; }` — wrapper provider with session-specific services + root fallback
  - Extends `IAsyncDisposable`
  - File: `src/FluentUIScaffold.Core/Interfaces/IBrowserSession.cs`

- [x] `FluentUIScaffoldOptions` — simplified
  - Remove: `DefaultWaitTimeout` (Playwright handles timeouts), `RequestedDriverType` (no driver concept)
  - Keep: `BaseUrl`, `HeadlessMode`, `SlowMo`
  - **No `SessionLifecycle` enum** (per simplicity review — always PerTest, users can hold session references themselves for class/assembly sharing)
  - File: `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldOptions.cs` (modify in place)

- [x] `FluentUIScaffoldBuilder` — updated
  - `UsePlugin(IUITestingPlugin plugin)` — replaces old overload
  - Remove: `UsePlaywright()` convenience (plugin is passed directly)
  - Remove: `WithAutoPageDiscovery()` / `RegisterPage<T>()` — pages registered as transient services
  - Keep: hosting strategy methods, `Web<TApp>()`, `Build<TApp>()`
  - Builder registers pages via assembly scanning into DI as transient
  - File: `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs` (modify in place)

- [x] `AppScaffold<TWebApp>` — updated
  - **Explicit session lifecycle methods:**
    ```csharp
    public async Task<IBrowserSession> CreateSessionAsync()
    public async Task DisposeSessionAsync()
    ```
  - `NavigateTo<TPage>()` — synchronous: creates page via `ActivatorUtilities.CreateInstance(session.ServiceProvider, typeof(TPage))`, enqueues navigation to page's route, returns the page
    - **Guard:** throws `InvalidOperationException("No browser session is active. Call CreateSessionAsync() in [TestInitialize] before navigating.")` when `AsyncLocal<IBrowserSession>.Value` is null
  - `NavigateTo<TPage>(object routeParams)` — same with parameterized routes (same guard)
  - **Per-test session tracking via `AsyncLocal<IBrowserSession>`** for parallel test safety
  - `On<TPage>()` — resolves page without navigating (for asserting current page)
  - Remove: `Framework<T>()` (replaced by Enqueue<T> DI injection), reflection-based navigation
  - Add: type-safe navigation constraints: `where TPage : Page<TPage>`
  - File: `src/FluentUIScaffold.Core/AppScaffold.cs` (modify in place)

- [x] Unit tests for plugin + session
  - Plugin ConfigureServices registers expected types
  - Plugin InitializeAsync called during StartAsync
  - Plugin CreateSessionAsync creates isolated sessions
  - Session disposal cleans up resources
  - AppScaffold.NavigateTo resolves page with type constraint
  - AppScaffold.NavigateTo enqueues navigation action
  - AppScaffold.NavigateTo throws InvalidOperationException when no session active
  - AppScaffold parallel session tracking (AsyncLocal isolation)
  - File: `tests/FluentUIScaffold.Core.Tests/PluginSessionTests.cs`
  - File: `tests/FluentUIScaffold.Core.Tests/AppScaffoldTests.cs` (rewrite)

### Research Insights: DI Scoping Model

**The most critical implementation detail** (flagged by architecture, performance, and best-practices agents):

`Page<TSelf>` has a **single `IServiceProvider`** — the session provider. The session provider is a lightweight wrapper that checks session-local services first (IPage, IBrowserContext, IBrowser) and falls back to root for everything else (logging, options, hosting). There is no dual-provider concept.

- **Construction:** `ActivatorUtilities.CreateInstance(sessionProvider, typeof(TPage))` — the session provider satisfies both session services (IPage) and root services (ILogger) in the constructor
- **Execution:** `Enqueue<T>` lambdas receive the same `_serviceProvider` stored on the page — which IS the session provider

The `_actions` list stores `Func<IServiceProvider, Task>` — the `IServiceProvider` argument is the page's own `_serviceProvider` (the session provider).

**Session provider implementation — wrapper pattern** (recommended by architecture + best-practices agents):

```csharp
// In PlaywrightBrowserSession
// Implements IServiceProviderIsService so ActivatorUtilities.CreateInstance can
// correctly resolve which constructor parameters are available as services.
internal class SessionServiceProvider : IServiceProvider, IServiceProviderIsService
{
    private readonly IServiceProvider _root;
    private readonly Dictionary<Type, object> _sessionServices;

    public SessionServiceProvider(IServiceProvider root, IPage page, IBrowserContext context, IBrowser browser)
    {
        _root = root;
        _sessionServices = new Dictionary<Type, object>
        {
            [typeof(IPage)] = page,
            [typeof(IBrowserContext)] = context,
            [typeof(IBrowser)] = browser,
        };
    }

    public object? GetService(Type serviceType)
    {
        return _sessionServices.TryGetValue(serviceType, out var service)
            ? service
            : _root.GetService(serviceType);
    }

    public bool IsService(Type serviceType)
    {
        if (_sessionServices.ContainsKey(serviceType))
            return true;
        var rootIsService = _root.GetService(typeof(IServiceProviderIsService)) as IServiceProviderIsService;
        return rootIsService?.IsService(serviceType) ?? false;
    }
}
```

This avoids `BuildServiceProvider()` per session (~1-5ms) and maintains access to root singletons (logging, options, Aspire DistributedApplication).

**Page resolution** — use `ActivatorUtilities.CreateInstance(sessionServiceProvider, typeof(TPage))` instead of DI resolution. This handles constructor injection from the session provider without requiring pages to be registered in the session container.

**References:**
- [Microsoft DI Guidelines: Scoped Services](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/guidelines)
- [AsyncServiceScope in .NET 6](https://andrewlock.net/exploring-dotnet-6-part-10-new-dependency-injection-features-in-dotnet-6/)
- Playwright .NET uses identical pattern: `IBrowser` singleton, `IBrowserContext` + `IPage` per test (PageTest/ContextTest/BrowserTest hierarchy)

### Research Insights: Custom Awaitable Pattern

**Implementation recommendation** (from custom-awaitable research + architecture review):

```csharp
public abstract class Page<TSelf> where TSelf : Page<TSelf>
{
    private readonly List<Func<IServiceProvider, Task>> _actions;
    private readonly IServiceProvider _serviceProvider; // Always the session provider (falls through to root)
    private bool _isFrozen;
    private bool _isConsumed;

    // Public constructor — AppScaffold.NavigateTo creates initial page with fresh action list
    protected Page(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _actions = new List<Func<IServiceProvider, Task>>(capacity: 8);
    }

    // Internal constructor — Page.NavigateTo passes shared action list
    internal Page(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)
    {
        _serviceProvider = serviceProvider;
        _actions = sharedActions;
    }

    public TaskAwaiter GetAwaiter()
    {
        // ExecuteAllAsync uses ConfigureAwait(false) internally,
        // but the returned Task does NOT — caller's SynchronizationContext
        // controls the final continuation.
        return ExecuteAllAsync().GetAwaiter();
    }

    private async Task ExecuteAllAsync()
    {
        #if DEBUG
        GC.SuppressFinalize(this);
        #endif
        if (_isConsumed) return;
        _isConsumed = true;

        // Take a snapshot and clear to release closures for GC
        var snapshot = _actions.ToArray();
        _actions.Clear();

        for (int i = 0; i < snapshot.Length; i++)
        {
            // _serviceProvider is the session provider — resolves IPage, IBrowserContext
            // AND root services (ILogger, options) via fallback
            await snapshot[i](_serviceProvider).ConfigureAwait(false);
        }
    }
}
```

**Edge cases to test:**
- `GetAwaiter()` on a frozen page must still execute the shared queue (frozen pages share `_actions` by reference)
- Double-await returns immediately (consistent with `Task` behavior)
- Exception in action N skips actions N+1..end (fail-fast)
- All .NET target frameworks (net6.0-net9.0) support duck-typed `GetAwaiter()` since C# 5.0

**SynchronizationContext safety** (from performance review):
- `ConfigureAwait(false)` on all internal `await`s within `ExecuteAllAsync`
- Do NOT apply `ConfigureAwait(false)` to the `Task` returned by `GetAwaiter()` — let the test framework's context control the final continuation
- MSTest, NUnit, xUnit all handle async correctly in modern versions

#### Phase 3: Playwright Plugin Rewrite

Rewrite `PlaywrightPlugin` to implement new contracts.

**Deliverables:**

- [x] `PlaywrightPlugin` — implements `IUITestingPlugin` (**single class — merged per simplicity review**)
  - Owns `IPlaywright` and `IBrowser` fields directly (no separate `PlaywrightBrowserManager`)
  - `ConfigureServices()` — registers shared services (logging, options forwarding)
  - `InitializeAsync()` — creates `IPlaywright`, launches `IBrowser` with headless/slowmo from options
  - `CreateSessionAsync()` — creates `IBrowserContext` + `IPage` from `_browser`, returns `PlaywrightBrowserSession`
  - `DisposeAsync()` — closes browser, disposes playwright
  - File: `src/FluentUIScaffold.Playwright/PlaywrightPlugin.cs` (rewrite in place)

- [x] `PlaywrightBrowserSession` — implements `IBrowserSession`
  - Owns `IBrowserContext` and `IPage` for this session
  - `NavigateToUrlAsync(Uri)` — `_page.GotoAsync(url)` with **`WaitUntilState.DOMContentLoaded`** (not NetworkIdle — per performance review, saves 500ms+ per navigation)
  - `ServiceProvider` — `SessionServiceProvider` wrapper (session-specific `IPage`/`IBrowserContext` + root fallback)
  - `DisposeAsync()` — explicitly `CloseAsync()` context before scope disposal
  - File: `src/FluentUIScaffold.Playwright/PlaywrightBrowserSession.cs`

- [x] `SessionServiceProvider` — lightweight wrapper IServiceProvider
  - Checks session-local dictionary first, falls back to root
  - File: `src/FluentUIScaffold.Playwright/SessionServiceProvider.cs`

- [x] `FluentUIScaffoldPlaywrightBuilder` — convenience extensions
  - `UsePlaywright()` extension on `FluentUIScaffoldBuilder` that calls `UsePlugin(new PlaywrightPlugin())`
  - File: `src/FluentUIScaffold.Playwright/FluentUIScaffoldPlaywrightBuilder.cs` (rewrite in place)

- [x] Integration tests
  - PlaywrightPlugin initializes browser successfully
  - Plugin creates isolated sessions directly (no factory indirection)
  - Session IPage navigates correctly
  - Session disposal closes context
  - Multiple concurrent sessions work independently
  - SessionServiceProvider resolves session services (IPage) and root services (ILogger)
  - SessionServiceProvider.IsService returns true for session types (IPage, IBrowserContext, IBrowser)
  - SessionServiceProvider.IsService delegates to root for non-session types
  - ActivatorUtilities.CreateInstance resolves page constructors correctly with SessionServiceProvider
  - File: `tests/FluentUIScaffold.Playwright.Tests/PlaywrightPluginTests.cs` (rewrite)
  - File: `tests/FluentUIScaffold.Playwright.Tests/PlaywrightSessionTests.cs`

### Research Insights: Playwright Browser Context Isolation

**From Context7 docs + Playwright research:**
- `browser.NewContextAsync()` creates an isolated session (like an incognito window) — separate cookies, localStorage, cache
- Context creation: ~50-80ms; page creation: ~10-30ms; context close: ~10-20ms
- Total per-test overhead: ~70-130ms — the dominant cost, but correct trade-off for test reliability
- `IBrowser.NewContextAsync()` is thread-safe — supports parallel test execution natively

**Performance projections:**

| Test count | Session overhead | Framework overhead (DI + closures) |
|-----------|-----------------|-----------------------------------|
| 50 tests | ~5s | <10ms total |
| 200 tests | ~16s | <40ms total |
| 500 tests | ~40s | <100ms total |

**Future optimizations** (not in scope for initial release):
- Session pooling: reuse contexts with `ClearCookiesAsync()` instead of destroying (~30s savings over 500 tests)
- Pre-warm first context during `StartAsync()` for faster first-test experience
- Browser restart every N sessions for long-running suites to prevent Chromium memory bloat

#### Phase 4: Sample App Rewrite

Rewrite all sample page objects and tests to demonstrate the new API.

**Deliverables:**

- [x] `HomePage` — new-style page object
  ```csharp
  [Route("/")]
  public class HomePage : Page<HomePage>
  {
      public HomePage(IServiceProvider services) : base(services) { }

      public HomePage VerifyWelcomeVisible()
          => Enqueue(async (IPage page) =>
          {
              await Assertions.Expect(page.GetByTestId("welcome-message"))
                  .ToBeVisibleAsync();
          });

      public HomePage ClickGetStarted()
          => Enqueue(async (IPage page) =>
          {
              await page.GetByRole(AriaRole.Link, new() { Name = "Get Started" })
                  .ClickAsync();
          });
  }
  ```
  - File: `samples/SampleApp.Tests/Pages/HomePage.cs` (rewrite)

- [x] `LoginPage` — demonstrates form interaction
  - File: `samples/SampleApp.Tests/Pages/LoginPage.cs` (rewrite)

- [x] `RegistrationPage` — demonstrates multi-field form + navigation
  - File: `samples/SampleApp.Tests/Pages/RegistrationPage.cs` (rewrite)

- [x] `TodosPage` — demonstrates dynamic content interaction
  - File: `samples/SampleApp.Tests/Pages/TodosPage.cs` (rewrite)

- [x] `ProfilePage`, `UserPage` — demonstrates parameterized routes
  - File: `samples/SampleApp.Tests/Pages/ProfilePage.cs` (rewrite)
  - File: `samples/SampleApp.Tests/Pages/UserPage.cs` (rewrite)

- [x] `TestAssemblyHooks` — new setup pattern with explicit session lifecycle
  ```csharp
  [TestClass]
  public class TestAssemblyHooks
  {
      private static AppScaffold<WebApp>? _app;

      [AssemblyInitialize]
      public static async Task Init(TestContext context)
      {
          _app = new FluentUIScaffoldBuilder()
              .UsePlaywright()
              .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost:5000"))
              .Build<WebApp>();

          await _app.StartAsync();
      }

      [AssemblyCleanup]
      public static async Task Cleanup() => await _app!.DisposeAsync();

      public static AppScaffold<WebApp> App => _app!;
  }
  ```
  - File: `samples/SampleApp.Tests/TestAssemblyHooks.cs` (rewrite)

- [x] `TestBase` — per-test session management pattern
  ```csharp
  [TestClass]
  public class TestBase
  {
      [TestInitialize]
      public async Task TestSetup()
          => await TestAssemblyHooks.App.CreateSessionAsync();

      [TestCleanup]
      public async Task TestCleanup()
          => await TestAssemblyHooks.App.DisposeSessionAsync();
  }
  ```
  - File: `samples/SampleApp.Tests/TestBase.cs` (new)

- [x] Example tests — rewrite all in `samples/SampleApp.Tests/Examples/`
  - `HomePageTests.cs` — basic navigation + verification
  - `LoginFlowTests.cs` — multi-page chain with scope switching
  - `FormInteractionTests.cs` — Enqueue with Playwright form APIs
  - `BrowserInteractionTests.cs` — Enqueue<IPage> for advanced Playwright features
  - `RouteNavigationTests.cs` — parameterized routes
  - `VerificationTests.cs` — assertions using Playwright Assertions.Expect()
  - Remove tests that tested old API features (AdvancedNavigationTests, BDDStepDefinitionsExample, DebugModeTests, FrameworkTests)

- [x] **"Common Mistakes" documentation** — include in sample tests as comments showing:
  - Correct: `await app.NavigateTo<HomePage>().ClickGetStarted();`
  - Wrong: `app.NavigateTo<HomePage>().ClickGetStarted(); // Missing await — actions never execute!`

- [x] Aspire sample tests — rewrite `samples/SampleApp.AspireTests/`
  - `TestAssemblyHooks.cs` — Aspire + new plugin pattern
  - `SimpleAspireTests.cs` — basic Aspire test with new API

#### Phase 5: Remove Old Code

Delete all files that are no longer needed. **Grep for references to each deleted type before removing.**

**Files to DELETE from `src/FluentUIScaffold.Core/`:**

- [x] `Element.cs`
- [x] `ElementBuilder.cs`
- [x] `ElementCollection.cs`
- [x] `ElementFactory.cs`
- [x] `FluentUIScaffold.cs` (facade class, if unused)
- [x] `Interfaces/IElement.cs`
- [x] `Interfaces/IElementCollection.cs`
- [x] `Interfaces/IUIDriver.cs`
- [x] `Interfaces/IUITestingFrameworkPlugin.cs` (replaced by `IUITestingPlugin.cs`)
- [x] `Interfaces/IVerificationContext.cs`
- [x] `Configuration/VerificationContext.cs`
- [x] `Configuration/WaitStrategy.cs`
- [x] `Configuration/FrameworkOptions.cs` (if unused after builder rewrite)
- [x] `Configuration/SharedOptionsManager.cs` (if unused)
- [x] `Plugins/PluginManager.cs`
- [x] `Plugins/PluginRegistry.cs`
- [x] `Exceptions/ElementTimeoutException.cs`
- [x] `Exceptions/ElementValidationException.cs`
- [x] `Exceptions/VerificationException.cs`

**Files to DELETE from `src/FluentUIScaffold.Playwright/`:**

- [x] `PlaywrightDriver.cs`
- [x] `PlaywrightWaitStrategy.cs`
- [x] `PlaywrightAdvancedFeatures.cs`
- [x] `PlaywrightExceptions.cs` (if only driver-related)

**Test files to DELETE or REWRITE:**

- [x] `tests/FluentUIScaffold.Core.Tests/ElementBuilderTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/ElementCollectionTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/ElementFactoryTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/ElementTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/PluginManagerTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/PluginRegistryTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/VerificationContextTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/VerificationContextV2Tests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/MockDriverScriptTests.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/Mocks/MockUIDriver.cs`
- [x] `tests/FluentUIScaffold.Core.Tests/Mocks/StatefulMockDriver.cs`
- [x] `tests/FluentUIScaffold.Playwright.Tests/PlaywrightDriverTests.cs`
- [x] `tests/FluentUIScaffold.Playwright.Tests/PlaywrightDriverSingletonTests.cs`
- [x] `tests/FluentUIScaffold.Playwright.Tests/PlaywrightDriverScriptTests.cs`
- [x] `tests/FluentUIScaffold.Playwright.Tests/PlaywrightWaitStrategyTests.cs`
- [x] `tests/FluentUIScaffold.Playwright.Tests/PlaywrightAdvancedFeaturesTests.cs`

**Validation step:** After all deletions, run:
```bash
dotnet build
dotnet test
```
Fix any remaining references to deleted types.

#### Phase 6: Documentation

- [x] **Update CLAUDE.md** with new architecture, patterns, and code examples

- [x] **Update `docs/future-enhancement-framework-escape-hatch.md`** — mark as superseded

- [x] **Run `dotnet format`** on all changed files

## Acceptance Criteria

### Functional Requirements

- [x] `Page<TSelf>` implements `GetAwaiter()` — chains are awaitable
- [x] `Enqueue<T>()` resolves services from DI at execution time (not enqueue time)
- [x] `NavigateTo<T>()` freezes source page and returns target page sharing action list
- [x] Frozen page throws `FrozenPageException` on enqueue attempts
- [x] Chain fail-fast: first exception stops execution, propagates to await
- [x] `IUITestingPlugin.CreateSessionAsync()` creates isolated sessions per test
- [x] `PlaywrightPlugin` implements `IUITestingPlugin` with browser singleton + session creation
- [x] Sample page objects demonstrate the new API pattern
- [x] All existing hosting strategies work unchanged with new architecture
- [x] Parameterized routes work: `app.NavigateTo<UserPage>(new { userId = "123" })`
- [x] Runtime unawaited-chain detection fires in DEBUG builds

### Non-Functional Requirements

- [x] Core has zero Playwright dependency (framework-agnostic)
- [x] All async operations use `ConfigureAwait(false)` internally
- [x] No sync-over-async anywhere in the codebase
- [x] `dotnet build` succeeds for all target frameworks (net6.0, net7.0, net8.0, net9.0)
- [x] `dotnet test` passes for Core and Playwright test projects

### Quality Gates

- [x] All new code has unit tests
- [x] Sample app tests pass end-to-end
- [x] `dotnet format` clean
- [x] No references to deleted types remain (verified by full build)

## Risk Analysis & Mitigation

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Unawaited chains silently pass | High | High | **DEBUG finalizer detection in Phase 1.** Roslyn analyzer as separate follow-up. Document prominently with "Common Mistakes" examples. |
| Session scoped DI complexity | Medium | Medium | **Wrapper IServiceProvider pattern specified** (check session dict, fallback to root). Spike in Phase 2 before full implementation. |
| Aspire integration breakage | Medium | High | Test Aspire sample early in Phase 4. Session wrapper provider falls through to root (where DistributedApplication lives). |
| Custom awaitable edge cases in test frameworks | Low | Medium | `ConfigureAwait(false)` on internal awaits only. Final continuation respects caller's context. Tested with MSTest. |
| Large PR size | High | Medium | Implement in phases. Each phase can be a separate PR. |
| Parallel test execution races | Medium | Medium | `AsyncLocal<IBrowserSession>` for per-test session tracking. Playwright's `IBrowser.NewContextAsync()` is thread-safe. |

### Research Insights: Security Considerations

**From security-sentinel review:**
- Route parameter injection: validate/sanitize route parameters in `ResolveRouteParameters()` to prevent URL manipulation
- Session provider isolation: the wrapper `SessionServiceProvider` pattern ensures sessions cannot access each other's `IPage` instances
- Plugin trust boundary: `IUITestingPlugin` has full access to `IServiceCollection` — document that plugins must be trusted code (test infrastructure, not user input)

## Dependencies & Prerequisites

- Playwright NuGet package (already a dependency)
- No new external dependencies needed
- Docker required for Aspire sample tests (existing requirement)

## Simplification Changelog

Changes made from original plan based on simplicity review:

| Original | Simplified | Rationale |
|----------|-----------|-----------|
| `ActionQueue` as separate class + file | Inlined as fields on `Page<TSelf>` | One list + one boolean doesn't warrant a class |
| `SessionLifecycle` enum (3 values) | Removed — always PerTest | YAGNI; PerClass/PerAssembly achievable by holding session reference |
| `IBrowserSessionFactory` interface | `CreateSessionAsync()` on `IUITestingPlugin` | Single method, single implementation — premature abstraction |
| 5 Enqueue overloads (0-4 type params) | 2 overloads (0 and 1 type param) | All examples use exactly 1 param; use `ServiceProvider` for 2+ |
| `PlaywrightBrowserManager` class | Merged into `PlaywrightPlugin` | 1:1 lifecycle with plugin; ~30 lines of logic |
| `PlaywrightSessionFactory` class | Merged into `PlaywrightPlugin` | Single implementation of removed interface |
| Phase 6 Roslyn Analyzer | Tracked as separate follow-up issue | Out of scope; DEBUG finalizer provides immediate protection |

**Net result:** ~12 new files reduced to ~7. Three new interfaces reduced to one (`IBrowserSession`). Playwright project: 2 files (`PlaywrightPlugin.cs`, `PlaywrightBrowserSession.cs` + `SessionServiceProvider.cs`).

## References & Research

### Internal References

- Brainstorm: [2026-03-09-foundational-api-redesign-brainstorm.md](../brainstorms/2026-03-09-foundational-api-redesign-brainstorm.md)
- Supersedes: [future-enhancement-framework-escape-hatch.md](../future-enhancement-framework-escape-hatch.md)
- Prior plan pattern: [2026-02-17-refactor-unified-hosting-environment-config-plan.md](2026-02-17-refactor-unified-hosting-environment-config-plan.md)

### Key Files

- Current Page: `src/FluentUIScaffold.Core/Pages/Page.cs`
- Current AppScaffold: `src/FluentUIScaffold.Core/AppScaffold.cs`
- Current Builder: `src/FluentUIScaffold.Core/Configuration/FluentUIScaffoldBuilder.cs`
- Current PlaywrightPlugin: `src/FluentUIScaffold.Playwright/PlaywrightPlugin.cs`
- Sample Pages: `samples/SampleApp.Tests/Pages/`

### External References

- [Await Anything - .NET Blog (Stephen Toub)](https://devblogs.microsoft.com/dotnet/await-anything/) — custom awaitable pattern
- [ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/) — SynchronizationContext guidance
- [Async Unit Testing (Stephen Cleary)](https://learn.microsoft.com/en-us/archive/msdn-magazine/2014/november/async-programming-unit-testing-asynchronous-code) — test framework async behavior
- [Microsoft DI Guidelines](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/guidelines) — scoped service management
- [AsyncServiceScope in .NET 6](https://andrewlock.net/exploring-dotnet-6-part-10-new-dependency-injection-features-in-dotnet-6/) — async scope disposal
- [Playwright Browser Contexts](https://playwright.dev/docs/browser-contexts) — isolation model
- [Playwright .NET Test Runners](https://playwright.dev/dotnet/docs/test-runners) — PageTest/ContextTest hierarchy
