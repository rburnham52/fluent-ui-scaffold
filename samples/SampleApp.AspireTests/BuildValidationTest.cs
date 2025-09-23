using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SampleApp.AspireTests
{
    /// <summary>
    /// Simple test to validate the project builds and basic dependencies work.
    /// </summary>
    [TestClass]
    public class BuildValidationTest
    {
        [TestMethod]
        [TestCategory("Build")]
        public void BuildValidation_BasicDependencies_ShouldWork()
        {
            // This test verifies that basic dependencies are available
            var testString = "Hello, Aspire Integration!";
            Assert.IsNotNull(testString);
            Assert.IsTrue(testString.Contains("Aspire"));

            // Test that we can create basic types
            var testUri = new Uri("http://localhost:5000");
            Assert.IsNotNull(testUri);
            Assert.AreEqual(5000, testUri.Port);

            Console.WriteLine("✅ Basic build validation passed!");
        }

        [TestMethod]
        [TestCategory("Build")]
        public void BuildValidation_FluentUIScaffoldCore_ShouldLoad()
        {
            // This test verifies that FluentUIScaffold.Core types are available
            try
            {
                var coreAssembly = typeof(FluentUIScaffold.Core.WebApp).Assembly;
                Assert.IsNotNull(coreAssembly);

                var serverAssembly = typeof(FluentUIScaffold.Core.Server.IServerManager).Assembly;
                Assert.IsNotNull(serverAssembly);

                Console.WriteLine("✅ FluentUIScaffold.Core dependencies loaded successfully!");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to load FluentUIScaffold.Core dependencies: {ex.Message}");
            }
        }
    }
}
