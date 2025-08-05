using System;
using System.IO;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Example tests demonstrating form interactions with the FluentUIScaffold framework.
    /// These tests showcase the fluent API for form interactions using the existing sample app.
    /// </summary>
    [TestClass]
    public class FormInteractionTests
    {
        private FluentUIScaffoldApp<WebApp>? _fluentUI;

        [TestInitialize]
        public async Task TestInitialize()
        {
            // Configure FluentUIScaffold with auto-discovery and web server launch
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = TestConfiguration.BaseUri,
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                HeadlessMode = true, // Run in headless mode for CI/CD
                EnableWebServerLaunch = true,
                WebServerProjectPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "SampleApp")
            };

            _fluentUI = new FluentUIScaffoldApp<WebApp>(options);
            await _fluentUI.InitializeAsync();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _fluentUI?.Dispose();
        }

        [TestMethod]
        public Task Can_Use_Fluent_API_With_Existing_Elements()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Use fluent API with existing elements
            homePage
                .Click(e => e.CounterButton)
                .VerifyText(e => e.CounterValue, "count is 1");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Chain_Element_Actions_With_Wait_Operations()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Chain multiple actions with wait operations
            homePage
                .WaitForElement(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .WaitForElementToBeVisible(e => e.CounterValue)
                .VerifyText(e => e.CounterValue, "count is 1");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Element_State_Checking()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Check element states
            homePage
                .WaitForElement(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .WaitForElementToBeVisible(e => e.CounterValue);

            // Verify element is visible
            homePage.Verify.ElementIsVisible(".card button");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Focus_And_Hover_Actions()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Use focus and hover actions
            homePage
                .Focus(e => e.CounterButton)
                .Hover(e => e.CounterButton)
                .Click(e => e.CounterButton);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Wait_For_Element_To_Be_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Wait for elements to be visible
            homePage
                .WaitForElementToBeVisible(e => e.CounterButton)
                .WaitForElementToBeVisible(e => e.CounterValue)
                .WaitForElementToBeVisible(e => e.PageTitle);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Element_Text_Retrieval()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Get element text
            var counterText = homePage.CounterValue.GetText();
            var pageTitle = homePage.PageTitle.GetText();

            // Assert - Verify text is retrieved
            Assert.IsNotNull(counterText);
            Assert.IsNotNull(pageTitle);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Element_Interaction_With_Verification()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Combine interaction with verification
            homePage
                .WaitForElement(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .VerifyText(e => e.CounterValue, "count is 1")
                .Click(e => e.CounterButton)
                .VerifyText(e => e.CounterValue, "count is 2");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Multiple_Element_Interactions()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Multiple element interactions
            homePage
                .WaitForElement(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .VerifyText(e => e.CounterValue, "count is 3");
            return Task.CompletedTask;
        }
    }
}
