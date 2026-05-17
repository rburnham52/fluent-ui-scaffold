using System;
using System.Threading.Tasks;

using FluentUIScaffold.AspireHosting;
using FluentUIScaffold.Core.Configuration;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FluentUIScaffold.AspireHosting.Tests
{
    /// <summary>
    /// Tests for the Docker pre-flight health check (Fix 1).
    /// </summary>
    [TestClass]
    public class DockerPreflightCheckTests
    {
        [TestMethod]
        public void DockerUnreachableMessage_IsExposedAsPublicConstant()
        {
            // The message text must be exposed publicly so downstream test suites
            // (e.g., the kitchen-chef BDD suite) can assert on it without hardcoding strings.
            Assert.IsFalse(string.IsNullOrWhiteSpace(DockerPreflightCheck.DockerUnreachableMessage));
            StringAssert.Contains(DockerPreflightCheck.DockerUnreachableMessage, "Docker daemon is not reachable");
            StringAssert.Contains(DockerPreflightCheck.DockerUnreachableMessage, "Resource Saver");
        }

        [TestMethod]
        public async Task EnsureDockerHealthyAsync_WithImpossiblyShortTimeout_FailsWithUserMessage()
        {
            // 1 millisecond is short enough that the docker CLI cannot possibly start + respond in time,
            // even on a healthy machine. This proves the unreachable path throws our user-facing message
            // (and not some inner Docker / process exception).
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => DockerPreflightCheck.EnsureDockerHealthyAsync(timeout: TimeSpan.FromMilliseconds(1)));

            Assert.AreEqual(DockerPreflightCheck.DockerUnreachableMessage, ex.Message);

            // Critical: no inner exception. The whole point of fix 1 is a clean single-line error,
            // not a stack trace through 20 frames of Aspire's bootstrap.
            Assert.IsNull(ex.InnerException);
        }

        [TestMethod]
        public void SkipDockerPreflightCheck_OptOut_IsHonored()
        {
            // Verify the builder extension flips the per-builder config flag.
            var builder = new FluentUIScaffoldBuilder();
            var configBefore = AspireHostingExtensions.GetOrCreateConfig(builder);
            Assert.IsFalse(configBefore.SkipDockerPreflightCheck);

            var returned = builder.SkipDockerPreflightCheck();
            Assert.AreSame(builder, returned, "Extension should return the builder for chaining.");

            var configAfter = AspireHostingExtensions.GetOrCreateConfig(builder);
            Assert.IsTrue(configAfter.SkipDockerPreflightCheck);
        }
    }
}
