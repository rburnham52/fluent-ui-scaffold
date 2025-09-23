using System;
using System.Net.Http;

using Aspire.Hosting;
using Aspire.Hosting.Testing;

using FluentUIScaffold.Core;

using Microsoft.Extensions.DependencyInjection;

namespace FluentUIScaffold.AspireHosting
{
    public static class AspireResourceExtensions
    {
        /// <summary>
        /// Creates an HttpClient for the specified resource name from the underlying DistributedApplication.
        /// </summary>
        public static HttpClient CreateHttpClient(this AppScaffold<object> app, string resourceName)
        {
            // Note: The generic type <object> is used because extension methods on generic types are tricky if we want them to apply to all TWebApp.
            // We can cast or use a non-generic interface for AppScaffold if we had one.
            // For now, let's make this an extension on AppScaffold<T> by being generic itself.
            throw new NotImplementedException("Use the overload that accepts AppScaffold<T>");
        }

        public static HttpClient CreateHttpClient<T>(this AppScaffold<T> app, string resourceName)
        {
            var distributedApp = app.ServiceProvider.GetRequiredService<DistributedApplication>();
            return distributedApp.CreateHttpClient(resourceName);
        }
    }
}
