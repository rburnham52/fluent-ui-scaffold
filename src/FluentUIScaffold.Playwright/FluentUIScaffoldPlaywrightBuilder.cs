using System;

using FluentUIScaffold.Core.Configuration;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// Extension methods for configuring Playwright as the UI testing plugin.
    /// </summary>
    public static class FluentUIScaffoldPlaywrightBuilder
    {
        /// <summary>
        /// Registers the Playwright plugin for browser automation.
        /// </summary>
        public static FluentUIScaffoldBuilder UsePlaywright(this FluentUIScaffoldBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder.UsePlugin(new PlaywrightPlugin());
        }
    }
}
