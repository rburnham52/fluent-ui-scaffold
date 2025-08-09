# Testing Strategy

## Overview

The FluentUIScaffold project has two distinct types of tests with different purposes:

### 1. Framework Tests (Unit/Integration Tests)

**Location**: `tests/FluentUIScaffold.Core.Tests/` and `tests/FluentUIScaffold.Playwright.Tests/`

**Purpose**: Verify that the FluentUIScaffold framework itself works correctly.

**Examples**:
- Configuration builder tests
- Server launcher tests
- Plugin management tests
- Driver initialization tests
- Options validation tests

**Focus**: Testing the framework's internal functionality, APIs, and integration points.

### 2. Sample UI Tests (Functional E2E Tests)

**Location**: `samples/SampleApp.Tests/`

**Purpose**: Demonstrate how to use FluentUIScaffold to create functional UI tests for a real application.

**Examples**:
- Testing actual page navigation and content
- Testing form interactions and validation
- Testing user workflows (registration, login, etc.)
- Testing component behavior (counter, todo list, etc.)
- Testing responsive design and accessibility

**Focus**: Testing the behavior of the SampleApp UI and demonstrating real-world usage patterns.

## Test Naming Standards

### Three-Part Test Naming Convention

The name of your test should consist of three parts:

1. **Name of the method being tested**
2. **Scenario under which the method is being tested**
3. **Expected behavior when the scenario is invoked**

### Format: `MethodName_Scenario_ExpectedBehavior`

#### Framework Test Examples

```csharp
// Good examples
[Test]
public void Constructor_WithValidOptions_CreatesInstance()
[Test]
public void WithBaseUrl_WithValidUrl_SetsBaseUrl()
[Test]
public void WithDefaultWaitTimeout_WithValidTimeout_SetsDefaultWaitTimeout()
[Test]
public void InitializeAsync_WhenCalled_InitializesFramework()
[Test]
public void NavigateToUrl_WithInvalidUrl_ThrowsException()

// Bad examples
[Test]
public void TestConstructor() // Missing scenario and expected behavior
[Test]
public void TestBaseUrl() // Missing scenario and expected behavior
[Test]
public void ConstructorTest() // Missing scenario and expected behavior
```

#### Sample UI Test Examples

```csharp
// Good examples
[TestMethod]
public async Task NavigateToHome_WhenCalled_LoadsHomePageWithExpectedContent()
[TestMethod]
public async Task ClickCounter_WhenClicked_IncrementsCounterValue()
[TestMethod]
public async Task CompleteRegistration_WithValidData_CreatesAccountAndShowsSuccessMessage()
[TestMethod]
public async Task SubmitLoginForm_WithInvalidCredentials_ShowsErrorMessage()
[TestMethod]
public async Task FillRegistrationForm_WithEmptyFields_ShowsValidationErrors()

// Bad examples
[TestMethod]
public async Task Can_Load_Home_Page() // Missing scenario and expected behavior
[TestMethod]
public async Task TestCounter() // Missing scenario and expected behavior
[TestMethod]
public async Task RegistrationTest() // Missing scenario and expected behavior
```

### Naming Guidelines

1. **Use descriptive method names**: The method name should clearly indicate what functionality is being tested
2. **Include the scenario**: Describe the specific conditions or inputs being tested
3. **Specify expected behavior**: Clearly state what should happen when the scenario is executed
4. **Use underscores to separate parts**: This makes the test name more readable
5. **Be specific**: Avoid generic terms like "works" or "succeeds" - describe the actual expected outcome

### Common Patterns

- `MethodName_WithValidInput_ReturnsExpectedResult()`
- `MethodName_WithInvalidInput_ThrowsException()`
- `MethodName_WhenConditionIsMet_BehavesAsExpected()`
- `MethodName_WithSpecificScenario_ProducesExpectedOutput()`

## Testing Best Practices

### Framework Tests
- Test individual components in isolation
- Mock external dependencies
- Focus on API contracts and edge cases
- Ensure framework reliability and performance

### Sample UI Tests
- Test complete user workflows
- Verify actual UI behavior and content
- Test responsive design and accessibility
- Demonstrate real-world testing scenarios
- Show how to handle dynamic content and user interactions

## Test Structure

```
tests/
├── FluentUIScaffold.Core.Tests/          # Framework unit tests
├── FluentUIScaffold.Playwright.Tests/    # Framework integration tests
└── samples/
    └── SampleApp.Tests/                   # Sample UI functional tests
        └── Examples/
            ├── HomePageTests.cs           # Tests home page functionality
            ├── FormInteractionTests.cs    # Tests form interactions
            ├── RegistrationFlowTests.cs   # Tests registration workflow
            ├── LoginFlowTests.cs         # Tests login workflow
            └── AdvancedNavigationTests.cs # Tests navigation scenarios
```

## Writing Sample UI Tests

When writing sample UI tests, focus on:

1. **Real User Scenarios**: Test actual user workflows, not just framework setup
2. **Content Verification**: Verify that pages load with expected content
3. **Interaction Testing**: Test user interactions like clicking, typing, form submission
4. **State Changes**: Verify that UI state changes appropriately after interactions
5. **Error Handling**: Test error scenarios and validation messages
6. **Accessibility**: Ensure tests work with screen readers and keyboard navigation

### Example: Good Sample Test

```csharp
[TestMethod]
public async Task Can_Complete_Registration_Flow()
{
    // Arrange
    var options = new FluentUIScaffoldOptionsBuilder()
        .WithBaseUrl(new Uri("http://localhost:5000"))
        .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
        .Build();

    // Act
    using var app = new FluentUIScaffoldApp<WebApp>(options);
    await app.InitializeAsync();

    // Navigate to registration page
    app.NavigateToUrl(new Uri("http://localhost:5000"));
    
    // Click registration tab
    var playwright = app.Framework<FluentUIScaffold.Playwright.PlaywrightDriver>();
    await playwright.ClickAsync("[data-testid='nav-register']");

    // Fill registration form
    await playwright.FillAsync("#email-input", "test@example.com");
    await playwright.FillAsync("#password-input", "password123");
    await playwright.FillAsync("#first-name-input", "John");
    await playwright.FillAsync("#last-name-input", "Doe");

    // Submit form
    await playwright.ClickAsync("#register-button");

    // Assert
    // Verify success message appears
    var successMessage = await playwright.WaitForSelectorAsync("#success-message");
    Assert.IsNotNull(successMessage);
    
    // Verify form is cleared
    var emailValue = await playwright.GetAttributeAsync("#email-input", "value");
    Assert.AreEqual("", emailValue);
}
```

### Example: Bad Sample Test (Avoid This)

```csharp
[TestMethod]
public async Task Can_Complete_Registration_Flow()
{
    // Arrange
    var options = new FluentUIScaffoldOptionsBuilder()
        .WithBaseUrl(new Uri("http://localhost:5000"))
        .Build();

    // Act & Assert
    // This test would normally complete a registration flow, but for now we'll just verify the options are set correctly
    Assert.AreEqual(new Uri("http://localhost:5000"), options.BaseUrl);
    Assert.IsTrue(true, "Successfully set up registration flow test");
}
```

## Key Principles

1. **SampleApp.Tests should test SampleApp behavior**, not FluentUIScaffold framework features
2. **Framework tests should test framework functionality**, not application behavior
3. **Sample tests should be realistic examples** of how to use FluentUIScaffold in real projects
4. **Tests should verify actual UI content and interactions**, not just configuration options
5. **Tests should demonstrate best practices** for UI testing with FluentUIScaffold

