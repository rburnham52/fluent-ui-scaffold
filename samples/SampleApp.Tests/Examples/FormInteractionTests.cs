using System;
using System.Threading.Tasks;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Plugins;
using FluentUIScaffold.Playwright;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SampleApp.Tests.Pages;

namespace SampleApp.Tests.Examples
{
    /// <summary>
    /// Example tests demonstrating form interactions with the FluentUIScaffold framework.
    /// These tests showcase the fluent API for form interactions using the existing sample app.
    /// </summary>
    [TestClass]
    public class FormInteractionTests
    {
        private FluentUIScaffoldApp<WebApp>? _fluentUI;

        [TestInitialize]
        public void TestInitialize()
        {
            // Create services manually to register PlaywrightPlugin
            var services = new ServiceCollection();

            // Register core services
            var options = new FluentUIScaffoldOptions
            {
                BaseUrl = TestConfiguration.BaseUri,
                DefaultWaitTimeout = TimeSpan.FromSeconds(10),
                LogLevel = LogLevel.Information,
                HeadlessMode = true // Run in headless mode for CI/CD
            };
            services.AddSingleton(options);

            // Register logging
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(options.LogLevel);
                builder.AddConsole();
            });

            // Register ILogger (non-generic) for DI
            services.AddSingleton<ILogger>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger("FluentUIScaffold");
            });

            // Register ILogger<T> for specific types
            services.AddSingleton<ILogger<HomePage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<HomePage>();
            });

            services.AddSingleton<ILogger<ProfilePage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<ProfilePage>();
            });

            services.AddSingleton<ILogger<TodosPage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<TodosPage>();
            });

            // Register PluginManager
            services.AddSingleton<PluginManager>();

            // Register PlaywrightPlugin
            var playwrightPlugin = new PlaywrightPlugin();
            playwrightPlugin.ConfigureServices(services);

            // Register pages with their URL patterns
            services.AddTransient<HomePage>(provider =>
                new HomePage(provider, options.BaseUrl ?? new Uri("http://localhost")));
            services.AddTransient<ProfilePage>(provider =>
                new ProfilePage(provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/profile")));
            services.AddTransient<TodosPage>(provider =>
                new TodosPage(provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/todos")));

            var serviceProvider = services.BuildServiceProvider();

            // Register the plugin with PluginManager
            var pluginManager = serviceProvider.GetRequiredService<PluginManager>();
            pluginManager.RegisterPlugin<PlaywrightPlugin>();

            // Create FluentUIScaffoldApp using reflection to access internal constructor
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FluentUIScaffold");

            var constructor = typeof(FluentUIScaffoldApp<WebApp>).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                new[] { typeof(IServiceProvider), typeof(ILogger) },
                null);

            _fluentUI = (FluentUIScaffoldApp<WebApp>)constructor!.Invoke(new object[] { serviceProvider, logger });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _fluentUI?.Dispose();
        }

        [TestMethod]
        public Task Can_Use_Fluent_API_With_Existing_Elements()
        {
            // Arrange & Act - Navigate to home page and interact with existing elements
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Use fluent API to interact with the counter button
            homePage.Click(e => e.CounterButton);

            // Assert
            Assert.IsTrue(homePage.CounterButton.IsVisible(), "Counter button should be visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Chain_Element_Actions_With_Wait_Operations()
        {
            // Arrange & Act - Chain element actions with wait operations
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            homePage
                .WaitForElement(e => e.CounterButton)
                .Click(e => e.CounterButton)
                .WaitForElement(e => e.CounterValue);

            // Assert
            Assert.IsTrue(homePage.CounterButton.IsVisible(), "Counter button should be visible");
            Assert.IsTrue(homePage.CounterValue.IsVisible(), "Counter value should be visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Element_State_Checking()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Assert - Check element states
            Assert.IsTrue(homePage.CounterButton.IsVisible(), "Counter button should be visible");
            Assert.IsTrue(homePage.CounterValue.IsVisible(), "Counter value should be visible");

            // Act - Click the counter button
            homePage.Click(e => e.CounterButton);

            // Assert - Verify the interaction worked
            var counterText = homePage.CounterValue.GetText();
            Assert.IsNotNull(counterText, "Counter value should have text");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Focus_And_Hover_Actions()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Use focus and hover actions
            homePage
                .Focus(e => e.CounterButton)
                .Hover(e => e.CounterButton)
                .Click(e => e.CounterButton);

            // Assert
            Assert.IsTrue(homePage.CounterButton.IsVisible(), "Counter button should be visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Wait_For_Element_To_Be_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Wait for elements to be visible
            homePage
                .WaitForElementToBeVisible(e => e.CounterButton)
                .WaitForElementToBeVisible(e => e.CounterValue);

            // Assert
            Assert.IsTrue(homePage.CounterButton.IsVisible(), "Counter button should be visible");
            Assert.IsTrue(homePage.CounterValue.IsVisible(), "Counter value should be visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Element_Text_Retrieval()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Get element text
            var counterText = homePage.CounterValue.GetText();
            var buttonText = homePage.CounterButton.GetText();

            // Assert
            Assert.IsNotNull(counterText, "Counter value should have text");
            Assert.IsNotNull(buttonText, "Counter button should have text");
            return Task.CompletedTask;
        }
    }
}
