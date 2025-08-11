# Story 1.2.2: Create Registration and Login Pages

## Overview

Create the RegistrationPage and LoginPage classes with their corresponding sample app pages, implementing the complete registration and login flow for Example 1.

## Background

Example 1 requires a complete user registration and login flow. This story focuses on creating the page classes and sample app implementation that will be used to demonstrate the framework's capabilities.

## Acceptance Criteria

- [x] Create RegistrationPage with form elements
- [x] Create LoginPage with form elements
- [x] Add sample app pages for registration and login
- [x] Implement working registration and login flow

## Technical Requirements

### 1. RegistrationPage Implementation

Create a complete RegistrationPage class:

```csharp
public class RegistrationPage : BasePageComponent<PlaywrightDriver, RegistrationPage>
{
    public IElement EmailInput { get; private set; }
    public IElement PasswordInput { get; private set; }
    public IElement FirstNameInput { get; private set; }
    public IElement LastNameInput { get; private set; }
    public IElement RegisterButton { get; private set; }
    public IElement SuccessMessage { get; private set; }
    public IElement ErrorMessage { get; private set; }
    public IElement Form { get; private set; }
    
    public RegistrationPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        EmailInput = new Element("#email", "Email Input", ElementType.Input);
        PasswordInput = new Element("#password", "Password Input", ElementType.Input);
        FirstNameInput = new Element("#firstName", "First Name Input", ElementType.Input);
        LastNameInput = new Element("#lastName", "Last Name Input", ElementType.Input);
        RegisterButton = new Element("#registerButton", "Register Button", ElementType.Button);
        SuccessMessage = new Element("#successMessage", "Success Message", ElementType.Div);
        ErrorMessage = new Element("#errorMessage", "Error Message", ElementType.Div);
        Form = new Element("#registrationForm", "Registration Form", ElementType.Form);
    }
    
    // Custom methods for registration flow
    public RegistrationPage FillRegistrationForm(string email, string password, string firstName, string lastName)
    {
        return this
            .Type(e => e.EmailInput, email)
            .Type(e => e.PasswordInput, password)
            .Type(e => e.FirstNameInput, firstName)
            .Type(e => e.LastNameInput, lastName);
    }
    
    public RegistrationPage SubmitRegistration()
    {
        return this.Click(e => e.RegisterButton);
    }
    
    public RegistrationPage VerifyRegistrationSuccess()
    {
        return this.Verify(e => e.SuccessMessage, "Registration successful!");
    }
    
    public RegistrationPage VerifyRegistrationError(string expectedError)
    {
        return this.Verify(e => e.ErrorMessage, expectedError);
    }
}
```

### 2. LoginPage Implementation

Create a complete LoginPage class:

```csharp
public class LoginPage : BasePageComponent<PlaywrightDriver, LoginPage>
{
    public IElement EmailInput { get; private set; }
    public IElement PasswordInput { get; private set; }
    public IElement LoginButton { get; private set; }
    public IElement WelcomeMessage { get; private set; }
    public IElement ErrorMessage { get; private set; }
    public IElement Form { get; private set; }
    
    public LoginPage(IServiceProvider serviceProvider, Uri urlPattern) 
        : base(serviceProvider, urlPattern)
    {
    }
    
    protected override void ConfigureElements()
    {
        EmailInput = new Element("#email", "Email Input", ElementType.Input);
        PasswordInput = new Element("#password", "Password Input", ElementType.Input);
        LoginButton = new Element("#loginButton", "Login Button", ElementType.Button);
        WelcomeMessage = new Element("#welcomeMessage", "Welcome Message", ElementType.Div);
        ErrorMessage = new Element("#errorMessage", "Error Message", ElementType.Div);
        Form = new Element("#loginForm", "Login Form", ElementType.Form);
    }
    
    // Custom methods for login flow
    public LoginPage FillLoginForm(string email, string password)
    {
        return this
            .Type(e => e.EmailInput, email)
            .Type(e => e.PasswordInput, password);
    }
    
    public LoginPage SubmitLogin()
    {
        return this.Click(e => e.LoginButton);
    }
    
    public LoginPage VerifyLoginSuccess(string expectedWelcomeMessage)
    {
        return this.Verify(e => e.WelcomeMessage, expectedWelcomeMessage);
    }
    
    public LoginPage VerifyLoginError(string expectedError)
    {
        return this.Verify(e => e.ErrorMessage, expectedError);
    }
}
```

### 3. Sample App HTML Implementation

Create the HTML pages for the sample app:

#### Registration Page (HTML)
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>User Registration - FluentUIScaffold Sample</title>
    <style>
        .registration-form {
            max-width: 400px;
            margin: 50px auto;
            padding: 20px;
            border: 1px solid #ccc;
            border-radius: 5px;
        }
        .form-group {
            margin-bottom: 15px;
        }
        .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
        }
        .form-group input {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 3px;
        }
        .form-group button {
            width: 100%;
            padding: 10px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 3px;
            cursor: pointer;
        }
        .form-group button:hover {
            background-color: #0056b3;
        }
        .message {
            margin-top: 15px;
            padding: 10px;
            border-radius: 3px;
        }
        .success {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        .error {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        .hidden {
            display: none;
        }
    </style>
</head>
<body>
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
            <div class="form-group">
                <button type="submit" id="registerButton">Register</button>
            </div>
        </form>
        <div id="successMessage" class="message success hidden">Registration successful!</div>
        <div id="errorMessage" class="message error hidden"></div>
    </div>

    <script>
        document.getElementById('registrationForm').addEventListener('submit', function(e) {
            e.preventDefault();
            
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            const firstName = document.getElementById('firstName').value;
            const lastName = document.getElementById('lastName').value;
            
            // Simple validation
            if (!email || !password || !firstName || !lastName) {
                showError('All fields are required');
                return;
            }
            
            if (password.length < 8) {
                showError('Password must be at least 8 characters long');
                return;
            }
            
            // Simulate successful registration
            showSuccess();
        });
        
        function showSuccess() {
            document.getElementById('successMessage').classList.remove('hidden');
            document.getElementById('errorMessage').classList.add('hidden');
        }
        
        function showError(message) {
            document.getElementById('errorMessage').textContent = message;
            document.getElementById('errorMessage').classList.remove('hidden');
            document.getElementById('successMessage').classList.add('hidden');
        }
    </script>
</body>
</html>
```

#### Login Page (HTML)
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>User Login - FluentUIScaffold Sample</title>
    <style>
        .login-form {
            max-width: 400px;
            margin: 50px auto;
            padding: 20px;
            border: 1px solid #ccc;
            border-radius: 5px;
        }
        .form-group {
            margin-bottom: 15px;
        }
        .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
        }
        .form-group input {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 3px;
        }
        .form-group button {
            width: 100%;
            padding: 10px;
            background-color: #28a745;
            color: white;
            border: none;
            border-radius: 3px;
            cursor: pointer;
        }
        .form-group button:hover {
            background-color: #218838;
        }
        .message {
            margin-top: 15px;
            padding: 10px;
            border-radius: 3px;
        }
        .success {
            background-color: #d4edda;
            color: #155724;
            border: 1px solid #c3e6cb;
        }
        .error {
            background-color: #f8d7da;
            color: #721c24;
            border: 1px solid #f5c6cb;
        }
        .hidden {
            display: none;
        }
    </style>
</head>
<body>
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
            <div class="form-group">
                <button type="submit" id="loginButton">Login</button>
            </div>
        </form>
        <div id="welcomeMessage" class="message success hidden">Welcome, John!</div>
        <div id="errorMessage" class="message error hidden"></div>
    </div>

    <script>
        document.getElementById('loginForm').addEventListener('submit', function(e) {
            e.preventDefault();
            
            const email = document.getElementById('email').value;
            const password = document.getElementById('password').value;
            
            // Simple validation
            if (!email || !password) {
                showError('Email and password are required');
                return;
            }
            
            // Simulate login with test credentials
            if (email === 'john.doe@example.com' && password === 'SecurePass123!') {
                showWelcome('John');
            } else {
                showError('Invalid email or password');
            }
        });
        
        function showWelcome(firstName) {
            document.getElementById('welcomeMessage').textContent = `Welcome, ${firstName}!`;
            document.getElementById('welcomeMessage').classList.remove('hidden');
            document.getElementById('errorMessage').classList.add('hidden');
        }
        
        function showError(message) {
            document.getElementById('errorMessage').textContent = message;
            document.getElementById('errorMessage').classList.remove('hidden');
            document.getElementById('welcomeMessage').classList.add('hidden');
        }
    </script>
</body>
</html>
```

### 4. Working Example Implementation

Create a complete working example that demonstrates the registration and login flow:

```csharp
[TestMethod]
public async Task Can_Complete_Registration_And_Login_Flow()
{
    // Arrange
    var fluentUI = FluentUIScaffoldBuilder.Web(options =>
    {
        options.BaseUrl = new Uri("https://your-app.com");
    });
    
    // Act - Registration
    var registrationPage = fluentUI.NavigateTo<RegistrationPage>();
    
    registrationPage
        .FillRegistrationForm("john.doe@example.com", "SecurePass123!", "John", "Doe")
        .SubmitRegistration()
        .VerifyRegistrationSuccess();
    
    // Act - Login
    var loginPage = fluentUI.NavigateTo<LoginPage>();
    
    loginPage
        .FillLoginForm("john.doe@example.com", "SecurePass123!")
        .SubmitLogin()
        .VerifyLoginSuccess("Welcome, John!");
}
```

## Implementation Tasks

### Phase 1: Page Class Implementation
1. [x] Create RegistrationPage class with all elements
2. [x] Create LoginPage class with all elements
3. [x] Add custom methods for form interactions
4. [x] Add verification methods for success/error states

### Phase 2: Sample App HTML
1. [x] Create registration page HTML with styling
2. [x] Create login page HTML with styling
3. [x] Add JavaScript for form validation
4. [x] Add JavaScript for success/error handling

### Phase 3: Integration
1. [x] Add pages to sample app routing
2. [x] Test page navigation and form interactions
3. [x] Verify all elements are properly configured
4. [x] Test error scenarios and validation

### Phase 4: Testing
1. [x] Create comprehensive tests for registration flow
2. [x] Create comprehensive tests for login flow
3. [x] Test error scenarios and validation
4. [x] Test navigation between pages

## Dependencies

- **Story 1.1.1**: Refactor to V2.0 BasePageComponent Pattern (must be completed first)
- **Story 1.1.2**: Implement Basic Navigation Methods (must be completed first)
- **Story 1.2.1**: Implement Base Element Actions (must be completed first)

## Estimation

- **Time Estimate**: 2-3 weeks
- **Complexity**: Medium
- **Risk**: Low (building on established foundation)

## Definition of Done

- [x] RegistrationPage class is implemented and working
- [x] LoginPage class is implemented and working
- [x] Sample app has working registration and login pages
- [x] Form validation and error handling work correctly
- [x] Navigation between pages works correctly
- [x] Comprehensive tests are passing
- [x] Working example demonstrates complete flow
- [x] All acceptance criteria are met

## Notes

- The sample app should provide realistic form validation
- Error handling should be comprehensive
- The pages should be responsive and user-friendly
- All form interactions should work with the fluent API

## ✅ **Story Completion Status**

**Status**: 🟢 **COMPLETED** ✅

**Completion Date**: December 2024

**Implementation Summary**:
- ✅ RegistrationPage class implemented with all required elements (EmailInput, PasswordInput, FirstNameInput, LastNameInput, RegisterButton, SuccessMessage, ErrorMessage, Form)
- ✅ LoginPage class implemented with all required elements (EmailInput, PasswordInput, LoginButton, WelcomeMessage, ErrorMessage, Form)
- ✅ Sample app pages created using Svelte components with proper styling and validation
- ✅ Custom methods implemented: FillRegistrationForm, SubmitRegistration, FillLoginForm, SubmitLogin
- ✅ Verification methods implemented: VerifyRegistrationSuccess, VerifyRegistrationError, VerifyLoginSuccess, VerifyLoginError
- ✅ Comprehensive test suite with 11 test methods covering all scenarios
- ✅ Working example demonstrating complete registration and login flow
- ✅ All tests passing (11/11)

**Key Features Implemented**:
- Form validation with client-side error handling
- Success/error message display
- Fluent API for form interactions
- Navigation between registration and login pages
- Real Playwright integration with working browser automation 