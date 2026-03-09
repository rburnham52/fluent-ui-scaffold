# Future Enhancement: Framework Escape Hatch & IUIDriver Refactoring

> **Note (Phase 4 — Foundation Redesign):** The concerns in this document have been largely addressed by the foundational API redesign described in [2026-03-09-foundational-api-redesign-brainstorm.md](brainstorms/2026-03-09-foundational-api-redesign-brainstorm.md). The `IUIDriver` abstraction has been removed from the public-facing API, and the "escape hatch" problem is solved by the `Enqueue<T>()` pattern, which gives page objects direct access to native framework services via DI. This document is retained as historical context.

## Original Problem Statement

`IUIDriver` reimplemented ~19 methods that mirrored what Playwright (and other frameworks) already provide — Click, Type, Hover, Focus, SelectOption, etc. This created a thin abstraction that added maintenance overhead without much value, since the fluent `Page<TSelf>` layer was what test code actually used.

Meanwhile, when tests needed framework-specific capabilities (network interception, geolocation, device emulation), there was no clean pattern to "escape" from the fluent API into the native framework.

## How the Redesign Solves This

### Enqueue<T>() as the Universal Escape Hatch

The foundation redesign replaces `IUIDriver` entirely. Page objects now use `Enqueue<T>()` to get direct access to any registered service — including Playwright's native `IPage`, `IBrowserContext`, `IBrowser`, or custom services — via DI injection into action lambdas:

```csharp
[Route("/products")]
public class ProductsPage : Page<ProductsPage>
{
    protected ProductsPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    // Direct Playwright IPage access — no IUIDriver indirection
    public ProductsPage SearchFor(string query)
        => Enqueue(async (IPage page) =>
        {
            await page.GetByPlaceholder("Search...").FillAsync(query);
            await page.GetByRole(AriaRole.Button, new() { Name = "Search" }).ClickAsync();
        });

    // Framework-specific capabilities that previously had no clean escape hatch
    public ProductsPage MockApiResponses()
        => Enqueue(async (IPage page) =>
        {
            await page.RouteAsync("**/api/*", route => route.FulfillAsync(new()
            {
                Body = "{\"mocked\": true}"
            }));
        });

    // Multiple services injected — IBrowserContext for cookies, ILogger for diagnostics
    public ProductsPage ClearCookiesAndLog()
        => Enqueue(async (IBrowserContext context, ILogger<ProductsPage> logger) =>
        {
            logger.LogInformation("Clearing cookies");
            await context.ClearCookiesAsync();
        });
}
```

### Why This Works Better Than the Original Proposals

The original document proposed three options:

- **Option A (Lambda-based access on AppScaffold)** — Required a separate `app.Framework<T>()` call outside the fluent chain, breaking flow.
- **Option B (Direct DI resolution)** — Similar idea but scattered framework calls across test methods rather than encapsulating them in page objects.
- **Option C (Page-level UseFramework)** — Closest to what we implemented, but as a separate `UseFramework<T>()` method alongside the existing element-based API, creating two parallel interaction patterns.

`Enqueue<T>()` unifies all of these. There is no separate "escape hatch" because **all page interactions go through `Enqueue<T>()`**. Whether you are clicking a button, intercepting network requests, or reading cookies, the pattern is the same: resolve services from DI, execute async actions, return `TSelf` for fluent chaining.

### Key Differences from the Original Design

| Original Design | Foundation Redesign |
|----------------|---------------------|
| `IUIDriver` with 19+ methods | Removed — no driver abstraction |
| `Element` / `IElement` abstraction | Removed — use Playwright locators directly |
| `PlaywrightAdvancedFeatures` in DI | Not needed — `IPage` / `IBrowserContext` available directly |
| `GetFrameworkDriver<T>()` on IUIDriver | Replaced by `Enqueue<T>()` DI injection |
| Separate "escape hatch" API | No escape needed — all interactions are native |
| Sync-over-async throughout | Fully async via deferred execution chains |

### Session Isolation

The plugin + session architecture also addresses lifecycle concerns. `IBrowserSessionFactory` creates isolated sessions (one `IBrowserContext` + `IPage` per test), so framework access is always scoped correctly. There is no risk of one test's network interception leaking into another.

## Related

- Foundational redesign brainstorm: [2026-03-09-foundational-api-redesign-brainstorm.md](brainstorms/2026-03-09-foundational-api-redesign-brainstorm.md)
