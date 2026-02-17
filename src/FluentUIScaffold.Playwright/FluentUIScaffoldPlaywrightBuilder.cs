using System;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// Convenience helpers for registering the Playwright plugin.
    /// </summary>
    public static class FluentUIScaffoldPlaywrightBuilder
    {
        /// <summary>
        /// Registers the Playwright plugin using the FluentUIScaffoldBuilder fluent API.
        /// </summary>
        public static Core.Configuration.FluentUIScaffoldBuilder UsePlaywright(this Core.Configuration.FluentUIScaffoldBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.UsePlugin(new PlaywrightPlugin());
            return builder;
        }
    }
}
