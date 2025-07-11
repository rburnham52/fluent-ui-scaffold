// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Moq;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests;

[TestFixture]
public class ElementTests
{
    private Mock<IUIDriver> _mockDriver = null!;
    private FluentUIScaffoldOptions _options = null!;
    private Element _element = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDriver = new Mock<IUIDriver>();
        _options = new FluentUIScaffoldOptions();
        _element = new Element("#test-element", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(10), WaitStrategy.Visible, "Test Element",
            TimeSpan.FromMilliseconds(500), null, new Dictionary<string, string>());
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange & Act
        var element = new Element("#test", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), WaitStrategy.Clickable, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>());

        // Assert
        Assert.That(element.Selector, Is.EqualTo("#test"));
        Assert.That(element.Description, Is.EqualTo("Test"));
        Assert.That(element.Timeout, Is.EqualTo(TimeSpan.FromSeconds(5)));
        Assert.That(element.WaitStrategy, Is.EqualTo(WaitStrategy.Clickable));
        Assert.That(element.RetryInterval, Is.EqualTo(TimeSpan.FromMilliseconds(100)));
    }

    [Test]
    public void Constructor_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new Element(null!, _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), WaitStrategy.Visible, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>()));

        Assert.That(ex.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void Click_ShouldCallDriverClick()
    {
        // Arrange
        _mockDriver.Setup(d => d.WaitForElementToBeVisible("#test-element"));

        // Act
        _element.Click();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test-element"), Times.Once);
        _mockDriver.Verify(d => d.Click("#test-element"), Times.Once);
    }

    [Test]
    public void Type_ShouldCallDriverType()
    {
        // Arrange
        _mockDriver.Setup(d => d.WaitForElementToBeVisible("#test-element"));

        // Act
        _element.Type("test text");

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test-element"), Times.Once);
        _mockDriver.Verify(d => d.Type("#test-element", "test text"), Times.Once);
    }

    [Test]
    public void Select_ShouldCallDriverSelectOption()
    {
        // Arrange
        _mockDriver.Setup(d => d.WaitForElementToBeVisible("#test-element"));

        // Act
        _element.SelectOption("option1");

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test-element"), Times.Once);
        _mockDriver.Verify(d => d.SelectOption("#test-element", "option1"), Times.Once);
    }

    [Test]
    public void GetText_ShouldCallDriverGetText()
    {
        // Arrange
        _mockDriver.Setup(d => d.WaitForElementToBeVisible("#test-element"));
        _mockDriver.Setup(d => d.GetText("#test-element")).Returns("test text");

        // Act
        var result = _element.GetText();

        // Assert
        Assert.That(result, Is.EqualTo("test text"));
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test-element"), Times.Once);
        _mockDriver.Verify(d => d.GetText("#test-element"), Times.Once);
    }

    [Test]
    public void IsVisible_ShouldCallDriverIsVisible()
    {
        // Arrange
        _mockDriver.Setup(d => d.IsVisible("#test-element")).Returns(true);

        // Act
        var result = _element.IsVisible();

        // Assert
        Assert.That(result, Is.True);
        _mockDriver.Verify(d => d.IsVisible("#test-element"), Times.Once);
    }

    [Test]
    public void IsEnabled_ShouldCallDriverIsEnabled()
    {
        // Arrange
        _mockDriver.Setup(d => d.IsEnabled("#test-element")).Returns(true);

        // Act
        var result = _element.IsEnabled();

        // Assert
        Assert.That(result, Is.True);
        _mockDriver.Verify(d => d.IsEnabled("#test-element"), Times.Once);
    }

    [Test]
    public void IsDisplayed_ShouldReturnIsVisibleResult()
    {
        // Arrange
        _mockDriver.Setup(d => d.IsVisible("#test-element")).Returns(true);

        // Act
        var result = _element.IsDisplayed();

        // Assert
        Assert.That(result, Is.True);
        _mockDriver.Verify(d => d.IsVisible("#test-element"), Times.Once);
    }

    [Test]
    public void WaitFor_WithVisibleStrategy_ShouldCallWaitForVisible()
    {
        // Arrange
        var element = new Element("#test", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), WaitStrategy.Visible, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>());

        // Act
        element.WaitFor();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test"), Times.Once);
    }

    [Test]
    public void WaitFor_WithHiddenStrategy_ShouldCallWaitForHidden()
    {
        // Arrange
        var element = new Element("#test", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), WaitStrategy.Hidden, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>());

        // Act
        element.WaitFor();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeHidden("#test"), Times.Once);
    }

    [Test]
    public void WaitFor_WithClickableStrategy_ShouldCallWaitForClickable()
    {
        // Arrange
        var element = new Element("#test", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), WaitStrategy.Clickable, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>());

        // Act
        element.WaitFor();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test"), Times.Exactly(2));
    }

    [Test]
    public void WaitFor_WithNoneStrategy_ShouldNotCallAnyWaitMethods()
    {
        // Arrange
        var element = new Element("#test", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), WaitStrategy.None, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>());

        // Act
        element.WaitFor();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible(It.IsAny<string>()), Times.Never);
        _mockDriver.Verify(d => d.WaitForElementToBeHidden(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void WaitFor_WithUnsupportedStrategy_ShouldThrowArgumentException()
    {
        // Arrange
        var element = new Element("#test", _mockDriver.Object, _options,
            TimeSpan.FromSeconds(5), (WaitStrategy)999, "Test",
            TimeSpan.FromMilliseconds(100), null, new Dictionary<string, string>());

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => element.WaitFor());
        Assert.That(ex.Message, Does.Contain("Unsupported wait strategy"));
    }

    [Test]
    public void WaitForVisible_ShouldCallDriverWaitForElementToBeVisible()
    {
        // Act
        _element.WaitForVisible();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeVisible("#test-element"), Times.Once);
    }

    [Test]
    public void WaitForHidden_ShouldCallDriverWaitForElementToBeHidden()
    {
        // Act
        _element.WaitForHidden();

        // Assert
        _mockDriver.Verify(d => d.WaitForElementToBeHidden("#test-element"), Times.Once);
    }

    [Test]
    public void Exists_WhenElementExists_ShouldReturnTrue()
    {
        // Arrange
        _mockDriver.Setup(d => d.GetText("#test-element")).Returns("test");

        // Act
        var result = _element.Exists();

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void Exists_WhenElementDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _mockDriver.Setup(d => d.GetText("#test-element")).Throws(new ElementTimeoutException("Element not found"));

        // Act
        var result = _element.Exists();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsSelected_ShouldReturnFalse()
    {
        // Act
        var result = _element.IsSelected();

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void GetAttribute_ShouldReturnEmptyString()
    {
        // Act
        var result = _element.GetAttribute("class");

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetCssValue_ShouldReturnEmptyString()
    {
        // Act
        var result = _element.GetCssValue("color");

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }
}
