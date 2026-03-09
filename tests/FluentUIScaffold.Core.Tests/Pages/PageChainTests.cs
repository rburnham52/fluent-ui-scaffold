using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Pages
{
    [TestFixture]
    public class PageChainTests
    {
        private IServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            });
            _serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();
        }

        #region Empty Chain

        [Test]
        public async Task EmptyChain_Await_IsSuccessfulNoOp()
        {
            var page = new TestPage(_serviceProvider);
            await page;
        }

        #endregion

        #region Single Action Execution

        [Test]
        public async Task SingleAction_ExecutesOnAwait()
        {
            var executed = false;
            var page = new TestPage(_serviceProvider);

            page.DoAction(() =>
            {
                executed = true;
                return Task.CompletedTask;
            });

            Assert.That(executed, Is.False, "Action should not execute before await");
            await page;
            Assert.That(executed, Is.True, "Action should execute after await");
        }

        #endregion

        #region Multiple Actions Execute In Order

        [Test]
        public async Task MultipleActions_ExecuteInOrder()
        {
            var order = new List<int>();
            var page = new TestPage(_serviceProvider);

            page.DoAction(() => { order.Add(1); return Task.CompletedTask; })
                .DoAction(() => { order.Add(2); return Task.CompletedTask; })
                .DoAction(() => { order.Add(3); return Task.CompletedTask; });

            await page;
            Assert.That(order, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        #endregion

        #region Fail-Fast

        [Test]
        public void FailFast_FirstExceptionStopsChain()
        {
            var executed = new List<int>();
            var page = new TestPage(_serviceProvider);

            page.DoAction(() => { executed.Add(1); return Task.CompletedTask; })
                .DoAction(() => throw new InvalidOperationException("boom"))
                .DoAction(() => { executed.Add(3); return Task.CompletedTask; });

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await page);
            Assert.That(ex.Message, Is.EqualTo("boom"));
            Assert.That(executed, Is.EqualTo(new[] { 1 }), "Only actions before the failure should execute");
        }

        #endregion

        #region Frozen Page

        [Test]
        public void FrozenPage_ThrowsOnEnqueue()
        {
            var page = new TestPage(_serviceProvider);
            page.DoAction(() => Task.CompletedTask);

            // NavigateTo freezes the source page
            page.NavigateTo<TargetPage>();

            Assert.Throws<FrozenPageException>(() =>
                page.DoAction(() => Task.CompletedTask));
        }

        [Test]
        public void FrozenPage_ThrowsOnNavigateTo()
        {
            var page = new TestPage(_serviceProvider);
            page.NavigateTo<TargetPage>();

            // Second NavigateTo on already-frozen page should throw
            Assert.Throws<FrozenPageException>(() =>
                page.NavigateTo<TargetPage>());
        }

        #endregion

        #region NavigateTo Transfers Queue

        [Test]
        public async Task NavigateTo_TransfersQueueAndFreezesSource()
        {
            var order = new List<string>();
            var page = new TestPage(_serviceProvider);

            page.DoAction(() => { order.Add("source-1"); return Task.CompletedTask; });

            var target = page.NavigateTo<TargetPage>();
            target.DoAction(() => { order.Add("target-1"); return Task.CompletedTask; });

            // Awaiting target executes the full shared chain
            await target;
            Assert.That(order, Does.Contain("source-1"));
            Assert.That(order, Does.Contain("target-1"));
            Assert.That(order.IndexOf("source-1"), Is.LessThan(order.IndexOf("target-1")));
        }

        [Test]
        public async Task NavigateTo_ChainingDepth_ThreePagesDeep()
        {
            var order = new List<string>();
            var pageA = new TestPage(_serviceProvider);

            pageA.DoAction(() => { order.Add("A"); return Task.CompletedTask; });

            var pageB = pageA.NavigateTo<TargetPage>();
            pageB.DoAction(() => { order.Add("B"); return Task.CompletedTask; });

            var pageC = pageB.NavigateTo<ThirdPage>();
            pageC.DoAction(() => { order.Add("C"); return Task.CompletedTask; });

            await pageC;

            Assert.That(order, Does.Contain("A"));
            Assert.That(order, Does.Contain("B"));
            Assert.That(order, Does.Contain("C"));
            // Verify order
            Assert.That(order.IndexOf("A"), Is.LessThan(order.IndexOf("B")));
            Assert.That(order.IndexOf("B"), Is.LessThan(order.IndexOf("C")));
        }

        #endregion

        #region Double-Await

        [Test]
        public async Task DoubleAwait_SecondAwaitIsNoOp()
        {
            var count = 0;
            var page = new TestPage(_serviceProvider);

            page.DoAction(() => { count++; return Task.CompletedTask; });

            await page;
            Assert.That(count, Is.EqualTo(1));

            await page; // Second await
            Assert.That(count, Is.EqualTo(1), "Second await should not re-execute actions");
        }

        #endregion

        #region Enqueue<T> DI Resolution

        [Test]
        public async Task EnqueueT_ResolvesFromServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton(new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            });
            services.AddSingleton<ITestService>(new TestService("resolved"));
            var sp = services.BuildServiceProvider();

            string capturedValue = null;
            var page = new TestPage(sp);
            page.DoWithService<ITestService>(svc =>
            {
                capturedValue = svc.Value;
                return Task.CompletedTask;
            });

            await page;
            Assert.That(capturedValue, Is.EqualTo("resolved"));

            (sp as IDisposable)?.Dispose();
        }

        [Test]
        public void EnqueueT_MissingService_ThrowsAtExecutionTime()
        {
            // No ITestService registered
            var page = new TestPage(_serviceProvider);
            page.DoWithService<ITestService>(svc => Task.CompletedTask);

            // Should not throw at enqueue time
            // Should throw when awaited
            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await page);
            Assert.That(ex.Message, Does.Contain("ITestService"));
            Assert.That(ex.Message, Does.Contain("TestPage"));
        }

        #endregion

        #region GetAwaiter on Frozen Page

        [Test]
        public async Task GetAwaiter_OnFrozenPage_StillExecutesSharedQueue()
        {
            var order = new List<string>();
            var page = new TestPage(_serviceProvider);

            page.DoAction(() => { order.Add("source"); return Task.CompletedTask; });

            var target = page.NavigateTo<TargetPage>();
            target.DoAction(() => { order.Add("target"); return Task.CompletedTask; });

            // Await the frozen source page — should still execute the shared queue
            await page;

            Assert.That(order, Does.Contain("source"));
            // NavigateTo enqueues a navigation action too, so target actions are in the shared list
            Assert.That(order, Does.Contain("target"));
        }

        #endregion

        #region Test Page Implementations

        private class TestPage : Page<TestPage>
        {
            public TestPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

            internal TestPage(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)
                : base(serviceProvider, sharedActions) { }

            public TestPage DoAction(Func<Task> action) => Enqueue(action);

            public TestPage DoWithService<T>(Func<T, Task> action) where T : notnull
                => Enqueue(action);
        }

        [Route("/target")]
        private class TargetPage : Page<TargetPage>
        {
            public TargetPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

            internal TargetPage(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)
                : base(serviceProvider, sharedActions) { }

            public TargetPage DoAction(Func<Task> action) => Enqueue(action);
        }

        [Route("/third")]
        private class ThirdPage : Page<ThirdPage>
        {
            public ThirdPage(IServiceProvider serviceProvider) : base(serviceProvider) { }

            internal ThirdPage(IServiceProvider serviceProvider, List<Func<IServiceProvider, Task>> sharedActions)
                : base(serviceProvider, sharedActions) { }

            public ThirdPage DoAction(Func<Task> action) => Enqueue(action);
        }

        private interface ITestService
        {
            string Value { get; }
        }

        private class TestService : ITestService
        {
            public string Value { get; }
            public TestService(string value) => Value = value;
        }

        #endregion
    }
}
