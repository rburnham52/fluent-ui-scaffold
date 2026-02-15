// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests
{
    /// <summary>
    /// Tests for PlaywrightDriver ExecuteScriptAsync and TakeScreenshotAsync input validation.
    /// </summary>
    [TestFixture]
    public class PlaywrightDriverScriptTests
    {
        private FluentUIScaffoldOptions _options = null!;

        [SetUp]
        public void SetUp()
        {
            _options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("https://example.com"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(10)
            };
        }

        [Test]
        public void ExecuteScriptAsync_WithNullScript_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => driver.ExecuteScriptAsync(null!));
            Assert.That(exception!.ParamName, Is.EqualTo("script"));
        }

        [Test]
        public void ExecuteScriptAsync_WithEmptyScript_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => driver.ExecuteScriptAsync(string.Empty));
            Assert.That(exception!.ParamName, Is.EqualTo("script"));
        }

        [Test]
        public void ExecuteScriptAsyncGeneric_WithNullScript_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => driver.ExecuteScriptAsync<string>(null!));
            Assert.That(exception!.ParamName, Is.EqualTo("script"));
        }

        [Test]
        public void ExecuteScriptAsyncGeneric_WithEmptyScript_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => driver.ExecuteScriptAsync<string>(string.Empty));
            Assert.That(exception!.ParamName, Is.EqualTo("script"));
        }

        [Test]
        public void TakeScreenshotAsync_WithNullFilePath_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => driver.TakeScreenshotAsync(null!));
            Assert.That(exception!.ParamName, Is.EqualTo("filePath"));
        }

        [Test]
        public void TakeScreenshotAsync_WithEmptyFilePath_ShouldThrowArgumentException()
        {
            // Arrange
            using var driver = new PlaywrightDriver(_options);

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(() => driver.TakeScreenshotAsync(string.Empty));
            Assert.That(exception!.ParamName, Is.EqualTo("filePath"));
        }
    }
}
