# Page Object Pattern

## Overview

The Page Object Pattern is a design pattern that creates an abstraction layer between your test code and the web page elements. FluentUIScaffold provides a robust implementation of this pattern through the `BasePageComponent<TApp>` class.

## Benefits

- **Maintainability**: Centralizes element selectors and page logic
- **Reusability**: Page objects can be reused across multiple tests
- **Readability**: Tests become more readable and self-documenting
- **Reliability**: Reduces test flakiness through proper element handling
- **Separation of Concerns**: Separates test logic from page interaction logic

## Core Components

### BasePageComponent<TDriver, TPage>

The base class for all page objects in FluentUIScaffold with dual generic types for fluent API context.

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    protected TDriver Driver { get; }
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected FluentUIScaffoldOptions Options { get; }
    protected ElementFactory ElementFactory { get; }
    
    public Uri UrlPattern { get; }
    public virtual bool ShouldValidateOnNavigation => false;
    
    protected abstract void ConfigureElements();
    
    // Framework-agnostic element interaction methods
    protected virtual void ClickElement(string selector) => Driver.Click(selector);
    protected virtual void TypeText(string selector, string text) => Driver.Type(selector, text);
    protected virtual void SelectOption(string selector, string value) => Driver.SelectOption(selector, value);
    protected virtual string GetElementText(string selector) => Driver.GetText(selector);
    protected virtual bool IsElementVisible(string selector) => Driver.IsVisible(selector);
    protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);
    
    // Fluent API element action methods
    public virtual TPage Click(Func<TPage, IElement> elementSelector);
    public virtual TPage Type(Func<TPage, IElement> elementSelector, string text);
    public virtual TPage Select(Func<TPage, IElement> elementSelector, string value);
    public virtual TPage Focus(Func<TPage, IElement> elementSelector);
    public virtual TPage Hover(Func<TPage, IElement> elementSelector);
    public virtual TPage Clear(Func<TPage, IElement> elementSelector);
    
    // Additional fluent element actions
    public virtual TPage WaitForElement(Func<TPage, IElement> elementSelector);
    public virtual TPage WaitForElementToBeVisible(Func<TPage, IElement> elementSelector);
    public virtual TPage WaitForElementToBeHidden(Func<TPage, IElement> elementSelector);
    
    // Generic verification methods
    public virtual TPage VerifyValue<TValue>(Func<TPage, IElement> elementSelector, TValue expectedValue, string description = null);
    public virtual TPage VerifyText(Func<TPage, IElement> elementSelector, string expectedText, string description = null);
    public virtual TPage VerifyProperty(Func<TPage, IElement> elementSelector, string expectedValue, string propertyName, string description = null);
    
    // Navigation methods
    public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>;
    
    // Framework-specific access
    protected TDriver FrameworkDriver => Driver;
    public TDriver TestDriver => Driver;
    
    // Verification access
    public IVerificationContext Verify { get; }
    
    // Helper methods
    protected ElementBuilder Element(string selector);
    protected virtual void NavigateToUrl(Uri url);
    
    // IPageComponent implementation
    public virtual bool IsCurrentPage();
    public virtual void ValidateCurrentPage();
}
```

### IPageComponent<TDriver, TPage>

Interface that defines the contract for page components with dual generic types.

```csharp
public interface IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    Uri UrlPattern { get; }
    bool ShouldValidateOnNavigation { get; }
    bool IsCurrentPage();
    void ValidateCurrentPage();
    TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>;
    IVerificationContext Verify { get; }
}
```

## Creating Page Objects

### Fluent API Methods

The `BasePageComponent<TDriver, TPage>` provides fluent API methods for element interactions:

```csharp
// Element interaction methods
page.Click(e => e.ElementName)
page.Type(e => e.ElementName, "text")
page.Select(e => e.ElementName, "value")
page.Focus(e => e.ElementName)
page.Hover(e => e.ElementName)
page.Clear(e => e.ElementName)

// Wait methods
page.WaitForElement(e => e.ElementName)
page.WaitForElementToBeVisible(e => e.ElementName)
page.WaitForElementToBeHidden(e => e.ElementName)

// Verification methods
page.VerifyText(e => e.ElementName, "expected text")
page.VerifyValue(e => e.ElementName, expectedValue)
page.VerifyProperty(e => e.ElementName, "expected value", "propertyName")
```

### Basic Page Object

```csharp
public class LoginPage : BasePageComponent<PlaywrightDriver, LoginPage>
{
    public override Uri UrlPattern => new Uri("/login");
    
    private IElement _emailInput;
    private IElement _passwordInput;
    private IElement _loginButton;
    private IElement _errorMessage;
    
    protected override void ConfigureElements()
    {
        _emailInput = Element("#email")
            .WithDescription("Email Input")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _passwordInput = Element("#password")
            .WithDescription("Password Input")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _loginButton = Element("#login-btn")
            .WithDescription("Login Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
            
        _errorMessage = Element(".error-message")
            .WithDescription("Error Message")
            .WithWaitStrategy(WaitStrategy.Visible);
    }
    
    public LoginPage EnterEmail(string email)
    {
        Logger.LogInformation($"Entering email: {email}");
        Type(e => e._emailInput, email);
        return this;
    }
    
    public LoginPage EnterPassword(string password)
    {
        Logger.LogInformation("Entering password");
        Type(e => e._passwordInput, password);
        return this;
    }
    
    public HomePage ClickLogin()
    {
        Logger.LogInformation("Clicking login button");
        Click(e => e._loginButton);
        return NavigateTo<HomePage>();
    }
    
    public LoginPage VerifyErrorMessage(string expectedMessage)
    {
        VerifyText(e => e._errorMessage, expectedMessage);
        return this;
    }
}
```

### Advanced Page Object with Complex Logic

```csharp
public class UserManagementPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/users";
    
    private IElement _createUserButton;
    private IElement _userTable;
    private IElement _searchInput;
    private IElement _filterDropdown;
    
    protected override void ConfigureElements()
    {
        _createUserButton = Element("[data-testid='create-user-btn']")
            .WithDescription("Create User Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
            
        _userTable = Element("#users-table")
            .WithDescription("Users Table")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _searchInput = Element("#search-users")
            .WithDescription("Search Users Input")
            .WithWaitStrategy(WaitStrategy.Visible);
            
        _filterDropdown = Element("#filter-users")
            .WithDescription("Filter Users Dropdown")
            .WithWaitStrategy(WaitStrategy.Visible);
    }
    
    public CreateUserPage ClickCreateUser()
    {
        Logger.LogInformation("Navigating to create user page");
        _createUserButton.Click();
        return NavigateTo<CreateUserPage>();
    }
    
    public UserManagementPage SearchUser(string searchTerm)
    {
        Logger.LogInformation($"Searching for user: {searchTerm}");
        _searchInput.Type(searchTerm);
        return this;
    }
    
    public UserManagementPage FilterByRole(string role)
    {
        Logger.LogInformation($"Filtering by role: {role}");
        _filterDropdown.Select(role);
        return this;
    }
    
    public UserManagementPage VerifyUserExists(string userName)
    {
        Verify.ElementContainsText("#users-table", userName);
        return this;
    }
    
    public UserManagementPage VerifyUserCount(int expectedCount)
    {
        Verify.That(
            () => GetUserCount(),
            count => count == expectedCount,
            $"Expected {expectedCount} users, but found {{0}}"
        );
        return this;
    }
    
    private int GetUserCount()
    {
        var userRows = Driver.GetElements("#users-table tbody tr");
        return userRows.Count;
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
    var button = Element("#submit-button");
    
    // Element with description
    var input = Element("#email")
        .WithDescription("Email Input Field");
    
    // Element with timeout
    var dropdown = Element("#country-select")
        .WithTimeout(TimeSpan.FromSeconds(10));
    
    // Element with wait strategy
    var loadingSpinner = Element(".loading-spinner")
        .WithWaitStrategy(WaitStrategy.Hidden);
    
    // Complex element configuration
    var complexElement = Element("[data-testid='user-card']")
        .WithDescription("User Card Component")
        .WithTimeout(TimeSpan.FromSeconds(15))
        .WithWaitStrategy(WaitStrategy.Visible)
        .WithRetryInterval(TimeSpan.FromMilliseconds(200));
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
    void Select(string value);
    string GetText();
    bool IsVisible();
    bool IsEnabled();
    void WaitFor();
}
```

## Page Navigation

### Navigation Methods

```csharp
// Navigate to another page
public HomePage ClickLogin()
{
    _loginButton.Click();
    return NavigateTo<HomePage>();
}

// Navigate with parameters
public UserProfilePage OpenUserProfile(int userId)
{
    var profileLink = Element($"[data-user-id='{userId}']");
    profileLink.Click();
    return NavigateTo<UserProfilePage>();
}

// Conditional navigation
public T NavigateToPage<T>() where T : BasePageComponent<WebApp>
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
    return Driver.CurrentUrl.Contains("/login") && 
           IsVisible("#login-form");
}
```

## Verification Context

### Using Verification

```csharp
public LoginPage VerifyLoginForm()
{
    Verify
        .ElementIsVisible("#email")
        .ElementIsVisible("#password")
        .ElementIsEnabled("#login-btn")
        .ElementIsHidden(".error-message");
    
    return this;
}

public LoginPage VerifyErrorMessage(string expectedMessage)
{
    Verify.ElementContainsText(".error-message", expectedMessage);
    return this;
}

public LoginPage VerifyPageLoaded()
{
    Verify
        .CurrentPageIs<LoginPage>()
        .UrlMatches("/login")
        .TitleContains("Login");
    
    return this;
}
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

public UserManagementPage VerifyUserExists(string userName)
{
    Verify.That(
        () => GetUserNames(),
        names => names.Contains(userName),
        $"User '{userName}' should exist in the table"
    );
    return this;
}
```

## Framework-Specific Access

### Accessing Framework Drivers

```csharp
public class AdvancedPage : BasePageComponent<PlaywrightDriver, AdvancedPage>
{
    public void TakeScreenshot()
    {
        // Direct access to the driver
        TestDriver.TakeScreenshotAsync("screenshot.png");
    }
    
    public void InterceptNetworkRequests()
    {
        // Direct access to the driver
        TestDriver.InterceptNetworkRequests("/api/*", response =>
        {
            // Handle intercepted response
        });
    }
    
    public void UseFrameworkSpecificFeatures()
    {
        // Access framework-specific features directly
        var page = TestDriver.Page;
        page.SetViewportSizeAsync(1920, 1080);
    }
}
```

## Best Practices

### 1. Element Organization

```csharp
public class WellOrganizedPage : BasePageComponent<WebApp>
{
    // Group related elements
    private IElement _emailInput;
    private IElement _passwordInput;
    private IElement _loginButton;
    
    // Form elements
    private IElement _firstNameInput;
    private IElement _lastNameInput;
    private IElement _submitButton;
    
    // Navigation elements
    private IElement _homeLink;
    private IElement _profileLink;
    private IElement _logoutLink;
    
    protected override void ConfigureElements()
    {
        ConfigureLoginElements();
        ConfigureFormElements();
        ConfigureNavigationElements();
    }
    
    private void ConfigureLoginElements()
    {
        _emailInput = Element("#email").WithDescription("Email Input");
        _passwordInput = Element("#password").WithDescription("Password Input");
        _loginButton = Element("#login-btn").WithDescription("Login Button");
    }
    
    private void ConfigureFormElements()
    {
        _firstNameInput = Element("#first-name").WithDescription("First Name Input");
        _lastNameInput = Element("#last-name").WithDescription("Last Name Input");
        _submitButton = Element("#submit").WithDescription("Submit Button");
    }
    
    private void ConfigureNavigationElements()
    {
        _homeLink = Element("#home-link").WithDescription("Home Link");
        _profileLink = Element("#profile-link").WithDescription("Profile Link");
        _logoutLink = Element("#logout-link").WithDescription("Logout Link");
    }
}
```

### 2. Method Chaining

```csharp
public class ChainedPage : BasePageComponent<PlaywrightDriver, ChainedPage>
{
    public ChainedPage EnterEmail(string email)
    {
        Type(e => e._emailInput, email);
        return this;
    }
    
    public ChainedPage EnterPassword(string password)
    {
        Type(e => e._passwordInput, password);
        return this;
    }
    
    public ChainedPage ClickLogin()
    {
        Click(e => e._loginButton);
        return this;
    }
    
    // Usage: page.EnterEmail("test@example.com").EnterPassword("password").ClickLogin();
    
    // Or use the fluent API directly:
    public ChainedPage Login(string email, string password)
    {
        return Type(e => e._emailInput, email)
               .Type(e => e._passwordInput, password)
               .Click(e => e._loginButton);
    }
}
```

### 3. Error Handling

```csharp
public class RobustPage : BasePageComponent<PlaywrightDriver, RobustPage>
{
    public RobustPage ClickButtonSafely()
    {
        try
        {
            Click(e => e._button);
        }
        catch (ElementTimeoutException ex)
        {
            Logger.LogWarning($"Button not found: {ex.Selector}");
            // Fallback logic using direct driver access
            TestDriver.ExecuteScript("document.querySelector('#button').click();");
        }
        
        return this;
    }
    
    public RobustPage WaitForElementWithRetry(Func<RobustPage, IElement> elementSelector, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                WaitForElement(elementSelector);
                return this;
            }
            catch (ElementTimeoutException)
            {
                if (i == maxRetries - 1) throw;
                Thread.Sleep(1000);
            }
        }
        
        return this;
    }
}
```

### 4. Page State Management

```csharp
public class StatefulPage : BasePageComponent<PlaywrightDriver, StatefulPage>
{
    private bool _isLoggedIn = false;
    private string _currentUser = null;
    
    public StatefulPage Login(string email, string password)
    {
        Type(e => e._emailInput, email);
        Type(e => e._passwordInput, password);
        Click(e => e._loginButton);
        
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
        
        Click(e => e._logoutLink);
        _isLoggedIn = false;
        _currentUser = null;
        
        return this;
    }
    
    public bool IsLoggedIn => _isLoggedIn;
    public string CurrentUser => _currentUser;
}
```

## Navigation Patterns

### 1. Basic URL Pattern Navigation

The simplest way to navigate to a specific path is by overriding the `UrlPattern` property:

```csharp
public class LoginPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/login";
    
    // ... rest of page implementation
}
```

When you call `NavigateTo<LoginPage>()`, the framework automatically navigates to `/login`.

### 2. Custom Navigation Methods

For more complex navigation logic, you can override the `Navigate()` method:

```csharp
public class DashboardPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/dashboard";
    
    public override void Navigate()
    {
        // Add custom logic before navigation
        if (!IsAuthenticated())
        {
            Driver.NavigateToUrl(new Uri(Driver.BaseUrl, "/login"));
            return;
        }
        
        // Navigate to dashboard
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, "/dashboard"));
    }
    
    private bool IsAuthenticated()
    {
        // Your authentication logic here
        return true;
    }
}
```

### 3. Parameterized Navigation

For pages that need dynamic paths, create custom navigation methods:

```csharp
public class UserProfilePage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/profile";
    
    public UserProfilePage NavigateToUserProfile(int userId)
    {
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, $"/profile/{userId}"));
        return this;
    }
    
    public UserProfilePage NavigateToUserProfile(string username)
    {
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, $"/profile/user/{username}"));
        return this;
    }
}
```

### 4. Advanced Navigation Patterns

#### Navigation with Query Parameters

```csharp
public class SearchPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/search";
    
    public SearchPage NavigateWithQuery(string query, string category = "all")
    {
        var queryString = $"q={Uri.EscapeDataString(query)}&category={Uri.EscapeDataString(category)}";
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, $"/search?{queryString}"));
        return this;
    }
    
    public SearchPage NavigateWithFilters(Dictionary<string, string> filters)
    {
        var queryParams = string.Join("&", filters.Select(kvp => 
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, $"/search?{queryParams}"));
        return this;
    }
}
```

#### Conditional Navigation

```csharp
public class AdminPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/admin";
    
    public AdminPage NavigateBasedOnRole(string userRole)
    {
        var path = userRole.ToLower() switch
        {
            "admin" => "/admin/dashboard",
            "moderator" => "/admin/moderator",
            "viewer" => "/admin/readonly",
            _ => "/admin/unauthorized"
        };
        
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, path));
        return this;
    }
}
```

#### Navigation with State Management

```csharp
public class ShoppingCartPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => "/cart";
    
    private string _currentCartId;
    
    public ShoppingCartPage NavigateToCart(string cartId)
    {
        _currentCartId = cartId;
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, $"/cart/{cartId}"));
        return this;
    }
    
    public ShoppingCartPage NavigateToCartWithItems(string cartId, List<string> itemIds)
    {
        _currentCartId = cartId;
        var itemsParam = string.Join(",", itemIds);
        Driver.NavigateToUrl(new Uri(Driver.BaseUrl, $"/cart/{cartId}?items={itemsParam}"));
        return this;
    }
    
    public string GetCurrentCartId() => _currentCartId;
}
```

### 5. Direct URL Navigation

You can also navigate directly using the main app instance:

```csharp
// Navigate to a specific URL
_fluentUI.NavigateToUrl(new Uri("https://your-app.com/specific-path"));

// Then get the page object
var page = _fluentUI.GetPage<YourPage>();
```

### 6. Navigation Best Practices

1. **Use `UrlPattern` for simple, static paths**
2. **Use custom `Navigate()` methods for complex logic**
3. **Use parameterized methods for dynamic paths**
4. **Keep navigation logic in the page object**
5. **Use descriptive method names for custom navigation**
6. **Handle authentication and authorization in navigation methods**
7. **Validate navigation parameters before use**

## Common Patterns

### 1. Modal Dialogs

```csharp
public class ModalPage : BasePageComponent<WebApp>
{
    public override string UrlPattern => ".*"; // Modal can appear on any page
    
    private IElement _modalOverlay;
    private IElement _modalContent;
    private IElement _closeButton;
    private IElement _confirmButton;
    
    protected override void ConfigureElements()
    {
        _modalOverlay = Element(".modal-overlay");
        _modalContent = Element(".modal-content");
        _closeButton = Element(".modal-close");
        _confirmButton = Element(".modal-confirm");
    }
    
    public ModalPage WaitForModal()
    {
        _modalOverlay.WaitFor();
        return this;
    }
    
    public ModalPage CloseModal()
    {
        _closeButton.Click();
        return this;
    }
    
    public ModalPage ConfirmModal()
    {
        _confirmButton.Click();
        return this;
    }
}
```

### 2. Dynamic Content

```csharp
public class DynamicPage : BasePageComponent<WebApp>
{
    public DynamicPage WaitForContentToLoad()
    {
        // Wait for loading spinner to disappear
        Element(".loading-spinner").WithWaitStrategy(WaitStrategy.Hidden).WaitFor();
        
        // Wait for content to appear
        Element(".content").WithWaitStrategy(WaitStrategy.Visible).WaitFor();
        
        return this;
    }
    
    public DynamicPage RefreshUntilElementAppears(string selector, int maxAttempts = 5)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            try
            {
                Element(selector).WithWaitStrategy(WaitStrategy.Visible).WaitFor();
                return this;
            }
            catch (ElementTimeoutException)
            {
                Driver.Navigate().Refresh();
                Thread.Sleep(2000);
            }
        }
        
        throw new ElementTimeoutException($"Element {selector} did not appear after {maxAttempts} attempts");
    }
}
```

### 3. Form Handling

```csharp
public class FormPage : BasePageComponent<WebApp>
{
    public FormPage FillForm(Dictionary<string, string> formData)
    {
        foreach (var field in formData)
        {
            var element = Element($"#{field.Key}");
            element.Type(field.Value);
        }
        
        return this;
    }
    
    public FormPage ClearForm()
    {
        var inputs = Driver.GetElements("input[type='text'], input[type='email'], textarea");
        foreach (var input in inputs)
        {
            input.Clear();
        }
        
        return this;
    }
    
    public FormPage ValidateFormFields(List<string> requiredFields)
    {
        foreach (var field in requiredFields)
        {
            Verify.ElementIsVisible($"#{field}");
        }
        
        return this;
    }
}
```

## Testing Page Objects

### Unit Testing

```csharp
[TestClass]
public class LoginPageTests
{
    private Mock<IUIDriver> _mockDriver;
    private Mock<ILogger> _mockLogger;
    private FluentUIScaffoldOptions _options;
    private LoginPage _loginPage;
    
    [TestInitialize]
    public void Setup()
    {
        _mockDriver = new Mock<IUIDriver>();
        _mockLogger = new Mock<ILogger>();
        _options = new FluentUIScaffoldOptions();
        
        _loginPage = new LoginPage(_mockDriver.Object, _options, _mockLogger.Object);
    }
    
    [TestMethod]
    public void EnterEmail_ShouldTypeEmail()
    {
        // Act
        _loginPage.EnterEmail("test@example.com");
        
        // Assert
        _mockDriver.Verify(d => d.Type("#email", "test@example.com"), Times.Once);
    }
    
    [TestMethod]
    public void ClickLogin_ShouldNavigateToHomePage()
    {
        // Act
        var result = _loginPage.ClickLogin();
        
        // Assert
        Assert.IsInstanceOfType(result, typeof(HomePage));
        _mockDriver.Verify(d => d.Click("#login-btn"), Times.Once);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class LoginPageIntegrationTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = new Uri("https://your-app.com");
        });
    }
    
    [TestMethod]
    public async Task Can_Login_Successfully()
    {
        // Arrange
        var loginPage = _fluentUI.NavigateTo<LoginPage>();
        
        // Act
        var homePage = loginPage
            .EnterEmail("test@example.com")
            .EnterPassword("password")
            .ClickLogin();
        
        // Assert
        homePage.Verify
            .CurrentPageIs<HomePage>()
            .ElementIsVisible("#welcome-message");
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
}
```

## Performance Considerations

### Element Caching

```csharp
public class OptimizedPage : BasePageComponent<WebApp>
{
    private readonly Dictionary<string, IElement> _elementCache = new();
    
    protected override void ConfigureElements()
    {
        // Elements are cached automatically by ElementFactory
        _button = Element("#button");
        _input = Element("#input");
    }
    
    public OptimizedPage ClickButtonMultipleTimes(int times)
    {
        // Element is cached, no need to re-find
        for (int i = 0; i < times; i++)
        {
            _button.Click();
        }
        
        return this;
    }
}
```

### Lazy Loading

```csharp
public class LazyPage : BasePageComponent<WebApp>
{
    private IElement _lazyElement;
    
    private IElement LazyElement => _lazyElement ??= Element("#lazy-loaded-element");
    
    public LazyPage InteractWithLazyElement()
    {
        // Element is only found when first accessed
        LazyElement.Click();
        return this;
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

For more information, see the [API Reference](api-reference.md) and [Testing Best Practices](testing-best-practices.md) guides. 