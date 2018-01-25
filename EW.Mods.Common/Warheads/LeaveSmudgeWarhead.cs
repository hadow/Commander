using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Traits;

namespace EW.Mods.Common.Warheads
{
    /// <summary>
    /// Leave smudge warhead.
    /// </summary>
    public class LeaveSmudgeWarhead:Warhead
    {

        /// <summary>
        /// Size of the area.A smudge will be created in each tile.
        /// </summary>
        public readonly int[] Size = { 0, 0 };

        /// <summary>
        /// Type of smudge to apply to terrain.
        /// </summary>
        public readonly HashSet<string> SmudgeType = new HashSet<string>();

        /// <summary>
        /// How close to ground must the impact happen to spawn smudges.
        /// </summary>
        public readonly WDist AirThreshold = new WDist(128);

        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers)
        {

            var world = firedBy.World;
            var pos = target.CenterPosition;

            var dat = world.Map.DistanceAboveTerrain(pos);

            if (dat > AirThreshold)
                return;


            var targetTile = world.Map.CellContaining(pos);

            var smudgeLayers = world.WorldActor.TraitsImplementing<SmudgeLayer>().ToDictionary(x => x.Info.Type);

            var minRange = (Size.Length > 1 && Size[1] > 0) ? Size[1] : Size[0];
            var allCells = world.Map.FindTilesInAnnulus(targetTile, minRange, Size[0]);

            //Draw the smudges:
            foreach(var sc in allCells){

                var smudgeType = world.Map.GetTerrainInfo(sc).AcceptsSmudgeType.FirstOrDefault(SmudgeType.Contains);
                if (smudgeType == null)
                    continue;

                var cellActors = world.ActorMap.GetActorsAt(sc);
                if (cellActors.Any(a => !IsValidAgainst(a, firedBy)))
                    continue;

                SmudgeLayer smudgeLayer;
                if (!smudgeLayers.TryGetValue(smudgeType, out smudgeLayer))
                    throw new NotImplementedException("Unknown smudge type '{0}'".F(smudgeType));

                smudgeLayer.AddSmudge(sc);
                
            }
        }
    }
}