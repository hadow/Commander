using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
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

    public class BuildingInfo : ITraitInfo,IOccupySpaceInfo
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

        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos topLeft,SubCell subCell = SubCell.Any)
        {
            var occupied = UnpathableTiles(topLeft).ToDictionary(c => c, c => SubCell.FullCell);
            return new ReadOnlyDictionary<CPos,SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

        public IEnumerable<CPos> UnpathableTiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.Occupied))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedUntargetable))
                yield return t;
        }

        public IEnumerable<CPos> PathableTiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.Empty))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedPassable))
                yield return t;
        }

        public object Create(ActorInitializer init)
        {
            return new Building(init,this);
        }
    }
    public class Building:IOccupySpace,ISync,INotifyAddToWorld,INotifyRemovedFromWorld
    {
        public readonly BuildingInfo Info;
        readonly Actor self;
        [Sync]
        readonly CPos topLeft;
        public bool BuildComplete { get; private set; }

        public CPos TopLeft { get { return topLeft; } }

        public WPos CenterPosition { get; private set; }

        Pair<CPos, SubCell>[] occupiedCells;
        Pair<CPos, SubCell>[] targetableCells;

        public Building(ActorInitializer init,BuildingInfo info)
        {
            topLeft = init.Get<LocationInit, CPos>();

            Info = info;
            occupiedCells = Info.UnpathableTiles(TopLeft).Select(c => Pair.New(c, SubCell.FullCell)).ToArray();

        }

        public IEnumerable<Pair<CPos,SubCell>> OccupiedCells() { return occupiedCells; }

        public virtual void AddedToWorld(Actor self)
        {

        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {

        }
    }
}