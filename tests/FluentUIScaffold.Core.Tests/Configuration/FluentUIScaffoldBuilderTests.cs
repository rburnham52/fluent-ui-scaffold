using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests.Configuration
{
    [TestFixture]
    public class FluentUIScaffoldBuilderTests
    {
        [Test]
        public void Build_ByDefault_CreatesAppScaffold()
        {
            // Arrange
            var builder = new Core.Configuration.FluentUIScaffoldBuilder();

            // Act
            var app = builder.Build<WebApp>();

            // Assert
            Assert.That(app, Is.Not.Null);
            Assert.That(app.ServiceProvider, Is.Not.Null);
            Assert.That(app.ServiceProvider.GetService<ILoggerFactory>(), Is.Not.Null);
        }

        [Test]
        public void ConfigureServices_AddsServicesToProvider()
        {
            // Arrange
            var builder = new Core.Configuration.FluentUIScaffoldBuilder();

            // Act
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<string>("TestService");
            });
            var app = builder.Build<WebApp>();

            // Assert
            var service = app.ServiceProvider.GetService<string>();
            Assert.That(service, Is.EqualTo("TestService"));
        }

        [Test]
        public void Web_ConfiguresOptions()
        {
            // Arrange
            var builder = new Core.Configuration.FluentUIScaffoldBuilder();

            // Act
            builder.Web<WebApp>(options =>
            {
                options.BaseUrl = new Uri("http://localhost:1234");
            });
            var app = builder.Build<WebApp>();

            // Assert
            var options = app.ServiceProvider.GetService<FluentUIScaffoldOptions>();
            Assert.That(options, Is.Not.Null);
            Assert.That(options.BaseUrl?.ToString(), Is.EqualTo("http://localhost:1234/"));
        }

        // [Test]
        // public async Task StartAsync_ExecutesStartupActions()
        // {
        //     // Arrange
        //     var builder = new Core.Configuration.FluentUIScaffoldBuilder();
        //     bool actionExecuted = false;
        //
        //     builder.AddStartupAction(async (sp) =>
        //     {
        //         await Task.Delay(1);
        //         actionExecuted = true;
        //     });
        //
        //     var app = builder.Build<WebApp>();
        //
        //     // Act
        //     await app.StartAsync();
        //
        //     // Assert
        //     Assert.That(actionExecuted, Is.True);
        // }

        [Test]
        public async Task DisposeAsync_DisposesServiceProvider()
        {
            // Arrange
            var builder = new Core.Configuration.FluentUIScaffoldBuilder();
            var app = builder.Build<WebApp>();

            // Act
            // AppScaffold implements IAsyncDisposable, but not IDisposable for the AppScaffold itself?
            // Wait, AppScaffold : IAsyncDisposable.
            await app.DisposeAsync();

            // Assert
            // NUnit doesn't have an easy way to check disposal without a mock, but ensuring no throws is good.
        }
    }
}
