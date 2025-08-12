using System;
using System.Threading.Tasks;
using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class FluentUIScaffoldAppTests
    {
        private sealed class FakeDriver : IUIDriver
        {
            public Uri? CurrentUrl { get; private set; }
            public void Click(string selector) { }
            public void Type(string selector, string text) { }
            public void SelectOption(string selector, string value) { }
            public string GetText(string selector) => string.Empty;
            public string GetAttribute(string selector, string attributeName) => string.Empty;
            public string GetValue(string selector) => string.Empty;
            public bool IsVisible(string selector) => true;
            public bool IsEnabled(string selector) => true;
            public void WaitForElement(string selector) { }
            public void WaitForElementToBeVisible(string selector) { }
            public void WaitForElementToBeHidden(string selector) { }
            public void Focus(string selector) { }
            public void Hover(string selector) { }
            public void Clear(string selector) { }
            public string GetPageTitle() => "Title";
            public void NavigateToUrl(Uri url) { CurrentUrl = url; }
            public TTarget NavigateTo<TTarget>() where TTarget : class => Activator.CreateInstance<TTarget>();
            public TDriver GetFrameworkDriver<TDriver>() where TDriver : class => Activator.CreateInstance<TDriver>();
            public void Dispose() { }
        }

        private sealed class FakePlugin : IUITestingFrameworkPlugin
        {
            public string Name => "FakePlugin";
            public string Version => "1.0.0";
            public System.Collections.Generic.IReadOnlyList<Type> SupportedDriverTypes => new[] { typeof(IUIDriver) };
            public bool CanHandle(Type driverType) => driverType == typeof(IUIDriver);
            private static FakeDriver? _driverInstance;
            public IUIDriver CreateDriver(FluentUIScaffoldOptions options)
            {
                return _driverInstance ??= new FakeDriver();
            }
            public void ConfigureServices(IServiceCollection services)
            {
                _driverInstance ??= new FakeDriver();
                services.AddSingleton<IUIDriver>(_driverInstance);
            }
        }

        [Test]
        public async Task Builder_Web_Creates_App_And_Allows_BaseUrl_And_Navigation()
        {
            PluginRegistry.Register(new FakePlugin());
            var app = FluentUIScaffoldBuilder.Web<WebApp>(opts =>
            {
                opts.WithBaseUrl(new Uri("http://localhost:7555"));
            });

            Assert.That(app, Is.Not.Null);
            await app.InitializeAsync();

            var newBase = new Uri("http://localhost:7666");
            app.WithBaseUrl(newBase).NavigateToUrl(newBase);
            Assert.That((app.Framework<IUIDriver>()).CurrentUrl, Is.EqualTo(newBase));

            app.Dispose();
        }

        [Test]
        public void Builder_Web_Throws_On_Null_BaseUrl_In_WithBaseUrl()
        {
            PluginRegistry.Register(new FakePlugin());
            var app = FluentUIScaffoldBuilder.Web<WebApp>(_ => { });
            Assert.That(() => app.WithBaseUrl(null!), Throws.Exception);
            app.Dispose();
        }
    }
}