// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using System.Collections.Generic; // Added missing import

namespace FluentUIScaffold.Core;

/// <summary>
/// Builder class for creating and configuring UI elements with a fluent API.
/// </summary>
public class ElementBuilder
{
    private readonly string _selector;
    private readonly IUIDriver _driver;
    private readonly FluentUIScaffoldOptions _options;
    
    private TimeSpan _timeout;
    private WaitStrategy _waitStrategy;
    private string _description = string.Empty;
    private TimeSpan _retryInterval;
    private Func<bool>? _customWaitCondition;
    private readonly Dictionary<string, string> _attributes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementBuilder"/> class.
    /// </summary>
    /// <param name="selector">The CSS selector or locator for the element.</param>
    /// <param name="driver">The UI driver instance.</param>
    /// <param name="options">The framework options.</param>
    public ElementBuilder(string selector, IUIDriver driver, FluentUIScaffoldOptions options)
    {
        _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        _driver = driver ?? throw new ArgumentNullException(nameof(driver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        
        // Initialize with default values from options
        _timeout = options.DefaultWaitTimeout;
        _waitStrategy = options.WaitStrategy;
        _retryInterval = options.RetryInterval;
    }

    /// <summary>
    /// Sets the timeout for element operations.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ElementBuilder WithTimeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the wait strategy for the element.
    /// </summary>
    /// <param name="strategy">The wait strategy to use.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ElementBuilder WithWaitStrategy(WaitStrategy strategy)
    {
        _waitStrategy = strategy;
        return this;
    }

    /// <summary>
    /// Sets a human-readable description for the element.
    /// </summary>
    /// <param name="description">The element description.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ElementBuilder WithDescription(string description)
    {
        _description = description ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Sets the retry interval for wait operations.
    /// </summary>
    /// <param name="interval">The retry interval.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ElementBuilder WithRetryInterval(TimeSpan interval)
    {
        _retryInterval = interval;
        return this;
    }

    /// <summary>
    /// Sets a custom wait condition for the element.
    /// </summary>
    /// <param name="waitCondition">The custom wait condition function.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ElementBuilder WithCustomWait(Func<bool> waitCondition)
    {
        _customWaitCondition = waitCondition ?? throw new ArgumentNullException(nameof(waitCondition));
        return this;
    }

    /// <summary>
    /// Adds an attribute expectation for the element.
    /// </summary>
    /// <param name="name">The attribute name.</param>
    /// <param name="value">The expected attribute value.</param>
    /// <returns>The current builder instance for method chaining.</returns>
    public ElementBuilder WithAttribute(string name, string value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Attribute name cannot be null or empty.", nameof(name));
            
        _attributes[name] = value ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Builds and returns the configured element.
    /// </summary>
    /// <returns>A configured <see cref="IElement"/> instance.</returns>
    public IElement Build()
    {
        // Create a concrete element implementation
        return new Element(_selector, _driver, _options, _timeout, _waitStrategy, 
            _description, _retryInterval, _customWaitCondition, _attributes);
    }
} 