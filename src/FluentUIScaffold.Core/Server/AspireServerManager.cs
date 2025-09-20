using System;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using Microsoft.Extensions.Logging;

namespace FluentUIScaffold.Core.Server
{
    public sealed class AspireServerManager : IServerManager
    {
        private readonly object _sync = new();
        private ServerStatus? _status;
        private LaunchPlan? _lastPlan;
        private readonly ProcessLauncher _launcher;
        private readonly IProcessRegistry _registry;
        private readonly IConfigHasher _hasher;

        public AspireServerManager(ProcessLauncher launcher, IProcessRegistry registry, IConfigHasher hasher)
        {
            _launcher = launcher;
            _registry = registry;
            _hasher = hasher;
        }

        public async Task<ServerStatus> EnsureStartedAsync(LaunchPlan plan, ILogger logger, CancellationToken ct)
        {
            var hash = _hasher.Compute(plan);
            var persisted = _registry.TryLoad(hash);

            if (persisted is { IsHealthy: true } && _registry.IsAlive(persisted.Pid))
            {
                _status = persisted;
                _lastPlan = plan;
                return persisted;
            }

            if (plan.KillOrphansOnStart && persisted?.Pid > 0)
            {
                _ = _registry.TryKill(persisted.Pid, logger);
                _registry.Delete(persisted.ConfigHash);
            }

            if (plan.AssetsBuild is not null)
            {
                await plan.AssetsBuild(ct);
            }

            await _launcher.StartAsync(plan, ct);

            // As ProcessLauncher does not expose PID, fall back to registry heuristic
            var proc = _registry.FindProcessFor(plan);
            if (proc == null || proc.HasExited)
            {
                throw new InvalidOperationException("Server process not found after launch.");
            }

            var status = new ServerStatus(proc.Id, DateTimeOffset.UtcNow, plan.BaseUrl, hash, true);
            _registry.Save(status);
            _status = status;
            _lastPlan = plan;
            return status;
        }

        public async Task RestartAsync(CancellationToken ct)
        {
            if (_lastPlan is null) return;
            await StopAsync(ct);
            var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
            await EnsureStartedAsync(_lastPlan, logger, ct);
        }

        public Task StopAsync(CancellationToken ct)
        {
            if (_status is null) return Task.CompletedTask;
            _registry.TryKill(_status.Pid, null);
            _registry.Delete(_status.ConfigHash);
            _status = null;
            return Task.CompletedTask;
        }

        public ServerStatus GetStatus() => _status ?? ServerStatus.None;
    }
}

