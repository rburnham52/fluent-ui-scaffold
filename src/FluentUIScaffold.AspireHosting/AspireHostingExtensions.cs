using System;
using System.Collections.Generic;

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
        public TimeSpan? AspireStartupTimeout { get; set; }
        public bool HttpOnlyMode { get; set; }
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
                    var strategy = new AspireHostingStrategy<TEntryPoint>(configure, scaffoldOptions, baseUrlResourceName)
                    {
                        SkipDockerPreflightCheck = aspireConfig.SkipDockerPreflightCheck,
                    };
                    if (aspireConfig.AspireStartupTimeout.HasValue)
                    {
                        strategy.AspireStartupTimeout = aspireConfig.AspireStartupTimeout.Value;
                    }
                    return strategy;
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
        /// Bounds the time spent waiting for the Aspire AppHost to finish startup.
        /// When the timeout fires, a <see cref="TimeoutException"/> is thrown instead
        /// of hanging indefinitely. Default is 90 seconds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// During the wait, FluentUIScaffold emits an <c>ILogger.LogInformation</c> heartbeat
        /// line every 10 seconds (<c>"Aspire host starting... ({elapsed}s elapsed)"</c>) so
        /// you can tell whether startup is making progress.
        /// </para>
        /// </remarks>
        public static FluentUIScaffoldBuilder WithAspireStartupTimeout(this FluentUIScaffoldBuilder builder, TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(timeout), "Startup timeout must be positive.");
            GetOrCreateConfig(builder).AspireStartupTimeout = timeout;
            return builder;
        }

        /// <summary>
        /// Forces all ASP.NET Core resources hosted by Aspire to bind HTTP only and disables
        /// <c>UseHttpsRedirection</c>'s port resolution. Opt-in; disabled by default.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>The footgun this avoids:</b> many ASP.NET Core templates ship with
        /// <c>app.UseHttpsRedirection()</c> enabled, which is fine in Development but in
        /// other environments (including <c>Testing</c>) issues a <c>307 Temporary Redirect</c>
        /// to the absolute HTTPS upstream URL, e.g. <c>https://localhost:7039/api/...</c>.
        /// When the API sits behind a YARP reverse proxy serving an SPA, the browser sees
        /// the absolute redirect and the SPA's CSP <c>connect-src 'self'</c> blocks the
        /// follow-up request, producing a cryptic <c>"Failed to fetch"</c> in every test.
        /// </para>
        /// <para>
        /// Enabling HTTP-only mode injects:
        /// <list type="bullet">
        ///   <item><c>ASPNETCORE_URLS=http://+:0</c> — Aspire picks the port; no HTTPS endpoint is advertised.</item>
        ///   <item><c>ASPNETCORE_HTTPS_PORT=</c> (empty) — disables the <c>UseHttpsRedirection</c> middleware's port resolution.</item>
        /// </list>
        /// These env vars are propagated to every Aspire-hosted process via FluentUIScaffold's
        /// existing environment-variable bag.
        /// </para>
        /// <para>
        /// Off by default because it changes wire-protocol expectations — opt in only when
        /// your tests genuinely run over HTTP.
        /// </para>
        /// </remarks>
        /// <param name="builder">The FluentUIScaffold builder.</param>
        /// <param name="enabled">When true (default), enables HTTP-only mode.</param>
        public static FluentUIScaffoldBuilder WithHttpOnlyMode(this FluentUIScaffoldBuilder builder, bool enabled = true)
        {
            GetOrCreateConfig(builder).HttpOnlyMode = enabled;
            if (enabled)
            {
                ApplyHttpOnlyModeEnvVars(builder);
            }
            return builder;
        }

        /// <summary>
        /// Names of the env vars HTTP-only mode sets. Exposed for tests.
        /// </summary>
        internal static readonly IReadOnlyDictionary<string, string> HttpOnlyEnvVars
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ASPNETCORE_URLS"] = "http://+:0",
                ["ASPNETCORE_HTTPS_PORT"] = string.Empty,
            };

        private static void ApplyHttpOnlyModeEnvVars(FluentUIScaffoldBuilder builder)
        {
            // Use WithEnvironmentVariable so the same key-validation + storage path is exercised.
            foreach (var kv in HttpOnlyEnvVars)
            {
                builder.WithEnvironmentVariable(kv.Key, kv.Value);
            }
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
