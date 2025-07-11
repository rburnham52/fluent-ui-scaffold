// Copyright (c) FluentUIScaffold. All rights reserved.
using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests;

/// <summary>
/// Unit tests for the PlaywrightPlugin class.
/// </summary>
[TestFixture]
public class PlaywrightPluginTests
{
    private PlaywrightPlugin _plugin = null!;

    [SetUp]
    public void SetUp()
    {
        _plugin = new PlaywrightPlugin();
    }

    [Test]
    public void Name_ShouldReturnPlaywright()
    {
        // Act
        var name = _plugin.Name;

        // Assert
        Assert.That(name, Is.EqualTo("Playwright"));
    }

    [Test]
    public void Version_ShouldReturnExpectedVersion()
    {
        // Act
        var version = _plugin.Version;

        // Assert
        Assert.That(version, Is.EqualTo("1.0.0"));
    }

    [Test]
    public void SupportedDriverTypes_ShouldContainPlaywrightDriver()
    {
        // Act
        var supportedTypes = _plugin.SupportedDriverTypes;

        // Assert
        Assert.That(supportedTypes, Is.Not.Null);
        Assert.That(supportedTypes.Count, Is.EqualTo(1));
        Assert.That(supportedTypes[0], Is.EqualTo(typeof(PlaywrightDriver)));
    }

    [Test]
    public void CanHandle_WithPlaywrightDriverType_ShouldReturnTrue()
    {
        // Act
        var canHandle = _plugin.CanHandle(typeof(PlaywrightDriver));

        // Assert
        Assert.That(canHandle, Is.True);
    }

    [Test]
    public void CanHandle_WithOtherDriverType_ShouldReturnFalse()
    {
        // Act
        var canHandle = _plugin.CanHandle(typeof(string));

        // Assert
        Assert.That(canHandle, Is.False);
    }

    [Test]
    public void CreateDriver_WithValidOptions_ShouldReturnPlaywrightDriver()
    {
        // Arrange
        var options = new FluentUIScaffoldOptions
        {
            BaseUrl = new Uri("https://example.com"),
            HeadlessMode = true
        };

        // Act
        var driver = _plugin.CreateDriver(options);

        // Assert
        Assert.That(driver, Is.Not.Null);
        Assert.That(driver, Is.InstanceOf<PlaywrightDriver>());
    }

    [Test]
    public void CreateDriver_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => _plugin.CreateDriver(null!));
        Assert.That(exception.ParamName, Is.EqualTo("options"));
    }

    [Test]
    public void ConfigureServices_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _plugin.ConfigureServices(services);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Check that PlaywrightPlugin is registered as singleton
        var plugin = serviceProvider.GetService<PlaywrightPlugin>();
        Assert.That(plugin, Is.Not.Null);

        // Check that PlaywrightDriver is registered as transient
        var driver = serviceProvider.GetService<PlaywrightDriver>();
        Assert.That(driver, Is.Not.Null);

        // Check that IPlaywright is registered as singleton
        var playwright = serviceProvider.GetService<Microsoft.Playwright.IPlaywright>();
        Assert.That(playwright, Is.Not.Null);
    }

    [Test]
    public void ConfigureServices_ShouldRegisterServicesWithCorrectLifetimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        _plugin.ConfigureServices(services);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        // Get the same plugin instance twice to verify singleton
        var plugin1 = serviceProvider.GetService<PlaywrightPlugin>();
        var plugin2 = serviceProvider.GetService<PlaywrightPlugin>();
        Assert.That(plugin1, Is.SameAs(plugin2));

        // Get different driver instances to verify transient
        var driver1 = serviceProvider.GetService<PlaywrightDriver>();
        var driver2 = serviceProvider.GetService<PlaywrightDriver>();
        Assert.That(driver1, Is.Not.SameAs(driver2));

        // Get the same playwright instance twice to verify singleton
        var playwright1 = serviceProvider.GetService<Microsoft.Playwright.IPlaywright>();
        var playwright2 = serviceProvider.GetService<Microsoft.Playwright.IPlaywright>();
        Assert.That(playwright1, Is.SameAs(playwright2));
    }
}
