// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Playwright;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests
{
    /// <summary>
    /// Unit tests for the PlaywrightPlugin class.
    /// </summary>
    [TestFixture]
    public class PlaywrightPluginTests
    {
        [Test]
        public void Constructor_WithValidOptions_CreatesInstance()
        {
            // Arrange & Act
            var plugin = new PlaywrightPlugin();

            // Assert
            Assert.That(plugin, Is.Not.Null);
            Assert.That(plugin, Is.InstanceOf<PlaywrightPlugin>());
        }

        [Test]
        public void Name_ReturnsPlaywright()
        {
            // Arrange
            var plugin = new PlaywrightPlugin();

            // Act
            var name = plugin.Name;

            // Assert
            Assert.That(name, Is.EqualTo("Playwright"));
        }

        [Test]
        public void Version_ReturnsExpectedVersion()
        {
            // Arrange
            var plugin = new PlaywrightPlugin();

            // Act
            var version = plugin.Version;

            // Assert
            Assert.That(version, Is.EqualTo("1.0.0"));
        }

        [Test]
        public void CreateDriver_WithValidOptions_ReturnsDriver()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30)
            };
            var plugin = new PlaywrightPlugin();

            // Act
            var driver = plugin.CreateDriver(options);

            // Assert
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<PlaywrightDriver>());
        }

        [Test]
        public void CreateDriver_WithNullOptions_ThrowsException()
        {
            // Arrange
            var plugin = new PlaywrightPlugin();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => plugin.CreateDriver(null!));
        }

        [Test]
        public void CanHandle_WithPlaywrightDriverType_ReturnsTrue()
        {
            // Arrange
            var plugin = new PlaywrightPlugin();

            // Act
            var canHandle = plugin.CanHandle(typeof(PlaywrightDriver));

            // Assert
            Assert.That(canHandle, Is.True);
        }

        [Test]
        public void CanHandle_WithOtherDriverType_ReturnsFalse()
        {
            // Arrange
            var plugin = new PlaywrightPlugin();

            // Act
            var canHandle = plugin.CanHandle(typeof(string));

            // Assert
            Assert.That(canHandle, Is.False);
        }
    }
}
