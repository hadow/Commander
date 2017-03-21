using System;
using RA.Activities;
using RA.Support;
using System.Diagnostics;
namespace RA.Traits
{
    public static class ActivityUtils
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static Activity RunActivity(Actor self,Activity act)
        {
            if (act == null)
                return act;

            var longTickThresholdInStopwatchTicks = PerfTimer.LongTickThresholdInStopwatchTicks;
            var start = Stopwatch.GetTimestamp();

            while (act != null)
            {
                var prev = act;
                act = act.Tick(self);
                var current = Stopwatch.GetTimestamp();
                if (current - start > longTickThresholdInStopwatchTicks)
                {
                    start = Stopwatch.GetTimestamp();
                }
                else
                    start = current;
                if (prev == act)
                    break;
            }
            return act;

        }

    }
}