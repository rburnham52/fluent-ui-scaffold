// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

namespace FluentUIScaffold.Core;

/// <summary>
/// Factory class for creating and managing UI elements with caching capabilities.
/// </summary>
public class ElementFactory
{
    private readonly Dictionary<string, IElement> _elementCache = new();
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementFactory"/> class.
    /// </summary>
    /// <param name="driver">The UI driver instance.</param>
    /// <param name="options">The framework options.</param>
    public ElementFactory(IUIDriver driver, FluentUIScaffoldOptions options)
    {
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Creates a new element with the specified selector.
    /// </summary>
    /// <param name="selector">The CSS selector or locator for the element.</param>
    /// <returns>A new element instance.</returns>
    public IElement CreateElement(string selector)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        return new ElementBuilder(selector, _driver, _options).Build();
    }

    /// <summary>
    /// Creates a new element with the specified selector and configuration.
    /// </summary>
    /// <param name="selector">The CSS selector or locator for the element.</param>
    /// <param name="configure">Action to configure the element builder.</param>
    /// <returns>A new configured element instance.</returns>
    public IElement CreateElement(string selector, Action<ElementBuilder> configure)
    {
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));
        
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        var builder = new ElementBuilder(selector, _driver, _options);
        configure(builder);
        return builder.Build();
    }

    /// <summary>
    /// Gets or creates an element with the specified key and selector.
    /// </summary>
    /// <param name="key">The cache key for the element.</param>
    /// <param name="selector">The CSS selector or locator for the element.</param>
    /// <returns>The cached or newly created element instance.</returns>
    public IElement GetOrCreateElement(string key, string selector)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        
        if (string.IsNullOrEmpty(selector))
            throw new ArgumentException("Selector cannot be null or empty.", nameof(selector));

        if (_elementCache.TryGetValue(key, out var cachedElement))
        {
            return cachedElement;
        }

        var element = CreateElement(selector);
        _elementCache[key] = element;
        return element;
    }

    /// <summary>
    /// Registers an element with the specified key.
    /// </summary>
    /// <param name="key">The cache key for the element.</param>
    /// <param name="element">The element to register.</param>
    public void RegisterElement(string key, IElement element)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        
        if (element == null)
            throw new ArgumentNullException(nameof(element));

        _elementCache[key] = element;
    }

    /// <summary>
    /// Clears the element cache.
    /// </summary>
    public void ClearCache()
    {
        _elementCache.Clear();
    }

    /// <summary>
    /// Gets the number of cached elements.
    /// </summary>
    public int CacheCount => _elementCache.Count;

    /// <summary>
    /// Checks if an element with the specified key is cached.
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <returns>True if the element is cached; otherwise, false.</returns>
    public bool IsCached(string key)
    {
        return _elementCache.ContainsKey(key);
    }

    /// <summary>
    /// Removes an element from the cache.
    /// </summary>
    /// <param name="key">The cache key of the element to remove.</param>
    /// <returns>True if the element was removed; otherwise, false.</returns>
    public bool RemoveFromCache(string key)
    {
        return _elementCache.Remove(key);
    }
} 