using System;

namespace FluentUIScaffold.Core.Pages
{
    /// <summary>
    /// Specifies the route path for a page.
    /// The route is combined with the application's base URL to form the full page URL.
    /// </summary>
    /// <example>
    /// <code>
    /// // For a traditional web app with base URL http://localhost:5000
    /// [Route("/login")]
    /// public class LoginPage : Page&lt;LoginPage&gt; { }
    /// // Full URL: http://localhost:5000/login
    ///
    /// // For an SPA with hash-based routing and base URL http://localhost:5000/#
    /// [Route("/login")]
    /// public class LoginPage : Page&lt;LoginPage&gt; { }
    /// // Full URL: http://localhost:5000/#/login
    ///
    /// // Home page (empty or "/" route)
    /// [Route("/")]
    /// public class HomePage : Page&lt;HomePage&gt; { }
    /// // Full URL: http://localhost:5000/ or http://localhost:5000/#/
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class RouteAttribute : Attribute
    {
        /// <summary>
        /// Gets the route path for this page.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Creates a new RouteAttribute with the specified path.
        /// </summary>
        /// <param name="path">The route path (e.g., "/login", "/users/profile").</param>
        public RouteAttribute(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }
    }
}
