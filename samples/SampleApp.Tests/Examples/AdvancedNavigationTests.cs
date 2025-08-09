using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;


namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Advanced navigation tests demonstrating complex navigation scenarios.
    /// These tests show how to handle navigation between different sections and pages.
    /// </summary>
    [TestClass]
    public class AdvancedNavigationTests
    {
        private FluentUIScaffoldApp<WebApp>? _app;
        private HomePage? _homePage;

        [TestInitialize]
        public async Task Setup()
        {
            // Arrange - Set up the application and page objects
            var options = new FluentUIScaffoldOptionsBuilder()
                .WithBaseUrl(TestConfiguration.BaseUri)
                .WithDefaultWaitTimeout(TimeSpan.FromSeconds(30))
                .WithDebugMode(false)
                .Build();

            _app = new FluentUIScaffoldApp<WebApp>(options);
            await _app.InitializeAsync();

            // Create page objects
            _homePage = new HomePage(_app.ServiceProvider);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _app?.Dispose();
        }

        [TestMethod]
        public async Task NavigateToAllSections_WhenNavigationRequested_NavigatesToAllSections()
        {
            // Arrange
            await _homePage!.NavigateToHomeAsync();

            // Act & Assert - Navigate to different sections and verify navigation
            await _homePage.NavigateToSectionAsync("todos");
            var isTodosActive = await _homePage.IsNavigationButtonActiveAsync("todos");
            Assert.IsTrue(isTodosActive, "Todos button should be active when on todos page");

            await _homePage.NavigateToSectionAsync("profile");
            var isProfileActive = await _homePage.IsNavigationButtonActiveAsync("profile");
            Assert.IsTrue(isProfileActive, "Profile button should be active when on profile page");

            await _homePage.NavigateToSectionAsync("register");
            var isRegisterActive = await _homePage.IsNavigationButtonActiveAsync("register");
            Assert.IsTrue(isRegisterActive, "Register button should be active when on register page");

            await _homePage.NavigateToSectionAsync("login");
            var isLoginActive = await _homePage.IsNavigationButtonActiveAsync("login");
            Assert.IsTrue(isLoginActive, "Login button should be active when on login page");

            await _homePage.NavigateToSectionAsync("home");
            var isHomeActive = await _homePage.IsNavigationButtonActiveAsync("home");
            Assert.IsTrue(isHomeActive, "Home button should be active when on home page");
        }

        [TestMethod]
        public async Task HandleDeepNavigation_WhenDeepPathsRequested_NavigatesToDeepPaths()
        {
            // Arrange
            await _homePage!.NavigateToHomeAsync();

            // Act - Navigate through different sections in sequence
            await _homePage.NavigateToSectionAsync("todos");
            var isTodosActive = await _homePage.IsNavigationButtonActiveAsync("todos");
            Assert.IsTrue(isTodosActive, "Todos should be active");

            await _homePage.NavigateToSectionAsync("profile");
            var isProfileActive = await _homePage.IsNavigationButtonActiveAsync("profile");
            Assert.IsTrue(isProfileActive, "Profile should be active");

            await _homePage.NavigateToSectionAsync("register");
            var isRegisterActive = await _homePage.IsNavigationButtonActiveAsync("register");
            Assert.IsTrue(isRegisterActive, "Register should be active");

            await _homePage.NavigateToSectionAsync("login");
            var isLoginActive = await _homePage.IsNavigationButtonActiveAsync("login");
            Assert.IsTrue(isLoginActive, "Login should be active");

            // Assert - Verify navigation state preservation
            var isTodosStillActive = await _homePage.IsNavigationButtonActiveAsync("todos");
            Assert.IsFalse(isTodosStillActive, "Todos should not be active when on other pages");

            var isRegisterStillActive = await _homePage.IsNavigationButtonActiveAsync("register");
            Assert.IsFalse(isRegisterStillActive, "Register should not be active when on other pages");

            // Navigate back to home
            await _homePage.NavigateToSectionAsync("home");
            var isHomeActive = await _homePage.IsNavigationButtonActiveAsync("home");
            Assert.IsTrue(isHomeActive, "Home should be active when back on home page");
        }
    }
}
