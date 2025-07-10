// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentUIScaffold.Core.Interfaces;

namespace FluentUIScaffold.Core;

/// <summary>
/// Concrete implementation of the IElementCollection interface.
/// </summary>
public class ElementCollection : IElementCollection
{
    private readonly List<IElement> _elements;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> class.
    /// </summary>
    /// <param name="elements">The initial elements in the collection.</param>
    public ElementCollection(IEnumerable<IElement> elements)
    {
        _elements = elements?.ToList() ?? new List<IElement>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementCollection"/> class.
    /// </summary>
    public ElementCollection() : this(Enumerable.Empty<IElement>())
    {
    }

    /// <inheritdoc/>
    public int Count => _elements.Count;

    /// <inheritdoc/>
    public IElement this[int index]
    {
        get
        {
            if (index < 0 || index >= _elements.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            
            return _elements[index];
        }
    }

    /// <inheritdoc/>
    public IElementCollection Filter(Func<IElement, bool> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var filteredElements = _elements.Where(predicate);
        return new ElementCollection(filteredElements);
    }

    /// <inheritdoc/>
    public IElementCollection FilterByText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new ElementCollection();

        return Filter(element => 
        {
            try
            {
                var elementText = element.GetText();
                return elementText.Contains(text, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        });
    }

    /// <inheritdoc/>
    public IElementCollection FilterByAttribute(string attribute, string value)
    {
        if (string.IsNullOrEmpty(attribute))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(attribute));

        return Filter(element =>
        {
            try
            {
                var attributeValue = element.GetAttribute(attribute);
                return string.Equals(attributeValue, value, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        });
    }

    /// <inheritdoc/>
    public IElement First()
    {
        if (_elements.Count == 0)
            throw new InvalidOperationException("Collection is empty.");

        return _elements[0];
    }

    /// <inheritdoc/>
    public IElement? FirstOrDefault()
    {
        return _elements.Count > 0 ? _elements[0] : null;
    }

    /// <inheritdoc/>
    public IElement Last()
    {
        if (_elements.Count == 0)
            throw new InvalidOperationException("Collection is empty.");

        return _elements[_elements.Count - 1];
    }

    /// <inheritdoc/>
    public IElement? LastOrDefault()
    {
        return _elements.Count > 0 ? _elements[_elements.Count - 1] : null;
    }

    /// <inheritdoc/>
    public IEnumerator<IElement> GetEnumerator()
    {
        return _elements.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
} 