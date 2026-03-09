using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Tests.Mocks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Configuration
{
    [TestFixture]
    public class FluentUIScaffoldBuilderTests
    {
        [Test]
        public void Build_WithPlugin_CreatesAppScaffold()
        {
            var builder = new FluentUIScaffoldBuilder()
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"));

            var app = builder.Build<WebApp>();

            Assert.That(app, Is.Not.Null);
            Assert.That(app.ServiceProvider, Is.Not.Null);
            Assert.That(app.ServiceProvider.GetService<ILoggerFactory>(), Is.Not.Null);
        }

        [Test]
        public void Build_WithoutPlugin_ThrowsInvalidOperationException()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.Build<WebApp>());
        }

        [Test]
        public void ConfigureServices_AddsServicesToProvider()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<string>("TestService");
            });
            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var service = app.ServiceProvider.GetService<string>();
            Assert.That(service, Is.EqualTo("TestService"));
        }

        [Test]
        public void Web_ConfiguresOptions()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.Web<WebApp>(options =>
            {
                options.BaseUrl = new Uri("http://localhost:1234");
            });
            var app = builder
                .UsePlugin(new MockPlugin())
                .Build<WebApp>();

            var options = app.ServiceProvider.GetService<FluentUIScaffoldOptions>();
            Assert.That(options, Is.Not.Null);
            Assert.That(options.BaseUrl?.ToString(), Is.EqualTo("http://localhost:1234/"));
        }

        [Test]
        public async Task DisposeAsync_DisposesServiceProvider()
        {
            var builder = new FluentUIScaffoldBuilder()
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"));
            var app = builder.Build<WebApp>();

            await app.DisposeAsync();
        }
    }
}
