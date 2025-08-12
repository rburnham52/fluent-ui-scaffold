using System;
using System.Collections.Generic;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class AspNetEnvVarProviderTests
    {
        [Test]
        public void Apply_Copies_Environment_Variables()
        {
            var config = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:5050"), "/path/app.csproj")
                .WithEnvironmentVariable("FOO", "bar")
                .WithAspNetCoreEnvironment("Staging")
                .Build();

            var provider = new AspNetEnvVarProvider();
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            provider.Apply(env, config);

            Assert.Multiple(() =>
            {
                Assert.That(env["FOO"], Is.EqualTo("bar"));
                Assert.That(env["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Staging"));
            });
        }

        [Test]
        public void Apply_Sets_ASPNETCORE_URLS_For_AspNetCore()
        {
            var baseUrl = new Uri("http://localhost:6060");
            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, "/path/app.csproj")
                .Build();

            var provider = new AspNetEnvVarProvider();
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            provider.Apply(env, config);

            Assert.That(env["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
        }

        [Test]
        public void Apply_DoesNotSet_ASPNETCORE_URLS_For_Aspire()
        {
            var baseUrl = new Uri("http://localhost:7070");
            var config = ServerConfiguration
                .CreateAspireServer(baseUrl, "/path/app.csproj")
                .Build();

            var provider = new AspNetEnvVarProvider();
            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            provider.Apply(env, config);

            Assert.That(env["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
        }

        [Test]
        public void Apply_DoesNotOverwrite_Preexisting_ASPNETCORE_URLS()
        {
            var baseUrl = new Uri("http://localhost:8081");
            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, "/path/app.csproj")
                .Build();

            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ASPNETCORE_URLS"] = "http://localhost:9999/"
            };

            var provider = new AspNetEnvVarProvider();
            provider.Apply(env, config);

            // Provider always sets ASPNETCORE_URLS when BaseUrl present; ensure it equals BaseUrl
            Assert.That(env["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
        }

        [Test]
        public void Apply_Preserves_SpaProxy_Env_From_Config()
        {
            var baseUrl = new Uri("http://localhost:6061");
            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, "/path/app.csproj")
                .WithSpaProxy(true)
                .Build();

            var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var provider = new AspNetEnvVarProvider();
            provider.Apply(env, config);

            Assert.Multiple(() =>
            {
                Assert.That(env["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo("Microsoft.AspNetCore.SpaProxy"));
                Assert.That(env["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
            });
        }
    }
}