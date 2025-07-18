using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Pages
{
    /// <summary>
    /// Base class for all page components in the FluentUIScaffold framework.
    /// Implements the V2.0 specification with dual generic types for fluent API context.
    /// </summary>
    /// <typeparam name="TDriver">The type of the UI driver (PlaywrightDriver, SeleniumDriver, etc.)</typeparam>
    /// <typeparam name="TPage">The type of the page component itself for fluent API context</typeparam>
    public abstract class BasePageComponent<TDriver, TPage> : IPageComponent<TDriver, TPage>
        where TDriver : class, IUIDriver
        where TPage : class, IPageComponent<TDriver, TPage>
    {
        protected TDriver Driver { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected ILogger Logger { get; }
        protected FluentUIScaffoldOptions Options { get; }
        protected ElementFactory ElementFactory { get; }

        protected BasePageComponent(IServiceProvider serviceProvider, Uri urlPattern)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Driver = serviceProvider.GetRequiredService<TDriver>();
            Logger = serviceProvider.GetRequiredService<ILogger<BasePageComponent<TDriver, TPage>>>();
            Options = serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
            ElementFactory = new ElementFactory(Driver, Options);

            UrlPattern = urlPattern;
            NavigateToUrl(urlPattern);
            ConfigureElements();
        }

        public Uri UrlPattern { get; }

        protected abstract void ConfigureElements();

        // Framework-agnostic element interaction methods
        protected virtual void ClickElement(string selector) => Driver.Click(selector);
        protected virtual void TypeText(string selector, string text) => Driver.Type(selector, text);
        protected virtual void SelectOption(string selector, string value) => Driver.SelectOption(selector, value);
        protected virtual string GetElementText(string selector) => Driver.GetText(selector);
        protected virtual bool IsElementVisible(string selector) => Driver.IsVisible(selector);
        protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);

        // Fluent API element action methods
        public virtual TPage Click(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.Click();
            return (TPage)(object)this;
        }

        public virtual TPage Type(Func<TPage, IElement> elementSelector, string text)
        {
            var element = GetElementFromSelector(elementSelector);
            element.Type(text);
            return (TPage)(object)this;
        }

        public virtual TPage Select(Func<TPage, IElement> elementSelector, string value)
        {
            var element = GetElementFromSelector(elementSelector);
            element.SelectOption(value);
            return (TPage)(object)this;
        }

        public virtual TPage Focus(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            Driver.Focus(element.Selector);
            return (TPage)(object)this;
        }

        public virtual TPage Hover(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            Driver.Hover(element.Selector);
            return (TPage)(object)this;
        }

        public virtual TPage Clear(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            Driver.Clear(element.Selector);
            return (TPage)(object)this;
        }

        // Additional fluent element actions
        public virtual TPage WaitForElement(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.WaitFor();
            return (TPage)(object)this;
        }

        public virtual TPage WaitForElementToBeVisible(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.WaitForVisible();
            return (TPage)(object)this;
        }

        public virtual TPage WaitForElementToBeHidden(Func<TPage, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.WaitForHidden();
            return (TPage)(object)this;
        }

        // Generic verification methods
        public virtual TPage VerifyValue<TValue>(Func<TPage, IElement> elementSelector, TValue expectedValue, string? description = null)
        {
            var element = GetElementFromSelector(elementSelector);
            var actualValue = GetElementValue<TValue>(element);

            if (!EqualityComparer<TValue>.Default.Equals(actualValue, expectedValue))
            {
                var message = description ?? $"Expected '{expectedValue}', but got '{actualValue}'";
                throw new ElementValidationException(message);
            }

            return (TPage)(object)this;
        }

        // Verify with default inner text comparison
        public virtual TPage VerifyText(Func<TPage, IElement> elementSelector, string expectedText, string? description = null)
        {
            return VerifyValue(elementSelector, expectedText, description);
        }

        // Verify with specific property comparison
        public virtual TPage VerifyProperty(Func<TPage, IElement> elementSelector, string expectedValue, string propertyName, string? description = null)
        {
            var element = GetElementFromSelector(elementSelector);
            var actualValue = GetElementPropertyValue(element, propertyName);

            if (actualValue != expectedValue)
            {
                var message = description ?? $"Expected property '{propertyName}' to be '{expectedValue}', but got '{actualValue}'";
                throw new ElementValidationException(message);
            }

            return (TPage)(object)this;
        }

        // Helper methods for element value retrieval
        protected virtual TValue GetElementValue<TValue>(IElement element)
        {
            if (typeof(TValue) == typeof(string))
            {
                return (TValue)(object)Driver.GetText(element.Selector);
            }

            throw new NotSupportedException($"Type {typeof(TValue)} is not supported for element value retrieval");
        }

        protected virtual string GetElementPropertyValue(IElement element, string propertyName)
        {
            switch (propertyName.ToLower())
            {
                case "innertext":
                case "text":
                    return Driver.GetText(element.Selector);
                case "classname":
                case "class":
                    return element.GetAttribute("class");
                case "value":
                    return element.GetAttribute("value");
                case "enabled":
                    return Driver.IsEnabled(element.Selector).ToString().ToLower();
                case "visible":
                    return Driver.IsVisible(element.Selector).ToString().ToLower();
                default:
                    return element.GetAttribute(propertyName);
            }
        }

        // Helper method to get element from selector
        protected virtual IElement GetElementFromSelector(Func<TPage, IElement> elementSelector)
        {
            return elementSelector((TPage)(object)this);
        }

        // Navigation methods
        public virtual TTarget NavigateTo<TTarget>() where TTarget : BasePageComponent<TDriver, TTarget>
        {
            var targetPage = ServiceProvider.GetRequiredService<TTarget>();
            return targetPage;
        }

        // Framework-specific access - direct driver access
        protected TDriver FrameworkDriver => Driver;

        // Public access to driver for test purposes
        public TDriver TestDriver => Driver;

        // Verification access - using the fluent verification context
        public IVerificationContext Verify => new VerificationContext(Driver, Options, Logger);

        // Helper methods
        protected ElementBuilder Element(string selector)
        {
            return new ElementBuilder(selector, Driver, Options);
        }

        protected virtual void NavigateToUrl(Uri url)
        {
            Driver.NavigateToUrl(url);
        }

        // IPageComponent implementation
        public virtual bool ShouldValidateOnNavigation => false;

        public virtual bool IsCurrentPage()
        {
            // Default implementation - can be overridden by derived classes
            return true;
        }

        public virtual void ValidateCurrentPage()
        {
            // Default implementation - can be overridden by derived classes
        }
    }
}
