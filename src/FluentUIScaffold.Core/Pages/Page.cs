using System;
using System.Collections.Generic;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Exceptions;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Pages
{
    /// <summary>
    /// Simplified base class for all page components in the FluentUIScaffold framework.
    /// Uses a single generic type parameter for fluent API context.
    /// The driver is obtained from dependency injection.
    /// </summary>
    /// <typeparam name="TSelf">The type of the page component itself for fluent API context</typeparam>
    public abstract class Page<TSelf> where TSelf : Page<TSelf>
    {
        protected IUIDriver Driver { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected ILogger Logger { get; }
        protected FluentUIScaffoldOptions Options { get; }
        protected ElementFactory ElementFactory { get; }

        /// <summary>
        /// Gets the route template for this page (may contain placeholders like {id}).
        /// </summary>
        public string RouteTemplate { get; }

        /// <summary>
        /// Gets the full URL for this page (BaseUrl combined with Route).
        /// If the route contains placeholders, they will not be substituted until Navigate is called with parameters.
        /// </summary>
        public Uri PageUrl { get; private set; }

        /// <summary>
        /// Creates a new page instance.
        /// </summary>
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        /// <param name="pageUrl">The full URL for this page (with route template applied).</param>
        /// <param name="routeTemplate">The route template (may contain placeholders like {id}).</param>
        protected Page(IServiceProvider serviceProvider, Uri pageUrl, string routeTemplate = "")
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Driver = serviceProvider.GetRequiredService<IUIDriver>();
            Logger = serviceProvider.GetRequiredService<ILogger<Page<TSelf>>>();
            Options = serviceProvider.GetRequiredService<FluentUIScaffoldOptions>();
            ElementFactory = new ElementFactory(Driver, Options);

            PageUrl = pageUrl ?? throw new ArgumentNullException(nameof(pageUrl));
            RouteTemplate = routeTemplate ?? "";
            ConfigureElements();
        }

        /// <summary>
        /// Override this method to configure elements for this page.
        /// </summary>
        protected abstract void ConfigureElements();

        #region Fluent Element Actions

        /// <summary>
        /// Clicks an element on the page.
        /// </summary>
        public virtual TSelf Click(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.Click();
            return Self;
        }

        /// <summary>
        /// Types text into an element on the page.
        /// </summary>
        public virtual TSelf Type(Func<TSelf, IElement> elementSelector, string text)
        {
            var element = GetElementFromSelector(elementSelector);
            element.Type(text);
            return Self;
        }

        /// <summary>
        /// Selects an option in a dropdown element.
        /// </summary>
        public virtual TSelf Select(Func<TSelf, IElement> elementSelector, string value)
        {
            var element = GetElementFromSelector(elementSelector);
            element.SelectOption(value);
            return Self;
        }

        /// <summary>
        /// Focuses an element on the page.
        /// </summary>
        public virtual TSelf Focus(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            Driver.Focus(element.Selector);
            return Self;
        }

        /// <summary>
        /// Hovers over an element on the page.
        /// </summary>
        public virtual TSelf Hover(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            Driver.Hover(element.Selector);
            return Self;
        }

        /// <summary>
        /// Clears an element on the page.
        /// </summary>
        public virtual TSelf Clear(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            Driver.Clear(element.Selector);
            return Self;
        }

        #endregion

        #region Wait Methods

        /// <summary>
        /// Waits for an element to exist on the page.
        /// </summary>
        public virtual TSelf WaitForElement(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.WaitFor();
            return Self;
        }

        /// <summary>
        /// Waits for an element to be visible on the page.
        /// </summary>
        public virtual TSelf WaitForVisible(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.WaitForVisible();
            return Self;
        }

        /// <summary>
        /// Waits for an element to be hidden on the page.
        /// </summary>
        public virtual TSelf WaitForHidden(Func<TSelf, IElement> elementSelector)
        {
            var element = GetElementFromSelector(elementSelector);
            element.WaitForHidden();
            return Self;
        }

        #endregion

        #region Verification Methods

        /// <summary>
        /// Verifies an element's value matches the expected value.
        /// </summary>
        public virtual TSelf VerifyValue<TValue>(Func<TSelf, IElement> elementSelector, TValue expectedValue, string? description = null)
        {
            var element = GetElementFromSelector(elementSelector);
            var actualValue = GetElementValue<TValue>(element);

            if (!EqualityComparer<TValue>.Default.Equals(actualValue, expectedValue))
            {
                var message = description ?? $"Expected '{expectedValue}', but got '{actualValue}'";
                throw new ElementValidationException(message);
            }

            return Self;
        }

        /// <summary>
        /// Verifies an element's text matches the expected text.
        /// </summary>
        public virtual TSelf VerifyText(Func<TSelf, IElement> elementSelector, string expectedText, string? description = null)
        {
            return VerifyValue(elementSelector, expectedText, description);
        }

        /// <summary>
        /// Verifies an element's property matches the expected value.
        /// </summary>
        public virtual TSelf VerifyProperty(Func<TSelf, IElement> elementSelector, string expectedValue, string propertyName, string? description = null)
        {
            var element = GetElementFromSelector(elementSelector);
            var actualValue = GetElementPropertyValue(element, propertyName);

            if (actualValue != expectedValue)
            {
                var message = description ?? $"Expected property '{propertyName}' to be '{expectedValue}', but got '{actualValue}'";
                throw new ElementValidationException(message);
            }

            return Self;
        }

        /// <summary>
        /// Gets the verification context for this page.
        /// </summary>
        public IVerificationContext<TSelf> Verify => new VerificationContext<TSelf>(Driver, Options, Logger, Self);

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigates to this page's URL.
        /// </summary>
        public virtual TSelf Navigate()
        {
            Driver.NavigateToUrl(PageUrl);
            return Self;
        }

        /// <summary>
        /// Navigates to this page's URL with route parameter substitution.
        /// </summary>
        /// <param name="routeParams">Anonymous object or dictionary with route parameters (e.g., new { id = "123" }).</param>
        /// <returns>The page instance for fluent chaining.</returns>
        public virtual TSelf Navigate(object routeParams)
        {
            var resolvedUrl = ResolveRouteParameters(routeParams);
            PageUrl = resolvedUrl;
            Driver.NavigateToUrl(resolvedUrl);
            return Self;
        }

        /// <summary>
        /// Resolves route parameters in the PageUrl.
        /// </summary>
        /// <param name="routeParams">Anonymous object or dictionary with route parameters.</param>
        /// <returns>The resolved URL with placeholders replaced.</returns>
        protected Uri ResolveRouteParameters(object routeParams)
        {
            if (routeParams == null)
                return PageUrl;

            var urlString = PageUrl.ToString();

            // Handle dictionary
            if (routeParams is IDictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    urlString = urlString.Replace($"{{{kvp.Key}}}", Uri.EscapeDataString(kvp.Value?.ToString() ?? ""));
                }
            }
            // Handle anonymous object via reflection
            else
            {
                var properties = routeParams.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(routeParams);
                    urlString = urlString.Replace($"{{{prop.Name}}}", Uri.EscapeDataString(value?.ToString() ?? ""));
                }
            }

            return new Uri(urlString);
        }

        /// <summary>
        /// Navigates to a specific URL.
        /// </summary>
        protected virtual TSelf NavigateToUrl(Uri url)
        {
            Driver.NavigateToUrl(url);
            return Self;
        }

        /// <summary>
        /// Navigates to another page.
        /// </summary>
        public virtual TTarget NavigateTo<TTarget>() where TTarget : class
        {
            return ServiceProvider.GetRequiredService<TTarget>();
        }

        /// <summary>
        /// Alias for NavigateTo to improve readability in fluent chains.
        /// </summary>
        public virtual TTarget Then<TTarget>() where TTarget : class
        {
            return NavigateTo<TTarget>();
        }

        #endregion

        #region Page State

        /// <summary>
        /// Gets whether this page should validate on navigation.
        /// </summary>
        public virtual bool ShouldValidateOnNavigation => false;

        /// <summary>
        /// Returns true if this is the current page.
        /// Override to provide custom page detection logic.
        /// </summary>
        public virtual bool IsCurrentPage() => true;

        /// <summary>
        /// Validates that the current page state is correct.
        /// Override to provide custom validation logic.
        /// </summary>
        public virtual void ValidateCurrentPage()
        {
            // Default implementation - can be overridden by derived classes
        }

        #endregion

        #region Element Building

        /// <summary>
        /// Creates an element builder for the specified selector.
        /// </summary>
        protected ElementBuilder Element(string selector)
        {
            return new ElementBuilder(selector, Driver, Options);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the current page instance as TSelf.
        /// </summary>
        protected TSelf Self => (TSelf)this;

        /// <summary>
        /// Gets the element from a selector function.
        /// </summary>
        protected virtual IElement GetElementFromSelector(Func<TSelf, IElement> elementSelector)
        {
            return elementSelector(Self);
        }

        /// <summary>
        /// Gets a value from an element.
        /// </summary>
        protected virtual TValue GetElementValue<TValue>(IElement element)
        {
            if (typeof(TValue) == typeof(string))
            {
                return (TValue)(object)Driver.GetText(element.Selector);
            }

            throw new NotSupportedException($"Type {typeof(TValue)} is not supported for element value retrieval");
        }

        /// <summary>
        /// Gets a property value from an element.
        /// </summary>
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

        #endregion

        #region Direct Element Actions (for selector strings)

        /// <summary>
        /// Clicks an element by selector.
        /// </summary>
        protected virtual void ClickElement(string selector) => Driver.Click(selector);

        /// <summary>
        /// Types text into an element by selector.
        /// </summary>
        protected virtual void TypeText(string selector, string text) => Driver.Type(selector, text);

        /// <summary>
        /// Selects an option by selector.
        /// </summary>
        protected virtual void SelectOption(string selector, string value) => Driver.SelectOption(selector, value);

        /// <summary>
        /// Gets text from an element by selector.
        /// </summary>
        protected virtual string GetElementText(string selector) => Driver.GetText(selector);

        /// <summary>
        /// Checks if an element is visible by selector.
        /// </summary>
        protected virtual bool IsElementVisible(string selector) => Driver.IsVisible(selector);

        /// <summary>
        /// Waits for an element by selector.
        /// </summary>
        protected virtual void WaitForElement(string selector) => Driver.WaitForElement(selector);

        #endregion
    }
}
