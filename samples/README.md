# FluentUIScaffold Sample Application

This directory contains a comprehensive sample application that demonstrates the FluentUIScaffold E2E testing framework. The sample includes a Svelte-based web application with various UI components and corresponding test examples.

## Overview

The sample application showcases:

- **Modern Web Application**: ASP.NET Core backend with Svelte frontend using Vite
- **Complex UI Components**: Todo list, user profile forms, interactive elements
- **Comprehensive Testing**: Page objects, fluent API usage, various testing scenarios
- **Framework Features**: Element configuration, wait strategies, navigation, verification

## Project Structure

```
samples/
├── SampleApp/                    # ASP.NET Core web application
│   ├── ClientApp/               # Svelte frontend with Vite
│   │   ├── src/
│   │   │   ├── App.svelte      # Main application component
│   │   │   └── lib/
│   │   │       ├── Counter.svelte      # Interactive counter component
│   │   │       ├── TodoList.svelte     # Todo management component
│   │   │       └── UserProfile.svelte  # User profile form component
│   │   └── package.json
│   ├── Controllers/             # API controllers
│   ├── Program.cs               # ASP.NET Core startup
│   └── SampleApp.csproj
└── SampleApp.Tests/             # Test project with examples
    ├── Pages/                   # Page object implementations
    │   ├── HomePage.cs         # Home page page object
    │   ├── TodosPage.cs        # Todos page page object
    │   └── ProfilePage.cs      # Profile page page object
    ├── Examples/                # Example test implementations
    │   └── HomePageTests.cs    # Comprehensive test examples
    └── SampleApp.Tests.csproj
```

## Features Demonstrated

### Web Application Features

1. **Multi-tab Navigation**: Home, Todos, and Profile tabs
2. **Interactive Counter**: Clickable counter with state management
3. **Todo Management**: Add, edit, filter, and delete todos
4. **User Profile Form**: Complex form with validation
5. **Weather Data**: Async data loading and display
6. **Responsive Design**: Modern UI with CSS Grid and Flexbox

### Testing Framework Features

1. **Fluent API**: Chainable method calls for readable tests
2. **Page Object Pattern**: Encapsulated page interactions
3. **Element Configuration**: Flexible element definition with wait strategies
4. **Navigation**: Seamless page transitions
5. **Verification**: Comprehensive assertion methods
6. **Error Handling**: Graceful handling of timeouts and failures

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Node.js 16+ and npm
- Visual Studio 2022 or VS Code

### Running the Sample Application

1. **Navigate to the sample app directory**:
   ```bash
   cd samples/SampleApp
   ```

2. **Install frontend dependencies**:
   ```bash
   cd ClientApp
   npm install
   cd ..
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Access the application**:
   - Open your browser to `http://localhost:5001`
   - The application will automatically proxy to the Vite dev server

### Running the Tests

1. **Navigate to the test project**:
   ```bash
   cd samples/SampleApp.Tests
   ```

2. **Run the tests**:
   ```bash
   dotnet test
   ```

## Framework Usage Examples

### Basic Setup

```csharp
// Configure FluentUIScaffold with auto-discovery
var options = new FluentUIScaffoldOptions
{
    BaseUrl = new Uri("http://localhost:5000"),
    DefaultWaitTimeout = TimeSpan.FromSeconds(30),
    DefaultRetryInterval = TimeSpan.FromMilliseconds(500),
    LogLevel = LogLevel.Information,
    CaptureScreenshotsOnFailure = true,
    HeadlessMode = true
};

var fluentUI = new FluentUIScaffoldApp<WebApp>(options);
```

### Page Object Pattern

```csharp
public class HomePage : BasePageComponent<WebApp>
{
    private IElement _counterButton;
    private IElement _navTodosButton;

    protected override void ConfigureElements()
    {
        _counterButton = Element("button")
            .WithDescription("Counter Button")
            .WithWaitStrategy(WaitStrategy.Clickable);

        _navTodosButton = Element("[data-testid='nav-todos']")
            .WithDescription("Todos Navigation Button")
            .WithWaitStrategy(WaitStrategy.Clickable);
    }

    public HomePage ClickCounter()
    {
        _counterButton.Click();
        return this;
    }

    public TodosPage NavigateToTodos()
    {
        _navTodosButton.Click();
        return new TodosPage(Driver, Options, Logger);
    }
}
```

### Writing Tests

```csharp
[TestMethod]
public async Task Can_Interact_With_Counter_Component()
{
    // Arrange
    var homePage = _fluentUI.NavigateTo<HomePage>();

    // Act - Click the counter multiple times
    homePage
        .WaitFor(e => e.CounterButton)
        .ClickCounter()
        .ClickCounter()
        .ClickCounter();

    // Assert - Verify the counter shows the expected value
    homePage.VerifyCounterValue("3");
}
```

### Complex Workflows

```csharp
[TestMethod]
public async Task Can_Perform_Complex_User_Workflow()
{
    // Arrange
    var homePage = _fluentUI.NavigateTo<HomePage>();

    // Act - Perform a complex workflow
    homePage
        .WaitFor(e => e.CounterButton)
        .ClickCounter() // Click counter once
        .VerifyCounterValue("1") // Verify counter updated
        .ClickCounterMultipleTimes(3) // Click 3 more times
        .VerifyCounterValue("4") // Verify final count
        .WaitForWeatherData() // Wait for weather data
        .VerifyWeatherItemsAreDisplayed(); // Verify weather data loaded

    // Act - Navigate to todos page
    var todosPage = homePage.NavigateToTodos();
    todosPage.VerifyTodoCount(3); // Verify default todos

    // Act - Navigate back to home
    var backToHome = todosPage.NavigateToHome();
    backToHome.VerifyPageTitle("FluentUIScaffold Sample App"); // Verify we're back
}
```

## Framework Features

### Element Configuration

The framework provides a fluent API for configuring elements:

```csharp
private IElement _counterButton = Element("button")
    .WithDescription("Counter Button")
    .WithWaitStrategy(WaitStrategy.Clickable)
    .WithTimeout(TimeSpan.FromSeconds(10))
    .WithRetryInterval(TimeSpan.FromMilliseconds(200));
```

### Wait Strategies

- `None`: No waiting
- `Visible`: Wait for element to be visible
- `Hidden`: Wait for element to be hidden
- `Clickable`: Wait for element to be clickable
- `Enabled`: Wait for element to be enabled
- `Disabled`: Wait for element to be disabled
- `TextPresent`: Wait for specific text to be present
- `Smart`: Framework-specific intelligent waiting

### Navigation

Seamless navigation between pages:

```csharp
var todosPage = homePage.NavigateToTodos();
var profilePage = todosPage.NavigateToProfile();
var backToHome = profilePage.NavigateToHome();
```

### Verification

Comprehensive verification methods:

```csharp
homePage
    .VerifyPageTitle("Expected Title")
    .VerifyCounterValue("3")
    .VerifyWeatherItemsAreDisplayed()
    .VerifyLogosAreVisible();
```

## Best Practices

### 1. Page Object Design

- **Encapsulate Elements**: Define all page elements in `ConfigureElements()`
- **Method Chaining**: Return `this` for fluent method chaining
- **Clear Descriptions**: Use descriptive names for elements and methods
- **Separation of Concerns**: Keep page logic separate from test logic

### 2. Element Configuration

- **Use Data Attributes**: Prefer `data-testid` attributes for reliable selection
- **Appropriate Wait Strategies**: Choose wait strategies based on element behavior
- **Meaningful Descriptions**: Provide clear descriptions for debugging

### 3. Test Structure

- **Arrange-Act-Assert**: Follow the AAA pattern
- **Single Responsibility**: Each test should verify one specific behavior
- **Descriptive Names**: Use clear, descriptive test method names
- **Proper Cleanup**: Always dispose of resources in test cleanup

### 4. Error Handling

- **Graceful Degradation**: Handle missing elements gracefully
- **Timeout Configuration**: Set appropriate timeouts for your application
- **Screenshot Capture**: Enable screenshot capture for debugging failures

## Advanced Features

### Framework-Specific Access

Access underlying framework features when needed:

```csharp
var playwrightDriver = _fluentUI.Framework<PlaywrightDriver>();
// Use Playwright-specific features
```

### Custom Wait Conditions

Implement custom wait conditions for complex scenarios:

```csharp
private IElement _customElement = Element("selector")
    .WithCustomWaitCondition(() => /* custom condition */)
    .WithDescription("Custom Element");
```

### Data-Driven Testing

Structure tests for data-driven scenarios:

```csharp
[TestMethod]
[DataRow(1, "1")]
[DataRow(3, "3")]
[DataRow(5, "5")]
public async Task Can_Test_Counter_With_Different_Values(int clickCount, string expectedValue)
{
    // Test implementation
}
```

## Troubleshooting

### Common Issues

1. **Element Not Found**: Check selectors and ensure elements are present
2. **Timeout Errors**: Increase timeout or check element visibility
3. **Navigation Failures**: Verify URL patterns and page validation
4. **Framework Errors**: Ensure proper driver configuration

### Debugging Tips

1. **Enable Logging**: Set appropriate log levels for debugging
2. **Screenshot Capture**: Enable screenshots on failure
3. **Element Inspection**: Use browser dev tools to verify selectors
4. **Wait Strategies**: Choose appropriate wait strategies for your elements

## Contributing

When contributing to the sample application:

1. **Follow Patterns**: Use existing patterns for consistency
2. **Add Documentation**: Document new features and examples
3. **Update Tests**: Ensure all new features have corresponding tests
4. **Maintain Quality**: Follow coding standards and best practices

## Next Steps

1. **Explore the Framework**: Review the main FluentUIScaffold framework documentation
2. **Extend Examples**: Add more complex scenarios and edge cases
3. **Customize**: Adapt the examples to your specific testing needs
4. **Contribute**: Share improvements and additional examples

This sample application provides a solid foundation for understanding and using the FluentUIScaffold E2E testing framework effectively. 