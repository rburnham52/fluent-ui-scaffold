using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Pages
{
    [TestFixture]
    public class PageTests
    {
        private Mock<IUIDriver> _mockDriver;
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _mockDriver = new Mock<IUIDriver>();
            var services = new ServiceCollection();
            services.AddSingleton(_mockDriver.Object);
            services.AddSingleton(new FluentUIScaffoldOptions { BaseUrl = new Uri("http://localhost") });
            services.AddLogging(builder => builder.AddConsole());

            _serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        [Test]
        public void Constructor_SetsPageUrl()
        {
            // Arrange
            var pageUrl = new Uri("http://test.local/page");

            // Act
            var page = new TestPage(_serviceProvider, pageUrl);

            // Assert
            Assert.That(page.PageUrl, Is.EqualTo(pageUrl));
        }

        [Test]
        public void Navigate_CallsDriverNavigateToUrl()
        {
            // Arrange
            var urlPattern = new Uri("http://test.local/page");
            var page = new TestPage(_serviceProvider, urlPattern);

            // Act
            var result = page.Navigate();

            // Assert
            _mockDriver.Verify(d => d.NavigateToUrl(urlPattern), Times.Once);
            Assert.That(result, Is.SameAs(page));
        }

        [Test]
        public void Click_CallsElementClick()
        {
            // Arrange
            var page = new TestPage(_serviceProvider, new Uri("http://test.local"));
            _mockDriver.Setup(d => d.IsVisible(It.IsAny<string>())).Returns(true);
            _mockDriver.Setup(d => d.IsEnabled(It.IsAny<string>())).Returns(true);

            // Act
            var result = page.Click(p => p.TestButton);

            // Assert
            _mockDriver.Verify(d => d.Click("[data-testid='test-button']"), Times.Once);
            Assert.That(result, Is.SameAs(page));
        }

        [Test]
        public void Type_CallsElementType()
        {
            // Arrange
            var page = new TestPage(_serviceProvider, new Uri("http://test.local"));
            _mockDriver.Setup(d => d.IsVisible(It.IsAny<string>())).Returns(true);
            _mockDriver.Setup(d => d.IsEnabled(It.IsAny<string>())).Returns(true);

            // Act
            var result = page.Type(p => p.TestInput, "Hello World");

            // Assert
            _mockDriver.Verify(d => d.Type("[data-testid='test-input']", "Hello World"), Times.Once);
            Assert.That(result, Is.SameAs(page));
        }

        [Test]
        public void Select_CallsElementSelectOption()
        {
            // Arrange
            var page = new TestPage(_serviceProvider, new Uri("http://test.local"));
            _mockDriver.Setup(d => d.IsVisible(It.IsAny<string>())).Returns(true);
            _mockDriver.Setup(d => d.IsEnabled(It.IsAny<string>())).Returns(true);

            // Act
            var result = page.Select(p => p.TestSelect, "Option1");

            // Assert
            _mockDriver.Verify(d => d.SelectOption("[data-testid='test-select']", "Option1"), Times.Once);
            Assert.That(result, Is.SameAs(page));
        }

        [Test]
        public void IsCurrentPage_ReturnsTrue_ByDefault()
        {
            // Arrange
            var page = new TestPage(_serviceProvider, new Uri("http://test.local"));

            // Act
            var result = page.IsCurrentPage();

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldValidateOnNavigation_ReturnsFalse_ByDefault()
        {
            // Arrange
            var page = new TestPage(_serviceProvider, new Uri("http://test.local"));

            // Act
            var result = page.ShouldValidateOnNavigation;

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test page implementation for unit testing Page&lt;TSelf&gt;.
        /// </summary>
        private class TestPage : Page<TestPage>
        {
            public IElement TestButton { get; private set; }
            public IElement TestInput { get; private set; }
            public IElement TestSelect { get; private set; }

            public TestPage(IServiceProvider serviceProvider, Uri pageUrl)
                : base(serviceProvider, pageUrl)
            {
            }

            protected override void ConfigureElements()
            {
                TestButton = Element("[data-testid='test-button']")
                    .WithDescription("Test Button")
                    .Build();

                TestInput = Element("[data-testid='test-input']")
                    .WithDescription("Test Input")
                    .Build();

                TestSelect = Element("[data-testid='test-select']")
                    .WithDescription("Test Select")
                    .Build();
            }
        }
    }
}
