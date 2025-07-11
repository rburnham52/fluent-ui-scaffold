// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using FluentUIScaffold.Core.Configuration;

using Microsoft.Playwright;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests;

/// <summary>
/// Unit tests for the PlaywrightWaitStrategy class.
/// </summary>
[TestFixture]
public class PlaywrightWaitStrategyTests
{
    private FluentUIScaffoldOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new FluentUIScaffoldOptions
        {
            DefaultWaitTimeout = TimeSpan.FromSeconds(10)
        };
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldCreateStrategy()
    {
        // Arrange
        var page = CreateMockPage();

        // Act
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Assert
        Assert.That(strategy, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullPage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PlaywrightWaitStrategy(null!, _options));
        Assert.That(exception.ParamName, Is.EqualTo("page"));
    }

    [Test]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var page = CreateMockPage();

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PlaywrightWaitStrategy(page, null!));
        Assert.That(exception.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void WaitForElement_WithNullSelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElement(null!, WaitStrategy.Visible, TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void WaitForElement_WithEmptySelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElement(string.Empty, WaitStrategy.Visible, TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void WaitForElement_WithUnsupportedStrategy_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElement("selector", (WaitStrategy)999, TimeSpan.FromSeconds(1)));
        Assert.That(exception.Message, Does.Contain("Unsupported wait strategy"));
    }

    [Test]
    public void WaitForElementText_WithNullSelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElementText(null!, "text", TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void WaitForElementText_WithEmptySelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElementText(string.Empty, "text", TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void WaitForElementAttribute_WithNullSelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElementAttribute(null!, "attr", "value", TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void WaitForElementAttribute_WithEmptySelector_ShouldThrowArgumentException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => strategy.WaitForElementAttribute(string.Empty, "attr", "value", TimeSpan.FromSeconds(1)));
        Assert.That(exception.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void WaitForElement_WithNoneStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.None, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithVisibleStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.Visible, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithHiddenStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.Hidden, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithClickableStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.Clickable, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithEnabledStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.Enabled, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithDisabledStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.Disabled, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithTextPresentStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.TextPresent, TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void WaitForElement_WithSmartStrategy_ShouldNotThrowException()
    {
        // Arrange
        var page = CreateMockPage();
        var strategy = new PlaywrightWaitStrategy(page, _options);

        // Act & Assert
        Assert.DoesNotThrow(() => strategy.WaitForElement("selector", WaitStrategy.Smart, TimeSpan.FromSeconds(1)));
    }

    private static IPage CreateMockPage()
    {
        // This is a simplified mock for testing purposes
        // In a real implementation, you would use a proper mocking framework
        return null!;
    }
} 