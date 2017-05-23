using System;
using System.Diagnostics;
namespace EW.Support
{
    public struct PerfSample:IDisposable
    {

        readonly string item;
        readonly long ticks;

        public PerfSample(string item)
        {
            this.item = item;
            ticks = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {

        }
    }
}