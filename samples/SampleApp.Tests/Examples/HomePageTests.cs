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
    /// Example tests demonstrating the FluentUIScaffold framework capabilities.
    /// These tests showcase various features and patterns for UI testing.
    /// </summary>
    [TestClass]
    public class HomePageTests
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

            services.AddSingleton<ILogger<LoginPage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<LoginPage>();
            });

            services.AddSingleton<ILogger<RegistrationPage>>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return loggerFactory.CreateLogger<RegistrationPage>();
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
            services.AddTransient<LoginPage>(provider =>
                new LoginPage(provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/login")));
            services.AddTransient<RegistrationPage>(provider =>
                new RegistrationPage(provider, new Uri(options.BaseUrl ?? new Uri("http://localhost"), "/register")));

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
        public Task Can_Navigate_To_Home_Page()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Assert
            Assert.IsNotNull(homePage);
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Verify_Page_Title()
        {
            // Arrange & Act
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Assert
            homePage.VerifyPageTitle();
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Interact_With_Counter()
        {
            // Arrange
            var homePage = _fluentUI!.NavigateTo<HomePage>();

            // Act
            homePage.ClickCounter();

            // Assert
            var counterValue = homePage.GetCounterValue();
            Assert.IsNotNull(counterValue);
            return Task.CompletedTask;
        }
    }
}
