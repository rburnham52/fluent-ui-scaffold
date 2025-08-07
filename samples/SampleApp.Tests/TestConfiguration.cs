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
        /// The default base URL for the sample application.
        /// Override with environment variable TEST_BASE_URL when needed (e.g., CI/multi-instance).
        /// </summary>
        private const string DefaultBaseUrl = "http://localhost:5000";

        /// <summary>
        /// Resolved base URL (reads TEST_BASE_URL; falls back to DefaultBaseUrl).
        /// </summary>
        public static string BaseUrl => Environment.GetEnvironmentVariable("TEST_BASE_URL") ?? DefaultBaseUrl;

        /// <summary>
        /// The base URI for the sample application.
        /// </summary>
        public static Uri BaseUri => new Uri(BaseUrl);
    }
}
