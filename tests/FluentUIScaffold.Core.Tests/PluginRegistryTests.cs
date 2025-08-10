using FluentUIScaffold.Core.Plugins;
using FluentUIScaffold.Core.Tests.Mocks;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class PluginRegistryTests
    {
        [SetUp]
        public void Setup()
        {
            PluginRegistry.ClearForTests();
        }

        [Test]
        public void Register_SamePluginTypeTwice_DeduplicatesByType()
        {
            // Arrange & Act
            PluginRegistry.Register(new MockPlugin());
            PluginRegistry.Register<MockPlugin>();

            // Assert
            Assert.That(PluginRegistry.GetAll().Count, Is.EqualTo(1));
        }
    }
}


