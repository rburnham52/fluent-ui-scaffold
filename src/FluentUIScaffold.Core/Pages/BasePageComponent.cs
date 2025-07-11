using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Drivers;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Pages
{
    /// <summary>
    /// Base class for all page components in the FluentUIScaffold framework.
    /// </summary>
    /// <typeparam name="TApp">The application type (WebApp, MobileApp, etc.)</typeparam>
    public abstract class BasePageComponent<TApp> : IPageComponent<TApp>
        where TApp : class
    {
        protected IUIDriver Driver { get; private set; } = default!;
        protected FluentUIScaffoldOptions Options { get; private set; } = default!;
        protected ILogger Logger { get; private set; } = default!;
        protected ElementFactory ElementFactory { get; private set; } = default!;

        protected BasePageComponent()
        {
            // Properties are initialized with default! to suppress nullability warnings
        }

        protected BasePageComponent(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
        {
            Driver = driver;
            Options = options;
            Logger = logger;
            ElementFactory = new ElementFactory(driver, options);
            // Do not call overridable methods in constructor
        }
        // If needed, call Initialize() after construction

        /// <summary>
        /// Initializes the page component with the required dependencies.
        /// </summary>
        public virtual void Initialize(IUIDriver driver, FluentUIScaffoldOptions options, ILogger logger)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ElementFactory = new ElementFactory(driver, options);
        }

        /// <summary>
        /// Creates an element with the specified selector.
        /// </summary>
        /// <param name="selector">The CSS selector for the element</param>
        /// <returns>An ElementBuilder instance for fluent configuration</returns>
        protected ElementBuilder Element(string selector)
        {
            return new ElementBuilder(selector, Driver, Options);
        }

        /// <summary>
        /// Called to configure page elements. Override in derived classes.
        /// </summary>
        protected virtual void ConfigureElements() { }

        public virtual Uri UrlPattern => new Uri("about:blank");
        public virtual bool ShouldValidateOnNavigation => false;
        public virtual bool IsCurrentPage() => true;
        public virtual void ValidateCurrentPage() { }
        public virtual TTarget NavigateTo<TTarget>() where TTarget : class => default!;
        public virtual IVerificationContext<TApp> Verify => default!;

        // Add other common page component logic as needed
    }
}
