using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration;
using FluentUIScaffold.Core.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.Core.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IUITestingPlugin for unit testing purposes.
    /// </summary>
    public sealed class MockPlugin : IUITestingPlugin
    {
        private bool _initialized;

        public bool IsInitialized => _initialized;

        public void ConfigureServices(IServiceCollection services)
        {
            // No additional services needed for mock
        }

        public Task InitializeAsync(FluentUIScaffoldOptions options, CancellationToken cancellationToken = default)
        {
            _initialized = true;
            return Task.CompletedTask;
        }

        public Task<IBrowserSession> CreateSessionAsync(IServiceProvider rootProvider)
        {
            return Task.FromResult<IBrowserSession>(new MockBrowserSession(rootProvider));
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }

    /// <summary>
    /// Mock implementation of IBrowserSession for unit testing purposes.
    /// </summary>
    public sealed class MockBrowserSession : IBrowserSession
    {
        private readonly IServiceProvider _serviceProvider;

        public MockBrowserSession()
        {
            _serviceProvider = new ServiceCollection().BuildServiceProvider();
        }

        public MockBrowserSession(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider => _serviceProvider;

        public Task NavigateToUrlAsync(Uri url)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }

    /// <summary>
    /// Marker type for tests that need a generic web app type parameter.
    /// </summary>
    public class WebApp { }
}
