using System;
using System.Collections.Generic;
using System.Diagnostics;
using EW.Support;
namespace EW
{
    public static class WorldUtils
    {

        public static void DoTimed<T>(this IEnumerable<T> e,Action<T> a,string text)
        {
            var longTickThresholdInStopwatchTicks = PerfTimer.LongTickThresholdInStopwatchTicks;

            using(var enumerator = e.GetEnumerator())
            {
                var start = Stopwatch.GetTimestamp();
                while (enumerator.MoveNext())
                {
                    a(enumerator.Current);

                    var current = Stopwatch.GetTimestamp();
                    if(current -start> longTickThresholdInStopwatchTicks)
                    {
                        PerfTimer.LogLongTick(start, current, text, enumerator.Current);
                        start = Stopwatch.GetTimestamp();
                    }
                    else
                    {
                        start = current;
                    }
                }
            }
        }
    }
}