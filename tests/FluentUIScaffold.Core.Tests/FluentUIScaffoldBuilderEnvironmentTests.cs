using System;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Tests.Mocks;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    /// <summary>
    /// Tests for the unified environment configuration API on FluentUIScaffoldBuilder.
    /// </summary>
    [TestFixture]
    public class FluentUIScaffoldBuilderEnvironmentTests
    {
        #region Default Values

        [Test]
        public void Options_Default_EnvironmentName_IsTesting()
        {
            var options = new FluentUIScaffoldOptions();
            Assert.That(options.EnvironmentName, Is.EqualTo("Testing"));
        }

        [Test]
        public void Options_Default_SpaProxyEnabled_IsFalse()
        {
            var options = new FluentUIScaffoldOptions();
            Assert.That(options.SpaProxyEnabled, Is.False);
        }

        [Test]
        public void Options_Default_EnvironmentVariables_IsEmpty()
        {
            var options = new FluentUIScaffoldOptions();
            Assert.That(options.EnvironmentVariables, Is.Empty);
        }

        #endregion

        #region Builder Environment Methods

        [Test]
        public void WithEnvironmentName_SetsEnvironmentName()
        {
            var builder = new FluentUIScaffoldBuilder();

            var result = builder.WithEnvironmentName("Staging");
            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            Assert.That(result, Is.SameAs(builder));
            Assert.That(options.EnvironmentName, Is.EqualTo("Staging"));
        }

        [Test]
        public void WithEnvironmentName_NullOrEmpty_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentName(null!));
            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentName(""));
            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentName("   "));
        }

        [Test]
        public void WithSpaProxy_True_OverridesDefault()
        {
            var builder = new FluentUIScaffoldBuilder();

            var result = builder.WithSpaProxy(true);
            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            Assert.That(result, Is.SameAs(builder));
            Assert.That(options.SpaProxyEnabled, Is.True);
        }

        [Test]
        public void WithHeadlessMode_ExplicitValue_OverridesResolution()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.WithHeadlessMode(false);
            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            // Explicit false should not be overridden by Build()
            Assert.That(options.HeadlessMode, Is.False);
        }

        [Test]
        public void WithHeadlessMode_Null_ResolvedAtBuildTime()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.WithHeadlessMode(null);
            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            // Build() should resolve null to a concrete value
            Assert.That(options.HeadlessMode, Is.Not.Null);
        }

        [Test]
        public void WithEnvironmentVariable_AddsToOptions()
        {
            var builder = new FluentUIScaffoldBuilder();

            var result = builder.WithEnvironmentVariable("MY_VAR", "my_value");
            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            Assert.That(result, Is.SameAs(builder));
            Assert.That(options.EnvironmentVariables["MY_VAR"], Is.EqualTo("my_value"));
        }

        [Test]
        public void WithEnvironmentVariable_NullKey_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable(null!, "value"));
            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable("", "value"));
        }

        [Test]
        public void WithEnvironmentVariable_MultipleVars_AllAdded()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder
                .WithEnvironmentVariable("KEY1", "val1")
                .WithEnvironmentVariable("KEY2", "val2")
                .WithEnvironmentVariable("KEY3", "val3");

            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            Assert.That(options.EnvironmentVariables.Count, Is.EqualTo(3));
            Assert.That(options.EnvironmentVariables["KEY1"], Is.EqualTo("val1"));
            Assert.That(options.EnvironmentVariables["KEY2"], Is.EqualTo("val2"));
            Assert.That(options.EnvironmentVariables["KEY3"], Is.EqualTo("val3"));
        }

        [Test]
        public void WithEnvironmentVariable_OverwritesSameKey()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder
                .WithEnvironmentVariable("KEY", "first")
                .WithEnvironmentVariable("KEY", "second");

            var app = builder
                .UsePlugin(new MockPlugin())
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"))
                .Build<WebApp>();

            var options = app.GetService<FluentUIScaffoldOptions>();

            Assert.That(options.EnvironmentVariables["KEY"], Is.EqualTo("second"));
        }

        [Test]
        public void EnvironmentVariables_CaseInsensitiveKeys()
        {
            var options = new FluentUIScaffoldOptions();

            options.EnvironmentVariables["MyKey"] = "value1";

            Assert.That(options.EnvironmentVariables["mykey"], Is.EqualTo("value1"));
            Assert.That(options.EnvironmentVariables["MYKEY"], Is.EqualTo("value1"));
        }

        #endregion

        #region Single Hosting Strategy Guard

        [Test]
        public void DuplicateHostingStrategy_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.UseDotNetHosting(opts =>
            {
                opts.BaseUrl = new Uri("http://localhost:5000");
                opts.ProjectPath = "test.csproj";
            });

            Assert.Throws<InvalidOperationException>(() =>
                builder.UseDotNetHosting(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:5001");
                    opts.ProjectPath = "other.csproj";
                }));
        }

        [Test]
        public void DuplicateHostingStrategy_DotNetThenNode_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.UseDotNetHosting(opts =>
            {
                opts.BaseUrl = new Uri("http://localhost:5000");
                opts.ProjectPath = "test.csproj";
            });

            Assert.Throws<InvalidOperationException>(() =>
                builder.UseNodeHosting(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:3000");
                    opts.ProjectPath = ".";
                }));
        }

        [Test]
        public void DuplicateHostingStrategy_DotNetThenExternal_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.UseDotNetHosting(opts =>
            {
                opts.BaseUrl = new Uri("http://localhost:5000");
                opts.ProjectPath = "test.csproj";
            });

            Assert.Throws<InvalidOperationException>(() =>
                builder.UseExternalServer(new Uri("http://localhost:5000")));
        }

        [Test]
        public void SetHostingStrategyRegistered_CalledTwice_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            builder.SetHostingStrategyRegistered();

            Assert.Throws<InvalidOperationException>(() =>
                builder.SetHostingStrategyRegistered());
        }

        #endregion

        #region Hosting Options Validation

        [Test]
        public void UseDotNetHosting_MissingBaseUrl_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() =>
                builder.UseDotNetHosting(opts =>
                {
                    opts.ProjectPath = "test.csproj";
                    // BaseUrl not set
                }));
        }

        [Test]
        public void UseDotNetHosting_MissingProjectPath_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() =>
                builder.UseDotNetHosting(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:5000");
                    // ProjectPath not set
                }));
        }

        [Test]
        public void UseNodeHosting_MissingBaseUrl_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() =>
                builder.UseNodeHosting(opts =>
                {
                    opts.ProjectPath = ".";
                    // BaseUrl not set
                }));
        }

        [Test]
        public void UseNodeHosting_MissingProjectPath_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() =>
                builder.UseNodeHosting(opts =>
                {
                    opts.BaseUrl = new Uri("http://localhost:3000");
                    // ProjectPath not set
                }));
        }

        #endregion

        #region Security Guards

        [Test]
        public void WithEnvironmentName_Production_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.WithEnvironmentName("Production"));
        }

        [Test]
        public void WithEnvironmentName_ProductionCaseInsensitive_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<InvalidOperationException>(() => builder.WithEnvironmentName("production"));
            Assert.Throws<InvalidOperationException>(() => builder.WithEnvironmentName("PRODUCTION"));
            Assert.Throws<InvalidOperationException>(() => builder.WithEnvironmentName(" Production "));
        }

        [TestCase("LD_PRELOAD")]
        [TestCase("LD_LIBRARY_PATH")]
        [TestCase("DYLD_INSERT_LIBRARIES")]
        [TestCase("DYLD_LIBRARY_PATH")]
        [TestCase("DYLD_FRAMEWORK_PATH")]
        [TestCase("PATH")]
        [TestCase("COMSPEC")]
        public void WithEnvironmentVariable_DangerousKey_Throws(string dangerousKey)
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable(dangerousKey, "malicious_value"));
        }

        [Test]
        public void WithEnvironmentVariable_DangerousKey_CaseInsensitive_Throws()
        {
            var builder = new FluentUIScaffoldBuilder();

            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable("ld_preload", "/tmp/evil.so"));
            Assert.Throws<ArgumentException>(() => builder.WithEnvironmentVariable("Path", "/tmp"));
        }

        [Test]
        public void WithEnvironmentVariable_SafeKey_Succeeds()
        {
            var builder = new FluentUIScaffoldBuilder();

            // Should not throw
            builder.WithEnvironmentVariable("MY_CUSTOM_VAR", "value");
            builder.WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            builder.WithEnvironmentVariable("NODE_ENV", "test");
        }

        #endregion

        #region Options Identity

        [Test]
        public void Build_OptionsInstance_IsSameForAllConsumers()
        {
            // The options object set during builder configuration should be
            // the same instance resolved from DI
            var builder = new FluentUIScaffoldBuilder();

            builder
                .UsePlugin(new MockPlugin())
                .WithEnvironmentName("CustomEnv")
                .WithEnvironmentVariable("KEY", "VALUE")
                .Web<WebApp>(opts => opts.BaseUrl = new Uri("http://localhost"));

            var app = builder.Build<WebApp>();
            var options = app.GetService<FluentUIScaffoldOptions>();

            Assert.That(options.EnvironmentName, Is.EqualTo("CustomEnv"));
            Assert.That(options.EnvironmentVariables["KEY"], Is.EqualTo("VALUE"));
        }

        #endregion
    }
}
