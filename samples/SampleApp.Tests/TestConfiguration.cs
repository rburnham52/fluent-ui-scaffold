using System;

namespace SampleApp.Tests
{
    /// <summary>
    /// Shared configuration for the test suite.
    /// Contains constants and settings used across all tests.
    /// </summary>
    public static class TestConfiguration
    {
        /// <summary>
        /// The base URL for the sample application.
        /// In release mode, the ASP.NET Core app serves the built SPA files directly.
        /// </summary>
        public const string BaseUrl = "http://localhost:5000";

        /// <summary>
        /// The base URI for the sample application.
        /// </summary>
        public static Uri BaseUri => new Uri(BaseUrl);
    }
}
