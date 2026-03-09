# Page Object Pattern

## Overview

The Page Object Pattern in FluentUIScaffold creates an abstraction layer between your test code and the browser. Each page object represents a page (or section) of your application, encapsulating all interactions as a deferred execution chain. Actions are not executed immediately -- they are queued and run only when the chain is awaited.

`Page<TSelf>` is both a fluent chain builder and an awaitable object. Every method call enqueues an action and returns the page for further chaining. When you `await` the chain, all queued actions execute in order.

## Benefits

- **Deferred execution**: Actions queue up and run only on `await`, giving you full control over when side effects happen
- **Cross-page chaining**: Navigate between pages in a single fluent chain without breaking the flow
- **Framework-agnostic**: Page methods use `Enqueue<T>` to resolve driver-specific services (e.g., Playwright's `IPage`) from DI
- **Maintainability**: Centralizes selectors and page logic in one place
- **Readability**: Tests read like a script describing user behavior

## Core Components

### Page&lt;TSelf&gt;

The base class for all page objects. It implements `GetAwaiter()` so that the entire action chain executes when awaited.

```csharp
public abstract class Page<TSelf>
    where TSelf : Page<TSelf>
{
    // Constructor
    protected Page(IServiceProvider serviceProvider);

    // Queue an action (no DI)
    protected TSelf Enqueue(Func<Task> action);

    // Queue an action with a DI-resolved service
    protected TSelf Enqueue<T>(Func<T, Task> action);

    // Navigate to another page, freezing the current one
    public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;

    // Awaiter support -- executes all queued actions
    public TaskAwaiter GetAwaiter();
}
```

Key points:

- **`Enqueue(Func<Task>)`** -- queues an action that takes no DI service.
- **`Enqueue<T>(Func<T, Task>)`** -- queues an action that receives a DI-resolved `T`. For Playwright, this is typically `IPage`.
- **`NavigateTo<TTarget>()`** -- freezes the current page (no more actions can be enqueued on it) and returns a new page that shares the same action queue. The chain continues seamlessly on the target page.
- **`GetAwaiter()`** -- makes the page awaitable. When you `await` any page in the chain, every queued action from every page in the chain runs in order.
- All methods return `TSelf` (or `TTarget` for navigation), enabling fluent chaining.

### Frozen Pages

When you call `NavigateTo<TTarget>()`, the current page becomes frozen. Any attempt to enqueue further actions on a frozen page throws `FrozenPageException`. This enforces a linear chain where actions always flow forward.

```csharp
var homePage = app.NavigateTo<HomePage>();
var loginPage = homePage.NavigateTo<LoginPage>();

// This throws FrozenPageException -- homePage is frozen
homePage.DoSomething();
```

## Creating Page Objects

### Basic Page Object

Define a page by extending `Page<TSelf>`, decorating with `[Route]`, and writing methods that use `Enqueue<IPage>` to interact with the browser.

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    protected LoginPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public LoginPage EnterEmail(string email)
    {
        return Enqueue<IPage>(async page =>
        {
            await page.FillAsync("#email-input", email).ConfigureAwait(false);
        });
    }

    public LoginPage EnterPassword(string password)
    {
        return Enqueue<IPage>(async page =>
        {
            await page.FillAsync("#password-input", password).ConfigureAwait(false);
        });
    }

    public LoginPage Submit()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("#login-button").ConfigureAwait(false);
        });
    }
}
```

### Page with Verification Methods

Verification methods are just regular page methods that enqueue assertions.

```csharp
[Route("/dashboard")]
public class DashboardPage : Page<DashboardPage>
{
    protected DashboardPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public DashboardPage VerifyWelcomeVisible()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator("[data-testid='welcome-message']")
                .WaitForAsync()
                .ConfigureAwait(false);
        });
    }

    public DashboardPage VerifyTitleContains(string expectedText)
    {
        return Enqueue<IPage>(async page =>
        {
            var title = await page.TitleAsync().ConfigureAwait(false);
            if (!title.Contains(expectedText))
                throw new AssertionException($"Expected title to contain '{expectedText}', but was '{title}'");
        });
    }

    public DashboardPage ClickLogout()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("[data-testid='logout-button']").ConfigureAwait(false);
        });
    }
}
```

### Advanced Page Object

For pages with more complex interactions, compose multiple enqueued actions within a single method or chain calls together.

```csharp
[Route("/users")]
public class UserManagementPage : Page<UserManagementPage>
{
    protected UserManagementPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public UserManagementPage SearchUser(string searchTerm)
    {
        return Enqueue<IPage>(async page =>
        {
            await page.FillAsync("[data-testid='search-input']", searchTerm).ConfigureAwait(false);
            await page.PressAsync("[data-testid='search-input']", "Enter").ConfigureAwait(false);
        });
    }

    public UserManagementPage FilterByRole(string role)
    {
        return Enqueue<IPage>(async page =>
        {
            await page.SelectOptionAsync("[data-testid='role-filter']", role).ConfigureAwait(false);
        });
    }

    public UserManagementPage VerifyUserVisible(string userName)
    {
        return Enqueue<IPage>(async page =>
        {
            await page.Locator($"[data-testid='user-row']:has-text('{userName}')")
                .WaitForAsync()
                .ConfigureAwait(false);
        });
    }

    public UserManagementPage ClickCreateUser()
    {
        return Enqueue<IPage>(async page =>
        {
            await page.ClickAsync("[data-testid='create-user-button']").ConfigureAwait(false);
        });
    }
}
```

## Cross-Page Navigation

### Chaining Across Pages with NavigateTo

`NavigateTo<TTarget>()` freezes the current page and returns a new page instance that shares the same action queue. The entire chain executes in order when awaited.

```csharp
await app.NavigateTo<HomePage>()
    .VerifyWelcomeVisible()
    .NavigateTo<LoginPage>()
    .EnterEmail("test@example.com")
    .EnterPassword("s3cret")
    .Submit();
```

This chain:
1. Navigates to `HomePage`
2. Verifies the welcome message is visible
3. Navigates to `LoginPage` (freezing `HomePage`)
4. Fills in the email and password
5. Clicks the submit button

All five steps execute sequentially when `await` is reached.

### Multi-Page Workflows

You can chain across as many pages as needed.

```csharp
await app.NavigateTo<LoginPage>()
    .EnterEmail("admin@example.com")
    .EnterPassword("admin-pass")
    .Submit()
    .NavigateTo<DashboardPage>()
    .VerifyWelcomeVisible()
    .NavigateTo<UserManagementPage>()
    .SearchUser("john")
    .VerifyUserVisible("John Doe");
```

## Route Attribute and Parameterized Routes

### Basic Routes

Use the `[Route]` attribute to define the URL path for a page. The path is combined with the configured `BaseUrl`.

```csharp
[Route("/login")]
public class LoginPage : Page<LoginPage>
{
    protected LoginPage(IServiceProvider serviceProvider) : base(serviceProvider) { }
    // ...
}

// Navigates to: http://localhost:5000/login
await app.NavigateTo<LoginPage>().EnterEmail("user@example.com").Submit();
```

### Parameterized Routes

For pages with dynamic URL segments, use `{placeholder}` syntax in the route.

```csharp
[Route("/users/{userId}")]
public class UserProfilePage : Page<UserProfilePage>
{
    protected UserProfilePage(IServiceProvider serviceProvider) : base(serviceProvider) { }
    // ...
}

// Pass parameters as an anonymous object
await app.NavigateTo<UserProfilePage>(new { userId = "123" })
    .VerifyProfileLoaded();
// Navigates to: http://localhost:5000/users/123
```

### Multiple Parameters

```csharp
[Route("/users/{userId}/posts/{postId}")]
public class UserPostPage : Page<UserPostPage>
{
    protected UserPostPage(IServiceProvider serviceProvider) : base(serviceProvider) { }
    // ...
}

await app.NavigateTo<UserPostPage>(new { userId = "456", postId = "789" })
    .VerifyPostContent();
// Navigates to: http://localhost:5000/users/456/posts/789
```

### SPA and Hash-Based Routing

For SPAs with hash-based routing, include the hash in the BaseUrl configuration:

```csharp
// Configuration
.Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost:5000/#"))

// Page definition
[Route("/login")]
public class LoginPage : Page<LoginPage> { /* ... */ }

// Results in: http://localhost:5000/#/login
```

## Using Enqueue

### Enqueue without DI

Use the no-argument `Enqueue` when you do not need a DI-resolved service.

```csharp
public LoginPage LogAction(string message)
{
    return Enqueue(async () =>
    {
        Console.WriteLine($"[{DateTime.UtcNow}] {message}");
        await Task.CompletedTask.ConfigureAwait(false);
    });
}
```

### Enqueue with DI-Resolved Services

Use `Enqueue<T>` to resolve a service from the DI container. For browser interactions, `T` is typically Playwright's `IPage`.

```csharp
public LoginPage EnterEmail(string email)
{
    return Enqueue<IPage>(async page =>
    {
        await page.FillAsync("[data-testid='email-input']", email).ConfigureAwait(false);
    });
}
```

You can resolve any registered service, not just `IPage`:

```csharp
public LoginPage DoSomethingCustom()
{
    return Enqueue<IMyCustomService>(async service =>
    {
        await service.PerformActionAsync().ConfigureAwait(false);
    });
}
```

## Testing with Page Objects

### Basic Test

```csharp
[TestClass]
public class LoginTests
{
    [TestMethod]
    public async Task Can_Login_Successfully()
    {
        await TestAssemblyHooks.App.NavigateTo<LoginPage>()
            .EnterEmail("test@example.com")
            .EnterPassword("password123")
            .Submit()
            .NavigateTo<DashboardPage>()
            .VerifyWelcomeVisible();
    }
}
```

### Test with Parameterized Route

```csharp
[TestMethod]
public async Task Can_View_User_Profile()
{
    await TestAssemblyHooks.App.NavigateTo<UserProfilePage>(new { userId = "42" })
        .VerifyProfileLoaded()
        .VerifyDisplayName("Jane Doe");
}
```

## Best Practices

### 1. Keep Methods Focused

Each page method should represent a single user action. Avoid bundling multiple unrelated interactions into one method.

```csharp
// Good -- one action per method
public LoginPage EnterEmail(string email)
{
    return Enqueue<IPage>(async page =>
    {
        await page.FillAsync("[data-testid='email-input']", email).ConfigureAwait(false);
    });
}

public LoginPage Submit()
{
    return Enqueue<IPage>(async page =>
    {
        await page.ClickAsync("[data-testid='submit-button']").ConfigureAwait(false);
    });
}

// Acceptable -- a convenience method that composes focused methods
public LoginPage Login(string email, string password)
{
    return EnterEmail(email).EnterPassword(password).Submit();
}
```

### 2. Use data-testid Selectors

Prefer `[data-testid='...']` selectors over CSS classes or element IDs. They are stable, decoupled from styling, and signal explicit test contracts.

```csharp
// Good
await page.ClickAsync("[data-testid='submit-button']").ConfigureAwait(false);

// Fragile -- tied to styling
await page.ClickAsync(".btn-primary.submit").ConfigureAwait(false);

// Fragile -- IDs may be auto-generated or change
await page.ClickAsync("#submit-btn").ConfigureAwait(false);
```

### 3. Always Use ConfigureAwait(false)

All `await` calls inside `Enqueue` lambdas should use `ConfigureAwait(false)` to avoid deadlocks and unnecessary synchronization context captures.

```csharp
return Enqueue<IPage>(async page =>
{
    await page.FillAsync("#input", "value").ConfigureAwait(false);
    await page.ClickAsync("#button").ConfigureAwait(false);
});
```

### 4. Avoid Storing Mutable State in Pages

Pages are chain builders, not stateful objects. Do not store mutable state (flags, counters) on the page instance. The deferred execution model means state would be set at enqueue time, not execution time.

### 5. Group Related Pages Together

Organize page objects in a `Pages/` folder that mirrors your application structure.

```
Tests/
  Pages/
    LoginPage.cs
    DashboardPage.cs
    Users/
      UserManagementPage.cs
      UserProfilePage.cs
```

### 6. Name Methods After User Intent

Method names should describe what the user is doing, not how the automation works.

```csharp
// Good -- describes user intent
public LoginPage EnterEmail(string email) { /* ... */ }
public LoginPage Submit() { /* ... */ }

// Bad -- describes implementation details
public LoginPage FillEmailInputField(string email) { /* ... */ }
public LoginPage ClickSubmitButton() { /* ... */ }
```

## Conclusion

The Page Object Pattern in FluentUIScaffold provides a deferred-execution, fluent API for writing maintainable UI tests. By building chains of actions that execute on `await`, cross-page workflows become simple linear sequences. The `Enqueue<T>` mechanism keeps pages framework-agnostic while still giving you full access to the underlying driver through DI.

For more information, see the [API Reference](api-reference.md) and [Getting Started](getting-started.md) guides.
