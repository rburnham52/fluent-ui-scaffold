using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FluentUIScaffold.Core.Server
{
    public interface IProcessRegistry
    {
        ServerStatus? TryLoad(string configHash);
        void Save(ServerStatus status);
        void Delete(string configHash);
        bool IsAlive(int pid);
        bool TryKill(int pid, Microsoft.Extensions.Logging.ILogger? logger);
        Process? FindProcessFor(Configuration.Launchers.LaunchPlan plan);
        string GetStatePath(string configHash);
    }

    internal sealed class ProcessRegistry : IProcessRegistry
    {
        private sealed record RegistryState(int Pid, DateTimeOffset StartTime, string BaseUrl, string ConfigHash);

        public ServerStatus? TryLoad(string configHash)
        {
            try
            {
                var path = GetStatePath(configHash);
                if (!File.Exists(path)) return null;
                var json = File.ReadAllText(path);
                var state = JsonSerializer.Deserialize<RegistryState>(json);
                if (state == null) return null;
                return new ServerStatus(state.Pid, state.StartTime, new Uri(state.BaseUrl), state.ConfigHash, IsAlive(state.Pid));
            }
            catch
            {
                return null;
            }
        }

        public void Save(ServerStatus status)
        {
            var path = GetStatePath(status.ConfigHash);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var state = new RegistryState(status.Pid, status.StartTime, status.BaseUrl.ToString(), status.ConfigHash);
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public void Delete(string configHash)
        {
            try
            {
                var path = GetStatePath(configHash);
                if (File.Exists(path)) File.Delete(path);
            }
            catch { }
        }

        public bool IsAlive(int pid)
        {
            try
            {
                var proc = Process.GetProcessById(pid);
                return !proc.HasExited;
            }
            catch { return false; }
        }

        public bool TryKill(int pid, Microsoft.Extensions.Logging.ILogger? logger)
        {
            try
            {
                var proc = Process.GetProcessById(pid);
                if (proc.HasExited) return true;
                logger?.LogInformation("Killing stale process PID {Pid}", pid);
                proc.Kill(true);
                proc.WaitForExit(5000);
                return true;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "Failed to kill PID {Pid}", pid);
                return false;
            }
        }

        public Process? FindProcessFor(Configuration.Launchers.LaunchPlan plan)
        {
            try
            {
                if (plan.StartInfo == null) return null;
                // Heuristic: find child process by executable name
                var name = Path.GetFileNameWithoutExtension(plan.StartInfo.FileName);
                var procs = Process.GetProcessesByName(name);
                // Best-effort; the actual PID should be captured in ProcessLauncher if exposed
                return procs.Length > 0 ? procs[0] : null;
            }
            catch { return null; }
        }

        public string GetStatePath(string configHash)
        {
            var root = GetRoot();
            return Path.Combine(root, "servers", configHash, "state.json");
        }

        private static string GetRoot()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, "FluentUIScaffold");
            }
            var xdg = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
            if (!string.IsNullOrWhiteSpace(xdg)) return Path.Combine(xdg, "fluentuiscaffold");
            return "/tmp/fluentuiscaffold";
        }
    }
}

