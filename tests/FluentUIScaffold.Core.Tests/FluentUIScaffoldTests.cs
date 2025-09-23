using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Tests.Mocks;

using Microsoft.Extensions.Logging;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Unit tests for FluentUIScaffold configuration options.
    /// </summary>
    [TestFixture]
    public class FluentUIScaffoldTests
    {
        [Test]
        public void Options_WithValidBaseUrl_SetsBaseUrl()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = new Uri("http://localhost:5000")
            };

            // Act & Assert
            Assert.That(options.BaseUrl, Is.EqualTo(new Uri("http://localhost:5000")));
        }

        [Test]
        public void Options_WithValidTimeout_SetsDefaultWaitTimeout()
        {
            // Arrange
            var options = new FluentUIScaffoldOptions
            {
                DefaultWaitTimeout = TimeSpan.FromSeconds(60)
            };

            // Act & Assert
            Assert.That(options.DefaultWaitTimeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
        }

        [Test]
        public void Builder_WithHeadlessMode_SetsHeadlessMode()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            bool? expectedHeadless = false;

            // Act
            var result = builder.WithHeadlessMode(expectedHeadless);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.HeadlessMode, Is.EqualTo(expectedHeadless));
            });
        }

        [Test]
        public void Builder_WithHeadlessMode_Null_SetsHeadlessModeToNull()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithHeadlessMode(null);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.HeadlessMode, Is.Null);
            });
        }

        [Test]
        public void Builder_WithSlowMo_SetsSlowMo()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();
            int? expectedSlowMo = 500;

            // Act
            var result = builder.WithSlowMo(expectedSlowMo);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.SlowMo, Is.EqualTo(expectedSlowMo));
            });
        }

        [Test]
        public void Builder_WithSlowMo_Null_SetsSlowMoToNull()
        {
            // Arrange
            var builder = new FluentUIScaffoldOptionsBuilder();

            // Act
            var result = builder.WithSlowMo(null);
            var options = builder.Build();

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.SameAs(builder));
                Assert.That(options.SlowMo, Is.Null);
            });
        }

        [Test]
        public async Task Builder_UsePlugin_RegistersPlugin()
        {
            // Arrange
            var builder = new FluentUIScaffoldBuilder();
            var plugin = new MockPlugin();

            // Act
            var result = builder.UsePlugin(plugin);

            // Assert
            Assert.That(result, Is.SameAs(builder));

            // Verify plugin is registered by building and checking
            var app = builder
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            await app.StartAsync();
            var driver = app.GetService<Interfaces.IUIDriver>();
            Assert.That(driver, Is.InstanceOf<MockUIDriver>());
            await app.DisposeAsync();
        }

        [Test]
        public async Task Builder_Web_ConfiguresOptions()
        {
            // Arrange & Act
            var app = new FluentUIScaffoldBuilder()
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts =>
                {
                    opts.BaseUrl = new Uri("http://test.local:8080");
                    opts.DefaultWaitTimeout = TimeSpan.FromSeconds(45);
                })
                .Build<WebApp>();

            await app.StartAsync();

            var options = app.GetService<FluentUIScaffoldOptions>();

            // Assert
            Assert.That(options.BaseUrl, Is.EqualTo(new Uri("http://test.local:8080")));
            Assert.That(options.DefaultWaitTimeout, Is.EqualTo(TimeSpan.FromSeconds(45)));

            await app.DisposeAsync();
        }
    }
}
