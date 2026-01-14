using System;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;

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

        /// <summary>
        /// Registers the Playwright plugin using the options builder fluent API.
        /// </summary>
        public static FluentUIScaffoldOptionsBuilder UsePlaywright(this FluentUIScaffoldOptionsBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.UsePlugin(new PlaywrightPlugin());
            return builder;
        }

        /// <summary>
        /// Registers the Playwright plugin for the specific options instance.
        /// </summary>
        public static FluentUIScaffoldOptions UsePlaywright(this FluentUIScaffoldOptions options)
        {
            options.Plugins.Add(new PlaywrightPlugin());

            return options;
        }
    }
}


