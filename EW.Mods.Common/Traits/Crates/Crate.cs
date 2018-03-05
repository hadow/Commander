using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{


    class CrateInfo : ITraitInfo,IPositionableInfo,Requires<RenderSpritesInfo>
    {
        [Desc("Length of time (in seconds) until the crate gets removed automatically. " +
            "A value of zero disables auto-removal.")]
        public readonly int Lifetime = 0;

        public readonly HashSet<string> TerrainTypes = new HashSet<string>();

        public readonly string CrushClass = "crate";

        public object Create(ActorInitializer init)
        {
            return new Crate(init,this);
        }

        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            var occupied = new Dictionary<CPos, SubCell>() { { location, subCell } };
            return new ReadOnlyDictionary<CPos, SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

        public bool CanEnterCell(World world,Actor self,CPos cell,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            return GetAvailableSubCell(world, cell, ignoreActor, checkTransientActors) != SubCell.Invalid;
        }
        public SubCell GetAvailableSubCell(World world, CPos cell, Actor ignoreActor = null, bool checkTransientActors = true)
        {
            if (!world.Map.Contains(cell))
                return SubCell.Invalid;

            var type = world.Map.GetTerrainInfo(cell).Type;
            if (!TerrainTypes.Contains(type))
                return SubCell.Invalid;

            if (world.WorldActor.Trait<BuildingInfluence>().GetBuildingAt(cell) != null)
                return SubCell.Invalid;

            if (!checkTransientActors)
                return SubCell.FullCell;

            return !world.ActorMap.GetActorsAt(cell).Any(x => x != ignoreActor)
                ? SubCell.FullCell : SubCell.Invalid;
        }


    }
    class Crate:ITick,IPositionable,ISync,INotifyAddedToWorld,INotifyRemovedFromWorld
    {

        readonly Actor self;
        readonly CrateInfo info;
        bool collected;

        [Sync] int ticks;
        [Sync] public CPos Location;

        public CPos TopLeft { get { return Location; } }
        public Pair<CPos, SubCell>[] OccupiedCells() { return new[] { Pair.New(Location, SubCell.FullCell) }; }

        public WPos CenterPosition { get; private set; }

        public Crate(ActorInitializer init, CrateInfo info)
        {
            self = init.Self;
            this.info = info;

            if (init.Contains<LocationInit>())
                SetPosition(self, init.Get<LocationInit, CPos>());
        }

        void ITick.Tick(Actor self){

            if (info.Lifetime != 0 && self.IsInWorld && ++ticks >= info.Lifetime * 25)
                self.Dispose();
        }

        public bool CanEnterCell(CPos a, Actor ignoreActor = null, bool checkTransientActors = true)
        {
            return GetAvailableSubCell(a, SubCell.Any, ignoreActor, checkTransientActors) != SubCell.Invalid;
        }


        public bool IsLeavingCell(CPos location, SubCell subCell = SubCell.Any) { return self.Location == location && ticks + 1 == info.Lifetime * 25; }
        public SubCell GetValidSubCell(SubCell preferred = SubCell.Any) { return SubCell.FullCell; }
        public SubCell GetAvailableSubCell(CPos cell, SubCell preferredSubCell = SubCell.Any, Actor ignoreActor = null, bool checkTransientActors = true){

            return info.GetAvailableSubCell(self.World, cell, ignoreActor, checkTransientActors);

        }
                                           
        // Sets the location (Location) and visual position (CenterPosition)
        public void SetPosition(Actor self, WPos pos)
        {
            var cell = self.World.Map.CellContaining(pos);
            SetLocation(self, cell);
            SetVisualPosition(self, self.World.Map.CenterOfCell(cell) + new WVec(WDist.Zero, WDist.Zero, self.World.Map.DistanceAboveTerrain(pos)));
        }

        // Sets the location (Location) and visual position (CenterPosition)
        public void SetPosition(Actor self, CPos cell, SubCell subCell = SubCell.Any)
        {
            SetLocation(self, cell, subCell);
            SetVisualPosition(self, self.World.Map.CenterOfCell(cell));
        }

        // Sets only the visual position (CenterPosition)
        public void SetVisualPosition(Actor self, WPos pos)
        {
            CenterPosition = pos;
            self.World.UpdateMaps(self, this);
        }

        // Sets only the location (Location)
        void SetLocation(Actor self, CPos cell, SubCell subCell = SubCell.Any)
        {
            self.World.ActorMap.RemoveInfluence(self, this);
            Location = cell;
            self.World.ActorMap.AddInfluence(self, this);
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            self.World.AddToMaps(self, this);

            var cs = self.World.WorldActor.TraitOrDefault<CrateSpawner>();
            if (cs != null)
                cs.IncrementCrates();
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            self.World.RemoveFromMaps(self, this);

            var cs = self.World.WorldActor.TraitOrDefault<CrateSpawner>();
            if (cs != null)
                cs.DecrementCrates();
        }
    }
}