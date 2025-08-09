# SampleApp.Tests

This project contains sample tests demonstrating the FluentUIScaffold framework usage.

## Test Categories

### 1. Framework Tests (`FrameworkTests.cs`)
These tests verify the framework functionality without requiring a running web application:
- Framework initialization
- Configuration options
- Page object creation
- Graceful handling when web app is not running

### 2. UI Tests (All other test files)
These tests require a running web application to test actual UI interactions:
- Home page functionality
- Registration and login flows
- Form interactions
- Navigation
- Debug mode functionality

## Running Tests

### Prerequisites
1. .NET 6.0, 7.0, or 8.0
2. Node.js (for the Svelte frontend)
3. Playwright browsers installed

### Running Framework Tests Only
```bash
# Run only framework tests (no web app required)
dotnet test --filter "FrameworkTests" --verbosity normal
```

### Running All Tests with Web Application

1. **Start the SampleApp**:
   ```bash
   cd samples/SampleApp
   dotnet run
   ```
   This will start the web application on `http://localhost:5000`

2. **In another terminal, run the tests**:
   ```bash
   cd samples/SampleApp.Tests
   dotnet test --verbosity normal
   ```

### Running Specific Test Categories

```bash
# Run only HomePage tests
dotnet test --filter "HomePageTests" --verbosity normal

# Run only Login tests
dotnet test --filter "LoginFlowTests" --verbosity normal

# Run only Registration tests
dotnet test --filter "RegistrationFlowTests" --verbosity normal
```

## Test Naming Convention

All tests follow the `MethodName_Scenario_ExpectedBehavior` naming convention:

- **MethodName**: The method or functionality being tested
- **Scenario**: The specific scenario or conditions under which the test runs
- **ExpectedBehavior**: The expected outcome when the scenario is invoked

### Examples:
- `FrameworkInitialization_WhenValidOptionsProvided_InitializesSuccessfully`
- `CompleteLoginFlow_WithValidCredentials_ShowsSuccessMessageAndClearsForm`
- `NavigateToSection_WhenValidSectionProvided_NavigatesToSection`

## Troubleshooting

### Browser Launch Issues
If you encounter browser launch timeouts:
1. Ensure Playwright browsers are installed: `npx playwright install`
2. Check system resources (memory, CPU)
3. Try running with headless mode enabled

### Web Application Not Running
If tests fail with connection errors:
1. Ensure the SampleApp is running on `http://localhost:5000`
2. Check that the Svelte frontend is built and served
3. Verify no firewall or antivirus is blocking the connection

### Test Timeouts
If tests timeout waiting for elements:
1. Increase the default wait timeout in test options
2. Check that the web application is responding correctly
3. Verify element selectors match the actual UI

## Test Structure

### Page Object Model
Tests use the Page Object Model pattern with classes like:
- `HomePage`: Home page interactions and verifications
- `RegistrationPage`: Registration form interactions
- `LoginPage`: Login form interactions
- `TodosPage`: Todo list interactions
- `ProfilePage`: Profile form interactions

### Test Setup
Each test class follows this pattern:
```csharp
[TestInitialize]
public async Task Setup()
{
    // Initialize FluentUIScaffoldApp
    // Create page objects
}

[TestCleanup]
public void Cleanup()
{
    // Dispose of resources
}
```

## Debug Mode

Tests can be run with debug mode enabled to get detailed logging:
```csharp
var options = new FluentUIScaffoldOptionsBuilder()
    .WithDebugMode(true)
    .Build();
```

This provides:
- Detailed interaction logging
- Slower execution for debugging
- More verbose error messages

