using System;
using System.Diagnostics;

namespace EW.Support
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class PerfTimer:IDisposable
    {


        public static long LongTickThresholdInStopwatchTicks
        {
            get
            {
                return Stopwatch.Frequency;
            }
        }


        public void Dispose() { }


    }
}