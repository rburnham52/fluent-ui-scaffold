// Copyright (c) FluentUIScaffold. All rights reserved.
using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

using NUnit.Framework;

namespace FluentUIScaffold.Playwright.Tests
{
    /// <summary>
    /// Tests to verify that PlaywrightDriver is registered as a singleton
    /// and browser context is reused across multiple resolutions.
    /// </summary>
    [TestFixture]
    public class PlaywrightDriverSingletonTests
    {
        [Test]
        public void IUIDriver_ResolvedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                HeadlessMode = true
            });
            services.AddLogging(builder => builder.AddConsole());

            var plugin = new PlaywrightPlugin();
            plugin.ConfigureServices(services);

            using var provider = services.BuildServiceProvider();

            // Act
            var driver1 = provider.GetRequiredService<IUIDriver>();
            var driver2 = provider.GetRequiredService<IUIDriver>();

            // Assert - Both resolutions should return the same instance
            Assert.That(driver2, Is.SameAs(driver1),
                "IUIDriver should be a singleton - multiple resolutions should return the same instance");
        }

        [Test]
        public void PlaywrightDriver_ResolvedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                HeadlessMode = true
            });
            services.AddLogging(builder => builder.AddConsole());

            var plugin = new PlaywrightPlugin();
            plugin.ConfigureServices(services);

            using var provider = services.BuildServiceProvider();

            // Act
            var driver1 = provider.GetRequiredService<PlaywrightDriver>();
            var driver2 = provider.GetRequiredService<PlaywrightDriver>();

            // Assert - Both resolutions should return the same instance
            Assert.That(driver2, Is.SameAs(driver1),
                "PlaywrightDriver should be a singleton - multiple resolutions should return the same instance");
        }

        [Test]
        public void IPage_ResolvedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                HeadlessMode = true
            });
            services.AddLogging(builder => builder.AddConsole());

            var plugin = new PlaywrightPlugin();
            plugin.ConfigureServices(services);

            using var provider = services.BuildServiceProvider();

            // Act
            var page1 = provider.GetRequiredService<IPage>();
            var page2 = provider.GetRequiredService<IPage>();

            // Assert - Both resolutions should return the same IPage instance
            Assert.That(page2, Is.SameAs(page1),
                "IPage should return the same instance from the singleton driver");
        }

        [Test]
        public async System.Threading.Tasks.Task IBrowserContext_ResolvedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                HeadlessMode = true
            });
            services.AddLogging(builder => builder.AddConsole());

            var plugin = new PlaywrightPlugin();
            plugin.ConfigureServices(services);

            var provider = services.BuildServiceProvider();

            try
            {
                // Act
                var context1 = provider.GetRequiredService<IBrowserContext>();
                var context2 = provider.GetRequiredService<IBrowserContext>();

                // Assert - Both resolutions should return the same context
                Assert.That(context2, Is.SameAs(context1),
                    "IBrowserContext should return the same instance from the singleton driver");
            }
            finally
            {
                // Playwright types only support async dispose
                await provider.DisposeAsync();
            }
        }

        [Test]
        public async System.Threading.Tasks.Task IBrowser_ResolvedMultipleTimes_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(30),
                HeadlessMode = true
            });
            services.AddLogging(builder => builder.AddConsole());

            var plugin = new PlaywrightPlugin();
            plugin.ConfigureServices(services);

            var provider = services.BuildServiceProvider();

            try
            {
                // Act
                var browser1 = provider.GetRequiredService<IBrowser>();
                var browser2 = provider.GetRequiredService<IBrowser>();

                // Assert - Both resolutions should return the same browser
                Assert.That(browser2, Is.SameAs(browser1),
                    "IBrowser should return the same instance from the singleton driver");
            }
            finally
            {
                // Playwright types only support async dispose
                await provider.DisposeAsync();
            }
        }
    }
}
