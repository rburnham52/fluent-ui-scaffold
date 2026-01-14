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
    /// Extension methods for configuring Aspire hosting with FluentUIScaffold.
    /// </summary>
    public static class AspireHostingExtensions
    {
        /// <summary>
        /// Configures hosting via Aspire's DistributedApplicationTestingBuilder.
        /// Delegates all lifecycle management to Aspire's testing infrastructure.
        /// </summary>
        /// <typeparam name="TEntryPoint">The Aspire AppHost entry point type.</typeparam>
        /// <param name="builder">The FluentUIScaffold builder.</param>
        /// <param name="configure">Action to configure the distributed application builder.</param>
        /// <param name="baseUrlResourceName">Optional resource name to extract the base URL from.</param>
        public static FluentUIScaffold.Core.Configuration.FluentUIScaffoldBuilder UseAspireHosting<TEntryPoint>(
            this FluentUIScaffold.Core.Configuration.FluentUIScaffoldBuilder builder,
            Action<IDistributedApplicationTestingBuilder> configure,
            string? baseUrlResourceName = null)
            where TEntryPoint : class
        {
            // Create the hosting strategy
            var hostingStrategy = new AspireHostingStrategy<TEntryPoint>(configure, baseUrlResourceName);

            // Register the hosting strategy and its types
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IHostingStrategy>(hostingStrategy);
                services.AddSingleton(hostingStrategy);
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
                var options = services.GetService<FluentUIScaffoldOptions>();
                if (options != null)
                {
                    options.BaseUrl = result.BaseUrl;
                }
            });

            return builder;
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
