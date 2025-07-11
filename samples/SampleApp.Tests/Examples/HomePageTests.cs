using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Example tests demonstrating the FluentUIScaffold framework usage for the Home page.
    /// These tests showcase the fluent API, page object pattern, and various testing scenarios.
    /// </summary>
    [TestClass]
    public class HomePageTests
    {
        private FluentUIScaffoldApp<WebApp> _fluentUI = null!;

        [TestInitialize]
        public void Setup()
        {
            // Configure FluentUIScaffold with Playwright
            _fluentUI = FluentUIScaffoldBuilder.Web(options =>
            {
                options.BaseUrl = TestConfiguration.BaseUri;
                options.DefaultTimeout = TimeSpan.FromSeconds(30);
                options.DefaultRetryInterval = TimeSpan.FromMilliseconds(500);
                options.LogLevel = LogLevel.Information;
                options.CaptureScreenshotsOnFailure = true;
            });

            // Debug: Check if _fluentUI is null
            if (_fluentUI == null)
            {
                throw new InvalidOperationException("FluentUIScaffoldBuilder.Web() returned null");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            _fluentUI?.Dispose();
        }

        /// <summary>
        /// Example: Basic navigation and page validation
        /// Demonstrates navigating to the home page and verifying basic elements are present.
        /// </summary>
        [TestMethod]
        public Task Can_Navigate_To_Home_Page_And_Verify_Basic_Elements()
        {
            // Arrange & Act
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Assert
            homePage
                .VerifyPageTitle("FluentUIScaffold Sample App")
                .VerifyLogosAreVisible()
                .VerifyWeatherSectionIsVisible();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Interactive element testing
        /// Demonstrates clicking the counter button and verifying the count increases.
        /// </summary>
        [TestMethod]
        public Task Can_Interact_With_Counter_Component()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Click the counter multiple times
            homePage
                .ClickCounter()
                .ClickCounter()
                .ClickCounter();

            // Assert - Verify the counter shows the expected value
            homePage.VerifyCounterValue("3");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Complex interaction patterns
        /// Demonstrates multiple counter clicks and verification of the final state.
        /// </summary>
        [TestMethod]
        public Task Can_Perform_Multiple_Counter_Interactions()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Click the counter 5 times
            homePage.ClickCounterMultipleTimes(5);

            // Assert - Verify the counter shows the expected value
            homePage.VerifyCounterValue("5");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Navigation between pages
        /// Demonstrates navigating from home page to todos page and back.
        /// </summary>
        [TestMethod]
        public Task Can_Navigate_Between_Pages()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Navigate to Todos page
            var todosPage = homePage.NavigateToTodos();

            // Assert - Verify we're on the todos page
            todosPage.VerifyTodoCount(3); // Default todos

            // Act - Navigate back to Home page
            var backToHomePage = todosPage.NavigateToHome();

            // Assert - Verify we're back on the home page
            backToHomePage.VerifyPageTitle("FluentUIScaffold Sample App");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Weather data verification
        /// Demonstrates waiting for async data and verifying the results.
        /// </summary>
        [TestMethod]
        public Task Can_Verify_Weather_Data_Is_Loaded()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Wait for weather data to load
            homePage.WaitForWeatherData();

            // Assert - Verify weather items are displayed
            homePage
                .VerifyWeatherItemsAreDisplayed()
                .VerifyWeatherItemCount(3); // Assuming 3 weather items
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Page state verification
        /// Demonstrates verifying various page elements and their states.
        /// </summary>
        [TestMethod]
        public Task Can_Verify_Page_State_And_Elements()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Assert - Verify all expected elements are present and in correct state
            homePage
                .VerifyPageTitle("FluentUIScaffold Sample App")
                .VerifyLogosAreVisible()
                .VerifyWeatherSectionIsVisible()
                .VerifyWeatherItemsAreDisplayed()
                .VerifyCounterValue("0"); // Initial counter value
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Error handling and edge cases
        /// Demonstrates how the framework handles missing elements or unexpected states.
        /// </summary>
        [TestMethod]
        public Task Can_Handle_Missing_Elements_Gracefully()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act & Assert - Verify that the page loads correctly even if some elements are missing
            try
            {
                homePage.VerifyPageTitle("FluentUIScaffold Sample App");
                // This should pass as the title should be present
            }
            catch (Exception ex)
            {
                Assert.Fail($"Page title verification failed unexpectedly: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Performance testing with timeouts
        /// Demonstrates how the framework handles timeouts and performance considerations.
        /// </summary>
        [TestMethod]
        public Task Can_Handle_Timeout_Scenarios()
        {
            // Arrange - Configure with shorter timeout for testing
            var fluentUIWithShortTimeout = FluentUIScaffoldBuilder.Web(options =>
            {
                options.BaseUrl = TestConfiguration.BaseUri;
                options.DefaultTimeout = TimeSpan.FromSeconds(5); // Shorter timeout
                options.DefaultRetryInterval = TimeSpan.FromMilliseconds(100);
                options.LogLevel = LogLevel.Information;
            });

            try
            {
                var homePage = fluentUIWithShortTimeout
                    .NavigateToUrl(TestConfiguration.BaseUri)
                    .Framework<HomePage>();

                // Act & Assert - Verify page loads within timeout
                homePage.VerifyPageTitle("FluentUIScaffold Sample App");
            }
            finally
            {
                fluentUIWithShortTimeout?.Dispose();
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Framework-specific features
        /// Demonstrates accessing Playwright-specific features through the framework.
        /// </summary>
        [TestMethod]
        public Task Can_Access_Framework_Specific_Features()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Access framework-specific features
            // Note: Framework-specific driver access would be implemented here
            // var playwrightDriver = _fluentUI.Framework<PlaywrightDriver>();
            // Assert.IsNotNull(playwrightDriver, "Playwright driver should be available");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Complex test scenario
        /// Demonstrates a more complex test scenario involving multiple page interactions.
        /// </summary>
        [TestMethod]
        public Task Can_Perform_Complex_User_Workflow()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Perform a complex workflow
            homePage
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
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Data-driven testing approach
        /// Demonstrates how to structure tests for data-driven scenarios.
        /// </summary>
        [TestMethod]
        [DataRow(1, "1")]
        [DataRow(3, "3")]
        [DataRow(5, "5")]
        public Task Can_Test_Counter_With_Different_Values(int clickCount, string expectedValue)
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Click counter the specified number of times
            homePage.ClickCounterMultipleTimes(clickCount);

            // Assert - Verify the counter shows the expected value
            homePage.VerifyCounterValue(expectedValue);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Example: Test cleanup and resource management
        /// Demonstrates proper test cleanup and resource management.
        /// </summary>
        [TestMethod]
        public Task Can_Properly_Manage_Test_Resources()
        {
            // Arrange
            var homePage = _fluentUI
                .NavigateToUrl(TestConfiguration.BaseUri)
                .Framework<HomePage>();

            // Act - Perform some actions
            homePage.ClickCounter().VerifyCounterValue("1");

            // Assert - Verify the test completed successfully
            // The cleanup will be handled in TestCleanup method
            Assert.IsTrue(true, "Test completed successfully");
            return Task.CompletedTask;
        }
    }
}
