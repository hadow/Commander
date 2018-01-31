using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Warheads
{
    public class DestroyResourceWarhead:Warhead
    {
        /// <summary>
        /// Size of the area
        /// </summary>
        public readonly int[] Size ={ 0,0};
        public override void DoImpact(Target target, Actor firedBy, IEnumerable<int> damagedModifiers)
        {

            var world = firedBy.World;
            var targetTile = world.Map.CellContaining(target.CenterPosition);
            var resLayer = world.WorldActor.Trait<ResourceLayer>();

            var minRange = (Size.Length > 1 && Size[1] > 0) ? Size[1] : 0;
            var allCells = world.Map.FindTilesInAnnulus(targetTile, minRange, Size[0]);

            //Destroy all resources in the selected tiles
            foreach (var cell in allCells)
                resLayer.Destroy(cell);


        }
    }
}