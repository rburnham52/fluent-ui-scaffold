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
    /// Example tests demonstrating verification functionality with the FluentUIScaffold framework.
    /// These tests showcase the verification methods for Story 1.3.1.
    /// </summary>
    [TestClass]
    public class VerificationTests
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
            services.AddSingleton<ILogger<RegistrationPage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<RegistrationPage>();
            });

            services.AddSingleton<ILogger<LoginPage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<LoginPage>();
            });

            services.AddSingleton<ILogger<HomePage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<HomePage>();
            });

            // Register PluginManager
            services.AddSingleton<PluginManager>();

            // Register PlaywrightPlugin
            var playwrightPlugin = new PlaywrightPlugin();
            playwrightPlugin.ConfigureServices(services);

            // Register pages with their URL patterns
            services.AddTransient<RegistrationPage>(provider =>
                new RegistrationPage(provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/registration")));
            services.AddTransient<LoginPage>(provider =>
                new LoginPage(provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/login")));
            services.AddTransient<HomePage>(provider =>
                new HomePage(provider, options.BaseUrl ?? new Uri("http://localhost")));

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
        public Task Can_Verify_Element_Text()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify text using VerifyText method
            homePage.VerifyText(e => e.PageTitle, "FluentUIScaffold Sample App");
            homePage.VerifyText(e => e.HomeSectionTitle, "Welcome to FluentUIScaffold Sample App");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Element_Property()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify element is enabled using VerifyProperty
            homePage.VerifyProperty(e => e.CounterButton, "true", "enabled");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Element_Value()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act - Click the counter button
            homePage.Click(e => e.CounterButton);

            // Assert - Verify the counter value
            homePage.VerifyText(e => e.CounterValue, "count is 1");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Element_Is_Visible()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify element is visible using VerifyProperty
            homePage.VerifyProperty(e => e.CounterButton, "true", "visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Verification_Context()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Use the verification context for fluent verification
            homePage.Verify
                .ElementIsVisible(".card button")
                .ElementIsEnabled(".card button")
                .ElementContainsText(".card button", "count is");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Page_Title()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act & Assert - Verify page title using verification context
            homePage.Verify.TitleContains("Vite + Svelte + TS");
            return Task.CompletedTask;
        }
    }
}
