// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests;

[TestFixture]
public class ElementCollectionTests
{
    private Mock<IUIDriver> _mockDriver = null!;
    private FluentUIScaffoldOptions _options = null!;
    private List<IElement> _elements = null!;
    private ElementCollection _collection = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDriver = new Mock<IUIDriver>();
        _options = new FluentUIScaffoldOptions();
        _elements = new List<IElement>
        {
            CreateMockElement("#element1", "Element 1"),
            CreateMockElement("#element2", "Element 2"),
            CreateMockElement("#element3", "Element 3")
        };
        _collection = new ElementCollection(_elements);
    }

    private IElement CreateMockElement(string selector, string text)
    {
        var mockElement = new Mock<IElement>();
        mockElement.Setup(e => e.Selector).Returns(selector);
        mockElement.Setup(e => e.GetText()).Returns(text);
        mockElement.Setup(e => e.GetAttribute("data-testid")).Returns("test-" + selector.Replace("#", ""));
        return mockElement.Object;
    }

    [Test]
    public void Constructor_WithValidElements_ShouldInitializeCollection()
    {
        // Assert
        Assert.That(_collection.Count, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_WithNullElements_ShouldInitializeEmptyCollection()
    {
        // Act
        var collection = new ElementCollection(null);

        // Assert
        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_WithEmptyElements_ShouldInitializeEmptyCollection()
    {
        // Act
        var collection = new ElementCollection(Enumerable.Empty<IElement>());

        // Assert
        Assert.That(collection.Count, Is.EqualTo(0));
    }

    [Test]
    public void Indexer_WithValidIndex_ShouldReturnElement()
    {
        // Act
        var element = _collection[0];

        // Assert
        Assert.That(element, Is.Not.Null);
        Assert.That(element.Selector, Is.EqualTo("#element1"));
    }

    [Test]
    public void Indexer_WithNegativeIndex_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = _collection[-1]);
        Assert.That(ex.ParamName, Is.EqualTo("index"));
    }

    [Test]
    public void Indexer_WithIndexEqualToCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = _collection[3]);
        Assert.That(ex.ParamName, Is.EqualTo("index"));
    }

    [Test]
    public void Indexer_WithIndexGreaterThanCount_ShouldThrowArgumentOutOfRangeException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _ = _collection[5]);
        Assert.That(ex.ParamName, Is.EqualTo("index"));
    }

    [Test]
    public void Filter_WithValidPredicate_ShouldReturnFilteredCollection()
    {
        // Act
        var filtered = _collection.Filter(e => e.Selector.Contains("1"));

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered.First().Selector, Is.EqualTo("#element1"));
    }

    [Test]
    public void Filter_WithNullPredicate_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentNullException>(() => _collection.Filter(null!));
        Assert.That(ex.ParamName, Is.EqualTo("predicate"));
    }

    [Test]
    public void Filter_WithNoMatchingElements_ShouldReturnEmptyCollection()
    {
        // Act
        var filtered = _collection.Filter(e => e.Selector.Contains("nonexistent"));

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(0));
    }

    [Test]
    public void FilterByText_WithMatchingText_ShouldReturnFilteredCollection()
    {
        // Act
        var filtered = _collection.FilterByText("Element 1");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered.First().GetText(), Is.EqualTo("Element 1"));
    }

    [Test]
    public void FilterByText_WithCaseInsensitiveMatch_ShouldReturnFilteredCollection()
    {
        // Act
        var filtered = _collection.FilterByText("element 1");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered.First().GetText(), Is.EqualTo("Element 1"));
    }

    [Test]
    public void FilterByText_WithEmptyText_ShouldReturnEmptyCollection()
    {
        // Act
        var filtered = _collection.FilterByText("");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(0));
    }

    [Test]
    public void FilterByText_WithNoMatchingText_ShouldReturnEmptyCollection()
    {
        // Act
        var filtered = _collection.FilterByText("Nonexistent");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(0));
    }

    [Test]
    public void FilterByAttribute_WithMatchingAttribute_ShouldReturnFilteredCollection()
    {
        // Act
        var filtered = _collection.FilterByAttribute("data-testid", "test-element1");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered.First().Selector, Is.EqualTo("#element1"));
    }

    [Test]
    public void FilterByAttribute_WithCaseInsensitiveMatch_ShouldReturnFilteredCollection()
    {
        // Act
        var filtered = _collection.FilterByAttribute("data-testid", "TEST-ELEMENT1");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(1));
        Assert.That(filtered.First().Selector, Is.EqualTo("#element1"));
    }

    [Test]
    public void FilterByAttribute_WithNullAttributeName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _collection.FilterByAttribute(null!, "value"));
        Assert.That(ex.ParamName, Is.EqualTo("attribute"));
    }

    [Test]
    public void FilterByAttribute_WithEmptyAttributeName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _collection.FilterByAttribute("", "value"));
        Assert.That(ex.ParamName, Is.EqualTo("attribute"));
    }

    [Test]
    public void FilterByAttribute_WithNoMatchingAttribute_ShouldReturnEmptyCollection()
    {
        // Act
        var filtered = _collection.FilterByAttribute("data-testid", "nonexistent");

        // Assert
        Assert.That(filtered.Count, Is.EqualTo(0));
    }

    [Test]
    public void First_WithNonEmptyCollection_ShouldReturnFirstElement()
    {
        // Act
        var first = _collection.First();

        // Assert
        Assert.That(first, Is.Not.Null);
        Assert.That(first.Selector, Is.EqualTo("#element1"));
    }

    [Test]
    public void First_WithEmptyCollection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var emptyCollection = new ElementCollection();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => emptyCollection.First());
        Assert.That(ex.Message, Does.Contain("Collection is empty"));
    }

    [Test]
    public void FirstOrDefault_WithNonEmptyCollection_ShouldReturnFirstElement()
    {
        // Act
        var first = _collection.FirstOrDefault();

        // Assert
        Assert.That(first, Is.Not.Null);
        Assert.That(first!.Selector, Is.EqualTo("#element1"));
    }

    [Test]
    public void FirstOrDefault_WithEmptyCollection_ShouldReturnNull()
    {
        // Arrange
        var emptyCollection = new ElementCollection();

        // Act
        var first = emptyCollection.FirstOrDefault();

        // Assert
        Assert.That(first, Is.Null);
    }

    [Test]
    public void Last_WithNonEmptyCollection_ShouldReturnLastElement()
    {
        // Act
        var last = _collection.Last();

        // Assert
        Assert.That(last, Is.Not.Null);
        Assert.That(last.Selector, Is.EqualTo("#element3"));
    }

    [Test]
    public void Last_WithEmptyCollection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var emptyCollection = new ElementCollection();

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => emptyCollection.Last());
        Assert.That(ex.Message, Does.Contain("Collection is empty"));
    }

    [Test]
    public void LastOrDefault_WithNonEmptyCollection_ShouldReturnLastElement()
    {
        // Act
        var last = _collection.LastOrDefault();

        // Assert
        Assert.That(last, Is.Not.Null);
        Assert.That(last!.Selector, Is.EqualTo("#element3"));
    }

    [Test]
    public void LastOrDefault_WithEmptyCollection_ShouldReturnNull()
    {
        // Arrange
        var emptyCollection = new ElementCollection();

        // Act
        var last = emptyCollection.LastOrDefault();

        // Assert
        Assert.That(last, Is.Null);
    }

    [Test]
    public void GetEnumerator_ShouldReturnAllElements()
    {
        // Act
        var elements = _collection.ToList();

        // Assert
        Assert.That(elements.Count, Is.EqualTo(3));
        Assert.That(elements[0].Selector, Is.EqualTo("#element1"));
        Assert.That(elements[1].Selector, Is.EqualTo("#element2"));
        Assert.That(elements[2].Selector, Is.EqualTo("#element3"));
    }

    [Test]
    public void IEnumerableGetEnumerator_ShouldReturnAllElements()
    {
        // Act
        var elements = new List<IElement>();
        foreach (var element in (System.Collections.IEnumerable)_collection)
        {
            elements.Add((IElement)element);
        }

        // Assert
        Assert.That(elements.Count, Is.EqualTo(3));
        Assert.That(elements[0].Selector, Is.EqualTo("#element1"));
        Assert.That(elements[1].Selector, Is.EqualTo("#element2"));
        Assert.That(elements[2].Selector, Is.EqualTo("#element3"));
    }
} 