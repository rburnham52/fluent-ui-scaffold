using System.Collections.Generic;

using FluentUIScaffold.Core;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Example tests demonstrating the [Route] attribute and parameterized navigation.
    /// These tests show how to use the route-based navigation features of FluentUIScaffold.
    /// </summary>
    /// <remarks>
    /// The [Route] attribute allows you to define URL patterns for your pages, including
    /// placeholders for dynamic parameters. This is especially useful for:
    /// - SPAs with client-side routing
    /// - RESTful URL patterns (e.g., /users/{id})
    /// - Multi-tenant applications
    ///
    /// Key concepts demonstrated:
    /// 1. Static routes: [Route("/settings")] - navigates to a fixed URL
    /// 2. Parameterized routes: [Route("/users/{userId}")] - URL with placeholders
    /// 3. Nested routes: [Route("/users/{userId}/posts/{postId}")] - multiple parameters
    ///
    /// NOTE: These tests are marked with [Ignore] because the sample app doesn't have
    /// these routes. They serve as documentation and examples of the pattern.
    /// </remarks>
    [TestClass]
    public class RouteNavigationTests
    {
        // Example of how to get the app instance - in real tests, use your configured app
        private static AppScaffold<WebApp> GetApp() => TestAssemblyHooks.CreateApp();

        /// <summary>
        /// Demonstrates navigating to a page with a static route.
        /// The [Route("/settings")] attribute defines the URL path.
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have a /settings route - this demonstrates the pattern")]
        public void Can_Navigate_To_Static_Route()
        {
            // Arrange
            var app = GetApp();

            // Act
            // NavigateTo<SettingsPage>() will navigate to: {BaseUrl}/settings
            // e.g., http://localhost:5000/settings
            var settingsPage = app.NavigateTo<SettingsPage>();

            // Assert
            settingsPage.Verify
                .UrlContains("/settings");
        }

        /// <summary>
        /// Demonstrates navigating to a page with a parameterized route.
        /// The [Route("/users/{userId}")] attribute defines the URL pattern.
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have a /users/{userId} route - this demonstrates the pattern")]
        public void Can_Navigate_With_Single_Route_Parameter()
        {
            // Arrange
            var app = GetApp();

            // Act
            // Pass route parameters as an anonymous object
            // The {userId} placeholder will be replaced with "123"
            // Result: http://localhost:5000/users/123
            var userPage = app.NavigateTo<UserPage>(new { userId = "123" });

            // Assert
            Assert.IsTrue(userPage.PageUrl.ToString().Contains("/users/123"));
        }

        /// <summary>
        /// Demonstrates navigating with multiple route parameters.
        /// The [Route("/users/{userId}/posts/{postId}")] attribute defines the URL pattern.
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have this route - this demonstrates the pattern")]
        public void Can_Navigate_With_Multiple_Route_Parameters()
        {
            // Arrange
            var app = GetApp();

            // Act
            // Pass multiple parameters - all placeholders will be replaced
            // Result: http://localhost:5000/users/456/posts/789
            var postPage = app.NavigateTo<UserPostPage>(new
            {
                userId = "456",
                postId = "789"
            });

            // Assert
            Assert.IsTrue(postPage.PageUrl.ToString().Contains("/users/456/posts/789"));
        }

        /// <summary>
        /// Demonstrates using a Dictionary for route parameters.
        /// Useful when parameters are determined at runtime.
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have this route - this demonstrates the pattern")]
        public void Can_Navigate_With_Dictionary_Parameters()
        {
            // Arrange
            var app = GetApp();
            var userId = GetUserIdFromTestData(); // Dynamic value
            var routeParams = new Dictionary<string, object>
            {
                { "userId", userId }
            };

            // Act
            var userPage = app.NavigateTo<UserPage>(routeParams);

            // Assert
            Assert.IsTrue(userPage.PageUrl.ToString().Contains($"/users/{userId}"));
        }

        /// <summary>
        /// Demonstrates using On&lt;T&gt;() to get a page reference without navigating.
        /// Useful when you're already on the page (e.g., after a button click).
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have this route - this demonstrates the pattern")]
        public void Can_Use_On_Without_Navigating()
        {
            // Arrange
            var app = GetApp();

            // First navigate to the page
            app.NavigateTo<UserPage>(new { userId = "123" });

            // Act - Get the page reference without navigating again
            // On<T>() never navigates - it just returns the page object
            var userPage = app.On<UserPage>();

            // Assert - The page URL is still the same
            Assert.IsTrue(userPage.PageUrl.ToString().Contains("/users/"));
        }

        /// <summary>
        /// Demonstrates chaining navigation with page methods.
        /// You can use Navigate(routeParams) directly on a page instance.
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have this route - this demonstrates the pattern")]
        public void Can_Navigate_From_Page_Instance()
        {
            // Arrange
            var app = GetApp();

            // Get a page reference first
            var userPage = app.On<UserPage>();

            // Act - Navigate using the page's Navigate method with parameters
            userPage.Navigate(new { userId = "different-user-789" });

            // Assert
            Assert.IsTrue(userPage.PageUrl.ToString().Contains("/users/different-user-789"));
        }

        /// <summary>
        /// Demonstrates that special characters in route parameters are URL-encoded.
        /// </summary>
        [TestMethod]
        [Ignore("Sample app doesn't have this route - this demonstrates the pattern")]
        public void Route_Parameters_Are_Url_Encoded()
        {
            // Arrange
            var app = GetApp();

            // Act
            // The @ character will be URL-encoded as %40
            var userPage = app.NavigateTo<UserPage>(new { userId = "user@example.com" });

            // Assert - The URL contains the encoded value
            Assert.IsTrue(userPage.PageUrl.ToString().Contains("user%40example.com"));
        }

        // Helper method for test data
        private static string GetUserIdFromTestData()
        {
            return "test-user-001";
        }
    }
}
