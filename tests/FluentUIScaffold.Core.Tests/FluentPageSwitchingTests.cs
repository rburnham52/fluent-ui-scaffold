using System;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Pages;
using FluentUIScaffold.Core.Plugins;
using FluentUIScaffold.Core.Tests.Mocks;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class FluentPageSwitchingTests
    {
        [SetUp]
        public void Setup()
        {
            PluginRegistry.ClearForTests();
            PluginRegistry.Register(new MockPlugin());
        }

        [Test]
        public void On_AttachesWithoutNavigation_AndOptionalValidate()
        {
            // Arrange
            var app = FluentUIScaffoldBuilder.Web<WebApp>(opts =>
            {
                opts.WithBaseUrl(new Uri("http://localhost"));
            });

            // Act
            var pageNoValidate = app.On<TestFakePageA>();
            var pageWithValidate = app.On<TestFakePageA>(validate: true);

            // Assert
            Assert.That(pageNoValidate, Is.Not.Null);
            Assert.That(pageNoValidate.NavigateWasCalled, Is.False, "On<T>() must not navigate");
            Assert.That(pageNoValidate.ValidateWasCalled, Is.False, "On<T>(validate:false) must not validate");

            Assert.That(pageWithValidate, Is.Not.Null);
            Assert.That(pageWithValidate.ValidateWasCalled, Is.True, "On<T>(validate:true) should validate");
        }

        [Test]
        public void NavigateTo_InvokesNavigate_AndReturnsPage()
        {
            // Arrange
            var app = FluentUIScaffoldBuilder.Web<WebApp>(opts =>
            {
                opts.WithBaseUrl(new Uri("http://localhost"));
            });

            // Act
            var page = app.NavigateTo<TestFakePageA>();

            // Assert
            Assert.That(page, Is.Not.Null);
            Assert.That(page.NavigateWasCalled, Is.True, "NavigateTo<T>() should call page.Navigate()");
        }

        [Test]
        public void Then_ChainsToTargetPage()
        {
            // Arrange
            var app = FluentUIScaffoldBuilder.Web<WebApp>(opts =>
            {
                opts.WithBaseUrl(new Uri("http://localhost"));
            });

            var pageA = app.On<TestFakePageA>();

            // Act
            var pageB = pageA.Then<TestFakePageB>();
            var pageAAgain = pageB.Then<TestFakePageA>();

            // Assert
            Assert.That(pageB, Is.Not.Null);
            Assert.That(pageAAgain, Is.Not.Null);
        }
    }

    /// <summary>
    /// Fake page A for testing fluent page switching.
    /// </summary>
    public sealed class TestFakePageA : BasePageComponent<MockUIDriver, TestFakePageA>
    {
        public bool NavigateWasCalled { get; private set; }
        public bool ValidateWasCalled { get; private set; }

        public TestFakePageA(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // no-op for tests
        }

        public override TestFakePageA Navigate()
        {
            NavigateWasCalled = true;
            return base.Navigate();
        }

        public override void ValidateCurrentPage()
        {
            ValidateWasCalled = true;
        }
    }

    /// <summary>
    /// Fake page B for testing fluent page switching.
    /// </summary>
    public sealed class TestFakePageB : BasePageComponent<MockUIDriver, TestFakePageB>
    {
        public TestFakePageB(IServiceProvider serviceProvider, Uri urlPattern)
            : base(serviceProvider, urlPattern)
        {
        }

        protected override void ConfigureElements()
        {
            // no-op for tests
        }
    }
}


