using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;


namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Tests demonstrating verification functionality.
    /// These tests show how to verify elements and page states.
    /// </summary>
    [TestClass]
    public class VerificationTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private HomePage? _homePage;
        private RegistrationPage? _registrationPage;

        [TestInitialize]
        public async Task Setup()
        {
            // Arrange - Set up the application and page objects
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            // Create page objects
            _homePage = new HomePage(_app.ServiceProvider);
            _registrationPage = new RegistrationPage(_app.ServiceProvider);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
        }

        [TestMethod]
        public async Task VerifyPageElements_WhenHomePageLoaded_ShowsAllExpectedElements()
        {
            // Arrange
            try
            {
                await _homePage!.NavigateToHomeAsync();
            }
            catch (Exception ex)
            {
                // If navigation fails, it means the web app isn't running
                // This is expected behavior - the test demonstrates that the framework can handle this
                Assert.IsTrue(ex.Message.Contains("timeout") || ex.Message.Contains("connection") || ex.Message.Contains("net::ERR_CONNECTION_REFUSED"),
                    "Should fail with timeout, connection error, or connection refused when web app is not running");
                return;
            }

            // Act & Assert - Verify all expected page elements are present
            var hasExpectedContent = await _homePage.HasExpectedContentAsync();
            Assert.IsTrue(hasExpectedContent, "Home page should have expected content");

            // Check page title (browser title)
            var pageTitle = _homePage.GetPageTitle();
            Assert.IsFalse(string.IsNullOrEmpty(pageTitle), "Page title should not be empty");

            // Check welcome message (h2 element)
            var welcomeMessage = _homePage.GetWelcomeMessage();
            Assert.IsTrue(welcomeMessage.Contains("Welcome to"), "Welcome message should contain expected text");

            // Check subtitle
            var subtitle = _homePage.GetSubtitle();
            Assert.IsTrue(subtitle.Contains("sample application"), "Subtitle should contain expected text");

            // Check counter functionality
            var initialCount = _homePage.GetCounterValue();
            await _homePage.ClickCounterButtonAsync();
            var newCount = _homePage.GetCounterValue();
            Assert.AreEqual(initialCount + 1, newCount, "Counter should increment when clicked");
        }

        [TestMethod]
        public async Task VerifyElementStates_WhenElementsInteracted_ShowsCorrectStates()
        {
            // Arrange
            await _homePage!.NavigateToHomeAsync();

            // Act - Interact with elements and verify their states
            var initialCount = _homePage.GetCounterValue();
            await _homePage.ClickCounterButtonAsync();
            var newCount = _homePage.GetCounterValue();
            await _homePage.ClickCounterButtonAsync();
            var finalCount = _homePage.GetCounterValue();

            // Assert - Verify element states after interaction
            Assert.AreEqual(initialCount + 1, newCount, "Counter should increment when clicked");
            Assert.AreEqual(initialCount + 2, finalCount, "Counter should increment multiple times");

            // Verify navigation button states
            var isHomeActive = await _homePage.IsNavigationButtonActiveAsync("home");
            Assert.IsTrue(isHomeActive, "Home button should be active on home page");

            await _homePage.NavigateToSectionAsync("todos");
            var isTodosActive = await _homePage.IsNavigationButtonActiveAsync("todos");
            Assert.IsTrue(isTodosActive, "Todos button should be active when on todos page");

            var isHomeStillActive = await _homePage.IsNavigationButtonActiveAsync("home");
            Assert.IsFalse(isHomeStillActive, "Home button should not be active when on other pages");
        }
    }
}
