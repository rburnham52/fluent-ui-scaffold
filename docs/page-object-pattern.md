# Page Object Pattern

## Overview

The Page Object Pattern is a design pattern that creates an abstraction layer between your test code and the web page elements. FluentUIScaffold provides a robust implementation of this pattern through the `Page<TSelf>` class.

## Benefits

- **Maintainability**: Centralizes element selectors and page logic
- **Reusability**: Page objects can be reused across multiple tests
- **Readability**: Tests become more readable and self-documenting
- **Reliability**: Reduces test flakiness through proper element handling
- **Separation of Concerns**: Separates test logic from page interaction logic

## Core Components

### Page<TSelf>

The base class for all page objects in FluentUIScaffold with a single self-referencing generic for fluent API context.

```csharp
public abstract class Page<TSelf> : IAsyncDisposable
    where TSelf : Page<TSelf>
{
    // Properties
    public IServiceProvider ServiceProvider { get; }
    public IUIDriver Driver { get; }
    public Uri UrlPattern { get; }
    protected ILogger Logger { get; }
    protected FluentUIScaffoldOptions Options { get; }

    // Constructor
    protected Page(IServiceProvider serviceProvider, Uri urlPattern);

    // Element Building
    protected ElementBuilder Element(string selector);

    // Abstract Configuration
    protected abstract void ConfigureElements();

    // Navigation
    public virtual TSelf Navigate();
    public TTarget NavigateTo<TTarget>() where TTarget : Page<TTarget>;

    // Fluent Interactions (all return TSelf)
    public virtual TSelf Click(Func<TSelf, IElement> elementSelector);
    public virtual TSelf Type(Func<TSelf, IElement> elementSelector, string text);
    public virtual TSelf Select(Func<TSelf, IElement> elementSelector, string value);
    public virtual TSelf Clear(Func<TSelf, IElement> elementSelector);
    public virtual TSelf Focus(Func<TSelf, IElement> elementSelector);
    public virtual TSelf Hover(Func<TSelf, IElement> elementSelector);
    public virtual TSelf WaitForVisible(Func<TSelf, IElement> elementSelector);
    public virtual TSelf WaitForHidden(Func<TSelf, IElement> elementSelector);

    // Verification
    public IVerificationContext<TSelf> Verify { get; }

    // Page Validation
    public virtual bool IsCurrentPage();
    public virtual void ValidateCurrentPage();
}
```

## Creating Page Objects

### Basic Page Object

```csharp
public class LoginPage : Page<LoginPage>
{
    public IElement EmailInput { get; private set; } = null!;
    public IElement PasswordInput { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;
    public IElement ErrorMessage { get; private set; } = null!;

    public LoginPage(IServiceProvider serviceProvider, Uri urlPattern)
        : base(serviceProvider, urlPattern)
    {
    }

    protected override void ConfigureElements()
    {
        EmailInput = Element("#email")
            .WithDescription("Email Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        PasswordInput = Element("#password")
            .WithDescription("Password Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        LoginButton = Element("#login-btn")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();

        ErrorMessage = Element(".error-message")
            .WithDescription("Error Message")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();
    }

    public LoginPage EnterEmail(string email)
    {
        Logger.LogInformation($"Entering email: {email}");
        return Type(p => p.EmailInput, email);
    }

    public LoginPage EnterPassword(string password)
    {
        Logger.LogInformation("Entering password");
        return Type(p => p.PasswordInput, password);
    }

    public HomePage ClickLogin()
    {
        Logger.LogInformation("Clicking login button");
        Click(p => p.LoginButton);
        return NavigateTo<HomePage>();
    }

    public LoginPage VerifyErrorMessage(string expectedMessage)
    {
        Verify.TextContains(p => p.ErrorMessage, expectedMessage);
        return this;
    }
}
```

### Fluent API Methods

The `Page<TSelf>` provides fluent API methods for element interactions:

```csharp
// Element interaction methods
page.Click(p => p.ElementName)
page.Type(p => p.ElementName, "text")
page.Select(p => p.ElementName, "value")
page.Focus(p => p.ElementName)
page.Hover(p => p.ElementName)
page.Clear(p => p.ElementName)

// Wait methods
page.WaitForVisible(p => p.ElementName)
page.WaitForHidden(p => p.ElementName)

// Verification methods via Verify property
page.Verify.Visible(p => p.ElementName)
page.Verify.TextContains(p => p.ElementName, "expected text")
page.Verify.And  // Returns to page for continued interaction
```

### Advanced Page Object with Complex Logic

```csharp
public class UserManagementPage : Page<UserManagementPage>
{
    public IElement CreateUserButton { get; private set; } = null!;
    public IElement UserTable { get; private set; } = null!;
    public IElement SearchInput { get; private set; } = null!;
    public IElement FilterDropdown { get; private set; } = null!;

    public UserManagementPage(IServiceProvider serviceProvider, Uri urlPattern)
        : base(serviceProvider, urlPattern)
    {
    }

    protected override void ConfigureElements()
    {
        CreateUserButton = Element("[data-testid='create-user-btn']")
            .WithDescription("Create User Button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .Build();

        UserTable = Element("#users-table")
            .WithDescription("Users Table")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        SearchInput = Element("#search-users")
            .WithDescription("Search Users Input")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();

        FilterDropdown = Element("#filter-users")
            .WithDescription("Filter Users Dropdown")
            .WithWaitStrategy(WaitStrategy.Visible)
            .Build();
    }

    public CreateUserPage ClickCreateUser()
    {
        Logger.LogInformation("Navigating to create user page");
        Click(p => p.CreateUserButton);
        return NavigateTo<CreateUserPage>();
    }

    public UserManagementPage SearchUser(string searchTerm)
    {
        Logger.LogInformation($"Searching for user: {searchTerm}");
        return Type(p => p.SearchInput, searchTerm);
    }

    public UserManagementPage FilterByRole(string role)
    {
        Logger.LogInformation($"Filtering by role: {role}");
        return Select(p => p.FilterDropdown, role);
    }

    public UserManagementPage VerifyUserExists(string userName)
    {
        Verify.TextContains(p => p.UserTable, userName);
        return this;
    }
}
```

## Element Configuration

### Element Definition

Elements are defined using the `Element()` method in the `ConfigureElements()` method:

```csharp
protected override void ConfigureElements()
{
    // Basic element
    var button = Element("#submit-button").Build();

    // Element with description
    var input = Element("#email")
        .WithDescription("Email Input Field")
        .Build();

    // Element with timeout
    var dropdown = Element("#country-select")
        .WithTimeout(TimeSpan.FromSeconds(10))
        .Build();

    // Element with wait strategy
    var loadingSpinner = Element(".loading-spinner")
        .WithWaitStrategy(WaitStrategy.Hidden)
        .Build();

    // Complex element configuration
    var complexElement = Element("[data-testid='user-card']")
        .WithDescription("User Card Component")
        .WithTimeout(TimeSpan.FromSeconds(15))
        .WithWaitStrategy(WaitStrategy.Visible)
        .WithRetryInterval(TimeSpan.FromMilliseconds(200))
        .Build();
}
```

### Element Properties

```csharp
public interface IElement
{
    string Selector { get; }
    string Description { get; }
    TimeSpan Timeout { get; }
    WaitStrategy WaitStrategy { get; }

    void Click();
    void Type(string text);
    void SelectOption(string value);
    string GetText();
    string GetValue();
    string GetAttribute(string attributeName);
    bool IsVisible();
    bool IsEnabled();
    void WaitForVisible();
    void Clear();
    void Focus();
    void Hover();
}
```

## Page Navigation

### Navigation Methods

```csharp
// Navigate to another page
public HomePage ClickLogin()
{
    Click(p => p.LoginButton);
    return NavigateTo<HomePage>();
}

// Navigate with parameters
public UserProfilePage OpenUserProfile(int userId)
{
    var profileLink = Element($"[data-user-id='{userId}']").Build();
    profileLink.Click();
    return NavigateTo<UserProfilePage>();
}

// Conditional navigation
public T NavigateToPage<T>() where T : Page<T>
{
    return NavigateTo<T>();
}
```

### Page Validation

```csharp
// Check if currently on this page
if (loginPage.IsCurrentPage())
{
    // Current page logic
}

// Validate current page (throws exception if not on correct page)
loginPage.ValidateCurrentPage();

// Custom validation
public override bool IsCurrentPage()
{
    return Driver.CurrentUrl.ToString().Contains("/login") &&
           Driver.IsVisible("#login-form");
}
```

## Verification Context

### Using Verification

```csharp
public LoginPage VerifyLoginForm()
{
    Verify
        .Visible(p => p.EmailInput)
        .Visible(p => p.PasswordInput)
        .Visible(p => p.LoginButton)
        .NotVisible(p => p.ErrorMessage);

    return this;
}

public LoginPage VerifyErrorMessage(string expectedMessage)
{
    Verify.TextContains(p => p.ErrorMessage, expectedMessage);
    return this;
}

public LoginPage VerifyPageLoaded()
{
    Verify
        .UrlContains("/login")
        .TitleContains("Login");

    return this;
}
```

### Fluent Verification with And

```csharp
// Verify and continue interacting
page.Verify
    .TitleContains("Dashboard")
    .UrlContains("/dashboard")
    .Visible(p => p.WelcomeMessage)
    .And  // Returns to page
    .Click(p => p.LogoutButton);
```

### Custom Verification

```csharp
public UserManagementPage VerifyUserTableNotEmpty()
{
    Verify.That(
        () => GetUserCount(),
        count => count > 0,
        "User table should not be empty"
    );
    return this;
}

private int GetUserCount()
{
    // Your logic to count users
    return 5;
}
```

## Best Practices

### 1. Element Organization

```csharp
public class WellOrganizedPage : Page<WellOrganizedPage>
{
    // Login elements
    public IElement EmailInput { get; private set; } = null!;
    public IElement PasswordInput { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;

    // Form elements
    public IElement FirstNameInput { get; private set; } = null!;
    public IElement LastNameInput { get; private set; } = null!;
    public IElement SubmitButton { get; private set; } = null!;

    // Navigation elements
    public IElement HomeLink { get; private set; } = null!;
    public IElement ProfileLink { get; private set; } = null!;
    public IElement LogoutLink { get; private set; } = null!;

    public WellOrganizedPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        ConfigureLoginElements();
        ConfigureFormElements();
        ConfigureNavigationElements();
    }

    private void ConfigureLoginElements()
    {
        EmailInput = Element("#email").WithDescription("Email Input").Build();
        PasswordInput = Element("#password").WithDescription("Password Input").Build();
        LoginButton = Element("#login-btn").WithDescription("Login Button").Build();
    }

    private void ConfigureFormElements()
    {
        FirstNameInput = Element("#first-name").WithDescription("First Name Input").Build();
        LastNameInput = Element("#last-name").WithDescription("Last Name Input").Build();
        SubmitButton = Element("#submit").WithDescription("Submit Button").Build();
    }

    private void ConfigureNavigationElements()
    {
        HomeLink = Element("#home-link").WithDescription("Home Link").Build();
        ProfileLink = Element("#profile-link").WithDescription("Profile Link").Build();
        LogoutLink = Element("#logout-link").WithDescription("Logout Link").Build();
    }
}
```

### 2. Method Chaining

```csharp
public class ChainedPage : Page<ChainedPage>
{
    public IElement EmailInput { get; private set; } = null!;
    public IElement PasswordInput { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;

    public ChainedPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        EmailInput = Element("#email").Build();
        PasswordInput = Element("#password").Build();
        LoginButton = Element("#login-btn").Build();
    }

    public ChainedPage EnterEmail(string email)
    {
        return Type(p => p.EmailInput, email);
    }

    public ChainedPage EnterPassword(string password)
    {
        return Type(p => p.PasswordInput, password);
    }

    public ChainedPage ClickLoginButton()
    {
        return Click(p => p.LoginButton);
    }

    // Combined method using fluent API
    public ChainedPage Login(string email, string password)
    {
        return Type(p => p.EmailInput, email)
               .Type(p => p.PasswordInput, password)
               .Click(p => p.LoginButton);
    }
}

// Usage: page.EnterEmail("test@example.com").EnterPassword("password").ClickLoginButton();
// Or: page.Login("test@example.com", "password");
```

### 3. Error Handling

```csharp
public class RobustPage : Page<RobustPage>
{
    public IElement Button { get; private set; } = null!;

    public RobustPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        Button = Element("#button")
            .WithWaitStrategy(WaitStrategy.Clickable)
            .WithTimeout(TimeSpan.FromSeconds(10))
            .Build();
    }

    public RobustPage ClickButtonSafely()
    {
        try
        {
            return Click(p => p.Button);
        }
        catch (ElementTimeoutException ex)
        {
            Logger.LogWarning($"Button not found: {ex.Selector}");
            // Fallback logic or rethrow
            throw;
        }
    }
}
```

### 4. Page State Management

```csharp
public class StatefulPage : Page<StatefulPage>
{
    private bool _isLoggedIn = false;
    private string? _currentUser = null;

    public IElement EmailInput { get; private set; } = null!;
    public IElement PasswordInput { get; private set; } = null!;
    public IElement LoginButton { get; private set; } = null!;
    public IElement LogoutLink { get; private set; } = null!;

    public StatefulPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        EmailInput = Element("#email").Build();
        PasswordInput = Element("#password").Build();
        LoginButton = Element("#login-btn").Build();
        LogoutLink = Element("#logout").Build();
    }

    public StatefulPage Login(string email, string password)
    {
        Type(p => p.EmailInput, email)
            .Type(p => p.PasswordInput, password)
            .Click(p => p.LoginButton);

        _isLoggedIn = true;
        _currentUser = email;

        return this;
    }

    public StatefulPage Logout()
    {
        if (!_isLoggedIn)
        {
            throw new InvalidOperationException("Not logged in");
        }

        Click(p => p.LogoutLink);
        _isLoggedIn = false;
        _currentUser = null;

        return this;
    }

    public bool IsLoggedIn => _isLoggedIn;
    public string? CurrentUser => _currentUser;
}
```

## Navigation Patterns

### 1. Basic URL Pattern Navigation

The simplest way to navigate is via the `UrlPattern` property and `Navigate()` method:

```csharp
public class LoginPage : Page<LoginPage>
{
    public LoginPage(IServiceProvider sp, Uri urlPattern) : base(sp, urlPattern) { }
    protected override void ConfigureElements() { /* ... */ }
}

// Usage
var loginPage = app.NavigateTo<LoginPage>();
```

### 2. Parameterized Navigation

For pages that need dynamic paths, create custom navigation methods:

```csharp
public class UserProfilePage : Page<UserProfilePage>
{
    public UserProfilePage(IServiceProvider sp, Uri urlPattern) : base(sp, urlPattern) { }

    protected override void ConfigureElements() { /* ... */ }

    public UserProfilePage NavigateToUserProfile(int userId)
    {
        var baseUrl = Options.BaseUrl ?? throw new InvalidOperationException("BaseUrl not configured");
        Driver.NavigateToUrl(new Uri(baseUrl, $"/profile/{userId}"));
        return this;
    }

    public UserProfilePage NavigateToUserProfile(string username)
    {
        var baseUrl = Options.BaseUrl ?? throw new InvalidOperationException("BaseUrl not configured");
        Driver.NavigateToUrl(new Uri(baseUrl, $"/profile/user/{username}"));
        return this;
    }
}
```

### 3. Navigation with Query Parameters

```csharp
public class SearchPage : Page<SearchPage>
{
    public SearchPage(IServiceProvider sp, Uri urlPattern) : base(sp, urlPattern) { }

    protected override void ConfigureElements() { /* ... */ }

    public SearchPage NavigateWithQuery(string query, string category = "all")
    {
        var baseUrl = Options.BaseUrl ?? throw new InvalidOperationException("BaseUrl not configured");
        var queryString = $"q={Uri.EscapeDataString(query)}&category={Uri.EscapeDataString(category)}";
        Driver.NavigateToUrl(new Uri(baseUrl, $"/search?{queryString}"));
        return this;
    }
}
```

## Common Patterns

### 1. Modal Dialogs

```csharp
public class ModalPage : Page<ModalPage>
{
    public IElement ModalOverlay { get; private set; } = null!;
    public IElement ModalContent { get; private set; } = null!;
    public IElement CloseButton { get; private set; } = null!;
    public IElement ConfirmButton { get; private set; } = null!;

    public ModalPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements()
    {
        ModalOverlay = Element(".modal-overlay").Build();
        ModalContent = Element(".modal-content").Build();
        CloseButton = Element(".modal-close").Build();
        ConfirmButton = Element(".modal-confirm").Build();
    }

    public ModalPage WaitForModal()
    {
        return WaitForVisible(p => p.ModalOverlay);
    }

    public ModalPage CloseModal()
    {
        return Click(p => p.CloseButton);
    }

    public ModalPage ConfirmModal()
    {
        return Click(p => p.ConfirmButton);
    }
}
```

### 2. Form Handling

```csharp
public class FormPage : Page<FormPage>
{
    public FormPage(IServiceProvider sp, Uri url) : base(sp, url) { }

    protected override void ConfigureElements() { /* ... */ }

    public FormPage FillForm(Dictionary<string, string> formData)
    {
        foreach (var field in formData)
        {
            var element = Element($"#{field.Key}").Build();
            element.Type(field.Value);
        }

        return this;
    }
}
```

## Testing Page Objects

### Integration Testing

```csharp
[TestClass]
public class LoginPageIntegrationTests
{
    [TestMethod]
    public void Can_Login_Successfully()
    {
        // Arrange
        var loginPage = TestAssemblyHooks.App.NavigateTo<LoginPage>();

        // Act
        var homePage = loginPage
            .EnterEmail("test@example.com")
            .EnterPassword("password")
            .ClickLogin();

        // Assert
        homePage.Verify
            .UrlContains("/home")
            .Visible(p => p.WelcomeMessage);
    }
}
```

## Conclusion

The Page Object Pattern in FluentUIScaffold provides a robust foundation for building maintainable and reliable UI tests. By following the best practices outlined in this guide, you can create page objects that are:

- **Maintainable**: Easy to update when the UI changes
- **Reusable**: Can be used across multiple test scenarios
- **Readable**: Self-documenting and easy to understand
- **Reliable**: Handle element interactions consistently
- **Testable**: Can be unit tested independently

For more information, see the [API Reference](api-reference.md) and [Getting Started](getting-started.md) guides.
