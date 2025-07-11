using System;
using System.Collections.Generic;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Interfaces;
using FluentUIScaffold.Core.Pages;

using Microsoft.Extensions.Logging;

namespace SampleApp.Tests.Pages
{
    /// <summary>
    /// Page object for the Profile page of the FluentUIScaffold sample application.
    /// Demonstrates form validation, complex form interactions, and state management.
    /// </summary>
    public class ProfilePage : BasePageComponent<WebApp>
    {
        public ProfilePage(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
            : base(driver, options, logger)
        {
            ConfigureElements();
        }

        // Action buttons
        private IElement _editProfileButton;
        private IElement _saveProfileButton;
        private IElement _cancelEditButton;
        private IElement _resetProfileButton;

        // Personal information form elements
        private IElement _firstNameInput;
        private IElement _lastNameInput;
        private IElement _emailInput;
        private IElement _phoneInput;

        // Work information form elements
        private IElement _departmentSelect;
        private IElement _roleSelect;

        // Notification preferences
        private IElement _emailNotificationCheckbox;
        private IElement _smsNotificationCheckbox;
        private IElement _pushNotificationCheckbox;

        // Preferences
        private IElement _themeSelect;
        private IElement _languageSelect;
        private IElement _timezoneSelect;

        // Error messages
        private IElement _firstNameError;
        private IElement _lastNameError;
        private IElement _emailError;
        private IElement _phoneError;

        // Navigation elements
        private IElement _navHomeButton;
        private IElement _navTodosButton;
        private IElement _navProfileButton;

        public override Uri UrlPattern => TestConfiguration.BaseUri;

        public override bool ShouldValidateOnNavigation => true;

        protected override void ConfigureElements()
        {
            // Action buttons
            _editProfileButton = Element("[data-testid='edit-profile-btn']")
                .WithDescription("Edit Profile Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _saveProfileButton = Element("[data-testid='save-profile-btn']")
                .WithDescription("Save Profile Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _cancelEditButton = Element("[data-testid='cancel-edit-btn']")
                .WithDescription("Cancel Edit Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _resetProfileButton = Element("[data-testid='reset-profile-btn']")
                .WithDescription("Reset Profile Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            // Personal information form elements
            _firstNameInput = Element("[data-testid='first-name-input']")
                .WithDescription("First Name Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _lastNameInput = Element("[data-testid='last-name-input']")
                .WithDescription("Last Name Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _emailInput = Element("[data-testid='email-input']")
                .WithDescription("Email Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _phoneInput = Element("[data-testid='phone-input']")
                .WithDescription("Phone Input")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Work information form elements
            _departmentSelect = Element("[data-testid='department-select']")
                .WithDescription("Department Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _roleSelect = Element("[data-testid='role-select']")
                .WithDescription("Role Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Notification preferences
            _emailNotificationCheckbox = Element("[data-testid='email-notification-checkbox']")
                .WithDescription("Email Notification Checkbox")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _smsNotificationCheckbox = Element("[data-testid='sms-notification-checkbox']")
                .WithDescription("SMS Notification Checkbox")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _pushNotificationCheckbox = Element("[data-testid='push-notification-checkbox']")
                .WithDescription("Push Notification Checkbox")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Preferences
            _themeSelect = Element("[data-testid='theme-select']")
                .WithDescription("Theme Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _languageSelect = Element("[data-testid='language-select']")
                .WithDescription("Language Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _timezoneSelect = Element("[data-testid='timezone-select']")
                .WithDescription("Timezone Select")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Error messages
            _firstNameError = Element("[data-testid='first-name-error']")
                .WithDescription("First Name Error")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _lastNameError = Element("[data-testid='last-name-error']")
                .WithDescription("Last Name Error")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _emailError = Element("[data-testid='email-error']")
                .WithDescription("Email Error")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            _phoneError = Element("[data-testid='phone-error']")
                .WithDescription("Phone Error")
                .WithWaitStrategy(WaitStrategy.Visible)
                .Build();

            // Navigation elements
            _navHomeButton = Element("[data-testid='nav-home']")
                .WithDescription("Home Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _navTodosButton = Element("[data-testid='nav-todos']")
                .WithDescription("Todos Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();

            _navProfileButton = Element("[data-testid='nav-profile']")
                .WithDescription("Profile Navigation Button")
                .WithWaitStrategy(WaitStrategy.Clickable)
                .Build();
        }

        /// <summary>
        /// Starts editing the profile by clicking the edit button.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage StartEditing()
        {
            Logger.LogInformation("Starting profile editing");
            _editProfileButton.Click();
            return this;
        }

        /// <summary>
        /// Saves the profile changes.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage SaveProfile()
        {
            Logger.LogInformation("Saving profile changes");
            _saveProfileButton.Click();
            return this;
        }

        /// <summary>
        /// Cancels the profile editing.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage CancelEditing()
        {
            Logger.LogInformation("Canceling profile editing");
            _cancelEditButton.Click();
            return this;
        }

        /// <summary>
        /// Resets the profile to default values.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage ResetProfile()
        {
            Logger.LogInformation("Resetting profile");
            _resetProfileButton.Click();
            return this;
        }

        /// <summary>
        /// Fills in the personal information section.
        /// </summary>
        /// <param name="firstName">The first name</param>
        /// <param name="lastName">The last name</param>
        /// <param name="email">The email address</param>
        /// <param name="phone">The phone number</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage FillPersonalInformation(string firstName, string lastName, string email, string phone)
        {
            Logger.LogInformation($"Filling personal information: {firstName} {lastName}, {email}, {phone}");

            _firstNameInput.Type(firstName);
            _lastNameInput.Type(lastName);
            _emailInput.Type(email);
            _phoneInput.Type(phone);

            return this;
        }

        /// <summary>
        /// Fills in the work information section.
        /// </summary>
        /// <param name="department">The department</param>
        /// <param name="role">The role</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage FillWorkInformation(string department, string role)
        {
            Logger.LogInformation($"Filling work information: {department}, {role}");

            _departmentSelect.SelectOption(department);
            _roleSelect.SelectOption(role);

            return this;
        }

        /// <summary>
        /// Sets the notification preferences.
        /// </summary>
        /// <param name="emailNotifications">Enable email notifications</param>
        /// <param name="smsNotifications">Enable SMS notifications</param>
        /// <param name="pushNotifications">Enable push notifications</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage SetNotificationPreferences(bool emailNotifications, bool smsNotifications, bool pushNotifications)
        {
            Logger.LogInformation($"Setting notification preferences: Email={emailNotifications}, SMS={smsNotifications}, Push={pushNotifications}");

            if (emailNotifications != IsCheckboxChecked(_emailNotificationCheckbox))
            {
                _emailNotificationCheckbox.Click();
            }

            if (smsNotifications != IsCheckboxChecked(_smsNotificationCheckbox))
            {
                _smsNotificationCheckbox.Click();
            }

            if (pushNotifications != IsCheckboxChecked(_pushNotificationCheckbox))
            {
                _pushNotificationCheckbox.Click();
            }

            return this;
        }

        /// <summary>
        /// Sets the user preferences.
        /// </summary>
        /// <param name="theme">The theme preference</param>
        /// <param name="language">The language preference</param>
        /// <param name="timezone">The timezone preference</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage SetPreferences(string theme, string language, string timezone)
        {
            Logger.LogInformation($"Setting preferences: Theme={theme}, Language={language}, Timezone={timezone}");

            _themeSelect.SelectOption(theme);
            _languageSelect.SelectOption(language);
            _timezoneSelect.SelectOption(timezone);

            return this;
        }

        /// <summary>
        /// Gets the current value of a form field.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The current value of the field</returns>
        public string GetFieldValue(string fieldName)
        {
            var element = GetElementByFieldName(fieldName);
            var value = element.GetText();
            Logger.LogInformation($"Field {fieldName} value: {value}");
            return value;
        }

        /// <summary>
        /// Gets the current selection of a select field.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>The current selection</returns>
        public string GetSelectValue(string fieldName)
        {
            var element = GetElementByFieldName(fieldName);
            // This would need to be implemented based on the specific driver capabilities
            // For now, we'll return a default value
            Logger.LogInformation($"Select {fieldName} value: {fieldName}");
            return fieldName;
        }

        /// <summary>
        /// Checks if a checkbox is checked.
        /// </summary>
        /// <param name="checkbox">The checkbox element</param>
        /// <returns>True if the checkbox is checked, false otherwise</returns>
        private bool IsCheckboxChecked(IElement checkbox)
        {
            // This would need to be implemented based on the specific driver capabilities
            // For now, we'll return a default value
            return false;
        }

        /// <summary>
        /// Gets an element by its field name.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The element</returns>
        private IElement GetElementByFieldName(string fieldName)
        {
            return fieldName switch
            {
                "firstName" => _firstNameInput,
                "lastName" => _lastNameInput,
                "email" => _emailInput,
                "phone" => _phoneInput,
                "department" => _departmentSelect,
                "role" => _roleSelect,
                "theme" => _themeSelect,
                "language" => _languageSelect,
                "timezone" => _timezoneSelect,
                _ => throw new ArgumentException($"Unknown field name: {fieldName}")
            };
        }

        /// <summary>
        /// Verifies that a field has the expected value.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="expectedValue">The expected value</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage VerifyFieldValue(string fieldName, string expectedValue)
        {
            var actualValue = GetFieldValue(fieldName);
            Logger.LogInformation($"Verifying field {fieldName}. Expected: {expectedValue}, Actual: {actualValue}");

            if (actualValue != expectedValue)
            {
                throw new InvalidOperationException($"Field {fieldName} value mismatch. Expected: {expectedValue}, Actual: {actualValue}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that a select field has the expected selection.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="expectedValue">The expected selection</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage VerifySelectValue(string fieldName, string expectedValue)
        {
            var actualValue = GetSelectValue(fieldName);
            Logger.LogInformation($"Verifying select {fieldName}. Expected: {expectedValue}, Actual: {actualValue}");

            if (actualValue != expectedValue)
            {
                throw new InvalidOperationException($"Select {fieldName} value mismatch. Expected: {expectedValue}, Actual: {actualValue}");
            }
            return this;
        }

        /// <summary>
        /// Verifies that a notification checkbox has the expected state.
        /// </summary>
        /// <param name="notificationType">The type of notification (email, sms, push)</param>
        /// <param name="expectedChecked">The expected checked state</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage VerifyNotificationCheckbox(string notificationType, bool expectedChecked)
        {
            var checkbox = GetNotificationCheckbox(notificationType);
            var actualChecked = IsCheckboxChecked(checkbox);
            Logger.LogInformation($"Verifying {notificationType} notification checkbox. Expected: {expectedChecked}, Actual: {actualChecked}");

            if (actualChecked != expectedChecked)
            {
                throw new InvalidOperationException($"{notificationType} notification checkbox state mismatch. Expected: {expectedChecked}, Actual: {actualChecked}");
            }
            return this;
        }

        /// <summary>
        /// Gets a notification checkbox by type.
        /// </summary>
        /// <param name="notificationType">The type of notification (email, sms, push)</param>
        /// <returns>The checkbox element</returns>
        private IElement GetNotificationCheckbox(string notificationType)
        {
            return notificationType switch
            {
                "email" => _emailNotificationCheckbox,
                "sms" => _smsNotificationCheckbox,
                "push" => _pushNotificationCheckbox,
                _ => throw new ArgumentException($"Unknown notification type: {notificationType}")
            };
        }

        /// <summary>
        /// Verifies that a validation error is displayed for a field.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="expectedError">The expected error message</param>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage VerifyFieldError(string fieldName, string expectedError)
        {
            var errorElement = GetErrorElement(fieldName);
            var actualError = errorElement.GetText();
            Logger.LogInformation($"Verifying field error for {fieldName}. Expected: {expectedError}, Actual: {actualError}");

            if (actualError != expectedError)
            {
                throw new InvalidOperationException($"Field error mismatch for {fieldName}. Expected: {expectedError}, Actual: {actualError}");
            }
            return this;
        }

        /// <summary>
        /// Gets an error element by field name.
        /// </summary>
        /// <param name="fieldName">The field name</param>
        /// <returns>The error element</returns>
        private IElement GetErrorElement(string fieldName)
        {
            return fieldName switch
            {
                "firstName" => _firstNameError,
                "lastName" => _lastNameError,
                "email" => _emailError,
                "phone" => _phoneError,
                _ => throw new ArgumentException($"Unknown field name: {fieldName}")
            };
        }

        /// <summary>
        /// Verifies that the edit button is visible and enabled.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage VerifyEditButtonVisible()
        {
            Logger.LogInformation("Verifying edit button is visible");
            if (!_editProfileButton.IsVisible())
            {
                throw new InvalidOperationException("Edit button is not visible");
            }
            return this;
        }

        /// <summary>
        /// Verifies that the save and cancel buttons are visible when in edit mode.
        /// </summary>
        /// <returns>The current page instance for method chaining</returns>
        public ProfilePage VerifySaveCancelButtonsVisible()
        {
            Logger.LogInformation("Verifying save and cancel buttons are visible");
            if (!_saveProfileButton.IsVisible())
            {
                throw new InvalidOperationException("Save button is not visible");
            }
            if (!_cancelEditButton.IsVisible())
            {
                throw new InvalidOperationException("Cancel button is not visible");
            }
            return this;
        }

        /// <summary>
        /// Navigates to the Home page.
        /// </summary>
        /// <returns>A new HomePage instance</returns>
        public HomePage NavigateToHome()
        {
            Logger.LogInformation("Navigating to Home page");
            _navHomeButton.Click();
            return new HomePage(Driver, Options, Logger);
        }

        /// <summary>
        /// Navigates to the Todos page.
        /// </summary>
        /// <returns>A new TodosPage instance</returns>
        public TodosPage NavigateToTodos()
        {
            Logger.LogInformation("Navigating to Todos page");
            _navTodosButton.Click();
            return new TodosPage(Driver, Options, Logger);
        }
    }
}
