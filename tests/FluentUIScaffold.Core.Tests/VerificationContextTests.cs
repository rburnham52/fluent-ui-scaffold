using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

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
            public static void Initialize() { }
            public void Dispose() { }
        }

        private static FluentUIScaffoldOptions Options() => new FluentUIScaffoldOptionsBuilder().Build();

        [Test]
        public void ElementIsVisible_Positive()
        {
            var ctx = new VerificationContext(new DriverStub(visible: true), Options(), NullLogger.Instance);
            Assert.That(() => ctx.ElementIsVisible("#id"), Throws.Nothing);
        }

        [Test]
        public void ElementIsVisible_Negative_Throws()
        {
            var ctx = new VerificationContext(new DriverStub(visible: false), Options(), NullLogger.Instance);
            Assert.That(() => ctx.ElementIsVisible("#id"), Throws.Exception.TypeOf<VerificationException>());
        }

        [Test]
        public void ElementIsEnabled_PositiveAndNegative()
        {
            var pos = new VerificationContext(new DriverStub(enabled: true), Options(), NullLogger.Instance);
            var neg = new VerificationContext(new DriverStub(enabled: false), Options(), NullLogger.Instance);
            Assert.That(() => pos.ElementIsEnabled("#btn"), Throws.Nothing);
            Assert.That(() => neg.ElementIsEnabled("#btn"), Throws.Exception.TypeOf<VerificationException>());
        }

        [Test]
        public void ElementContainsText_PositiveAndNegative()
        {
            var pos = new VerificationContext(new DriverStub(text: "hello world"), Options(), NullLogger.Instance);
            var neg = new VerificationContext(new DriverStub(text: "goodbye"), Options(), NullLogger.Instance);
            Assert.That(() => pos.ElementContainsText("#h1", "world"), Throws.Nothing);
            Assert.That(() => neg.ElementContainsText("#h1", "world"), Throws.Exception.TypeOf<VerificationException>());
        }

        [Test]
        public void UrlMatches_PositiveAndNegative()
        {
            var pos = new VerificationContext(new DriverStub(url: "http://localhost/home"), Options(), NullLogger.Instance);
            var neg = new VerificationContext(new DriverStub(url: "http://localhost/about"), Options(), NullLogger.Instance);
            Assert.That(() => pos.UrlMatches("home"), Throws.Nothing);
            Assert.That(() => neg.UrlMatches("home"), Throws.Exception.TypeOf<VerificationException>());
        }

        [Test]
        public void TitleContains_PositiveAndNegative()
        {
            var pos = new VerificationContext(new DriverStub(text: "My Title"), Options(), NullLogger.Instance);
            var neg = new VerificationContext(new DriverStub(text: "Other"), Options(), NullLogger.Instance);
            Assert.That(() => pos.TitleContains("Title"), Throws.Nothing);
            Assert.That(() => neg.TitleContains("Title"), Throws.Exception.TypeOf<VerificationException>());
        }

        [Test]
        public void That_DelegatesThrowOnFalse()
        {
            var ctx = new VerificationContext(new DriverStub(), Options(), NullLogger.Instance);
            Assert.That(() => ctx.That(() => true, "ok"), Throws.Nothing);
            Assert.That(() => ctx.That(() => false, "bad"), Throws.Exception.TypeOf<VerificationException>());

            Assert.That(() => ctx.That(() => 10, v => v == 10, "is ten"), Throws.Nothing);
            Assert.That(() => ctx.That(() => 11, v => v == 10, "is ten"), Throws.Exception.TypeOf<VerificationException>());
        }
    }
}
