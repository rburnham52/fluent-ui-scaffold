using System;
using FluentUIScaffold.Core.Configuration;
using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class DotNetServerConfigurationBuilderTests
    {
        [Test]
        public void WithAspNetCoreEnvironment_SetsEnvVar()
        {
            var cfg = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7001"), "/app/project.csproj")
                .WithAspNetCoreEnvironment("Staging")
                .Build();

            Assert.That(cfg.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Staging"));
        }

        [Test]
        public void WithDotNetEnvironment_SetsEnvVar()
        {
            var cfg = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7002"), "/app/host.csproj")
                .WithDotNetEnvironment("Production")
                .Build();

            Assert.That(cfg.EnvironmentVariables["DOTNET_ENVIRONMENT"], Is.EqualTo("Production"));
        }

        [Test]
        public void AspireEndpoints_SetToProvidedValues()
        {
            var cfg = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7003"), "/app/host.csproj")
                .WithAspireDashboardOtlpEndpoint("https://dash.example")
                .WithAspireResourceServiceEndpoint("https://res.example")
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(cfg.EnvironmentVariables["DOTNET_DASHBOARD_OTLP_ENDPOINT_URL"], Is.EqualTo("https://dash.example"));
                Assert.That(cfg.EnvironmentVariables["DOTNET_RESOURCE_SERVICE_ENDPOINT_URL"], Is.EqualTo("https://res.example"));
            });
        }

        [Test]
        public void WithAspNetCoreHostingStartupAssemblies_SetsValue()
        {
            var cfg = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7004"), "/app/app.csproj")
                .WithAspNetCoreHostingStartupAssemblies("Custom.Assembly")
                .Build();

            // Build() overwrites this value based on SpaProxy (disabled by default => "")
            Assert.That(cfg.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo(""));
        }

        [Test]
        public void WithAspNetCoreUrls_SetsValue()
        {
            var cfg = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7005"), "/app/host.csproj")
                .WithAspNetCoreUrls("http://0.0.0.0:7005")
                .Build();

            Assert.That(cfg.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo("http://0.0.0.0:7005"));
        }

        [Test]
        public void WithAspNetCoreForwardedHeaders_SetsBooleanString()
        {
            var cfgTrue = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7006"), "/app/host.csproj")
                .WithAspNetCoreForwardedHeaders(true)
                .Build();

            var cfgFalse = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7007"), "/app/host.csproj")
                .WithAspNetCoreForwardedHeaders(false)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(cfgTrue.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], Is.EqualTo("true"));
                Assert.That(cfgFalse.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], Is.EqualTo("false"));
            });
        }

        [Test]
        public void WithSpaProxy_Toggle_SetsHostingAssemblies_And_BuildAddsFrameworkConfig()
        {
            var cfgOn = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7008"), "/app/app.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Debug")
                .WithSpaProxy(true)
                .Build();

            var cfgOff = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7009"), "/app/app.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Debug")
                .WithSpaProxy(false)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(cfgOn.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo("Microsoft.AspNetCore.SpaProxy"));
                Assert.That(cfgOff.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo(""));
                Assert.That(string.Join(" ", cfgOn.Arguments), Does.Contain("--framework net8.0"));
                Assert.That(string.Join(" ", cfgOn.Arguments), Does.Contain("--configuration Debug"));
            });
        }
    }
}