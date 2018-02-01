using System;
using System.Linq;
using EW.Activities;
using EW.Support;
using System.Diagnostics;
namespace EW.Traits
{
    public static class ActivityUtils
    {

        /// <summary>
        /// 活动运行
        /// </summary>
        /// <param name="self"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static Activity RunActivity(Actor self,Activity act)
        {
            //PERF:If there are no activities we can bail straight away and save ourselves a call to Stopwatch.GetTimestamp.
            if (act == null)
                return act;

            //PERF: This is a hot path and must run with minimal added overhead.
            //Calling Stopwatch.GetTimestamp is a bit expensive, so we enumerate manually to allow us to call it only
            //once per iteration in the normal case.
            var longTickThresholdInStopwatchTicks = PerfTimer.LongTickThresholdInStopwatchTicks;
            var start = Stopwatch.GetTimestamp();

            while (act != null)
            {
                var prev = act;
                act = act.TickOuter(self);
                var current = Stopwatch.GetTimestamp();
                if (current - start > longTickThresholdInStopwatchTicks)
                {
                    start = Stopwatch.GetTimestamp();
                }
                else
                    start = current;
                if (prev == act || act == prev.ParentActivity)
                    break;
            }
            return act;

        }

        /// <summary>
        /// 活动序列
        /// </summary>
        /// <param name="acts">活动集</param>
        /// <returns></returns>
        public static Activity SequenceActivities(params Activity[] acts)
        {
            return acts.Reverse().Aggregate((next, a) => { a.Queue(next); return a; });
        }

    }
}