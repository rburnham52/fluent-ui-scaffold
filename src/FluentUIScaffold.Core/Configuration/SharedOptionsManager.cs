using System;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Manages shared FluentUIScaffoldOptions to ensure consistency between
    /// WebServerManager and FluentUIScaffoldApp instances.
    /// </summary>
    public static class SharedOptionsManager
    {
        private static FluentUIScaffoldOptions? _sharedOptions;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// Sets the shared options instance.
        /// </summary>
        /// <param name="options">The options to share</param>
        public static void SetSharedOptions(FluentUIScaffoldOptions options)
        {
            lock (_lockObject)
            {
                _sharedOptions = options ?? throw new ArgumentNullException(nameof(options));
            }
        }

        /// <summary>
        /// Gets the shared options instance.
        /// </summary>
        /// <returns>The shared options, or null if not set</returns>
        public static FluentUIScaffoldOptions? GetSharedOptions()
        {
            lock (_lockObject)
            {
                return _sharedOptions;
            }
        }

        /// <summary>
        /// Clears the shared options instance.
        /// </summary>
        public static void ClearSharedOptions()
        {
            lock (_lockObject)
            {
                _sharedOptions = null;
            }
        }

        /// <summary>
        /// Gets or creates shared options, using the provided options as a template if shared options don't exist.
        /// </summary>
        /// <param name="templateOptions">Template options to use if shared options don't exist</param>
        /// <returns>The shared options</returns>
        public static FluentUIScaffoldOptions GetOrCreateSharedOptions(FluentUIScaffoldOptions templateOptions)
        {
            lock (_lockObject)
            {
                if (_sharedOptions == null)
                {
                    _sharedOptions = templateOptions;
                }
                return _sharedOptions;
            }
        }
    }
}

