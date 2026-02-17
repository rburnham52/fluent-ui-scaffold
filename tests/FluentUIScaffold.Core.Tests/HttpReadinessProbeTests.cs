using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;
using FluentUIScaffold.Core.Configuration.Launchers.Defaults;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class HttpReadinessProbeTests
    {
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;
            public int Calls { get; private set; }
            public FakeHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) { _handler = handler; }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Calls++;
                return Task.FromResult(_handler(request));
            }
        }

        private static LaunchPlan BuildPlan(Uri baseUrl, TimeSpan? timeout = null, TimeSpan? initialDelay = null, TimeSpan? poll = null, params string[] endpoints)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "echo test",
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            return new LaunchPlan(
                startInfo,
                baseUrl,
                timeout ?? TimeSpan.FromSeconds(60),
                new HttpReadinessProbe(),
                endpoints.Length > 0 ? endpoints : new[] { "/" },
                initialDelay ?? TimeSpan.FromSeconds(2),
                poll ?? TimeSpan.FromMilliseconds(200));
        }

        [Test]
        public async Task WaitUntilReadyAsync_Succeeds_OnFirstHealthCheck()
        {
            var baseUrl = new Uri("http://localhost:5001");
            var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler);
            var probe = new HttpReadinessProbe(httpClient);

            var plan = BuildPlan(baseUrl, TimeSpan.FromSeconds(2), TimeSpan.Zero, TimeSpan.FromMilliseconds(10), "/health");

            await probe.WaitUntilReadyAsync(plan, logger: null);
            Assert.That(handler.Calls, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void WaitUntilReadyAsync_Throws_OnTimeout()
        {
            var baseUrl = new Uri("http://localhost:5002");
            var handler = new FakeHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            var httpClient = new HttpClient(handler);
            var probe = new HttpReadinessProbe(httpClient);
            var plan = BuildPlan(baseUrl, TimeSpan.FromMilliseconds(50), TimeSpan.Zero, TimeSpan.FromMilliseconds(10), "/health");

            Assert.That(async () => await probe.WaitUntilReadyAsync(plan, logger: null), Throws.Exception);
        }

        [Test]
        public async Task WaitUntilReadyAsync_Ignores_Non200_ThenSucceeds()
        {
            var baseUrl = new Uri("http://localhost:5003");
            int count = 0;
            var handler = new FakeHandler(_ =>
            {
                count++;
                return new HttpResponseMessage(count < 3 ? HttpStatusCode.BadGateway : HttpStatusCode.OK);
            });
            var httpClient = new HttpClient(handler);
            var probe = new HttpReadinessProbe(httpClient);
            var plan = BuildPlan(baseUrl, TimeSpan.FromSeconds(1), TimeSpan.Zero, TimeSpan.FromMilliseconds(5), "/health");

            await probe.WaitUntilReadyAsync(plan, logger: null);
            Assert.That(handler.Calls, Is.GreaterThanOrEqualTo(3));
        }
    }
}
