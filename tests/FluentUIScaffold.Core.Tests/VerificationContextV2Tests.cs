using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Tests.Mocks;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class VerificationContextV2Tests
    {
        private ServiceProvider BuildServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IUIDriver, MockUIDriver>();
            services.AddSingleton<MockUIDriver, MockUIDriver>();
            services.AddSingleton(new FluentUIScaffoldOptions { BaseUrl = new Uri("http://localhost") });
            services.AddLogging();
            services.AddTransient<TestVerifyPage>(provider => new TestVerifyPage(provider, new Uri("http://localhost")));
            return services.BuildServiceProvider();
        }

        private ServiceProvider BuildServicesWithStatefulDriver(StatefulMockDriver driver)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IUIDriver>(driver);
            services.AddSingleton(driver);
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost"),
                DefaultWaitTimeout = TimeSpan.FromSeconds(2) // Short timeout for fast tests
            });
            services.AddLogging();
            services.AddTransient<TestVerifyPage>(provider => new TestVerifyPage(provider, new Uri("http://localhost")));
            return services.BuildServiceProvider();
        }

        [Test]
        public void VerifyEx_Allows_Chaining_And_And_Returns_Page()
        {
            var sp = BuildServices();
            var page = sp.GetRequiredService<TestVerifyPage>();

            var returnedPage = page.Verify
                .TitleContains("FluentUIScaffold")
                .UrlContains("about:blank")
                .And;

            Assert.That(returnedPage, Is.SameAs(page));
        }

        [Test]
        public void VerifyEx_Element_Assertions_Work_With_Func_Selectors()
        {
            var sp = BuildServices();
            var page = sp.GetRequiredService<TestVerifyPage>();

            Assert.DoesNotThrow(() =>
                page.Verify
                    .Visible(p => p.Header)
                    .TextContains(p => p.Header, "FluentUIScaffold")
                    .And
                    .Click(p => p.Header));
        }

        [Test]
        public void VerifyEx_UrlIs_And_TitleIs_Pass_With_MockDriver()
        {
            var sp = BuildServices();
            var page = sp.GetRequiredService<TestVerifyPage>();

            Assert.DoesNotThrow(() =>
                page.Verify
                    .UrlIs("about:blank")
                    .TitleIs("FluentUIScaffold Sample App")
                    .And
                    .Click(p => p.Header));
        }

        [Test]
        public void VerifyEx_NotVisible_Throws_With_MockDriver()
        {
            var sp = BuildServices();
            var page = sp.GetRequiredService<TestVerifyPage>();

            Assert.Throws<VerificationException>(() => page.Verify.NotVisible(p => p.Header));
        }

        #region Polling Behavior Tests

        [Test]
        public void TextIs_Polls_Until_Text_Matches()
        {
            var driver = new StatefulMockDriver();
            // Element becomes visible immediately, but text takes 3 calls to appear
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetTextRule("h1", callCount => callCount >= 3 ? "Expected Text" : "Loading...");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should poll until text matches on 3rd call
            Assert.DoesNotThrow(() => page.Verify.TextIs(p => p.Header, "Expected Text"));
        }

        [Test]
        public void TextIs_Throws_When_Text_Never_Matches()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetTextRule("h1", _ => "Wrong Text");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.TextIs(p => p.Header, "Expected Text"));
            Assert.That(ex!.Message, Does.Contain("text never matched"));
            Assert.That(ex.Message, Does.Contain("timeout"));
        }

        [Test]
        public void TextContains_Polls_Until_Text_Contains_Substring()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetTextRule("h1", callCount => callCount >= 2 ? "Welcome to Dashboard" : "Loading...");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should poll until text contains "Dashboard" on 2nd call
            Assert.DoesNotThrow(() => page.Verify.TextContains(p => p.Header, "Dashboard"));
        }

        [Test]
        public void TextContains_Throws_When_Substring_Never_Appears()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetTextRule("h1", _ => "Some Other Text");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.TextContains(p => p.Header, "Dashboard"));
            Assert.That(ex!.Message, Does.Contain("text never contained"));
            Assert.That(ex.Message, Does.Contain("timeout"));
        }

        [Test]
        public void HasAttribute_Waits_For_Visibility_Then_Checks_Attribute()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetAttributeRule("h1", "class", _ => "btn-primary");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            Assert.DoesNotThrow(() => page.Verify.HasAttribute(p => p.Header, "class", "btn-primary"));
        }

        [Test]
        public void HasAttribute_Throws_When_Attribute_Value_Mismatch()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetAttributeRule("h1", "class", _ => "wrong-class");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.HasAttribute(p => p.Header, "class", "expected-class"));
            Assert.That(ex!.Message, Does.Contain("attribute"));
            Assert.That(ex.Message, Does.Contain("expected-class"));
        }

        [Test]
        public void UrlIs_Polls_Until_URL_Matches()
        {
            var driver = new StatefulMockDriver();
            driver.SetUrlRule(callCount => callCount >= 3
                ? new Uri("http://localhost/dashboard")
                : new Uri("http://localhost/loading"));

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should poll until URL matches on 3rd call
            Assert.DoesNotThrow(() => page.Verify.UrlIs("http://localhost/dashboard"));
        }

        [Test]
        public void UrlIs_Throws_When_URL_Never_Matches()
        {
            var driver = new StatefulMockDriver();
            driver.SetUrlRule(_ => new Uri("http://localhost/wrong"));

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.UrlIs("http://localhost/expected"));
            Assert.That(ex!.Message, Does.Contain("URL"));
            Assert.That(ex.Message, Does.Contain("never matched"));
        }

        [Test]
        public void UrlContains_Polls_Until_URL_Contains_Segment()
        {
            var driver = new StatefulMockDriver();
            driver.SetUrlRule(callCount => callCount >= 2
                ? new Uri("http://localhost/users/123")
                : new Uri("http://localhost/loading"));

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should poll until URL contains "/users" on 2nd call
            Assert.DoesNotThrow(() => page.Verify.UrlContains("/users"));
        }

        [Test]
        public void UrlContains_Throws_When_Segment_Never_Appears()
        {
            var driver = new StatefulMockDriver();
            driver.SetUrlRule(_ => new Uri("http://localhost/home"));

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.UrlContains("/dashboard"));
            Assert.That(ex!.Message, Does.Contain("URL"));
            Assert.That(ex.Message, Does.Contain("never did"));
        }

        [Test]
        public void TitleIs_Polls_Until_Title_Matches()
        {
            var driver = new StatefulMockDriver();
            driver.SetTitleRule(callCount => callCount >= 3 ? "Dashboard" : "Loading...");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should poll until title matches on 3rd call
            Assert.DoesNotThrow(() => page.Verify.TitleIs("Dashboard"));
        }

        [Test]
        public void TitleIs_Throws_When_Title_Never_Matches()
        {
            var driver = new StatefulMockDriver();
            driver.SetTitleRule(_ => "Wrong Title");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.TitleIs("Expected Title"));
            Assert.That(ex!.Message, Does.Contain("title"));
            Assert.That(ex.Message, Does.Contain("never matched"));
        }

        [Test]
        public void TitleContains_Polls_Until_Title_Contains_Text()
        {
            var driver = new StatefulMockDriver();
            driver.SetTitleRule(callCount => callCount >= 2 ? "Welcome - Dashboard" : "Loading...");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should poll until title contains "Dashboard" on 2nd call
            Assert.DoesNotThrow(() => page.Verify.TitleContains("Dashboard"));
        }

        [Test]
        public void TitleContains_Throws_When_Text_Never_Appears()
        {
            var driver = new StatefulMockDriver();
            driver.SetTitleRule(_ => "Some Other Title");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.TitleContains("Dashboard"));
            Assert.That(ex!.Message, Does.Contain("title"));
            Assert.That(ex.Message, Does.Contain("never did"));
        }

        [Test]
        public void Visible_Waits_For_Element_To_Become_Visible()
        {
            var driver = new StatefulMockDriver();
            // Element becomes visible after 3 calls
            driver.SetVisibilityRule("h1", callCount => callCount >= 3);

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should wait for element to become visible
            Assert.DoesNotThrow(() => page.Verify.Visible(p => p.Header));
        }

        [Test]
        public void Visible_Throws_When_Element_Never_Becomes_Visible()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => false);

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.Visible(p => p.Header));
            Assert.That(ex!.Message, Does.Contain("never became visible"));
        }

        [Test]
        public void NotVisible_Waits_For_Element_To_Become_Hidden()
        {
            var driver = new StatefulMockDriver();
            // Element becomes hidden after 3 calls
            driver.SetVisibilityRule("h1", callCount => callCount < 3);

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should wait for element to become hidden
            Assert.DoesNotThrow(() => page.Verify.NotVisible(p => p.Header));
        }

        [Test]
        public void NotVisible_Throws_When_Element_Never_Becomes_Hidden()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            var ex = Assert.Throws<VerificationException>(() => page.Verify.NotVisible(p => p.Header));
            Assert.That(ex!.Message, Does.Contain("never became hidden"));
        }

        [Test]
        public void Verify_Methods_Respect_DefaultWaitTimeout()
        {
            var driver = new StatefulMockDriver();
            driver.SetTextRule("h1", _ => "Wrong Text"); // Never matches

            var services = new ServiceCollection();
            services.AddSingleton<IUIDriver>(driver);
            services.AddSingleton(driver);
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost"),
                DefaultWaitTimeout = TimeSpan.FromMilliseconds(500) // Very short timeout
            });
            services.AddLogging();
            services.AddTransient<TestVerifyPage>(provider => new TestVerifyPage(provider, new Uri("http://localhost")));
            var sp = services.BuildServiceProvider();

            var page = sp.GetRequiredService<TestVerifyPage>();
            driver.SetVisibilityRule("h1", _ => true);

            // Should timeout in ~500ms and include timeout in message
            var ex = Assert.Throws<VerificationException>(() => page.Verify.TextIs(p => p.Header, "Expected"));
            Assert.That(ex!.Message, Does.Contain("timeout"));
            Assert.That(ex.Message, Does.Contain("0.5s"));
        }

        [Test]
        public void Verify_Chain_Allows_Multiple_Assertions_Before_And()
        {
            var driver = new StatefulMockDriver();
            driver.SetVisibilityRule("h1", _ => true);
            driver.SetTextRule("h1", _ => "Dashboard Title");
            driver.SetUrlRule(_ => new Uri("http://localhost/dashboard"));
            driver.SetTitleRule(_ => "Dashboard Page");

            var sp = BuildServicesWithStatefulDriver(driver);
            var page = sp.GetRequiredService<TestVerifyPage>();

            // Should chain multiple verifications before returning to page
            var returnedPage = page.Verify
                .UrlContains("/dashboard")
                .TitleContains("Dashboard")
                .Visible(p => p.Header)
                .TextContains(p => p.Header, "Dashboard")
                .And;

            Assert.That(returnedPage, Is.SameAs(page));
        }

        #endregion

        private sealed class TestVerifyPage : Page<TestVerifyPage>
        {
            public IElement Header { get; private set; } = null!;

            public TestVerifyPage(IServiceProvider serviceProvider, Uri urlPattern) : base(serviceProvider, urlPattern)
            {
            }

            protected override void ConfigureElements()
            {
                Header = Element("h1").WithDescription("Header").Build();
            }
        }
    }
}
