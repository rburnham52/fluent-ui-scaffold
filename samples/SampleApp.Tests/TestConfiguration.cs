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

        /// <summary>
        /// Determines if tests should run in headless mode.
        /// Returns true in CI environments or when explicitly set via environment variable.
        /// </summary>
        public static bool IsHeadlessMode
        {
            get
            {
                // Check for explicit headless setting
                var headlessEnv = Environment.GetEnvironmentVariable("HEADLESS_MODE");
                if (!string.IsNullOrEmpty(headlessEnv) && bool.TryParse(headlessEnv, out var explicitHeadless))
                {
                    return explicitHeadless;
                }

                // Auto-detect CI environments
                return IsCIEnvironment;
            }
        }

        /// <summary>
        /// Detects if running in a CI environment.
        /// </summary>
        public static bool IsCIEnvironment =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BUILD_BUILDID"));
    }
}
