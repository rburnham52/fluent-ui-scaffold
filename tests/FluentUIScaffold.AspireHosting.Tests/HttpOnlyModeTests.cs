using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentUIScaffold.AspireHosting.Tests
{
    /// <summary>
    /// Tests for the HTTP-only mode opt-in (Fix 3).
    /// </summary>
    [TestClass]
    public class HttpOnlyModeTests
    {
        [TestMethod]
        public void WithHttpOnlyMode_Enabled_SetsExpectedEnvironmentVariables()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.WithHttpOnlyMode();

            // Read via Web<T>(opts => ...) to peek at the options
            FluentUIScaffoldOptions? captured = null;
            builder.Web<object>(opts => captured = opts);
            Assert.IsNotNull(captured);

            Assert.IsTrue(captured!.EnvironmentVariables.ContainsKey("ASPNETCORE_URLS"),
                "HTTP-only mode should inject ASPNETCORE_URLS.");
            Assert.AreEqual("http://+:0", captured.EnvironmentVariables["ASPNETCORE_URLS"]);

            Assert.IsTrue(captured.EnvironmentVariables.ContainsKey("ASPNETCORE_HTTPS_PORT"),
                "HTTP-only mode should inject ASPNETCORE_HTTPS_PORT.");
            Assert.AreEqual(string.Empty, captured.EnvironmentVariables["ASPNETCORE_HTTPS_PORT"]);
        }

        [TestMethod]
        public void WithHttpOnlyMode_NotCalled_LeavesEnvironmentVariablesUntouched()
        {
            // Default behaviour must stay the same — opt-in only.
            var builder = new FluentUIScaffoldBuilder();

            FluentUIScaffoldOptions? captured = null;
            builder.Web<object>(opts => captured = opts);

            Assert.IsNotNull(captured);
            Assert.IsFalse(captured!.EnvironmentVariables.ContainsKey("ASPNETCORE_URLS"));
            Assert.IsFalse(captured.EnvironmentVariables.ContainsKey("ASPNETCORE_HTTPS_PORT"));
        }

        [TestMethod]
        public void WithHttpOnlyMode_ReturnsBuilderForChaining()
        {
            var builder = new FluentUIScaffoldBuilder();
            var returned = builder.WithHttpOnlyMode();
            Assert.AreSame(builder, returned);
        }
    }
}
