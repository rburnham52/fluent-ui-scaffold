using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Tests.Mocks;
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

        private sealed class TestVerifyPage : BasePageComponent<MockUIDriver, TestVerifyPage>
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


