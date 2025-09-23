using System;

namespace FluentUIScaffold.Core.Hosting
{
    /// <summary>
    /// Result of starting a hosted application.
    /// </summary>
    /// <param name="BaseUrl">The base URL where the application is accessible.</param>
    /// <param name="WasReused">True if an existing server was reused instead of starting a new one.</param>
    public sealed record HostingResult(Uri BaseUrl, bool WasReused);
}
