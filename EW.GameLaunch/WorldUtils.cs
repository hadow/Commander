using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EW.Support;
using EW.Traits;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="world"></param>
        /// <param name="origin"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static IEnumerable<Actor> FindActorsInCircle(this World world,WPos origin,WDist r)
        {
            var vec = new WVec(r, r, WDist.Zero);
            return world.ActorMap.ActorsInBox(origin - vec, origin + vec).Where(a => (a.CenterPosition - origin).HorizontalLengthSquared <= r.LengthSquared);
        }

        public static Actor ClosestTo(this IEnumerable<Actor> actors,WPos pos)
        {
            return actors.MinByOrDefault(a => (a.CenterPosition - pos).LengthSquared);
        }

        public static Actor ClosestTo(this IEnumerable<Actor> actors,Actor a)
        {
            return actors.ClosestTo(a.CenterPosition);
        }


        public static bool ContainsTemporaryBlocker(this World world,CPos cell,Actor ignoreActor = null)
        {
            var temporaryBlockers = world.ActorMap.GetActorsAt(cell);
            foreach(var temporaryBlocker in temporaryBlockers)
            {
                if (temporaryBlocker == ignoreActor)
                    continue;

                var temporaryBlockerTraits = temporaryBlocker.TraitsImplementing<ITemporaryBlocker>();
                foreach(var temporaryBlockerTrait in temporaryBlockerTraits)
                {
                    if (temporaryBlockerTrait.IsBlocking(temporaryBlocker, cell))
                        return true;
                }
            }
            return false;
        }

        public static WPos PositionClosestTo(this IEnumerable<WPos> positions,WPos pos)
        {
            return positions.MinByOrDefault(p => (p - pos).LengthSquared);
        }
    }
}