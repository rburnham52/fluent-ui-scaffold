using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IUIDriver for unit testing purposes only.
    /// This driver should NOT be used in production or integration tests.
    /// For real testing, use a proper framework plugin (PlaywrightPlugin, SeleniumPlugin, etc.).
    /// </summary>
    public sealed class MockUIDriver : IUIDriver, IDisposable
    {
        public Uri CurrentUrl => new Uri("about:blank");
        public void Click(string selector) { }
        public void Type(string selector, string text) { }
        public void SelectOption(string selector, string value) { }
        public string GetText(string selector)
        {
            // Provide mock text for testing purposes
            if (selector == "h1" || selector.Contains("h1"))
                return "FluentUIScaffold Sample App";
            if (selector.Contains("button"))
                return "0";
            if (selector.Contains("counter") || selector.Contains("count"))
                return "0";
            if (selector.Contains("weather"))
                return "Weather Data";
            if (selector.Contains("logo"))
                return "Logo";
            return string.Empty;
        }
        public bool IsVisible(string selector) => true;
        public bool IsEnabled(string selector) => true;
        public void WaitForElement(string selector) { }
        public void WaitForElementToBeVisible(string selector) { }
        public void WaitForElementToBeHidden(string selector) { }
        public void Focus(string selector) { }
        public void Hover(string selector) { }
        public void Clear(string selector) { }
        public string GetPageTitle() => "FluentUIScaffold Sample App";
        public void NavigateToUrl(Uri url) { }
        public void NavigateToUrl(string url) { }
        public TTarget NavigateTo<TTarget>() where TTarget : class => default!;
        public TDriver GetFrameworkDriver<TDriver>() where TDriver : class => default!;
        public static void Initialize() { }
        public void Dispose()
        {
            // No resources to dispose
        }
    }

    /// <summary>
    /// Mock plugin for unit testing purposes only.
    /// This plugin should NOT be used in production or integration tests.
    /// For real testing, use a proper framework plugin (PlaywrightPlugin, SeleniumPlugin, etc.).
    /// </summary>
    public sealed class MockPlugin : IUITestingFrameworkPlugin
    {
        public string Name => "MockPlugin";
        public string Version => "1.0.0";
        public IReadOnlyList<Type> SupportedDriverTypes => new List<Type> { typeof(MockUIDriver) };

        public bool CanHandle(Type driverType) => driverType == typeof(MockUIDriver);

        public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
        {
            return new MockUIDriver();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IUIDriver, MockUIDriver>();
            services.AddTransient<MockUIDriver, MockUIDriver>();
        }
    }
}
