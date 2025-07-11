// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests;

/// <summary>
/// Unit tests for the Playwright exception classes.
/// </summary>
[TestFixture]
public class PlaywrightExceptionsTests
{
    [Test]
    public void PlaywrightException_WithMessageAndSelector_ShouldSetProperties()
    {
        // Arrange
        var message = "Test exception";
        var selector = "#test-element";

        // Act
        var exception = new PlaywrightException(message, selector);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightException_WithMessageSelectorAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var message = "Test exception";
        var selector = "#test-element";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new PlaywrightException(message, selector, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void PlaywrightException_WithNullSelector_ShouldSetSelectorToNull()
    {
        // Arrange
        var message = "Test exception";

        // Act
        var exception = new PlaywrightException(message, null);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Selector, Is.Null);
    }

    [Test]
    public void PlaywrightTimeoutException_WithMessageAndSelector_ShouldSetProperties()
    {
        // Arrange
        var message = "Timeout exception";
        var selector = "#test-element";
        var timeout = TimeSpan.FromSeconds(30);

        // Act
        var exception = new PlaywrightTimeoutException(message, selector, timeout);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.Timeout, Is.EqualTo(timeout));
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightTimeoutException_WithDefaultTimeout_ShouldSetDefaultTimeout()
    {
        // Arrange
        var message = "Timeout exception";
        var selector = "#test-element";

        // Act
        var exception = new PlaywrightTimeoutException(message, selector);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.Timeout, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void PlaywrightElementNotFoundException_WithSelector_ShouldSetMessageAndSelector()
    {
        // Arrange
        var selector = "#missing-element";

        // Act
        var exception = new PlaywrightElementNotFoundException(selector);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"Element with selector '{selector}' was not found."));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightElementNotFoundException_WithSelectorAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var selector = "#missing-element";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new PlaywrightElementNotFoundException(selector, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"Element with selector '{selector}' was not found."));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void PlaywrightElementNotVisibleException_WithSelector_ShouldSetMessageAndSelector()
    {
        // Arrange
        var selector = "#hidden-element";

        // Act
        var exception = new PlaywrightElementNotVisibleException(selector);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"Element with selector '{selector}' is not visible."));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightElementNotVisibleException_WithSelectorAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var selector = "#hidden-element";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new PlaywrightElementNotVisibleException(selector, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"Element with selector '{selector}' is not visible."));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void PlaywrightElementNotEnabledException_WithSelector_ShouldSetMessageAndSelector()
    {
        // Arrange
        var selector = "#disabled-element";

        // Act
        var exception = new PlaywrightElementNotEnabledException(selector);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"Element with selector '{selector}' is not enabled."));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightElementNotEnabledException_WithSelectorAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var selector = "#disabled-element";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new PlaywrightElementNotEnabledException(selector, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"Element with selector '{selector}' is not enabled."));
        Assert.That(exception.Selector, Is.EqualTo(selector));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void PlaywrightNavigationException_WithMessageAndUrl_ShouldSetProperties()
    {
        // Arrange
        var message = "Navigation failed";
        var url = "https://example.com";

        // Act
        var exception = new PlaywrightNavigationException(message, url);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Url, Is.EqualTo(url));
        Assert.That(exception.Selector, Is.Null);
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightNavigationException_WithMessageUrlAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var message = "Navigation failed";
        var url = "https://example.com";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new PlaywrightNavigationException(message, url, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Url, Is.EqualTo(url));
        Assert.That(exception.Selector, Is.Null);
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void PlaywrightNavigationException_WithNullUrl_ShouldSetUrlToNull()
    {
        // Arrange
        var message = "Navigation failed";

        // Act
        var exception = new PlaywrightNavigationException(message, null);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.Url, Is.Null);
    }

    [Test]
    public void PlaywrightBrowserStartupException_WithMessageAndBrowserType_ShouldSetProperties()
    {
        // Arrange
        var message = "Browser startup failed";
        var browserType = "chromium";

        // Act
        var exception = new PlaywrightBrowserStartupException(message, browserType);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.BrowserType, Is.EqualTo(browserType));
        Assert.That(exception.Selector, Is.Null);
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void PlaywrightBrowserStartupException_WithMessageBrowserTypeAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var message = "Browser startup failed";
        var browserType = "firefox";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new PlaywrightBrowserStartupException(message, browserType, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.BrowserType, Is.EqualTo(browserType));
        Assert.That(exception.Selector, Is.Null);
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
    }

    [Test]
    public void PlaywrightBrowserStartupException_WithNullBrowserType_ShouldSetBrowserTypeToNull()
    {
        // Arrange
        var message = "Browser startup failed";

        // Act
        var exception = new PlaywrightBrowserStartupException(message, null);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.BrowserType, Is.Null);
    }

    [Test]
    public void PlaywrightException_ShouldInheritFromFluentUIScaffoldException()
    {
        // Act
        var exception = new PlaywrightException("Test");

        // Assert
        Assert.That(exception, Is.InstanceOf<FluentUIScaffold.Core.Exceptions.FluentUIScaffoldException>());
    }

    [Test]
    public void PlaywrightTimeoutException_ShouldInheritFromPlaywrightException()
    {
        // Act
        var exception = new PlaywrightTimeoutException("Test");

        // Assert
        Assert.That(exception, Is.InstanceOf<PlaywrightException>());
    }

    [Test]
    public void PlaywrightElementNotFoundException_ShouldInheritFromPlaywrightException()
    {
        // Act
        var exception = new PlaywrightElementNotFoundException("selector");

        // Assert
        Assert.That(exception, Is.InstanceOf<PlaywrightException>());
    }

    [Test]
    public void PlaywrightElementNotVisibleException_ShouldInheritFromPlaywrightException()
    {
        // Act
        var exception = new PlaywrightElementNotVisibleException("selector");

        // Assert
        Assert.That(exception, Is.InstanceOf<PlaywrightException>());
    }

    [Test]
    public void PlaywrightElementNotEnabledException_ShouldInheritFromPlaywrightException()
    {
        // Act
        var exception = new PlaywrightElementNotEnabledException("selector");

        // Assert
        Assert.That(exception, Is.InstanceOf<PlaywrightException>());
    }

    [Test]
    public void PlaywrightNavigationException_ShouldInheritFromPlaywrightException()
    {
        // Act
        var exception = new PlaywrightNavigationException("Test");

        // Assert
        Assert.That(exception, Is.InstanceOf<PlaywrightException>());
    }

    [Test]
    public void PlaywrightBrowserStartupException_ShouldInheritFromPlaywrightException()
    {
        // Act
        var exception = new PlaywrightBrowserStartupException("Test");

        // Assert
        Assert.That(exception, Is.InstanceOf<PlaywrightException>());
    }
}
