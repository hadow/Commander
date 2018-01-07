using System;
using System.Linq;

using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Where the unit should leave the building.
    /// </summary>
    public class ExitInfo : TraitInfo<Exit>,Requires<IOccupySpaceInfo>
    {
        public readonly WVec SpawnOffset = WVec.Zero;

        public readonly CVec ExitCell = CVec.Zero;

        public readonly int Facing = -1;

        public readonly HashSet<string> ProductionTypes = new HashSet<string>();

        public readonly bool MoveIntoWorld = true;

        public readonly int ExitDelay = 0;

    }
    public class Exit
    {
    }


    public static class ExitExts{


        public static IEnumerable<ExitInfo> Exits(this ActorInfo info,string productionType = null){

            var all = info.TraitInfos<ExitInfo>();

            if (string.IsNullOrEmpty(productionType))
                return all.Where(e => e.ProductionTypes.Count == 0);
            return all.Where(e => e.ProductionTypes.Count == 0 || e.ProductionTypes.Contains(productionType));
        }

        public static ExitInfo FirstExitOrDefault(this ActorInfo info,string productionType = null){

            var all = info.TraitInfos<ExitInfo>();

            if (string.IsNullOrEmpty(productionType))
                return all.FirstOrDefault(e => e.ProductionTypes.Count == 0);
            return all.FirstOrDefault(e => e.ProductionTypes.Count == 0 || e.ProductionTypes.Contains(productionType));
        }

        public static ExitInfo RandomExitOrDefault(this ActorInfo info,World world,string productionType,Func<ExitInfo,bool> p=null){

            var allOfType = Exits(info, productionType);
            if (!allOfType.Any())
                return null;

            var shuffled = allOfType.Shuffle(world.SharedRandom);

            return p != null ? shuffled.FirstOrDefault(p) : shuffled.First();
        }

        public static ExitInfo RandomExitOrDefault(this Actor self,string productionType,Func<ExitInfo,bool> p = null){

            return RandomExitOrDefault(self.Info, self.World, productionType, p);
        }
    }
}