using System;
using System.Diagnostics;
using System.Threading.Tasks;

using FluentUIScaffold.Core.Configuration.Launchers;

using NUnit.Framework;

namespace FluentUIScaffold.Core.Tests
{
    [TestFixture]
    public class SystemClockTests
    {
        [Test]
        public async Task Delay_Waits_Approximately_Requested_Time()
        {
            var clock = new SystemClock();
            var sw = Stopwatch.StartNew();
            await clock.Delay(TimeSpan.FromMilliseconds(10));
            sw.Stop();
            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(5));
        }

        [Test]
        public void UtcNow_Increases_Over_Time()
        {
            var clock = new SystemClock();
            var t1 = clock.UtcNow;
            System.Threading.Thread.Sleep(1);
            var t2 = clock.UtcNow;
            Assert.That(t2, Is.GreaterThan(t1));
        }
    }
}
