using System;
using System.Reflection;
using FluentUIScaffold.Core.Configuration.Launchers;
using NUnit.Framework;
using System.Threading.Tasks;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class PortProcessFinderTests
    {
        [Test]
        public void FilterOutputByPort_MatchesCorrectLines()
        {
            var sample = string.Join(Environment.NewLine, new[]
            {
                "tcp        0      0 127.0.0.1:8080          0.0.0.0:*               LISTEN      1234/node",
                "tcp        0      0 127.0.0.1:18080         0.0.0.0:*               LISTEN      5678/node",
                "udp        0      0 0.0.0.0:8080            0.0.0.0:*                           -"
            });

            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            var result = (string)method!.Invoke(null, new object[] { sample, 8080 })!;

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(":8080"));
                Assert.That(result, Does.Not.Contain(":18080"));
            });
        }

        [Test]
        public void FilterOutputByPort_EmptyOutput_ReturnsEmpty()
        {
            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (string)method!.Invoke(null, new object[] { string.Empty, 8080 })!;
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void FilterOutputByPort_Matches_EndOfLine_And_TabSeparated()
        {
            var sample = string.Join(Environment.NewLine, new[]
            {
                "tcp        0      0 127.0.0.1:9090\t0.0.0.0:*               LISTEN      2222/dotnet",
                "tcp        0      0 127.0.0.1:19090        0.0.0.0:*               LISTEN      3333/dotnet",
                "udp        0      0 0.0.0.0:9090"
            });

            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            var result = (string)method!.Invoke(null, new object[] { sample, 9090 })!;

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain(":9090"));
                Assert.That(result, Does.Not.Contain(":19090"));
            });
        }

        [Test]
        public void FilterOutputByPort_Handles_Ipv6_And_ExactEnd()
        {
            var port = 5050;
            var output = string.Join("\n", new[]
            {
                "tcp6       0      0 :::5050                 :::*                    LISTEN      1234/node",
                "udp        0      0 0.0.0.0:5050           0.0.0.0:*                           5678/app",
                "tcp        0      0 127.0.0.1:45050        0.0.0.0:*               LISTEN      9012/other" // should not match
            });

            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            var result = (string)method!.Invoke(null, new object[] { output, port })!;

            Assert.That(result, Does.Contain(":5050"));
            Assert.That(result, Does.Not.Contain("45050"));
        }

        [Test]
        public void FilterOutputByPort_MultiplePorts_FiltersCorrectly()
        {
            var output = "tcp 0 0 0.0.0.0:8080 0.0.0.0:* LISTEN 1111/app\n" +
                         "tcp 0 0 0.0.0.0:8081 0.0.0.0:* LISTEN 2222/app";
            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            var result = (string)method!.Invoke(null, new object[] { output, 8081 })!;
            Assert.That(result.Split('\n').Length, Is.EqualTo(1));
            Assert.That(result, Does.Contain(":8081 "));
        }

        [Test]
        public void FilterOutputByPort_Handles_Tabs_And_TrailingSpaces()
        {
            var port = 6060;
            var output = string.Join("\n", new[]
            {
                $"tcp        0      0 127.0.0.1:{port}\t0.0.0.0:*               LISTEN      1111/app ",
                $"udp        0      0 0.0.0.0:{port}           0.0.0.0:*                           2222/daemon\t",
                "tcp        0      0 127.0.0.1:16060        0.0.0.0:*               LISTEN      3333/other"
            });

            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            var result = (string)method!.Invoke(null, new object[] { output, port })!;
            Assert.That(result.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length, Is.EqualTo(2));
        }

        [Test]
        public void FilterOutputByPort_NoMatches_ReturnsEmpty()
        {
            var output = "tcp 0 0 127.0.0.1:8081 0.0.0.0:* LISTEN 9999/app";
            var method = typeof(PortProcessFinder).GetMethod("FilterOutputByPort", BindingFlags.NonPublic | BindingFlags.Static);
            var result = (string)method!.Invoke(null, new object[] { output, 8080 })!;
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void GetNetstatCommand_ReturnsExpectedArgs_ForPlatform()
        {
            var method = typeof(PortProcessFinder).GetMethod("GetNetstatCommand", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            var tuple = method!.Invoke(null, Array.Empty<object>());
            var fileName = (string)tuple!.GetType().GetField("Item1")!.GetValue(tuple)!;
            var args = (string)tuple.GetType().GetField("Item2")!.GetValue(tuple)!;

            Assert.That(fileName, Is.EqualTo("netstat"));

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                Assert.That(args, Is.EqualTo("-ano"));
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Assert.That(args, Is.EqualTo("-tulpn"));
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                Assert.That(args, Is.EqualTo("-anv"));
            }
        }

        [Test]
        public async Task FindProcessesOnPortLinuxSsAsync_Throws_On_NonLinux()
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Assert.That(async () => await PortProcessFinder.FindProcessesOnPortLinuxSsAsync(5050), Throws.Exception.TypeOf<PlatformNotSupportedException>());
            }
            else
            {
                Assert.Pass("Skipped assertion on Linux environment where ss is available");
            }
        }
    }
}