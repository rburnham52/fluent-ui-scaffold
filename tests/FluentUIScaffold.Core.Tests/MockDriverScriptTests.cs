// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Tests.Mocks;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Tests for MockUIDriver and StatefulMockDriver async method implementations.
    /// </summary>
    [TestFixture]
    public class MockDriverScriptTests
    {
        [Test]
        public async Task MockUIDriver_ExecuteScriptAsync_ReturnsDefault()
        {
            // Arrange
            var driver = new MockUIDriver();

            // Act
            var result = await driver.ExecuteScriptAsync<string>("return 'test'");

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task MockUIDriver_ExecuteScriptAsync_Completes()
        {
            // Arrange
            var driver = new MockUIDriver();

            // Act & Assert
            Assert.DoesNotThrowAsync(() => driver.ExecuteScriptAsync("console.log('test')"));
        }

        [Test]
        public async Task MockUIDriver_TakeScreenshotAsync_ReturnsEmptyArray()
        {
            // Arrange
            var driver = new MockUIDriver();

            // Act
            var result = await driver.TakeScreenshotAsync("screenshot.png");

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task StatefulMockDriver_ExecuteScriptAsync_WithRule_ReturnsConfiguredValue()
        {
            // Arrange
            var driver = new StatefulMockDriver();
            driver.SetScriptRule("window.location.href", _ => "https://example.com");

            // Act
            var result = await driver.ExecuteScriptAsync<string>("window.location.href");

            // Assert
            Assert.That(result, Is.EqualTo("https://example.com"));
        }

        [Test]
        public async Task StatefulMockDriver_ExecuteScriptAsync_WithoutRule_ReturnsDefault()
        {
            // Arrange
            var driver = new StatefulMockDriver();

            // Act
            var result = await driver.ExecuteScriptAsync<int>("document.querySelectorAll('h1').length");

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void StatefulMockDriver_Reset_ClearsScriptRules()
        {
            // Arrange
            var driver = new StatefulMockDriver();
            driver.SetScriptRule("test", _ => "value");

            // Act
            driver.Reset();
            var result = driver.ExecuteScriptAsync<string>("test").Result;

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}
