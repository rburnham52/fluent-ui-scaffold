using FluentUIScaffold.Core;

namespace FluentUIScaffold.Playwright
{
    /// <summary>
    /// Convenience helpers for registering the Playwright plugin.
    /// </summary>
    public static class FluentUIScaffoldPlaywrightBuilder
    {
        /// <summary>
        /// Registers the Playwright plugin globally for FluentUIScaffold.
        /// </summary>
        public static void UsePlaywright()
        {
            FluentUIScaffoldBuilder.UsePlugin(new PlaywrightPlugin());
        }
    }
}


