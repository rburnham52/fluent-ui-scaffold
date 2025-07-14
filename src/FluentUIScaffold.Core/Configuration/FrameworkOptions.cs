using System;

namespace FluentUIScaffold.Core.Configuration
{
    /// <summary>
    /// Base class for framework-specific options.
    /// </summary>
    public abstract class FrameworkOptions
    {
        /// <summary>
        /// Gets or sets the default timeout for operations.
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the default retry interval for operations.
        /// </summary>
        public TimeSpan DefaultRetryInterval { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets or sets a value indicating whether to capture screenshots on failure.
        /// </summary>
        public bool CaptureScreenshotsOnFailure { get; set; } = true;

        /// <summary>
        /// Gets or sets the path where screenshots are saved.
        /// </summary>
        public string ScreenshotPath { get; set; } = "./screenshots";
    }
}
