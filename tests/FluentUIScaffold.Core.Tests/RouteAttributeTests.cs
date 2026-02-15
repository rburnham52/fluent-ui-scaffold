using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Tests for the [Route] attribute and page URL generation.
    /// </summary>
    [TestFixture]
    public class RouteAttributeTests
    {
        private AppScaffold<WebApp> _app = null!;
        private TrackingDriver _driver = null!;

        [SetUp]
        public async Task Setup()
        {
            _driver = new TrackingDriver();

            _app = new FluentUIScaffoldBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton(_driver);
                    services.AddSingleton<IUIDriver>(_driver);
                })
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:5000");
                })
                .WithAutoPageDiscovery()
                .Build<WebApp>();

            await _app.StartAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
            }

            _driver?.Dispose();
        }

        [Test]
        public void PageWithRoute_HasCorrectPageUrl()
        {
            // Act
            var page = _app.On<RouteTestLoginPage>();

            // Assert
            Assert.That(page.PageUrl.ToString(), Is.EqualTo("http://localhost:5000/login"));
        }

        [Test]
        public void PageWithNestedRoute_HasCorrectPageUrl()
        {
            // Act
            var page = _app.On<RouteTestProfilePage>();

            // Assert
            Assert.That(page.PageUrl.ToString(), Is.EqualTo("http://localhost:5000/users/profile"));
        }

        [Test]
        public void PageWithRootRoute_HasCorrectPageUrl()
        {
            // Act
            var page = _app.On<RouteTestHomePage>();

            // Assert
            Assert.That(page.PageUrl.ToString(), Is.EqualTo("http://localhost:5000/"));
        }

        [Test]
        public void PageWithoutRoute_HasBaseUrl()
        {
            // Act
            var page = _app.On<RouteTestNoRoutePage>();

            // Assert
            Assert.That(page.PageUrl.ToString(), Is.EqualTo("http://localhost:5000/"));
        }

        [Test]
        public void NavigateTo_PageWithRoute_NavigatesToCorrectUrl()
        {
            // Arrange
            _driver.Reset();

            // Act
            var page = _app.NavigateTo<RouteTestLoginPage>();

            // Assert
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/login"));
        }

        [Test]
        public void NavigateTo_MultiplePages_NavigatesToCorrectUrls()
        {
            // Arrange
            _driver.Reset();

            // Act - Navigate through multiple pages
            _app.NavigateTo<RouteTestHomePage>();
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/"));

            _app.NavigateTo<RouteTestLoginPage>();
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/login"));

            _app.NavigateTo<RouteTestProfilePage>();
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/users/profile"));
        }

        [Test]
        public void NavigateTo_WithRouteParams_SubstitutesPlaceholders()
        {
            // Arrange
            _driver.Reset();

            // Act - Navigate to parameterized route
            var page = _app.NavigateTo<RouteTestUserPage>(new { userId = "123" });

            // Assert
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/users/123"));
            Assert.That(page.PageUrl.ToString(), Is.EqualTo("http://localhost:5000/users/123"));
        }

        [Test]
        public void NavigateTo_WithMultipleRouteParams_SubstitutesAllPlaceholders()
        {
            // Arrange
            _driver.Reset();

            // Act - Navigate to route with multiple parameters
            var page = _app.NavigateTo<RouteTestUserPostPage>(new { userId = "456", postId = "789" });

            // Assert
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/users/456/posts/789"));
            Assert.That(page.PageUrl.ToString(), Is.EqualTo("http://localhost:5000/users/456/posts/789"));
        }

        [Test]
        public void NavigateTo_WithDictionaryRouteParams_SubstitutesPlaceholders()
        {
            // Arrange
            _driver.Reset();
            var routeParams = new Dictionary<string, object> { { "userId", "abc" } };

            // Act - Navigate using dictionary
            var page = _app.NavigateTo<RouteTestUserPage>(routeParams);

            // Assert
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/users/abc"));
        }

        [Test]
        public void NavigateTo_WithSpecialCharacters_UrlEncodesParams()
        {
            // Arrange
            _driver.Reset();

            // Act - Navigate with special characters that need encoding
            var page = _app.NavigateTo<RouteTestUserPage>(new { userId = "user@example.com" });

            // Assert - @ should be URL encoded
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/users/user%40example.com"));
        }

        [Test]
        public void Page_Navigate_WithRouteParams_Works()
        {
            // Arrange
            _driver.Reset();
            var page = _app.On<RouteTestUserPage>();

            // Act - Use page's Navigate method directly
            page.Navigate(new { userId = "direct-123" });

            // Assert
            Assert.That(_driver.LastNavigatedUrl?.ToString(), Is.EqualTo("http://localhost:5000/users/direct-123"));
        }
    }

    [Route("/")]
    public sealed class RouteTestHomePage : Page<RouteTestHomePage>
    {
        public RouteTestHomePage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
        }
    }

    [Route("/login")]
    public sealed class RouteTestLoginPage : Page<RouteTestLoginPage>
    {
        public RouteTestLoginPage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
        }
    }

    [Route("/users/profile")]
    public sealed class RouteTestProfilePage : Page<RouteTestProfilePage>
    {
        public RouteTestProfilePage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
        }
    }

    public sealed class RouteTestNoRoutePage : Page<RouteTestNoRoutePage>
    {
        public RouteTestNoRoutePage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
        }
    }

    [Route("/users/{userId}")]
    public sealed class RouteTestUserPage : Page<RouteTestUserPage>
    {
        public RouteTestUserPage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
        }
    }

    [Route("/users/{userId}/posts/{postId}")]
    public sealed class RouteTestUserPostPage : Page<RouteTestUserPostPage>
    {
        public RouteTestUserPostPage(IServiceProvider serviceProvider, Uri pageUrl)
            : base(serviceProvider, pageUrl)
        {
        }

        protected override void ConfigureElements()
        {
        }
    }

    /// <summary>
    /// A mock UI driver that tracks navigation calls for testing.
    /// </summary>
    public sealed class TrackingDriver : IUIDriver, IDisposable
    {
        public int NavigateCallCount { get; private set; }
        public Uri? LastNavigatedUrl { get; private set; }

        public Uri CurrentUrl => LastNavigatedUrl ?? new Uri("about:blank");

        public void Reset()
        {
            NavigateCallCount = 0;
            LastNavigatedUrl = null;
        }

        public void NavigateToUrl(Uri url)
        {
            NavigateCallCount++;
            LastNavigatedUrl = url;
        }

        public void Click(string selector) { }
        public void Type(string selector, string text) { }
        public void SelectOption(string selector, string value) { }
        public string GetText(string selector) => string.Empty;
        public string GetAttribute(string selector, string attributeName) => string.Empty;
        public string GetValue(string selector) => string.Empty;
        public bool IsVisible(string selector) => true;
        public bool IsEnabled(string selector) => true;
        public void WaitForElement(string selector) { }
        public void WaitForElementToBeVisible(string selector) { }
        public void WaitForElementToBeHidden(string selector) { }
        public void Focus(string selector) { }
        public void Hover(string selector) { }
        public void Clear(string selector) { }
        public string GetPageTitle() => "Test Page";
        public TTarget NavigateTo<TTarget>() where TTarget : class => default!;
        public TDriver GetFrameworkDriver<TDriver>() where TDriver : class => default!;
        public Task<T> ExecuteScriptAsync<T>(string script) => Task.FromResult(default(T)!);
        public Task ExecuteScriptAsync(string script) => Task.CompletedTask;
        public Task<byte[]> TakeScreenshotAsync(string filePath) => Task.FromResult(Array.Empty<byte>());

        public void Dispose()
        {
        }
    }
}
