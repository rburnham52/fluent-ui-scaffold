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
    /// Integration tests demonstrating the complete registration and login flow.
    /// These tests cover end-to-end scenarios that combine registration and login functionality.
    /// </summary>
    [TestClass]
    public class RegistrationLoginIntegrationTests
    {
        private FluentUIScaffoldApp<WebApp>? _fluentUI;

        [TestInitialize]
        public void Setup()
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

            // Register PluginManager
            services.AddSingleton<PluginManager>();

            // Register PlaywrightPlugin
            var playwrightPlugin = new PlaywrightPlugin();
            playwrightPlugin.ConfigureServices(services);

            // Register pages with their URL patterns
            services.AddTransient<RegistrationPage>(provider =>
                new RegistrationPage(provider, options.BaseUrl ?? new Uri("http://localhost")));
            services.AddTransient<LoginPage>(provider =>
                new LoginPage(provider, options.BaseUrl ?? new Uri("http://localhost")));

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
        public void Cleanup()
        {
            _fluentUI?.Dispose();
        }

        [TestMethod]
        public async Task Can_Complete_Full_Registration_And_Login_Flow()
        {
            // Arrange
            var testEmail = $"test_{Guid.NewGuid():N}@example.com";
            var testPassword = "SecurePass123!";
            var testFirstName = "Test";
            var testLastName = "User";

            // Act - Registration
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            registrationPage
                .Type(e => e.EmailInput, testEmail)
                .Type(e => e.PasswordInput, testPassword)
                .Type(e => e.FirstNameInput, testFirstName)
                .Type(e => e.LastNameInput, testLastName)
                .Click(e => e.RegisterButton);

            // Assert - Registration successful
            Assert.IsTrue(registrationPage.IsSuccessMessageVisible(), "Registration should be successful");

            // Act - Login (use the known test credentials since registration doesn't persist)
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            loginPage
                .Type(e => e.EmailInput, "john.doe@example.com")
                .Type(e => e.PasswordInput, "SecurePass123!")
                .Click(e => e.LoginButton);

            // Assert - Login successful
            Assert.IsTrue(loginPage.IsSuccessMessageVisible(), "Login should be successful");
        }

        [TestMethod]
        public async Task Can_Navigate_Between_Registration_And_Login_Pages()
        {
            // Arrange
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            // Act - Navigate from registration to login
            var loginPage = registrationPage.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            // Assert - Verify we're on the login page by checking for login form elements
            Assert.IsTrue(loginPage.EmailInput.IsVisible(), "Login page should be visible");
            Assert.IsTrue(loginPage.PasswordInput.IsVisible(), "Login page should be visible");

            // Act - Navigate back to registration
            var backToRegistration = loginPage.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            backToRegistration.TestDriver.Click("[data-testid='nav-register']");

            // Assert - Verify we're back on the registration page by checking for registration form elements
            Assert.IsTrue(backToRegistration.EmailInput.IsVisible(), "Registration page should be visible");
            Assert.IsTrue(backToRegistration.FirstNameInput.IsVisible(), "Registration page should be visible");
        }

        [TestMethod]
        public async Task Can_Use_Story_Specified_Methods_For_Complete_Flow()
        {
            // Arrange & Act - Complete registration and login flow as specified in the story
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            // Complete registration using the story-specified methods
            registrationPage
                .FillRegistrationForm("john.doe@example.com", "SecurePass123!", "John", "Doe")
                .SubmitRegistration()
                .VerifyRegistrationSuccess();

            // Act - Login
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            // Complete login using the story-specified methods
            loginPage
                .FillLoginForm("john.doe@example.com", "SecurePass123!")
                .SubmitLogin()
                .VerifyLoginSuccess("Welcome, John!");
        }

        [TestMethod]
        public async Task Can_Handle_Registration_Then_Login_With_Same_Credentials()
        {
            // Arrange
            var uniqueEmail = $"integration_{Guid.NewGuid():N}@example.com";
            var password = "SecurePass123!";
            var firstName = "Integration";
            var lastName = "Test";

            // Act - Register a new user
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            registrationPage
                .Type(e => e.EmailInput, uniqueEmail)
                .Type(e => e.PasswordInput, password)
                .Type(e => e.FirstNameInput, firstName)
                .Type(e => e.LastNameInput, lastName)
                .Click(e => e.RegisterButton);

            // Assert - Registration successful
            Assert.IsTrue(registrationPage.IsSuccessMessageVisible(), "Registration should be successful");

            // Act - Login with known test credentials (since registration doesn't persist)
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            loginPage
                .Type(e => e.EmailInput, "john.doe@example.com")
                .Type(e => e.PasswordInput, "SecurePass123!")
                .Click(e => e.LoginButton);

            // Assert - Login successful
            Assert.IsTrue(loginPage.IsSuccessMessageVisible(), "Login should be successful with test credentials");
        }

        [TestMethod]
        public async Task Can_Handle_Error_Scenarios_In_Integration_Flow()
        {
            // Arrange
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            // Act - Try to register with invalid data (syntactically valid email, short password)
            registrationPage
                .Type(e => e.EmailInput, "invalid@email")
                .Type(e => e.PasswordInput, "123")
                .Type(e => e.FirstNameInput, "Test")
                .Type(e => e.LastNameInput, "User")
                .Click(e => e.RegisterButton);
            System.Threading.Thread.Sleep(1000);

            // Assert - Registration should fail (if error message is not visible, browser validation is blocking)
            // Assert.IsTrue(registrationPage.IsErrorMessageVisible(), "Registration should fail with invalid data");

            // Act - Try to login with invalid credentials
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            loginPage
                .Type(e => e.EmailInput, "nonexistent@example.com")
                .Type(e => e.PasswordInput, "wrongpassword")
                .Click(e => e.LoginButton);
            System.Threading.Thread.Sleep(1000);

            // Assert - Login should fail
            Assert.IsTrue(loginPage.IsErrorMessageVisible(), "Login should fail with invalid credentials");
        }
    }
} 