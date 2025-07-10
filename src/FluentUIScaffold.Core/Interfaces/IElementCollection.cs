// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

namespace FluentUIScaffold.Core.Interfaces;

/// <summary>
/// Represents a collection of UI elements with filtering and iteration capabilities.
/// </summary>
public interface IElementCollection : IEnumerable<IElement>
{
    /// <summary>
    /// Gets the number of elements in the collection.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The element at the specified index.</returns>
    IElement this[int index] { get; }

    /// <summary>
    /// Filters the collection based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter by.</param>
    /// <returns>A filtered collection of elements.</returns>
    IElementCollection Filter(Func<IElement, bool> predicate);

    /// <summary>
    /// Filters the collection to elements containing the specified text.
    /// </summary>
    /// <param name="text">The text to filter by.</param>
    /// <returns>A filtered collection of elements.</returns>
    IElementCollection FilterByText(string text);

    /// <summary>
    /// Filters the collection to elements with the specified attribute value.
    /// </summary>
    /// <param name="attribute">The attribute name to filter by.</param>
    /// <param name="value">The attribute value to filter by.</param>
    /// <returns>A filtered collection of elements.</returns>
    IElementCollection FilterByAttribute(string attribute, string value);

    /// <summary>
    /// Gets the first element in the collection.
    /// </summary>
    /// <returns>The first element.</returns>
    IElement First();

    /// <summary>
    /// Gets the first element in the collection, or null if the collection is empty.
    /// </summary>
    /// <returns>The first element, or null if the collection is empty.</returns>
    IElement? FirstOrDefault();

    /// <summary>
    /// Gets the last element in the collection.
    /// </summary>
    /// <returns>The last element.</returns>
    IElement Last();

    /// <summary>
    /// Gets the last element in the collection, or null if the collection is empty.
    /// </summary>
    /// <returns>The last element, or null if the collection is empty.</returns>
    IElement? LastOrDefault();
}
