# Story 1.3.2: Complete Example 1 Implementation

## Overview

Complete the implementation of Example 1 (User Registration and Login Flow) by integrating all the components from previous stories and creating a comprehensive working example that demonstrates the complete framework capabilities.

## Background

Example 1 from the V2.0 specification shows a complete user registration and login flow:

```csharp
fluentUI
    .NavigateTo<RegistrationPage>()
    .Type(e => e.EmailInput, "john.doe@example.com")
    .Type(e => e.PasswordInput, "SecurePass123!")
    .Type(e => e.FirstNameInput, "John")
    .Type(e => e.LastNameInput, "Doe")
    .Click(e => e.RegisterButton)
    .Verify(e => e.SuccessMessage, "Registration successful!")
    .NavigateTo<LoginPage>()                          // Returns LoginPage
    .Type(e => e.EmailInput, "john.doe@example.com")
    .Type(e => e.PasswordInput, "SecurePass123!")
    .Click(e => e.LoginButton)
    .Verify(e => e.WelcomeMessage, "Welcome, John!");
```

This story focuses on completing the implementation and creating comprehensive tests.

## Acceptance Criteria

- [x] Implement complete Example 1 scenario
- [x] Add comprehensive tests for registration and login flow
- [x] Update documentation with Example 1
- [x] All tests pass and demonstrate working framework

## Technical Requirements

### 1. Complete Example 1 Implementation

Create a complete working example that demonstrates the full registration and login flow:

```csharp
[TestMethod]
public async Task Can_Complete_User_Registration_And_Login_Flow()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
        options.DefaultTimeout = TimeSpan.FromSeconds(30);
        options.LogLevel = LogLevel.Information;
    });
    
    try
    {
        // Act - Registration Flow
        var registrationPage = fluentUI.NavigateTo<RegistrationPage>();
        
        registrationPage
            .Type(e => e.EmailInput, "john.doe@example.com")
            .Type(e => e.PasswordInput, "SecurePass123!")
            .Type(e => e.FirstNameInput, "John")
            .Type(e => e.LastNameInput, "Doe")
            .Click(e => e.RegisterButton)
            .Verify(e => e.SuccessMessage, "Registration successful!");
        
        // Act - Login Flow
        var loginPage = fluentUI.NavigateTo<LoginPage>();
        
        loginPage
            .Type(e => e.EmailInput, "john.doe@example.com")
            .Type(e => e.PasswordInput, "SecurePass123!")
            .Click(e => e.LoginButton)
            .Verify(e => e.WelcomeMessage, "Welcome, John!");
    }
    finally
    {
        fluentUI?.Dispose();
    }
}
```

### 2. Comprehensive Test Suite

Create a comprehensive test suite that covers all aspects of Example 1:

#### Registration Tests
```csharp
[TestClass]
public class RegistrationFlowTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = TestConfiguration.BaseUri;
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
    
    [TestMethod]
    public async Task Can_Register_New_User_With_Valid_Data()
    {
        // Arrange
        var registrationPage = _fluentUI.NavigateTo<RegistrationPage>();
        
        // Act
        registrationPage
            .Type(e => e.EmailInput, "test@example.com")
            .Type(e => e.PasswordInput, "SecurePass123!")
            .Type(e => e.FirstNameInput, "Test")
            .Type(e => e.LastNameInput, "User")
            .Click(e => e.RegisterButton);
        
        // Assert
        registrationPage.Verify(e => e.SuccessMessage, "Registration successful!");
    }
    
    [TestMethod]
    public async Task Can_Handle_Registration_Validation_Errors()
    {
        // Arrange
        var registrationPage = _fluentUI.NavigateTo<RegistrationPage>();
        
        // Act - Submit form without required fields
        registrationPage.Click(e => e.RegisterButton);
        
        // Assert
        registrationPage.Verify(e => e.ErrorMessage, "All fields are required");
    }
    
    [TestMethod]
    public async Task Can_Handle_Password_Validation()
    {
        // Arrange
        var registrationPage = _fluentUI.NavigateTo<RegistrationPage>();
        
        // Act - Submit form with weak password
        registrationPage
            .Type(e => e.EmailInput, "test@example.com")
            .Type(e => e.PasswordInput, "123")
            .Type(e => e.FirstNameInput, "Test")
            .Type(e => e.LastNameInput, "User")
            .Click(e => e.RegisterButton);
        
        // Assert
        registrationPage.Verify(e => e.ErrorMessage, "Password must be at least 8 characters long");
    }
}
```

#### Login Tests
```csharp
[TestClass]
public class LoginFlowTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = TestConfiguration.BaseUri;
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
    
    [TestMethod]
    public async Task Can_Login_With_Valid_Credentials()
    {
        // Arrange
        var loginPage = _fluentUI.NavigateTo<LoginPage>();
        
        // Act
        loginPage
            .Type(e => e.EmailInput, "john.doe@example.com")
            .Type(e => e.PasswordInput, "SecurePass123!")
            .Click(e => e.LoginButton);
        
        // Assert
        loginPage.Verify(e => e.WelcomeMessage, "Welcome, John!");
    }
    
    [TestMethod]
    public async Task Can_Handle_Invalid_Credentials()
    {
        // Arrange
        var loginPage = _fluentUI.NavigateTo<LoginPage>();
        
        // Act
        loginPage
            .Type(e => e.EmailInput, "invalid@example.com")
            .Type(e => e.PasswordInput, "wrongpassword")
            .Click(e => e.LoginButton);
        
        // Assert
        loginPage.Verify(e => e.ErrorMessage, "Invalid email or password");
    }
    
    [TestMethod]
    public async Task Can_Handle_Empty_Credentials()
    {
        // Arrange
        var loginPage = _fluentUI.NavigateTo<LoginPage>();
        
        // Act
        loginPage.Click(e => e.LoginButton);
        
        // Assert
        loginPage.Verify(e => e.ErrorMessage, "Email and password are required");
    }
}
```

#### Integration Tests
```csharp
[TestClass]
public class RegistrationLoginIntegrationTests
{
    private FluentUIScaffoldApp<WebApp> _fluentUI;
    
    [TestInitialize]
    public void Setup()
    {
        _fluentUI = FluentUIScaffoldBuilder.Web(options =>
        {
            options.BaseUrl = TestConfiguration.BaseUri;
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _fluentUI?.Dispose();
    }
    
    [TestMethod]
    public async Task Can_Complete_Full_Registration_And_Login_Flow()
    {
        // Arrange
        var testEmail = $"test_{Guid.NewGuid():N}@example.com";
        var testPassword = "SecurePass123!";
        var testFirstName = "Test";
        var testLastName = "User";
        
        // Act - Registration
        var registrationPage = _fluentUI.NavigateTo<RegistrationPage>();
        
        registrationPage
            .Type(e => e.EmailInput, testEmail)
            .Type(e => e.PasswordInput, testPassword)
            .Type(e => e.FirstNameInput, testFirstName)
            .Type(e => e.LastNameInput, testLastName)
            .Click(e => e.RegisterButton)
            .Verify(e => e.SuccessMessage, "Registration successful!");
        
        // Act - Login
        var loginPage = _fluentUI.NavigateTo<LoginPage>();
        
        loginPage
            .Type(e => e.EmailInput, testEmail)
            .Type(e => e.PasswordInput, testPassword)
            .Click(e => e.LoginButton)
            .Verify(e => e.WelcomeMessage, $"Welcome, {testFirstName}!");
    }
    
    [TestMethod]
    public async Task Can_Navigate_Between_Registration_And_Login_Pages()
    {
        // Arrange
        var registrationPage = _fluentUI.NavigateTo<RegistrationPage>();
        
        // Act - Navigate from registration to login
        var loginPage = registrationPage.NavigateTo<LoginPage>();
        
        // Assert - Verify we're on the login page
        loginPage.Verify(e => e.Form, "login-form", "className");
        
        // Act - Navigate back to registration
        var backToRegistration = loginPage.NavigateTo<RegistrationPage>();
        
        // Assert - Verify we're back on the registration page
        backToRegistration.Verify(e => e.Form, "registration-form", "className");
    }
}
```

### 3. Documentation Updates

Update the documentation to include Example 1:

#### API Documentation
```markdown
# Example 1: User Registration and Login Flow

This example demonstrates the basic capabilities of FluentUIScaffold V2.0, including:

- Navigation between pages
- Form interactions (Type, Click)
- Verification of results
- Error handling

## Complete Example

```csharp
fluentUI
    .NavigateTo<RegistrationPage>()
    .Type(e => e.EmailInput, "john.doe@example.com")
    .Type(e => e.PasswordInput, "SecurePass123!")
    .Type(e => e.FirstNameInput, "John")
    .Type(e => e.LastNameInput, "Doe")
    .Click(e => e.RegisterButton)
    .Verify(e => e.SuccessMessage, "Registration successful!")
    .NavigateTo<LoginPage>()
    .Type(e => e.EmailInput, "john.doe@example.com")
    .Type(e => e.PasswordInput, "SecurePass123!")
    .Click(e => e.LoginButton)
    .Verify(e => e.WelcomeMessage, "Welcome, John!");
```

## Key Features Demonstrated

1. **Navigation**: Direct navigation using `NavigateTo<T>()`
2. **Form Interactions**: Type text and click buttons
3. **Verification**: Check success messages and form states
4. **Fluent API**: Chainable methods for readable tests
5. **Error Handling**: Validation and error message verification
```

### 4. Sample App Integration

Ensure the sample app is fully integrated with Example 1:

#### Navigation Links
Add navigation links to the sample app:

```html
<nav>
    <a href="/">Home</a>
    <a href="/register">Register</a>
    <a href="/login">Login</a>
</nav>
```

#### Home Page Updates
Update the home page to include links to registration and login:

```html
<div class="welcome-section">
    <h1>Welcome to FluentUIScaffold Sample App</h1>
    <p>This sample app demonstrates the capabilities of FluentUIScaffold V2.0</p>
    <div class="action-buttons">
        <a href="/register" class="btn btn-primary">Register</a>
        <a href="/login" class="btn btn-secondary">Login</a>
    </div>
</div>
```

## Implementation Tasks

### Phase 1: Complete Implementation
1. [x] Integrate all components from previous stories
2. [x] Create complete working example
3. [x] Test all navigation and form interactions
4. [x] Verify all verification methods work correctly

### Phase 2: Comprehensive Testing
1. [x] Create registration flow tests
2. [x] Create login flow tests
3. [x] Create integration tests
4. [x] Test error scenarios and validation

### Phase 3: Documentation
1. [x] Update API documentation with Example 1
2. [x] Create tutorials and best practices
3. [x] Add code examples and explanations
4. [x] Update sample app documentation

### Phase 4: Sample App Integration
1. [x] Add navigation between pages
2. [x] Ensure all forms work correctly
3. [x] Test complete user flows
4. [x] Verify all tests pass

## Dependencies

- **Story 1.1.1**: Refactor to V2.0 BasePageComponent Pattern (must be completed first)
- **Story 1.1.2**: Implement Basic Navigation Methods (must be completed first)
- **Story 1.2.1**: Implement Base Element Actions (must be completed first)
- **Story 1.2.2**: Create Registration and Login Pages (must be completed first)
- **Story 1.3.1**: Implement Generic Verification (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [x] Complete Example 1 scenario is implemented and working
- [x] Comprehensive test suite is created and passing
- [x] Documentation is updated with Example 1
- [x] Sample app is fully integrated
- [x] All navigation and form interactions work correctly
- [x] All verification methods work correctly
- [x] Error scenarios are properly handled
- [x] All acceptance criteria are met

## Notes

- This story represents the completion of Example 1
- All previous stories must be completed before this one
- The implementation should demonstrate all framework capabilities
- The test suite should be comprehensive and cover all scenarios
- Documentation should be clear and helpful for developers
- **COMPLETED**: All acceptance criteria and definition of done items have been successfully implemented and tested
- **Note**: Some email validation tests are documented as not applicable due to browser-level validation preventing form submission for invalid email formats 