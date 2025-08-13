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
            var plan = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7001"), "/app/project.csproj")
                .WithAspNetCoreEnvironment("Staging")
                .Build();

            Assert.That(plan.StartInfo.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Staging"));
        }

        [Test]
        public void WithDotNetEnvironment_SetsEnvVar()
        {
            var plan = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7002"), "/app/host.csproj")
                .WithDotNetEnvironment("Production")
                .Build();

            Assert.That(plan.StartInfo.EnvironmentVariables["DOTNET_ENVIRONMENT"], Is.EqualTo("Production"));
        }

        [Test]
        public void AspireEndpoints_SetToProvidedValues()
        {
            var plan = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7003"), "/app/host.csproj")
                .WithAspireDashboardOtlpEndpoint("https://dash.example")
                .WithAspireResourceServiceEndpoint("https://res.example")
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(plan.StartInfo.EnvironmentVariables["DOTNET_DASHBOARD_OTLP_ENDPOINT_URL"], Is.EqualTo("https://dash.example"));
                Assert.That(plan.StartInfo.EnvironmentVariables["DOTNET_RESOURCE_SERVICE_ENDPOINT_URL"], Is.EqualTo("https://res.example"));
            });
        }

        [Test]
        public void WithAspNetCoreUrls_SetsValue()
        {
            var plan = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7005"), "/app/host.csproj")
                .WithAspNetCoreUrls("http://0.0.0.0:7005")
                .Build();

            Assert.That(plan.StartInfo.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo("http://0.0.0.0:7005"));
        }

        [Test]
        public void WithAspNetCoreForwardedHeaders_SetsBooleanString()
        {
            var planTrue = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7006"), "/app/host.csproj")
                .WithAspNetCoreForwardedHeaders(true)
                .Build();

            var planFalse = ServerConfiguration
                .CreateAspireServer(new Uri("http://localhost:7007"), "/app/host.csproj")
                .WithAspNetCoreForwardedHeaders(false)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(planTrue.StartInfo.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], Is.EqualTo("true"));
                Assert.That(planFalse.StartInfo.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], Is.EqualTo("false"));
            });
        }

        [Test]
        public void WithSpaProxy_Toggle_SetsHostingAssemblies_And_FrameworkConfigArgsPresent()
        {
            var planOn = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7008"), "/app/app.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Debug")
                .EnableSpaProxy(true)
                .Build();

            var planOff = ServerConfiguration
                .CreateDotNetServer(new Uri("http://localhost:7009"), "/app/app.csproj")
                .WithFramework("net8.0")
                .WithConfiguration("Debug")
                .EnableSpaProxy(false)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(planOn.StartInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo("Microsoft.AspNetCore.SpaProxy"));
                Assert.That(planOff.StartInfo.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo(""));
                Assert.That(planOn.StartInfo.Arguments, Does.Contain("--framework net8.0"));
                Assert.That(planOn.StartInfo.Arguments, Does.Contain("--configuration Debug"));
            });
        }
    }
}
