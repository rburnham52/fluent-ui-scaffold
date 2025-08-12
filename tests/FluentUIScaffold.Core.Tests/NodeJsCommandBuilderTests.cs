using System;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class NodeJsCommandBuilderTests
    {
        [Test]
        public void BuildCommand_IncludesStartAndCustomArgs()
        {
            var baseUrl = new Uri("http://localhost:6101");
            var config = ServerConfiguration
                .CreateNodeJsServer(baseUrl, "/path/package.json")
                .WithArguments("--", "--open")
                .Build();

            var builder = new NodeJsCommandBuilder();
            var args = builder.BuildCommand(config);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.StartWith("start "));
                Assert.That(args, Does.Contain("-- --open"));
            });
        }
    }
}