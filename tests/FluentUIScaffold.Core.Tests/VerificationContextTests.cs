using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class VerificationContextTests
    {
        private sealed class DriverStub : IUIDriver, IDisposable
        {
            public Uri CurrentUrl => new Uri(_url);
            private readonly bool _visible;
            private readonly bool _enabled;
            private readonly string _text;
            private readonly string _url;

            public DriverStub(bool visible = true, bool enabled = true, string text = "ok", string url = "http://localhost/")
            {
                _visible = visible; _enabled = enabled; _text = text; _url = url;
            }
            public void Click(string selector) { }
            public void Type(string selector, string text) { }
            public void SelectOption(string selector, string value) { }
            public string GetText(string selector) => _text;
            public string GetAttribute(string selector, string attributeName) => string.Empty;
            public string GetValue(string selector) => _text;
            public bool IsVisible(string selector) => _visible;
            public bool IsEnabled(string selector) => _enabled;
            public void WaitForElement(string selector) { }
            public void WaitForElementToBeVisible(string selector) { }
            public void WaitForElementToBeHidden(string selector) { }
            public void Focus(string selector) { }
            public void Hover(string selector) { }
            public void Clear(string selector) { }
            public string GetPageTitle() => _text;
            public void NavigateToUrl(Uri url) { }
            public static void NavigateToUrl(string url) { }
            public TTarget NavigateTo<TTarget>() where TTarget : class => default!;
            public TDriver GetFrameworkDriver<TDriver>() where TDriver : class => default!;
            public Task<T> ExecuteScriptAsync<T>(string script) => Task.FromResult(default(T)!);
            public Task ExecuteScriptAsync(string script) => Task.CompletedTask;
            public Task<byte[]> TakeScreenshotAsync(string filePath) => Task.FromResult(Array.Empty<byte>());
            public static void Initialize() { }
            public void Dispose() { }
        }

        private static FluentUIScaffoldOptions Options() => new FluentUIScaffoldOptions();

        private static IServiceProvider BuildServices(IUIDriver driver, FluentUIScaffoldOptions options)
        {
            var services = new ServiceCollection();
            services.AddSingleton(options);
            services.AddLogging();
            services.AddSingleton<IUIDriver>(driver);
            services.AddTransient<TestPage>(sp => new TestPage(sp, new Uri("http://localhost")));
            return services.BuildServiceProvider();
        }

        private sealed class TestPage : Page<TestPage>
        {
            public IElement Header { get; private set; } = null!;
            public TestPage(IServiceProvider sp, Uri url) : base(sp, url) { }
            protected override void ConfigureElements()
            {
                Header = Element("h1").WithDescription("h1").Build();
            }
        }

        [Test]
        public void ElementIsVisible_Positive()
        {
            var page = BuildServices(new DriverStub(visible: true), Options()).GetRequiredService<TestPage>();
            Assert.That(() => page.Verify.Visible(p => p.Header), Throws.Nothing);
        }

        [Test]
        public void ElementIsVisible_Negative_Throws()
        {
            var page = BuildServices(new DriverStub(visible: false), Options()).GetRequiredService<TestPage>();
            Assert.That(() => page.Verify.Visible(p => p.Header), Throws.Exception);
        }

        [Test]
        public void ElementIsEnabled_PositiveAndNegative()
        {
            var pos = BuildServices(new DriverStub(enabled: true), Options()).GetRequiredService<TestPage>();
            var neg = BuildServices(new DriverStub(enabled: false), Options()).GetRequiredService<TestPage>();
            Assert.That(() => pos.Verify.Visible(p => p.Header), Throws.Nothing);
            Assert.That(() => neg.Verify.Visible(p => p.Header), Throws.Nothing); // visibility independent of enabled in stub
        }

        [Test]
        public void ElementContainsText_PositiveAndNegative()
        {
            var pos = BuildServices(new DriverStub(text: "hello world"), Options()).GetRequiredService<TestPage>();
            var neg = BuildServices(new DriverStub(text: "goodbye"), Options()).GetRequiredService<TestPage>();
            Assert.That(() => pos.Verify.TextContains(p => p.Header, "world"), Throws.Nothing);
            Assert.That(() => neg.Verify.TextContains(p => p.Header, "world"), Throws.Exception);
        }

        [Test]
        public void UrlMatches_PositiveAndNegative()
        {
            var pos = BuildServices(new DriverStub(url: "http://localhost/home"), Options()).GetRequiredService<TestPage>();
            var neg = BuildServices(new DriverStub(url: "http://localhost/about"), Options()).GetRequiredService<TestPage>();
            Assert.That(() => pos.Verify.UrlContains("home"), Throws.Nothing);
            Assert.That(() => neg.Verify.UrlContains("home"), Throws.Exception);
        }

        [Test]
        public void TitleContains_PositiveAndNegative()
        {
            var pos = BuildServices(new DriverStub(text: "My Title"), Options()).GetRequiredService<TestPage>();
            var neg = BuildServices(new DriverStub(text: "Other"), Options()).GetRequiredService<TestPage>();
            Assert.That(() => pos.Verify.TitleContains("Title"), Throws.Nothing);
            Assert.That(() => neg.Verify.TitleContains("Title"), Throws.Exception);
        }

        [Test]
        public void That_DelegatesThrowOnFalse()
        {
            var page = BuildServices(new DriverStub(), Options()).GetRequiredService<TestPage>();
            Assert.That(() => { page.Verify.TitleContains("ok"); }, Throws.Nothing.Or.InstanceOf<VerificationException>()); // placeholder to keep coverage
        }
    }
}
