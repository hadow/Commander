using System;
using System.Collections.Generic;
using System.Linq;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common
{
    public static class WorldExtensions
    {

        /// <summary>
        /// Finds all the actors of which their health radius is intersected by a line (with a definable width) between two points
        /// </summary>
        /// <param name="world"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <param name="lineWidth"></param>
        /// <param name="targetExtraSearchRadius"></param>
        /// <returns></returns>
        public static IEnumerable<Actor> FindActorsOnLine(this World world,WPos lineStart,WPos lineEnd,WDist lineWidth,WDist targetExtraSearchRadius)
        {
            var xDiff = lineEnd.X - lineStart.X;
            var yDiff = lineEnd.Y - lineStart.Y;
            var xDir = xDiff < 0 ? -1 : 1;
            var yDir = yDiff < 0 ? -1 : 1;

            var dir = new WVec(xDir, yDir, 0);
            var overselect = dir * (1024 + lineWidth.Length + targetExtraSearchRadius.Length);
            var finalTarget = lineEnd + overselect;
            var finalSource = lineStart - overselect;

            var actorsInSquare = world.ActorMap.ActorsInBox(finalTarget, finalSource);
            var intersectedActors = new List<Actor>();

            foreach(var currActor in actorsInSquare)
            {
                var actorWidth = 0;
                var shapes = currActor.TraitsImplementing<HitShape>().Where(Exts.IsTraitEnabled);
                if (shapes.Any())
                    actorWidth = shapes.Max(h => h.Info.Type.OuterRadius.Length);

                var projection = MinimumPointLineProjection(lineStart, lineEnd, currActor.CenterPosition);
                var distance = (currActor.CenterPosition - projection).HorizontalLength;
                var maxReach = actorWidth + lineWidth.Length;

                if (distance <= maxReach)
                    intersectedActors.Add(currActor);
            }
            return intersectedActors;
        }


        public static WPos MinimumPointLineProjection(WPos lineStart,WPos lineEnd,WPos point)
        {
            var squaredLength = (lineEnd - lineStart).HorizontalLengthSquared;

            if (squaredLength == 0)
                return lineEnd;

            var xDiff = ((long)point.X - lineEnd.X) * (lineStart.X - lineEnd.X);
            var yDiff = ((long)point.Y - lineEnd.Y) * (lineStart.Y - lineEnd.Y);

            var t = xDiff + yDiff;

            if (t < 0)
                return lineEnd;

            if (t > squaredLength)
                return lineStart;

            return WPos.Lerp(lineEnd, lineStart, t, squaredLength);
        }

    }
}