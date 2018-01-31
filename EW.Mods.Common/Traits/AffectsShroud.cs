using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public abstract class AffectsShroudInfo : ConditionalTraitInfo
    {

        public readonly WDist Range = WDist.Zero;

        /// <summary>
        /// If >=0,prevent cells that are this much higher than the actor from being revealed.
        /// </summary>
        public readonly int MaxHeightDelta = -1;

        /// <summary>
        /// Possible values are CenterPosition (measure range from the center) and Footprint(measure range from the footprint).
        /// </summary>
        public readonly VisibilityType Type = VisibilityType.Footprint;
    }

    public abstract class AffectsShroud:ConditionalTrait<AffectsShroudInfo>,ITick,ISync,INotifyAddedToWorld,INotifyRemovedFromWorld
    {

        static readonly PPos[] NoCells = { };

        [Sync]
        CPos cachedLocation;
        [Sync]
        bool cachedTraitDisabled;

        public WDist Range { get { return cachedTraitDisabled ? WDist.Zero : Info.Range; } }

        public AffectsShroud(Actor self,AffectsShroudInfo info) : base(info) { }

        protected abstract void AddCellsToPlayerShroud(Actor self, Player player, PPos[] uv);

        protected abstract void RemoveCellsFromPlayerShroud(Actor self, Player player);




        void ITick.Tick(Actor self)
        {
            if (!self.IsInWorld)
                return;

            var centerPosition = self.CenterPosition;
            var projectedPos = centerPosition - new WVec(0, centerPosition.Z, centerPosition.Z);
            var projectedLocation = self.World.Map.CellContaining(projectedPos);
            var traitDisabled = IsTraitDisabled;

            if (cachedLocation == projectedLocation && traitDisabled == cachedTraitDisabled)
                return;

            cachedLocation = projectedLocation;
            cachedTraitDisabled = traitDisabled;

            var cells = ProjectedCells(self);
            foreach(var p in self.World.Players)
            {
                RemoveCellsFromPlayerShroud(self, p);
                AddCellsToPlayerShroud(self, p, cells);
            }

        }

        PPos[] ProjectedCells(Actor self)
        {
            var map = self.World.Map;

            var range = Range;
            if (range == WDist.Zero)
                return NoCells;

            if (Info.Type == VisibilityType.Footprint)
                return self.OccupiesSpace.OccupiedCells()
                    .SelectMany(kv => Shroud.ProjectedCellsInRange(map, kv.First, range, Info.MaxHeightDelta))
                    .Distinct().ToArray();

            var pos = self.CenterPosition;

            if (Info.Type == VisibilityType.GroundPosition)
                pos -= new WVec(WDist.Zero, WDist.Zero, self.World.Map.DistanceAboveTerrain(pos));

            return Shroud.ProjectedCellsInRange(map, pos, range, Info.MaxHeightDelta).ToArray();
        }


        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            var centerPosition = self.CenterPosition;
            var projectedPos = centerPosition - new WVec(0, centerPosition.Z, centerPosition.Z);
            cachedLocation = self.World.Map.CellContaining(projectedPos);
            cachedTraitDisabled = IsTraitDisabled;

            var cells = ProjectedCells(self);

            foreach (var p in self.World.Players)
                AddCellsToPlayerShroud(self, p, cells);
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            foreach (var p in self.World.Players)
                RemoveCellsFromPlayerShroud(self, p);
        }
    }
}