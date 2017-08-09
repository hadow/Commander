using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public enum FootprintCellType
    {
        Empty='_',
        OccupiedPassable='=',
        Occupied='x',
        OccupiedUntargetable='X'
    }

    public class GivesBuildableAreaInfo : TraitInfo<GivesBuildableArea>
    {

    }
    public class GivesBuildableArea
    {

    }

    public class BuildingInfo : ITraitInfo
    {
        public readonly HashSet<string> TerrainTypes = new HashSet<string>();

        [FieldLoader.LoadUsing("LoadFootprint")]
        public readonly Dictionary<CVec, FootprintCellType> Footprint;

        protected static object LoadFootprint(MiniYaml yaml)
        {
            var ret = new Dictionary<CVec, FootprintCellType>();
            return ret;
        }

        public IEnumerable<CPos> FootprintTiles(CPos location,FootprintCellType type)
        {
            return Footprint.Where(kv => kv.Value == type).Select(kv => location+kv.Key);
        }

        public IEnumerable<CPos> Tiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedPassable))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.Occupied))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedUntargetable))
                yield return t;
        }


        public object Create(ActorInitializer init)
        {
            return new Building();
        }
    }
    public class Building
    {

        public bool BuildComplete { get; private set; }
    }
}