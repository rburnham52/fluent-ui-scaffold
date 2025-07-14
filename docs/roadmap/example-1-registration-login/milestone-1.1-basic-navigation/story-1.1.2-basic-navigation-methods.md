# Story 1.1.2: Implement Basic Navigation Methods

## Overview

Implement the basic navigation methods required for Example 1 (User Registration and Login Flow), including the `NavigateTo<TTarget>()` method and URL pattern configuration.

## Background

Example 1 requires the ability to navigate between pages using the fluent API. The V2.0 specification shows two navigation patterns:

1. **Base Navigation Method**: `fluentUI.NavigateTo<RegistrationPage>()`
2. **Custom Navigation Methods**: `homePage.NavigateToTodos()`

This story focuses on implementing the base navigation method and URL pattern configuration.

## Acceptance Criteria

- [ ] Implement `NavigateTo<TTarget>()` method
- [ ] Support direct navigation using IoC container
- [ ] Add URL pattern configuration for pages
- [ ] Create working example with RegistrationPage and LoginPage navigation

## Technical Requirements

### 1. Navigation Method Implementation

Implement the `NavigateTo<TTarget>()` method in `BasePageComponent<TDriver, TPage>`:

```csharp
public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>
{
    var targetPage = ServiceProvider.GetRequiredService<TTarget>();
    return targetPage;
}
```

### 2. URL Pattern Configuration

Add URL pattern configuration to page components:

```csharp
public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
    where TDriver : class, IUIDriver
    where TPage : class, IPageComponent<TDriver, TPage>
{
    public Uri UrlPattern { get; }
    
    protected BasePageComponent(IServiceProvider serviceProvider, Uri urlPattern)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Driver = serviceProvider.GetRequiredService<TDriver>();
        Logger = serviceProvider.GetRequiredService<ILogger<BasePageComponent<TDriver, TPage>>>();
        Options = serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
        
        UrlPattern = urlPattern;
        NavigateToUrl(urlPattern);
        ConfigureElements();
    }
    
    protected virtual void NavigateToUrl(Uri url)
    {
        Driver.NavigateToUrl(url.ToString());
    }
}
```

### 3. Page Registration

Implement page registration in the IoC container:

```csharp
public static class FluentUIScaffoldBuilder
{
    private static void ConfigureServices(IServiceCollection services, FluentUIScaffoldOptions options, Action<FrameworkOptions> configureFramework)
    {
        // Register framework-specific services
        configureFramework?.Invoke(new FrameworkOptions(services));
        
        // Register pages with their URL patterns
        RegisterPages(services, options);
        
        // Register other services
        services.AddSingleton(options);
        services.AddLogging();
    }
    
    private static void RegisterPages(IServiceCollection services, FluentUIScaffoldOptions options)
    {
        // Register pages with their URL patterns
        services.AddTransient<RegistrationPage>(provider => 
            new RegistrationPage(provider, new Uri(options.BaseUrl, "/register")));
        services.AddTransient<LoginPage>(provider => 
            new LoginPage(provider, new Uri(options.BaseUrl, "/login")));
        services.AddTransient<HomePage>(provider => 
            new HomePage(provider, new Uri(options.BaseUrl, "/")));
    }
}
```

### 4. Sample App Pages

Create the basic pages needed for Example 1:

#### RegistrationPage
```csharp
public class RegistrationPage : BasePageComponent<PlaywrightDriver, RegistrationPage>
{
    public IElement EmailInput { get; private set; }
    public IElement PasswordInput { get; private set; }
    public IElement FirstNameInput { get; private set; }
    public IElement LastNameInput { get; private set; }
    public IElement RegisterButton { get; private set; }
    public IElement SuccessMessage { get; private set; }
    
    public RegistrationPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        EmailInput = new Element("#email");
        PasswordInput = new Element("#password");
        FirstNameInput = new Element("#firstName");
        LastNameInput = new Element("#lastName");
        RegisterButton = new Element("#registerButton");
        SuccessMessage = new Element("#successMessage");
    }
}
```

#### LoginPage
```csharp
public class LoginPage : BasePageComponent<PlaywrightDriver, LoginPage>
{
    public IElement EmailInput { get; private set; }
    public IElement PasswordInput { get; private set; }
    public IElement LoginButton { get; private set; }
    public IElement WelcomeMessage { get; private set; }
    
    public LoginPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        EmailInput = new Element("#email");
        PasswordInput = new Element("#password");
        LoginButton = new Element("#loginButton");
        WelcomeMessage = new Element("#welcomeMessage");
    }
}
```

### 5. Sample App Implementation

Add the registration and login pages to the sample app:

#### Registration Page (HTML)
```html
<div class="registration-form">
    <h2>User Registration</h2>
    <form id="registrationForm">
        <div class="form-group">
            <label for="email">Email:</label>
            <input type="email" id="email" name="email" required>
        </div>
        <div class="form-group">
            <label for="password">Password:</label>
            <input type="password" id="password" name="password" required>
        </div>
        <div class="form-group">
            <label for="firstName">First Name:</label>
            <input type="text" id="firstName" name="firstName" required>
        </div>
        <div class="form-group">
            <label for="lastName">Last Name:</label>
            <input type="text" id="lastName" name="lastName" required>
        </div>
        <button type="submit" id="registerButton">Register</button>
    </form>
    <div id="successMessage" style="display: none;">Registration successful!</div>
</div>
```

#### Login Page (HTML)
```html
<div class="login-form">
    <h2>User Login</h2>
    <form id="loginForm">
        <div class="form-group">
            <label for="email">Email:</label>
            <input type="email" id="email" name="email" required>
        </div>
        <div class="form-group">
            <label for="password">Password:</label>
            <input type="password" id="password" name="password" required>
        </div>
        <button type="submit" id="loginButton">Login</button>
    </form>
    <div id="welcomeMessage" style="display: none;">Welcome, John!</div>
</div>
```

## Implementation Tasks

### Phase 1: Core Navigation Implementation
1. [ ] Implement `NavigateTo<TTarget>()` method in BasePageComponent
2. [ ] Add URL pattern configuration to page constructors
3. [ ] Implement `NavigateToUrl()` method
4. [ ] Update service registration to include page registration

### Phase 2: Page Registration System
1. [ ] Create page registration system in FluentUIScaffoldBuilder
2. [ ] Add URL pattern configuration for each page
3. [ ] Implement IoC container registration for pages
4. [ ] Add page factory pattern for dynamic page creation

### Phase 3: Sample App Pages
1. [ ] Create RegistrationPage class
2. [ ] Create LoginPage class
3. [ ] Add HTML pages to sample app
4. [ ] Implement basic form functionality

### Phase 4: Testing
1. [ ] Create navigation tests
2. [ ] Test URL pattern configuration
3. [ ] Test page registration and instantiation
4. [ ] Verify navigation between pages works correctly

## Dependencies

- **Story 1.1.1**: Refactor to V2.0 BasePageComponent Pattern (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [ ] `NavigateTo<TTarget>()` method is implemented and working
- [ ] URL pattern configuration is working
- [ ] Page registration system is implemented
- [ ] RegistrationPage and LoginPage are created and working
- [ ] Sample app has working registration and login pages
- [ ] Navigation tests are passing
- [ ] All acceptance criteria are met

## Notes

- This story builds on the foundation established in Story 1.1.1
- The navigation system should be framework-agnostic
- URL patterns should be configurable per page
- The IoC container should handle page instantiation automatically 