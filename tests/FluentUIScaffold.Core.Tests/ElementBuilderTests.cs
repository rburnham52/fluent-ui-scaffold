// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Moq;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests;

[TestFixture]
public class ElementBuilderTests
{
    private Mock<IUIDriver> _mockDriver = null!;
    private FluentUIScaffoldOptions _options = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDriver = new Mock<IUIDriver>();
        _options = new FluentUIScaffoldOptions();
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        _options.DefaultWaitTimeout = TimeSpan.FromSeconds(15);

        // Act
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Assert
        var element = builder.Build();
        Assert.Multiple(() =>
        {
            Assert.That(element.Selector, Is.EqualTo("#test"));
            Assert.That(element.Timeout, Is.EqualTo(TimeSpan.FromSeconds(15)));
            Assert.That(element.WaitStrategy, Is.EqualTo(WaitStrategy.Smart)); // Default to Smart strategy
            Assert.That(element.RetryInterval, Is.EqualTo(TimeSpan.FromMilliseconds(100))); // Default retry interval
        });
    }

    [Test]
    public void Constructor_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ElementBuilder(null!, _mockDriver.Object, _options));
        Assert.That(ex.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void Constructor_WithNullDriver_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ElementBuilder("#test", null!, _options));
        Assert.That(ex.ParamName, Is.EqualTo("driver"));
    }

    [Test]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ElementBuilder("#test", _mockDriver.Object, null!));
        Assert.That(ex.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void WithTimeout_ShouldSetTimeout()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);
        var timeout = TimeSpan.FromSeconds(20);

        // Act
        var result = builder.WithTimeout(timeout);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        var element = builder.Build();
        Assert.That(element.Timeout, Is.EqualTo(timeout));
    }

    [Test]
    public void WithWaitStrategy_ShouldSetWaitStrategy()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act
        var result = builder.WithWaitStrategy(WaitStrategy.Enabled);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        var element = builder.Build();
        Assert.That(element.WaitStrategy, Is.EqualTo(WaitStrategy.Enabled));
    }

    [Test]
    public void WithDescription_ShouldSetDescription()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);
        var description = "Test Button";

        // Act
        var result = builder.WithDescription(description);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        var element = builder.Build();
        Assert.That(element.Description, Is.EqualTo(description));
    }

    [Test]
    public void WithDescription_WithNullDescription_ShouldSetEmptyString()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act
        var result = builder.WithDescription(null!);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        var element = builder.Build();
        Assert.That(element.Description, Is.EqualTo(string.Empty));
    }

    [Test]
    public void WithRetryInterval_ShouldSetRetryInterval()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);
        var interval = TimeSpan.FromMilliseconds(300);

        // Act
        var result = builder.WithRetryInterval(interval);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        var element = builder.Build();
        Assert.That(element.RetryInterval, Is.EqualTo(interval));
    }

    [Test]
    public void WithCustomWait_ShouldSetCustomWaitCondition()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);
        Func<bool> customWait = () => true;

        // Act
        var result = builder.WithCustomWait(customWait);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        // Note: Custom wait condition is not exposed in IElement interface
        // This test ensures the method doesn't throw and returns the builder
    }

    [Test]
    public void WithCustomWait_WithNullCondition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => builder.WithCustomWait(null!));
        Assert.That(ex.ParamName, Is.EqualTo("waitCondition"));
    }

    [Test]
    public void WithAttribute_ShouldAddAttribute()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act
        var result = builder.WithAttribute("data-testid", "submit-button");

        // Assert
        Assert.That(result, Is.SameAs(builder));
        // Note: Attributes are not exposed in IElement interface
        // This test ensures the method doesn't throw and returns the builder
    }

    [Test]
    public void WithAttribute_WithNullName_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => builder.WithAttribute(null!, "value"));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void WithAttribute_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => builder.WithAttribute("", "value"));
        Assert.That(ex.ParamName, Is.EqualTo("name"));
    }

    [Test]
    public void WithAttribute_WithNullValue_ShouldSetEmptyString()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options);

        // Act
        var result = builder.WithAttribute("data-testid", null!);

        // Assert
        Assert.That(result, Is.SameAs(builder));
        // Note: Attributes are not exposed in IElement interface
        // This test ensures the method doesn't throw and returns the builder
    }

    [Test]
    public void Build_ShouldReturnConfiguredElement()
    {
        // Arrange
        var builder = new ElementBuilder("#test", _mockDriver.Object, _options)
            .WithTimeout(TimeSpan.FromSeconds(25))
            .WithWaitStrategy(WaitStrategy.Disabled)
            .WithDescription("Submit Button")
            .WithRetryInterval(TimeSpan.FromMilliseconds(400));

        // Act
        var element = builder.Build();

        // Assert
        Assert.That(element, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(element.Selector, Is.EqualTo("#test"));
            Assert.That(element.Timeout, Is.EqualTo(TimeSpan.FromSeconds(25)));
            Assert.That(element.WaitStrategy, Is.EqualTo(WaitStrategy.Disabled));
            Assert.That(element.Description, Is.EqualTo("Submit Button"));
            Assert.That(element.RetryInterval, Is.EqualTo(TimeSpan.FromMilliseconds(400)));
        });
    }

    [Test]
    public void FluentChaining_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var element = new ElementBuilder("#test", _mockDriver.Object, _options)
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithWaitStrategy(WaitStrategy.Visible)
            .WithDescription("Test Element")
            .WithRetryInterval(TimeSpan.FromMilliseconds(500))
            .WithAttribute("data-testid", "test-element")
            .Build();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(element.Selector, Is.EqualTo("#test"));
            Assert.That(element.Timeout, Is.EqualTo(TimeSpan.FromSeconds(10)));
            Assert.That(element.WaitStrategy, Is.EqualTo(WaitStrategy.Visible));
            Assert.That(element.Description, Is.EqualTo("Test Element"));
            Assert.That(element.RetryInterval, Is.EqualTo(TimeSpan.FromMilliseconds(500)));
        });
    }
}
