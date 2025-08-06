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
    /// Example tests demonstrating verification capabilities with the FluentUIScaffold framework.
    /// These tests showcase various verification patterns and assertions.
    /// </summary>
    [TestClass]
    public class VerificationTests
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
                HeadlessMode = true // Run in headless mode for CI/CD
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
        public Task Can_Verify_Element_Is_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.ElementIsVisible(".card button");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Element_Contains_Text()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.ElementContainsText(".card button", "count is");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Page_Title()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.TitleContains("Vite + Svelte + TS");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_URL_Matches_Pattern()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert
            homePage.Verify.UrlMatches("localhost");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Multiple_Conditions()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Chain multiple verifications
            homePage.Verify
                .ElementIsVisible(".card button")
                .ElementContainsText(".card button", "count is")
                .TitleContains("Vite + Svelte + TS")
                .UrlMatches("localhost");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Custom_Condition()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify custom condition
            homePage.Verify.That(() => homePage.CounterButton.IsVisible(), "Counter button should be visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Element_Text_With_Condition()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify element text with condition
            homePage.Verify.That(
                () => homePage.CounterValue.GetText(),
                text => text.Contains("count is"),
                "Counter value should contain 'count is'");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Element_State_After_Interaction()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Interact with element
            homePage.Click(e => e.CounterButton);

            // Assert - Verify state after interaction
            homePage.Verify.ElementContainsText(".card button", "count is 1");
            return Task.CompletedTask;
        }
    }
}
