# Page Object Pattern

FluentUIScaffold's `Page<TSelf>` is a deferred execution chain builder that implements the page object pattern with a fluent API. Actions are queued up as you call methods, then all execute together when the chain is awaited.

## Overview

Traditional page objects return completed tasks from each method. FluentUIScaffold takes a different approach: each method queues an action and returns `this`, allowing you to build fluent chains that execute atomically on `await`.

```csharp
// Actions queue up...
var chain = app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .ClickCounter()
    .ClickCounter();

// ...and execute here
await chain;
```

This deferred model enables cross-page navigation within a single chain and prevents sync-over-async issues.

## Anatomy of a Page Object

A page object consists of four elements:

1. **Class declaration** with self-referencing generic
2. **Route attribute** declaring the URL path
3. **Protected constructor** accepting `IServiceProvider`
4. **Methods** that enqueue actions and return `TSelf`

```csharp
using FluentUIScaffold.Core.Pages;
using Microsoft.Playwright;

[Route("/")]
public class HomePage : Page<HomePage>
{
    protected HomePage(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public HomePage ClickCounter()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
        });
    }

    public HomePage VerifyWelcomeVisible()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator("h2:has-text('Welcome')").WaitForAsync().ConfigureAwait(false);
        });
    }
}
```

Key rules:
- The constructor **must** be `protected` and accept `IServiceProvider`.
- The class inherits from `Page<TSelf>` where `TSelf` is the class itself -- this enables the fluent API to return the correct type.
- Methods return `TSelf` (which is `HomePage` in this case) so the chain stays fluent.

## Enqueue Patterns

`Page<TSelf>` provides two `Enqueue` overloads for queuing actions.

### Enqueue without DI

`Enqueue(Func<Task>)` queues an action that takes no injected services:

```csharp
public HomePage LogMessage()
{
    return Enqueue(async () =>
    {
        await Task.Delay(100).ConfigureAwait(false);
        // Any async work that doesn't need DI
    });
}
```

### Enqueue with DI

`Enqueue<T>(Func<T, Task>)` queues an action that resolves `T` from DI at execution time:

```csharp
public HomePage ClickCounter()
{
    return Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("button:has-text('count is')").ConfigureAwait(false);
    });
}
```

The type parameter `T` is resolved from the session's `IServiceProvider` when the action runs, not when it is enqueued. This is important because the session (and its `IPage` instance) may not exist yet when you build the chain.

### Enqueue with IPage (most common)

The most common pattern is `Enqueue<IPage>(...)` which gives you Playwright's `IPage` for direct browser interaction. This provides full access to Playwright's API without any wrapper layer:

```csharp
public HomePage FillSearchBox(string query)
{
    return Enqueue<IPage>(async page =>
    {
        await page.Locator("#search").FillAsync(query).ConfigureAwait(false);
    });
}
```

## Deferred Execution

Actions do not run immediately. They are queued and execute in order when the chain is awaited.

`Page<TSelf>` implements a custom awaitable via `GetAwaiter()` which returns a `TaskAwaiter`. When you `await` a page chain, all queued actions execute sequentially:

```csharp
// Nothing has executed yet -- three actions are queued
var page = app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .ClickCounter()
    .ClickCounter();

// Now all three actions execute in order
await page;
```

You can also await inline:

```csharp
await app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .ClickCounter();
```

### Unawaited Chain Warning

In DEBUG builds, a finalizer warns via `Trace.TraceWarning` if a chain with queued actions is never awaited. Always `await` your chains to ensure actions execute.

## Cross-Page Navigation

`NavigateTo<TTarget>()` enables fluent navigation between pages within a single chain. When called, it:

1. **Freezes** the current page (no more actions can be enqueued on it)
2. **Creates** the target page, sharing the same action list
3. **Returns** the target page for continued chaining

```csharp
await app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .NavigateTo<LoginPage>()    // HomePage is frozen here
    .ClickLoginTab()
    .EnterEmail("test@example.com");
```

All actions from both pages execute in a single sequential pass when awaited.

If you try to enqueue actions on a frozen page, a `FrozenPageException` is thrown:

```csharp
var home = app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible();

var login = home.NavigateTo<LoginPage>(); // home is now frozen

home.ClickCounter(); // Throws FrozenPageException
```

## Parameterized Routes

Routes can contain parameters using `{paramName}` placeholders:

```csharp
[Route("/users/{userId}")]
public class UserPage : Page<UserPage>
{
    protected UserPage(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public UserPage VerifyUserName(string name)
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator($"text={name}").WaitForAsync().ConfigureAwait(false);
        });
    }
}
```

Supply route parameters as an anonymous object when navigating:

```csharp
await app.NavigateTo<UserPage>(new { userId = "123" })
    .VerifyUserName("Alice");
// Navigates to: http://localhost:5000/users/123
```

## Common Playwright Interactions

Within `Enqueue<IPage>(...)`, you have full access to Playwright's `IPage` API. Here are common patterns:

### Clicking

```csharp
public HomePage ClickButton()
{
    return Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("button#submit").ConfigureAwait(false);
    });
}
```

### Filling Input Fields

```csharp
public LoginPage EnterEmail(string email)
{
    return Enqueue<IPage>(async page =>
    {
        await page.Locator("#email").FillAsync(email).ConfigureAwait(false);
    });
}
```

### Waiting for Elements

```csharp
public HomePage VerifyLoaded()
{
    return Enqueue<IPage>(async page =>
    {
        await page.Locator(".content").WaitForAsync().ConfigureAwait(false);
    });
}
```

### Using Locator Patterns

```csharp
public HomePage VerifyItemCount(int expected)
{
    return Enqueue<IPage>(async page =>
    {
        var items = page.Locator(".list-item");
        await Assertions.Expect(items).ToHaveCountAsync(expected).ConfigureAwait(false);
    });
}
```

### Text Content Assertions

```csharp
public HomePage VerifyTitle(string title)
{
    return Enqueue<IPage>(async page =>
    {
        var heading = page.Locator("h1");
        await Assertions.Expect(heading).ToHaveTextAsync(title).ConfigureAwait(false);
    });
}
```

## Best Practices

### Always use ConfigureAwait(false) on internal awaits

Inside `Enqueue` callbacks, always append `.ConfigureAwait(false)` to awaited calls. Enqueue callbacks are library-internal code -- they don't need to resume on the caller's `SynchronizationContext`. Without `ConfigureAwait(false)`, if the library is ever used in a context with a sync context (e.g., a UI thread), the continuation could try to marshal back and deadlock. In typical test runners this is a no-op, but it's a defensive habit that keeps the internals safe regardless of the host environment:

```csharp
// Correct
return Enqueue<IPage>(async page =>
{
    await page.ClickAsync("button").ConfigureAwait(false);
});
```

### Always await your chains

Every chain with queued actions must be awaited. If you forget, actions will never execute. In DEBUG builds, the finalizer will emit a trace warning, but you should not rely on this.

```csharp
// Correct
await app.NavigateTo<HomePage>().VerifyWelcomeVisible();

// Wrong -- actions are never executed
app.NavigateTo<HomePage>().VerifyWelcomeVisible();
```

### Keep page methods thin

Each method should do one logical thing. This keeps the fluent chain readable and makes tests easy to compose:

```csharp
// Good -- each method does one thing
await app.NavigateTo<LoginPage>()
    .EnterEmail("test@example.com")
    .EnterPassword("secret")
    .ClickSubmit()
    .VerifyLoginSuccess();

// Avoid -- doing too much in one method
await app.NavigateTo<LoginPage>()
    .LoginAndVerify("test@example.com", "secret");
```

### Use the On method for pages you are already on

If a navigation has already happened (e.g., a redirect after login) and you want to interact with the current page without triggering a new navigation, use `On<TPage>()`:

```csharp
await app.NavigateTo<LoginPage>()
    .EnterEmail("test@example.com")
    .EnterPassword("secret")
    .ClickSubmit();

// Already on the dashboard after login redirect
await app.On<DashboardPage>()
    .VerifyWelcomeMessage();
```

## Further Reading

- [Getting Started](getting-started.md) -- project setup and first test
- [API Reference](api-reference.md) -- complete `Page<TSelf>` and `AppScaffold` API documentation
