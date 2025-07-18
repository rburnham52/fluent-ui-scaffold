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
    /// Example tests demonstrating registration and login flows with the FluentUIScaffold framework.
    /// These tests showcase the complete registration and login flow using the actual sample app.
    /// </summary>
    [TestClass]
    public class RegistrationLoginTests
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
        public void TestCleanup()
        {
            _fluentUI?.Dispose();
        }

        [TestMethod]
        public Task Can_Complete_Registration_Form_With_Fluent_API()
        {
            // Arrange & Act - Navigate to registration page and complete form using fluent API
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            registrationPage
                .Type(e => e.EmailInput, "john.doe@example.com")
                .Type(e => e.PasswordInput, "SecurePass123!")
                .Type(e => e.FirstNameInput, "John")
                .Type(e => e.LastNameInput, "Doe")
                .Click(e => e.RegisterButton);

            // Assert
            Assert.IsTrue(registrationPage.IsSuccessMessageVisible(), "Success message should be visible after registration");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Complete_Login_Form_With_Fluent_API()
        {
            // Arrange & Act - Navigate to login page and complete form using fluent API
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            loginPage
                .Type(e => e.EmailInput, "john.doe@example.com")
                .Type(e => e.PasswordInput, "SecurePass123!")
                .Click(e => e.LoginButton);

            // Assert
            Assert.IsTrue(loginPage.IsSuccessMessageVisible(), "Success message should be visible after login");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Convenience_Methods_For_Registration()
        {
            // Arrange & Act - Use convenience methods for registration
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            registrationPage.CompleteRegistration("john.doe@example.com", "SecurePass123!", "John", "Doe");

            // Assert
            Assert.IsTrue(registrationPage.IsSuccessMessageVisible(), "Success message should be visible after registration");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Convenience_Methods_For_Login()
        {
            // Arrange & Act - Use convenience methods for login
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            loginPage.CompleteLogin("john.doe@example.com", "SecurePass123!");

            // Assert
            Assert.IsTrue(loginPage.IsSuccessMessageVisible(), "Success message should be visible after login");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Test_Registration_Validation()
        {
            // Arrange & Act - Test registration validation (short password)
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            // Fill all fields but use a short password to trigger custom validation
            registrationPage
                .Type(e => e.EmailInput, "test@example.com")
                .Type(e => e.PasswordInput, "short") // short password
                .Type(e => e.FirstNameInput, "John")
                .Type(e => e.LastNameInput, "User");
            registrationPage.Click(e => e.RegisterButton);
            System.Threading.Thread.Sleep(1000);

            // Assert
            Assert.IsTrue(registrationPage.IsErrorMessageVisible(), "Error message should be visible for short password");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Test_Login_Validation()
        {
            // Arrange & Act - Test login validation (invalid credentials)
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            // Fill both fields but use invalid credentials to trigger custom validation
            loginPage
                .Type(e => e.EmailInput, "invalid@example.com")
                .Type(e => e.PasswordInput, "wrongpassword");
            loginPage.Click(e => e.LoginButton);
            System.Threading.Thread.Sleep(1000);

            // Assert
            Assert.IsTrue(loginPage.IsErrorMessageVisible(), "Error message should be visible for invalid credentials");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Test_Registration_Password_Validation()
        {
            // Arrange & Act - Test password validation
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            registrationPage
                .Type(e => e.EmailInput, "john.doe@example.com")
                .Type(e => e.PasswordInput, "weak")
                .Type(e => e.FirstNameInput, "John")
                .Type(e => e.LastNameInput, "Doe")
                .Click(e => e.RegisterButton);

            // Assert
            Assert.IsTrue(registrationPage.IsErrorMessageVisible(), "Error message should be visible for weak password");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Test_Login_Invalid_Credentials()
        {
            // Arrange & Act - Test invalid credentials
            var loginPage = _fluentUI!.NavigateTo<LoginPage>();

            // Click the Login navigation button to access the login page
            loginPage.TestDriver.Click("[data-testid='nav-login']");

            loginPage
                .Type(e => e.EmailInput, "invalid@example.com")
                .Type(e => e.PasswordInput, "wrongpassword")
                .Click(e => e.LoginButton);

            // Assert
            Assert.IsTrue(loginPage.IsErrorMessageVisible(), "Error message should be visible for invalid credentials");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Chain_Element_Actions_With_Wait_Operations()
        {
            // Arrange & Act - Chain element actions with wait operations
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            registrationPage
                .WaitForElement(e => e.EmailInput)
                .Type(e => e.EmailInput, "john.doe@example.com")
                .WaitForElement(e => e.PasswordInput)
                .Type(e => e.PasswordInput, "SecurePass123!")
                .WaitForElement(e => e.FirstNameInput)
                .Type(e => e.FirstNameInput, "John")
                .WaitForElement(e => e.LastNameInput)
                .Type(e => e.LastNameInput, "Doe")
                .WaitForElement(e => e.RegisterButton)
                .Click(e => e.RegisterButton);

            // Assert
            Assert.IsTrue(registrationPage.IsSuccessMessageVisible(), "Success message should be visible after registration");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Use_Element_State_Checking()
        {
            // Arrange & Act - Test element state checking
            var registrationPage = _fluentUI!.NavigateTo<RegistrationPage>();

            // Click the Register navigation button to access the registration page
            registrationPage.TestDriver.Click("[data-testid='nav-register']");

            // Assert
            Assert.IsTrue(registrationPage.EmailInput.IsVisible(), "Email input should be visible");
            Assert.IsTrue(registrationPage.PasswordInput.IsVisible(), "Password input should be visible");
            Assert.IsTrue(registrationPage.FirstNameInput.IsVisible(), "First name input should be visible");
            Assert.IsTrue(registrationPage.LastNameInput.IsVisible(), "Last name input should be visible");
            Assert.IsTrue(registrationPage.RegisterButton.IsVisible(), "Register button should be visible");
            return Task.CompletedTask;
        }

        [TestMethod]
        public Task Can_Complete_Registration_And_Login_Flow()
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

            return Task.CompletedTask;
        }
    }
}
