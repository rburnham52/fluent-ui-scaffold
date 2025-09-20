using System;

namespace FluentUIScaffold.Core.Server
{
    public sealed record ServerStatus(
        int Pid,
        DateTimeOffset StartTime,
        Uri BaseUrl,
        string ConfigHash,
        bool IsHealthy
    )
    {
        public static readonly ServerStatus None = new(0, DateTimeOffset.MinValue, new Uri("http://localhost"), string.Empty, false);
    }
}

