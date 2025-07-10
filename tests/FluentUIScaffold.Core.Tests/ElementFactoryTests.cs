// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests;

[TestFixture]
public class ElementFactoryTests
{
    private Mock<IUIDriver> _mockDriver = null!;
    private FluentUIScaffoldOptions _options = null!;
    private ElementFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDriver = new Mock<IUIDriver>();
        _options = new FluentUIScaffoldOptions();
        _factory = new ElementFactory(_mockDriver.Object, _options);
    }

    [Test]
    public void Constructor_WithValidParameters_ShouldInitializeFactory()
    {
        // Assert
        Assert.That(_factory.CacheCount, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithNullDriver_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ElementFactory(null!, _options));
        Assert.That(ex.ParamName, Is.EqualTo("driver"));
    }

    [Test]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => new ElementFactory(_mockDriver.Object, null!));
        Assert.That(ex.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void CreateElement_WithValidSelector_ShouldReturnElement()
    {
        // Act
        var element = _factory.CreateElement("#test");

        // Assert
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Selector, Is.EqualTo("#test"));
    }

    [Test]
    public void CreateElement_WithNullSelector_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.CreateElement(null!));
        Assert.That(ex.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void CreateElement_WithEmptySelector_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.CreateElement(""));
        Assert.That(ex.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void CreateElement_WithConfiguration_ShouldReturnConfiguredElement()
    {
        // Act
        var element = _factory.CreateElement("#test", builder => 
        {
            builder.WithTimeout(TimeSpan.FromSeconds(20))
                   .WithWaitStrategy(WaitStrategy.Enabled)
                   .WithDescription("Test Button");
        });

        // Assert
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Selector, Is.EqualTo("#test"));
        Assert.That(element.Timeout, Is.EqualTo(TimeSpan.FromSeconds(20)));
        Assert.That(element.WaitStrategy, Is.EqualTo(WaitStrategy.Enabled));
        Assert.That(element.Description, Is.EqualTo("Test Button"));
    }

    [Test]
    public void CreateElement_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _factory.CreateElement("#test", null!));
        Assert.That(ex.ParamName, Is.EqualTo("configure"));
    }

    [Test]
    public void GetOrCreateElement_WithNewKey_ShouldCreateAndCacheElement()
    {
        // Act
        var element = _factory.GetOrCreateElement("test-key", "#test");

        // Assert
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Selector, Is.EqualTo("#test"));
        Assert.That(_factory.CacheCount, Is.EqualTo(1));
        Assert.That(_factory.IsCached("test-key"), Is.True);
    }

    [Test]
    public void GetOrCreateElement_WithExistingKey_ShouldReturnCachedElement()
    {
        // Arrange
        var firstElement = _factory.GetOrCreateElement("test-key", "#test1");
        var firstSelector = firstElement.Selector;

        // Act
        var secondElement = _factory.GetOrCreateElement("test-key", "#test2");

        // Assert
        Assert.That(secondElement, Is.SameAs(firstElement));
        Assert.That(secondElement.Selector, Is.EqualTo(firstSelector));
        Assert.That(_factory.CacheCount, Is.EqualTo(1));
    }

    [Test]
    public void GetOrCreateElement_WithNullKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.GetOrCreateElement(null!, "#test"));
        Assert.That(ex.ParamName, Is.EqualTo("key"));
    }

    [Test]
    public void GetOrCreateElement_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.GetOrCreateElement("", "#test"));
        Assert.That(ex.ParamName, Is.EqualTo("key"));
    }

    [Test]
    public void GetOrCreateElement_WithNullSelector_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.GetOrCreateElement("test-key", null!));
        Assert.That(ex.ParamName, Is.EqualTo("selector"));
    }

    [Test]
    public void RegisterElement_WithValidParameters_ShouldCacheElement()
    {
        // Arrange
        var element = _factory.CreateElement("#test");

        // Act
        _factory.RegisterElement("test-key", element);

        // Assert
        Assert.That(_factory.CacheCount, Is.EqualTo(1));
        Assert.That(_factory.IsCached("test-key"), Is.True);
    }

    [Test]
    public void RegisterElement_WithNullKey_ShouldThrowArgumentException()
    {
        // Arrange
        var element = _factory.CreateElement("#test");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.RegisterElement(null!, element));
        Assert.That(ex.ParamName, Is.EqualTo("key"));
    }

    [Test]
    public void RegisterElement_WithEmptyKey_ShouldThrowArgumentException()
    {
        // Arrange
        var element = _factory.CreateElement("#test");

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _factory.RegisterElement("", element));
        Assert.That(ex.ParamName, Is.EqualTo("key"));
    }

    [Test]
    public void RegisterElement_WithNullElement_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _factory.RegisterElement("test-key", null!));
        Assert.That(ex.ParamName, Is.EqualTo("element"));
    }

    [Test]
    public void ClearCache_ShouldRemoveAllElements()
    {
        // Arrange
        _factory.GetOrCreateElement("key1", "#test1");
        _factory.GetOrCreateElement("key2", "#test2");
        Assert.That(_factory.CacheCount, Is.EqualTo(2));

        // Act
        _factory.ClearCache();

        // Assert
        Assert.That(_factory.CacheCount, Is.EqualTo(0));
        Assert.That(_factory.IsCached("key1"), Is.False);
        Assert.That(_factory.IsCached("key2"), Is.False);
    }

    [Test]
    public void IsCached_WithExistingKey_ShouldReturnTrue()
    {
        // Arrange
        _factory.GetOrCreateElement("test-key", "#test");

        // Act
        var result = _factory.IsCached("test-key");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsCached_WithNonExistingKey_ShouldReturnFalse()
    {
        // Act
        var result = _factory.IsCached("non-existing-key");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void RemoveFromCache_WithExistingKey_ShouldRemoveElement()
    {
        // Arrange
        _factory.GetOrCreateElement("test-key", "#test");
        Assert.That(_factory.IsCached("test-key"), Is.True);

        // Act
        var result = _factory.RemoveFromCache("test-key");

        // Assert
        Assert.That(result, Is.True);
        Assert.That(_factory.IsCached("test-key"), Is.False);
        Assert.That(_factory.CacheCount, Is.EqualTo(0));
    }

    [Test]
    public void RemoveFromCache_WithNonExistingKey_ShouldReturnFalse()
    {
        // Act
        var result = _factory.RemoveFromCache("non-existing-key");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void CacheCount_ShouldReturnCorrectCount()
    {
        // Assert initial state
        Assert.That(_factory.CacheCount, Is.EqualTo(0));

        // Add elements
        _factory.GetOrCreateElement("key1", "#test1");
        Assert.That(_factory.CacheCount, Is.EqualTo(1));

        _factory.GetOrCreateElement("key2", "#test2");
        Assert.That(_factory.CacheCount, Is.EqualTo(2));

        // Remove element
        _factory.RemoveFromCache("key1");
        Assert.That(_factory.CacheCount, Is.EqualTo(1));
    }
} 