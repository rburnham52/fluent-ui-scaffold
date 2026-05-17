using System;

using Aspire.Hosting;
using Aspire.Hosting.Testing;

using FluentUIScaffold.Core;
using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.AspireHosting
{
    /// <summary>
    /// Configuration bag for Aspire-specific options that the builder extensions need to
    /// propagate to the strategy at DI-resolution time.
    /// </summary>
    internal sealed class AspireHostingConfiguration
    {
        public bool SkipDockerPreflightCheck { get; set; }
    }

    /// <summary>
    /// Extension methods for configuring Aspire hosting with FluentUIScaffold.
    /// </summary>
    public static class AspireHostingExtensions
    {
        // Per-builder configuration store. ConditionalWeakTable lets us attach Aspire-specific
        // settings to the builder without modifying Core's public API surface.
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FluentUIScaffoldBuilder, AspireHostingConfiguration> _configs
            = new();

        internal static AspireHostingConfiguration GetOrCreateConfig(FluentUIScaffoldBuilder builder)
        {
            return _configs.GetValue(builder, _ => new AspireHostingConfiguration());
        }

        /// <summary>
        /// Configures hosting via Aspire's DistributedApplicationTestingBuilder.
        /// Delegates all lifecycle management to Aspire's testing infrastructure.
        /// Environment variables from FluentUIScaffoldOptions are applied as process-level
        /// env vars before CreateAsync, since Aspire reads them from the test process.
        /// </summary>
        /// <typeparam name="TEntryPoint">The Aspire AppHost entry point type.</typeparam>
        /// <param name="builder">The FluentUIScaffold builder.</param>
        /// <param name="configure">Action to configure the distributed application builder.</param>
        /// <param name="baseUrlResourceName">Optional resource name to extract the base URL from.</param>
        /// <param name="baseUrlPrefix">Optional prefix to append to the discovered base URL (e.g., "/#" for hash-based SPA routing, "/app" for a common base path).</param>
        public static FluentUIScaffold.Core.Configuration.FluentUIScaffoldBuilder UseAspireHosting<TEntryPoint>(
            this FluentUIScaffold.Core.Configuration.FluentUIScaffoldBuilder builder,
            Action<IDistributedApplicationTestingBuilder> configure,
            string? baseUrlResourceName = null,
            string? baseUrlPrefix = null)
            where TEntryPoint : class
        {
            // Enforce single-strategy guard (same as DotNet/Node paths)
            builder.SetHostingStrategyRegistered();

            var aspireConfig = GetOrCreateConfig(builder);

            // Register the hosting strategy via factory delegate so it receives the final options
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<AspireHostingStrategy<TEntryPoint>>(sp =>
                {
                    var scaffoldOptions = sp.GetRequiredService<FluentUIScaffoldOptions>();
                    return new AspireHostingStrategy<TEntryPoint>(configure, scaffoldOptions, baseUrlResourceName)
                    {
                        SkipDockerPreflightCheck = aspireConfig.SkipDockerPreflightCheck,
                    };
                });
                services.AddSingleton<IHostingStrategy>(sp =>
                    sp.GetRequiredService<AspireHostingStrategy<TEntryPoint>>());
                services.AddSingleton<DistributedApplicationHolder>();
                services.AddTransient<DistributedApplication>(sp =>
                {
                    var strategy = sp.GetRequiredService<AspireHostingStrategy<TEntryPoint>>();
                    return strategy.Application
                        ?? throw new InvalidOperationException("DistributedApplication not started yet. Call StartAsync() on AppScaffold.");
                });
            });

            // Add startup action to start the hosting strategy
            builder.AddStartupAction(async (services) =>
            {
                var logger = services.GetRequiredService<ILogger<AspireHostingStrategy<TEntryPoint>>>();
                var strategy = services.GetRequiredService<AspireHostingStrategy<TEntryPoint>>();

                var result = await strategy.StartAsync(logger);

                // Update the holder for backward compatibility
                var holder = services.GetRequiredService<DistributedApplicationHolder>();
                holder.Instance = strategy.Application;

                // Update options with discovered base URL
                var options = services.GetRequiredService<FluentUIScaffoldOptions>();
                options.BaseUrl = ApplyBaseUrlPrefix(result.BaseUrl, baseUrlPrefix);
            });

            return builder;
        }

        /// <summary>
        /// Opts out of the Docker daemon pre-flight check that <c>UseAspireHosting</c>
        /// performs by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, <c>UseAspireHosting</c> runs <c>docker info</c> with a 2-second
        /// timeout before booting the Aspire AppHost. If the daemon is unreachable it
        /// throws a single-line <see cref="InvalidOperationException"/> instead of letting
        /// Aspire hang and bury the failure ~20 stack frames deep behind
        /// <c>DistributedApplicationFactory → DcpHost → DcpDependencyCheck</c>.
        /// </para>
        /// <para>
        /// Call this when you run against a remote container runtime where the local
        /// <c>docker</c> CLI is not installed but Aspire still works (e.g., DOCKER_HOST
        /// points at a remote daemon).
        /// </para>
        /// </remarks>
        public static FluentUIScaffoldBuilder SkipDockerPreflightCheck(this FluentUIScaffoldBuilder builder)
        {
            GetOrCreateConfig(builder).SkipDockerPreflightCheck = true;
            return builder;
        }

        /// <summary>
        /// Applies a prefix to the base URL if specified.
        /// </summary>
        internal static Uri? ApplyBaseUrlPrefix(Uri? baseUrl, string? baseUrlPrefix)
        {
            if (baseUrl == null || string.IsNullOrEmpty(baseUrlPrefix))
            {
                return baseUrl;
            }

            var baseUrlString = baseUrl.ToString().TrimEnd('/');
            var prefix = baseUrlPrefix.StartsWith("/") ? baseUrlPrefix : "/" + baseUrlPrefix;
            return new Uri(baseUrlString + prefix);
        }
    }

    /// <summary>
    /// Holds the DistributedApplication instance for backward compatibility and DI access.
    /// </summary>
    public class DistributedApplicationHolder
    {
        /// <summary>
        /// The current DistributedApplication instance.
        /// </summary>
        public DistributedApplication? Instance { get; set; }
    }
}
