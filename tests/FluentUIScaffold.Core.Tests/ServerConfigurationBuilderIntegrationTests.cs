using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Configuration.Launchers;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class ServerConfigurationBuilderIntegrationTests
    {
        private static string InvokeAspNetServerBuildArgs(ServerConfiguration config)
        {
            var launcherType = typeof(FluentUIScaffold.Core.Configuration.Launchers.AspNetServerLauncher);
            var method = launcherType.GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method, "Could not reflect BuildCommandArguments on AspNetServerLauncher");
            var result = method!.Invoke(null, new object[] { config });
            return (string)result!;
        }

        private static void InvokeAspNetServerSetEnv(ProcessStartInfo startInfo, ServerConfiguration config)
        {
            var launcherType = typeof(FluentUIScaffold.Core.Configuration.Launchers.AspNetServerLauncher);
            var method = launcherType.GetMethod("SetServerSpecificEnvironmentVariables", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method, "Could not reflect SetServerSpecificEnvironmentVariables on AspNetServerLauncher");
            method!.Invoke(null, new object[] { startInfo, config });
        }

        private static void LogSnapshot(string title, string builderSnippet, string command, IDictionary<string, string> env)
        {
            TestContext.WriteLine($"=== {title} ===");
            TestContext.WriteLine("Builder methods:");
            TestContext.WriteLine(builderSnippet);
            TestContext.WriteLine("Run command:");
            TestContext.WriteLine($"dotnet {command}");
            TestContext.WriteLine("Environment variables:");
            foreach (var kv in env.OrderBy(k => k.Key))
            {
                TestContext.WriteLine($"{kv.Key}={kv.Value}");
            }
            TestContext.WriteLine("====================\n");
        }

        [Test]
        public void DotNet_WithFrameworkAndConfiguration_ProducesExpectedRunCommand()
        {
            var baseUrl = new Uri("http://localhost:5055");
            var projectPath = "/path/to/MyApp.csproj";

            var builderSnippet = @"ServerConfiguration
    .CreateDotNetServer(new Uri(\"http://localhost:5055\"), \"/path/to/MyApp.csproj\")
    .WithFramework(\"net9.0\")
    .WithConfiguration(\"Debug\")";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithFramework("net9.0")
                .WithConfiguration("Debug")
                .Build();

            var args = InvokeAspNetServerBuildArgs(config);

            LogSnapshot("ASP.NET Core (dotnet run)", builderSnippet, args, config.EnvironmentVariables);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.StartWith("run "));
                Assert.That(args, Does.Contain("--framework net9.0"));
                Assert.That(args, Does.Contain("--configuration Debug"));
                Assert.That(args, Does.Not.Contain("--urls"), "URL should be provided via ASPNETCORE_URLS env var, not command arg");
                Assert.That(args, Does.Contain("--no-launch-profile"));
            });
        }

        [Test]
        public void Aspire_WithFrameworkAndDefaults_ProducesExpectedRunCommandAndEnv()
        {
            var baseUrl = new Uri("http://localhost:6066");
            var projectPath = "/path/to/AspireApp.AppHost.csproj";

            var builderSnippet = @"ServerConfiguration
    .CreateAspireServer(new Uri(\"http://localhost:6066\"), \"/path/to/AspireApp.AppHost.csproj\")
    .WithFramework(\"net9.0\")
    .WithConfiguration(\"Release\")";

            var config = ServerConfiguration
                .CreateAspireServer(baseUrl, projectPath)
                .WithFramework("net9.0")
                .WithConfiguration("Release")
                .WithAspireDashboardOtlpEndpoint("https://localhost:21097")
                .WithAspireResourceServiceEndpoint("https://localhost:22268")
                .Build();

            var args = InvokeAspNetServerBuildArgs(config);

            LogSnapshot("Aspire (dotnet run)", builderSnippet, args, config.EnvironmentVariables);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.Contain("--framework net9.0"));
                Assert.That(args, Does.Contain("--configuration Release"));
                Assert.That(args, Does.Contain("--no-launch-profile"));
            });

            // Verify env assembled by builder defaults
            Assert.Multiple(() =>
            {
                Assert.That(config.EnvironmentVariables["DOTNET_DASHBOARD_OTLP_ENDPOINT_URL"], Is.EqualTo("https://localhost:21097"));
                Assert.That(config.EnvironmentVariables["DOTNET_RESOURCE_SERVICE_ENDPOINT_URL"], Is.EqualTo("https://localhost:22268"));
                Assert.That(config.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Development"));
                Assert.That(config.EnvironmentVariables["DOTNET_ENVIRONMENT"], Is.EqualTo("Development"));
                Assert.That(config.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
            });
        }

        [Test]
        public void SpaProxy_Toggle_SetsHostingStartupAssembliesEnv()
        {
            var baseUrl = new Uri("http://localhost:7007");
            var projectPath = "/path/to/MySpaApp.csproj";

            var configWithSpa = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithSpaProxy(true)
                .Build();

            LogSnapshot("SPA proxy enabled", ".WithSpaProxy(true)", InvokeAspNetServerBuildArgs(configWithSpa), configWithSpa.EnvironmentVariables);

            Assert.That(configWithSpa.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo("Microsoft.AspNetCore.SpaProxy"));

            var configWithoutSpa = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithSpaProxy(false)
                .Build();

            LogSnapshot("SPA proxy disabled", ".WithSpaProxy(false)", InvokeAspNetServerBuildArgs(configWithoutSpa), configWithoutSpa.EnvironmentVariables);

            Assert.That(configWithoutSpa.EnvironmentVariables["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"], Is.EqualTo(string.Empty));
        }

        [Test]
        public void AspNetCore_ServerSpecificUrlsInjectedIntoStartInfo()
        {
            var baseUrl = new Uri("http://localhost:8080");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .Build();

            var startInfo = new ProcessStartInfo();
            InvokeAspNetServerSetEnv(startInfo, config);

            Assert.That(startInfo.EnvironmentVariables["ASPNETCORE_URLS"], Is.EqualTo(baseUrl.ToString()));
        }

        [Test]
        public void CustomEnvironmentVariables_ArePreserved()
        {
            var baseUrl = new Uri("http://localhost:9091");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithEnvironmentVariable("MY_SETTING", "abc")
                .WithEnvironmentVariables(new Dictionary<string, string>
                {
                    ["FOO"] = "bar",
                    ["ASPNETCORE_ENVIRONMENT"] = "Staging"
                })
                .Build();

            LogSnapshot("Custom env vars", ".WithEnvironmentVariable(\"MY_SETTING\", \"abc\").WithEnvironmentVariables(...) ", InvokeAspNetServerBuildArgs(config), config.EnvironmentVariables);

            Assert.Multiple(() =>
            {
                Assert.That(config.EnvironmentVariables["MY_SETTING"], Is.EqualTo("abc"));
                Assert.That(config.EnvironmentVariables["FOO"], Is.EqualTo("bar"));
                Assert.That(config.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Staging"));
            });
        }

        [Test]
        public void NodeJs_WithScriptAndEnv_ProducesExpectedCommandAndEnv()
        {
            var baseUrl = new Uri("http://localhost:5123");
            var projectPath = "/path/to/package.json";

            var config = ServerConfiguration
                .CreateNodeJsServer(baseUrl, projectPath)
                .WithNpmScript("dev")
                .WithNodeEnvironment("production")
                .WithPort(7777)
                .WithArguments("--", "--open")
                .Build();

            // Build args via reflection
            var launcherType = typeof(FluentUIScaffold.Core.Configuration.Launchers.NodeJsServerLauncher);
            var method = launcherType.GetMethod("BuildCommandArguments", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method, "Could not reflect BuildCommandArguments on NodeJsServerLauncher");
            var args = (string)method!.Invoke(null, new object[] { config })!;

            TestContext.WriteLine("=== Node.js (npm run) ===");
            TestContext.WriteLine("Builder methods: WithNpmScript(\"dev\").WithNodeEnvironment(\"production\").WithPort(7777).WithArguments(\"--\", \"--open\")");
            TestContext.WriteLine($"npm {args}");
            foreach (var kv in config.EnvironmentVariables.OrderBy(k => k.Key))
            {
                TestContext.WriteLine($"{kv.Key}={kv.Value}");
            }
            TestContext.WriteLine("====================\n");

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.StartWith("start").Or.StartWith("dev"));
                Assert.That(args, Does.Contain("-- --open"));
                Assert.That(config.EnvironmentVariables["NODE_ENV"], Is.EqualTo("production"));
                Assert.That(config.EnvironmentVariables["PORT"], Is.EqualTo("7777"));
            });
        }

        [Test]
        public void DotNet_EnvironmentSetters_AreApplied()
        {
            var baseUrl = new Uri("http://localhost:7333");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithAspNetCoreEnvironment("Staging")
                .WithDotNetEnvironment("Production")
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(config.EnvironmentVariables["ASPNETCORE_ENVIRONMENT"], Is.EqualTo("Staging"));
                Assert.That(config.EnvironmentVariables["DOTNET_ENVIRONMENT"], Is.EqualTo("Production"));
            });
        }

        [Test]
        public void DotNet_ForwardedHeadersFlag_SerializesAsLowercaseBoolean()
        {
            var baseUrl = new Uri("http://localhost:7444");
            var projectPath = "/path/to/MyApp.csproj";

            var configTrue = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithAspNetCoreForwardedHeaders(true)
                .Build();

            var configFalse = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithAspNetCoreForwardedHeaders(false)
                .Build();

            Assert.Multiple(() =>
            {
                Assert.That(configTrue.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], Is.EqualTo("true"));
                Assert.That(configFalse.EnvironmentVariables["ASPNETCORE_FORWARDEDHEADERS_ENABLED"], Is.EqualTo("false"));
            });
        }

        [Test]
        public void DotNet_WorkingDirectory_IsApplied()
        {
            var baseUrl = new Uri("http://localhost:7555");
            var projectPath = "/root/app/MyApp/MyApp.csproj";
            var customWorkingDir = "/custom/dir";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithWorkingDirectory(customWorkingDir)
                .Build();

            Assert.That(config.WorkingDirectory, Is.EqualTo(customWorkingDir));
        }

        [Test]
        public void DotNet_CustomArguments_AreAppended()
        {
            var baseUrl = new Uri("http://localhost:7666");
            var projectPath = "/path/to/MyApp.csproj";

            var config = ServerConfiguration
                .CreateDotNetServer(baseUrl, projectPath)
                .WithFramework("net8.0")
                .WithConfiguration("Release")
                .WithArguments("--verbosity", "minimal")
                .Build();

            var args = InvokeAspNetServerBuildArgs(config);

            Assert.That(args, Does.Contain("--verbosity minimal"));
        }

        [Test]
        public void WebApplicationFactory_FrameworkAndConfiguration_ProducesExpectedRunCommand()
        {
            var baseUrl = new Uri("http://localhost:8888");
            var projectPath = "/path/to/MyApp.csproj";

            var config = new DotNetServerConfigurationBuilder(ServerType.WebApplicationFactory, baseUrl, projectPath)
                .WithFramework("net9.0")
                .WithConfiguration("Debug")
                .Build();

            var args = InvokeAspNetServerBuildArgs(config);

            LogSnapshot("WebApplicationFactory (fallback to dotnet run)", ".WithFramework(\"net9.0\").WithConfiguration(\"Debug\")", args, config.EnvironmentVariables);

            Assert.Multiple(() =>
            {
                Assert.That(args, Does.Contain("--framework net9.0"));
                Assert.That(args, Does.Contain("--configuration Debug"));
                Assert.That(args, Does.Contain("--no-launch-profile"));
            });
        }

        [Test]
        public void WebApplicationFactory_GetLauncher_SelectsCorrectLauncher()
        {
            var baseUrl = new Uri("http://localhost:9999");
            var projectPath = "/path/to/MyApp.csproj";

            var factory = new ServerLauncherFactory();
            factory.RegisterLauncher(new AspNetServerLauncher());
            factory.RegisterLauncher(new NodeJsServerLauncher());
            factory.RegisterLauncher(new WebApplicationFactoryServerLauncher());

            var config = new DotNetServerConfigurationBuilder(ServerType.WebApplicationFactory, baseUrl, projectPath).Build();

            var launcher = factory.GetLauncher(config);

            Assert.That(launcher, Is.InstanceOf<WebApplicationFactoryServerLauncher>());
            Assert.That(launcher.CanHandle(config), Is.True);
        }
    }
}