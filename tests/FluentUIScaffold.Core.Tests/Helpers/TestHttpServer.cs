using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentUIScaffold.Core.Tests.Helpers
{
    public sealed class TestHttpServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private Task? _loopTask;

        public int Port { get; }

        private TestHttpServer(int port)
        {
            Port = port;
            _listener = new TcpListener(IPAddress.Loopback, port);
        }

        public static async Task<TestHttpServer> StartAsync(HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var localPort = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();

            var server = new TestHttpServer(localPort)
            {
                _statusCode = statusCode
            };
            server._listener.Start();
            server._loopTask = Task.Run(() => server.RunLoopAsync(server._cts.Token));
            // Give it a moment to start
            await Task.Delay(50);
            return server;
        }

        private HttpStatusCode _statusCode = HttpStatusCode.OK;

        private static string GetReasonPhrase(HttpStatusCode code)
        {
            return code switch
            {
                HttpStatusCode.OK => "OK",
                HttpStatusCode.InternalServerError => "Internal Server Error",
                HttpStatusCode.NotFound => "Not Found",
                HttpStatusCode.BadRequest => "Bad Request",
                HttpStatusCode.ServiceUnavailable => "Service Unavailable",
                _ => code.ToString().Replace('_', ' ')
            };
        }

        private async Task RunLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!_listener.Pending())
                    {
                        await Task.Delay(10, cancellationToken);
                        continue;
                    }

                    using var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    using var stream = client.GetStream();

                    // Read and ignore the request
                    var buffer = new byte[4096];
                    // Non-blocking read best-effort
                    if (stream.DataAvailable)
                    {
                        await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    }

                    var statusCode = (int)_statusCode;
                    var reason = GetReasonPhrase(_statusCode);
                    var response = $"HTTP/1.1 {statusCode} {reason}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
                    var bytes = Encoding.ASCII.GetBytes(response);
                    await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch
            {
                // swallow for test helper
            }
        }

        public async ValueTask DisposeAsync()
        {
            try { _cts.Cancel(); } catch { }
            try { _listener.Stop(); } catch { }
            if (_loopTask != null)
            {
                try { await _loopTask; } catch { }
            }
            _cts.Dispose();
        }
    }
}