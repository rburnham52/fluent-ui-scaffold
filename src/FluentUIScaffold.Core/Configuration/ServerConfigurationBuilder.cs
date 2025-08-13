using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FluentUIScaffold.Core.Configuration.Launchers
{
    public class ServerProcessBuilder<TSelf> where TSelf : ServerProcessBuilder<TSelf>
    {
        private readonly Dictionary<string, string> _environment = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string?> _namedArguments = new(StringComparer.Ordinal);
        private readonly List<string> _positionalArguments = new();
        private readonly List<string> _healthEndpoints = new();
        private string _executable = string.Empty;
        private string? _workingDirectory;
        private string? _processName;
        private Uri? _baseUrl;
        private TimeSpan _startupTimeout = TimeSpan.FromSeconds(60);
        private IReadinessProbe _readinessProbe = new HttpReadinessProbe();
        private TimeSpan _initialDelay = TimeSpan.FromSeconds(2);
        private TimeSpan _pollInterval = TimeSpan.FromMilliseconds(200);
        private bool _streamOutput = true;

        protected TSelf This => (TSelf)this;

        public TSelf WithBaseUrl(Uri baseUrl) { _baseUrl = baseUrl; return This; }
        public TSelf WithProjectPath(string projectPath)
        {
            var dir = System.IO.Path.GetDirectoryName(projectPath);
            if (!string.IsNullOrWhiteSpace(dir) && System.IO.Directory.Exists(dir))
            {
                _workingDirectory ??= dir;
            }
            return This;
        }
        public TSelf WithWorkingDirectory(string workingDirectory) { _workingDirectory = workingDirectory; return This; }
        public TSelf WithProcessName(string processName) { _processName = processName; return This; }
        public TSelf WithExecutable(string executable) { _executable = executable; return This; }
        public TSelf WithArgument(string name, string? value = null) { _namedArguments[name] = value; return This; }
        public TSelf WithArguments(params string[] args) { _positionalArguments.AddRange(args); return This; }
        public TSelf WithEnvironmentVariable(string key, string value) { _environment[key] = value; return This; }
        public TSelf WithEnvironmentVariables(IDictionary<string, string> env) { foreach (var kv in env) _environment[kv.Key] = kv.Value; return This; }
        public TSelf WithStartupTimeout(TimeSpan timeout) { _startupTimeout = timeout; return This; }
        public TSelf WithHealthCheckEndpoints(params string[] endpoints) { _healthEndpoints.Clear(); _healthEndpoints.AddRange(endpoints); return This; }
        public TSelf WithReadiness(IReadinessProbe probe, TimeSpan? initialDelay = null, TimeSpan? pollInterval = null)
        {
            _readinessProbe = probe ?? _readinessProbe;
            if (initialDelay.HasValue) _initialDelay = initialDelay.Value;
            if (pollInterval.HasValue) _pollInterval = pollInterval.Value;
            return This;
        }
        public TSelf WithProcessOutputLogging(bool enabled = true) { _streamOutput = enabled; return This; }

        public LaunchPlan Build()
        {
            if (_baseUrl == null) throw new InvalidOperationException("BaseUrl must be provided");
            if (string.IsNullOrWhiteSpace(_executable)) throw new InvalidOperationException("Executable must be provided");

            var argsList = new List<string>();
            argsList.AddRange(_positionalArguments);
            foreach (var kv in _namedArguments.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                argsList.Add(kv.Key);
                if (!string.IsNullOrEmpty(kv.Value)) argsList.Add(kv.Value);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = _executable,
                Arguments = string.Join(" ", argsList),
                // If WorkingDirectory is null/empty, let ProcessStartInfo default to current directory.
                WorkingDirectory = string.IsNullOrWhiteSpace(_workingDirectory) ? Environment.CurrentDirectory : _workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            foreach (var kv in _environment)
            {
                startInfo.EnvironmentVariables[kv.Key] = kv.Value;
            }

            var endpoints = _healthEndpoints.Count > 0 ? _healthEndpoints : new List<string> { "/" };
            return new LaunchPlan(startInfo, _baseUrl, _startupTimeout, _readinessProbe, endpoints, _initialDelay, _pollInterval, _streamOutput);
        }
    }

    public class DotNetServerConfigurationBuilder : ServerProcessBuilder<DotNetServerConfigurationBuilder>
    {
        public DotNetServerConfigurationBuilder()
        {
            WithExecutable("dotnet");
            WithArguments("run");
            WithArgument("--no-launch-profile");
            WithFramework("net8.0");
            WithConfiguration("Release");
        }

        public DotNetServerConfigurationBuilder WithFramework(string tfm)
        {
            return WithArgument("--framework", tfm);
        }

        public DotNetServerConfigurationBuilder WithConfiguration(string config)
        {
            return WithArgument("--configuration", config);
        }

        public DotNetServerConfigurationBuilder WithAspNetCoreUrls(string urls)
        {
            WithEnvironmentVariable("ASPNETCORE_URLS", urls);
            return this;
        }

        public DotNetServerConfigurationBuilder WithAspNetCoreEnvironment(string env = "Development")
        {
            WithEnvironmentVariable("ASPNETCORE_ENVIRONMENT", env);
            return this;
        }

        public DotNetServerConfigurationBuilder WithDotNetEnvironment(string env = "Development")
        {
            WithEnvironmentVariable("DOTNET_ENVIRONMENT", env);
            return this;
        }

        public DotNetServerConfigurationBuilder EnableSpaProxy(bool enabled = true)
        {
            WithEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", enabled ? "Microsoft.AspNetCore.SpaProxy" : "");
            return this;
        }

        public DotNetServerConfigurationBuilder WithAspNetCoreForwardedHeaders(bool enabled)
        {
            WithEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED", enabled ? "true" : "false");
            return this;
        }
    }

    public class AspireServerConfigurationBuilder : DotNetServerConfigurationBuilder
    {
        public AspireServerConfigurationBuilder(Uri baseUrl)
        {
            WithAspNetCoreUrls(baseUrl.ToString());
            WithStartupTimeout(TimeSpan.FromSeconds(90));
            WithHealthCheckEndpoints("/", "/health");
        }

        public AspireServerConfigurationBuilder WithAspireDashboardOtlpEndpoint(string url)
        {
            WithEnvironmentVariable("DOTNET_DASHBOARD_OTLP_ENDPOINT_URL", url);
            return this;
        }

        public AspireServerConfigurationBuilder WithAspireResourceServiceEndpoint(string url)
        {
            WithEnvironmentVariable("DOTNET_RESOURCE_SERVICE_ENDPOINT_URL", url);
            return this;
        }
    }

    public class NodeJsServerConfigurationBuilder : ServerProcessBuilder<NodeJsServerConfigurationBuilder>
    {
        public NodeJsServerConfigurationBuilder(Uri baseUrl)
        {
            WithExecutable("npm");
            WithArguments("run", "start");
            WithEnvironmentVariable("PORT", baseUrl.Port.ToString());
            WithEnvironmentVariable("NODE_ENV", "development");
        }

        public NodeJsServerConfigurationBuilder WithNpmScript(string script = "start")
        {
            return WithArguments("run", script);
        }

        public NodeJsServerConfigurationBuilder WithNodeEnvironment(string env = "development")
        {
            WithEnvironmentVariable("NODE_ENV", env);
            return this;
        }

        public NodeJsServerConfigurationBuilder WithPort(int port)
        {
            WithEnvironmentVariable("PORT", port.ToString());
            return this;
        }
    }
}
