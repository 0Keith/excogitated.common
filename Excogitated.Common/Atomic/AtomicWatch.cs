using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using Excogitated.Common.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Excogitated.Common.Atomic
{
    public class WatchResult
    {
        public string Name { get; set; }
        public string SubName { get; set; }
        public long Total { get; set; }
        public long Count { get; set; }
        public TimeSpan Elapsed { get; set; }
        public double PerSecond { get; set; }
        public long Remaining { get; set; }
        public TimeSpan TimeRemaining { get; set; }

        public override string ToString()
        {
            var b = new StringBuilder(Name);
            if (!SubName.IsNullOrWhiteSpace())
                b.Append('.').Append(SubName);
            b.Append(" - ").Append(Count);
            if (Total != 0)
                b.Append(" of ").Append(Total);
            b.Append($" in {Elapsed.Format()} at {PerSecond:0.00}/sec");
            if (Total != 0)
                b.Append($" with {Remaining} or {TimeRemaining.Format()} remaining");
            return b.ToString();
        }
    }

    public class AtomicWatch : IDisposable
    {
        private readonly AsyncResult<bool> _running = new();
        private readonly Stopwatch _totalTime = Stopwatch.StartNew();
        private readonly AtomicInt64 _count = new();

        public AtomicInt64 Total { get; } = new AtomicInt64();

        public string Name { get; set; }
        public string SubName { get; set; }
        public TimeSpan Interval { get; set; } = TimeSpan.FromMilliseconds(5000);

        public Action<WatchResult> Report { get; set; }
        public ILogger Logger { get; set; }

        public long Increment() => _count.Increment();
        public void Dispose() => _running.TryComplete(true);

        public AtomicWatch(long total, [CallerMemberName] string name = null) : this(null, total, name) { }
        public AtomicWatch(string subName = null, long total = 0, [CallerMemberName] string name = null)
        {
            Logger = Loggers.All;
            Name = name;
            SubName = subName;
            Total.Value = total;
            Task.Run(async () =>
            {
                while (!_running.Completed)
                    try
                    {
                        var count = _count.Value;
                        await Task.WhenAny(Task.Delay(Interval), _running.Source);
                        count = _count.Value - count;
                        var result = GetResult();
                        Report?.Invoke(result);
                        Logger?.Info(result);
                    }
                    catch (Exception e)
                    {
                        if (Logger is ILogger l)
                            l.Error(e);
                        else
                            Loggers.Error(e);
                    }
            });
        }

        public WatchResult GetResult()
        {
            var count = _count.Value;
            var remaining = Total - count;
            var seconds = _totalTime.Elapsed.TotalSeconds;
            var perSecond = seconds == 0 ? 0 : Math.Round(count / seconds, 2);
            var timeRemaining = perSecond == 0 ? TimeSpan.MaxValue : TimeSpan.FromSeconds(remaining / perSecond);
            return new WatchResult
            {
                Name = Name,
                SubName = SubName,
                Total = Total,
                Count = count,
                Elapsed = _totalTime.Elapsed,
                PerSecond = perSecond,
                Remaining = remaining,
                TimeRemaining = timeRemaining
            };
        }
    }
}
