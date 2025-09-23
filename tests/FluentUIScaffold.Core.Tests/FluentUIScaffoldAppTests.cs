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
    public class AppScaffoldTests
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
        public async Task Builder_Creates_AppScaffold_And_Allows_Navigation()
        {
            var app = new FluentUIScaffoldBuilder()
                .UsePlugin(new FakePlugin())
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:7555");
                })
                .Build<WebApp>();

            Assert.That(app, Is.Not.Null);
            await app.StartAsync();

            var newBase = new Uri("http://localhost:7666");
            app.WithBaseUrl(newBase).NavigateToUrl(newBase);
            Assert.That(app.Framework<IUIDriver>().CurrentUrl, Is.EqualTo(newBase));

            await app.DisposeAsync();
        }

        [Test]
        public async Task Builder_WithBaseUrl_Throws_On_Null()
        {
            var app = new FluentUIScaffoldBuilder()
                .UsePlugin(new FakePlugin())
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:7555");
                })
                .Build<WebApp>();

            await app.StartAsync();
            Assert.That(() => app.WithBaseUrl(null!), Throws.Exception);
            await app.DisposeAsync();
        }

        [Test]
        public async Task GetService_Returns_Registered_Service()
        {
            var app = new FluentUIScaffoldBuilder()
                .UsePlugin(new FakePlugin())
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:7555");
                })
                .Build<WebApp>();

            await app.StartAsync();

            var driver = app.GetService<IUIDriver>();
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<FakeDriver>());

            await app.DisposeAsync();
        }

        [Test]
        public async Task Framework_Returns_Driver()
        {
            var app = new FluentUIScaffoldBuilder()
                .UsePlugin(new FakePlugin())
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:7555");
                })
                .Build<WebApp>();

            await app.StartAsync();

            var driver = app.Framework<IUIDriver>();
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<FakeDriver>());

            await app.DisposeAsync();
        }
    }
}
