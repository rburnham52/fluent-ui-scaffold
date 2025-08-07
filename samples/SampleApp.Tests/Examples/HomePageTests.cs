using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests for the home page functionality using Page Object Models.
    /// These tests demonstrate how to use POMs to create reusable, maintainable UI tests.
    /// </summary>
    [TestClass]
    public class HomePageTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private HomePage? _homePage;

        [TestInitialize]
        public async Task Setup()
        {
            // Arrange - Set up the application and page objects
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("https://localhost:5001"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            // Create the home page object
            _homePage = new HomePage(_app.ServiceProvider);
            await _homePage.NavigateToHomeAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
        }

        [TestMethod]
        public async Task HasExpectedContent_WhenHomePageLoaded_ReturnsTrue()
        {
            // Act & Assert - Use the Page Object Model to verify page content
            var hasExpectedContent = await _homePage!.HasExpectedContentAsync();
            Assert.IsTrue(hasExpectedContent, "Home page should have expected content");

            var pageTitle = _homePage.GetPageTitle();
            Assert.IsTrue(pageTitle.Contains("FluentUIScaffold Sample App"), "Page title should contain the app name");

            var welcomeMessage = _homePage.GetWelcomeMessage();
            Assert.IsTrue(welcomeMessage.Contains("Welcome to"), "Welcome message should be present");

            var subtitle = _homePage.GetSubtitle();
            Assert.IsTrue(subtitle.Contains("sample application demonstrating"), "Subtitle should describe the app purpose");
        }

        [TestMethod]
        public async Task ClickCounter_WhenClicked_IncrementsCounterValue()
        {
            // Act - Use the Page Object Model to interact with the counter
            var initialCount = _homePage!.GetCounterValue();
            var newCount = _homePage.ClickCounter();
            var finalCount = _homePage.ClickCounter();

            // Assert - Verify counter behavior
            Assert.AreEqual(initialCount + 1, newCount, "Counter should increment when clicked");
            Assert.AreEqual(initialCount + 2, finalCount, "Counter should increment multiple times");
        }

        [TestMethod]
        public async Task IsWeatherDataDisplayed_WhenWeatherSectionLoaded_ReturnsTrue()
        {
            // Act & Assert - Use the Page Object Model to verify weather data
            var isWeatherDisplayed = await _homePage!.IsWeatherDataDisplayedAsync();
            Assert.IsTrue(isWeatherDisplayed, "Weather data should be displayed");

            var weatherTitle = _homePage.GetWeatherSectionTitle();
            Assert.AreEqual("Weather Data", weatherTitle, "Weather section should have correct title");

            var weatherItemText = _homePage.GetFirstWeatherItemText();
            Assert.IsTrue(weatherItemText.Contains("Temperature:"), "Weather item should contain temperature information");
            Assert.IsTrue(weatherItemText.Contains("Summary:"), "Weather item should contain summary information");
        }

        [TestMethod]
        public async Task NavigateToSection_WhenValidSectionProvided_NavigatesToSection()
        {
            // Arrange - Create page objects for different sections
            var todosPage = new TodosPage(_app!.ServiceProvider, new Uri("https://localhost:5001"));
            var profilePage = new ProfilePage(_app.ServiceProvider, new Uri("https://localhost:5001"));
            var registrationPage = new RegistrationPage(_app.ServiceProvider);
            var loginPage = new LoginPage(_app.ServiceProvider, new Uri("https://localhost:5001"));

            // Act & Assert - Navigate to todos section and verify it loads
            await _homePage!.NavigateToSectionAsync("todos");
            var isTodosActive = await _homePage.IsNavigationButtonActiveAsync("todos");
            Assert.IsTrue(isTodosActive, "Todos button should be active when on todos page");

            // Verify todos page content is accessible
            todosPage.AddTodo("Test todo");
            var todoText = todosPage.GetTodoText(0);
            Assert.IsTrue(todoText.Contains("Test todo"), "Todos page should allow adding todos");

            // Act & Assert - Navigate to profile section and verify it loads
            await _homePage.NavigateToSectionAsync("profile");
            var isProfileActive = await _homePage.IsNavigationButtonActiveAsync("profile");
            Assert.IsTrue(isProfileActive, "Profile button should be active when on profile page");

            // Verify profile page content is accessible
            profilePage.EnterName("John Doe");
            profilePage.EnterEmail("john@example.com");
            profilePage.ClickSave();
            var profileName = profilePage.GetName();
            Assert.IsTrue(profileName.Contains("John Doe"), "Profile page should allow editing profile information");

            // Act & Assert - Navigate to register section and verify it loads
            await _homePage.NavigateToSectionAsync("register");
            var isRegisterActive = await _homePage.IsNavigationButtonActiveAsync("register");
            Assert.IsTrue(isRegisterActive, "Register button should be active when on register page");

            // Verify registration page content is accessible
            registrationPage.VerifyFormStructure();
            registrationPage.FillRegistrationForm("test@example.com", "password123", "Test", "User");

            // Act & Assert - Navigate to login section and verify it loads
            await _homePage.NavigateToSectionAsync("login");
            var isLoginActive = await _homePage.IsNavigationButtonActiveAsync("login");
            Assert.IsTrue(isLoginActive, "Login button should be active when on login page");

            // Verify login page content is accessible
            loginPage.FillLoginForm("test@example.com", "password123");
            loginPage.SubmitLogin();

            // Act & Assert - Navigate back to home and verify it loads
            await _homePage.NavigateToSectionAsync("home");
            var isHomeActive = await _homePage.IsNavigationButtonActiveAsync("home");
            Assert.IsTrue(isHomeActive, "Home button should be active when on home page");

            // Verify home page content is still accessible
            var hasExpectedContent = await _homePage.HasExpectedContentAsync();
            Assert.IsTrue(hasExpectedContent, "Home page should have expected content after navigation");
        }

        [TestMethod]
        public async Task IsNavigationButtonActive_WhenOnHomePage_ReturnsTrueForHomeButton()
        {
            // Act & Assert - Use the Page Object Model to verify navigation states
            var isHomeActive = await _homePage!.IsNavigationButtonActiveAsync("home");
            Assert.IsTrue(isHomeActive, "Home button should be active on home page");

            await _homePage.NavigateToSectionAsync("todos");
            var isTodosActive = await _homePage.IsNavigationButtonActiveAsync("todos");
            Assert.IsTrue(isTodosActive, "Todos button should be active when on todos page");

            var isHomeStillActive = await _homePage.IsNavigationButtonActiveAsync("home");
            Assert.IsFalse(isHomeStillActive, "Home button should not be active when on other pages");
        }

        [TestMethod]
        public async Task CompleteHomePageWorkflow_WhenAllComponentsInteracted_CompletesSuccessfully()
        {
            // Act - Complete a full workflow using the Page Object Model
            var hasExpectedContent = await _homePage!.HasExpectedContentAsync();
            Assert.IsTrue(hasExpectedContent, "Page should load with expected content");

            var initialCount = _homePage.GetCounterValue();
            var newCount = _homePage.ClickCounter();
            Assert.AreEqual(initialCount + 1, newCount, "Counter should increment");

            var isWeatherDisplayed = await _homePage.IsWeatherDataDisplayedAsync();
            Assert.IsTrue(isWeatherDisplayed, "Weather data should be displayed");

            await _homePage.NavigateToSectionAsync("register");
            var isRegisterActive = await _homePage.IsNavigationButtonActiveAsync("register");
            Assert.IsTrue(isRegisterActive, "Register button should be active after navigation");
        }

        [TestMethod]
        public async Task FrameworkInitialization_WhenValidOptionsProvided_InitializesSuccessfully()
        {
            // Arrange & Act - Test that the framework can be initialized
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("https://localhost:5001"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            using var app = new FluentUIScaffoldApp<WebApp>(options);
            await app.InitializeAsync();

            // Assert - Verify the framework initialized successfully
            Assert.IsNotNull(app, "FluentUIScaffoldApp should be created");
            Assert.IsNotNull(app.ServiceProvider, "ServiceProvider should be available");

            // Note: This test verifies framework initialization without requiring a running web app
            // In a real scenario, you would start the web application before running UI tests
        }

        [TestMethod]
        public async Task WebAppNotRunning_WhenAttemptingToNavigate_HandlesGracefully()
        {
            // Arrange
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(new Uri("https://localhost:5001"))
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(5)) // Shorter timeout for this test
                .WithDebugMode(false)
                .Build();

            using var app = new FluentUIScaffoldApp<WebApp>(options);
            await app.InitializeAsync();

            var homePage = new HomePage(app.ServiceProvider);

            // Act & Assert - This should fail gracefully when the web app isn't running
            try
            {
                await homePage.NavigateToHomeAsync();
                // If we get here, the web app might be running
                Assert.Fail("Expected navigation to fail when web app is not running");
            }
            catch (Exception ex)
            {
                // This is expected behavior when the web application isn't running
                Assert.IsTrue(ex.Message.Contains("timeout") || ex.Message.Contains("connection"),
                    "Should fail with timeout or connection error when web app is not running");
            }
        }
    }
}
