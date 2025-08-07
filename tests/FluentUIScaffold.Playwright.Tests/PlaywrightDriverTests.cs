// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests
{
    /// <summary>
    /// Unit tests for the PlaywrightDriver class.
    /// </summary>
    [TestFixture]
    public class PlaywrightDriverTests
    {
        private FluentUIScaffoldOptions _options = null!;
        private ILogger<PlaywrightDriver> _logger = null!;

        [SetUp]
        public void SetUp()
        {
            _options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10)
            };
            _logger = NullLogger<PlaywrightDriver>.Instance;
        }

        [Test]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                EnableDebugMode = false
            };

            // Act
            var driver = new PlaywrightDriver(options);

            // Assert
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<PlaywrightDriver>());
        }

        [Test]
        public void Constructor_WithNullOptions_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PlaywrightDriver(null!));
        }

        [Test]
        public void Constructor_WithLogger_ShouldCreateDriver()
        {
            // Act
            using var driver = new PlaywrightDriver(_options, _logger);

            // Assert
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<IUIDriver>());
        }

        [Test]
        public void CurrentUrl_ReturnsExpectedUrl()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            };
            var driver = new PlaywrightDriver(options);

            // Act
            var currentUrl = driver.CurrentUrl;

            // Assert
            Assert.That(currentUrl, Is.Not.Null);
        }

        [Test]
        public void Click_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.Click(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void Click_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.Click(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void Type_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.Type(null!, "text"));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void Type_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.Type(string.Empty, "text"));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void SelectOption_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.SelectOption(null!, "value"));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void SelectOption_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.SelectOption(string.Empty, "value"));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void GetText_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.GetText(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void GetText_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.GetText(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void IsVisible_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.IsVisible(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void IsVisible_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.IsVisible(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void IsEnabled_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.IsEnabled(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void IsEnabled_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.IsEnabled(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void WaitForElement_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.WaitForElement(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void WaitForElement_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.WaitForElement(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void WaitForElementToBeVisible_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.WaitForElementToBeVisible(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void WaitForElementToBeVisible_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.WaitForElementToBeVisible(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void WaitForElementToBeHidden_WithNullSelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.WaitForElementToBeHidden(null!));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void WaitForElementToBeHidden_WithEmptySelector_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => driver.WaitForElementToBeHidden(string.Empty));
            Assert.That(exception.ParamName, Is.EqualTo("selector"));
        }

        [Test]
        public void NavigateToUrl_WithNullUrl_ShouldThrowArgumentNullException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => driver.NavigateToUrl(null!));
            Assert.That(exception.ParamName, Is.EqualTo("url"));
        }

        [Test]
        public void GetFrameworkDriver_WithIPageType_ShouldReturnPage()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act
            var page = driver.GetFrameworkDriver<Microsoft.Playwright.IPage>();

            // Assert
            Assert.That(page, Is.Not.Null);
        }

        [Test]
        public void GetFrameworkDriver_WithIBrowserType_ShouldReturnBrowser()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act
            var browser = driver.GetFrameworkDriver<Microsoft.Playwright.IBrowser>();

            // Assert
            Assert.That(browser, Is.Not.Null);
        }

        [Test]
        public void GetFrameworkDriver_WithIBrowserContextType_ShouldReturnContext()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act
            var context = driver.GetFrameworkDriver<Microsoft.Playwright.IBrowserContext>();

            // Assert
            Assert.That(context, Is.Not.Null);
        }

        [Test]
        public void GetFrameworkDriver_WithIPlaywrightType_ShouldReturnPlaywright()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act
            var playwright = driver.GetFrameworkDriver<Microsoft.Playwright.IPlaywright>();

            // Assert
            Assert.That(playwright, Is.Not.Null);
        }

        [Test]
        public void GetFrameworkDriver_WithUnsupportedType_ShouldThrowInvalidOperationException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => driver.GetFrameworkDriver<string>());
            Assert.That(exception.Message, Does.Contain("Unsupported framework driver type"));
        }

        [Test]
        public void Dispose_DisposesCorrectly()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            };
            var driver = new PlaywrightDriver(options);

            // Act & Assert
            Assert.DoesNotThrow(() => driver.Dispose());
        }

        [Test]
        public void Dispose_ShouldBeCallableMultipleTimes()
        {
            // Arrange
            var driver = new PlaywrightDriver(_options);

            // Act & Assert
            Assert.DoesNotThrow(() => driver.Dispose());
            Assert.DoesNotThrow(() => driver.Dispose());
        }
    }
}
