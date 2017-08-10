using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    class ImmobileInfo : ITraitInfo, IOccupySpaceInfo
    {
        public readonly bool OccupiesSpace = true;
        public object Create(ActorInitializer init) { return new Immobile(init,this); }

        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            var occupied = OccupiesSpace ? new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } } : new Dictionary<CPos, SubCell>();
            return new ReadOnlyDictionary<CPos, SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

    }
    class Immobile:IOccupySpace,ISync,INotifyAddToWorld,INotifyRemovedFromWorld
    {

        [Sync]
        readonly CPos location;

        [Sync]
        readonly WPos position;

        readonly IEnumerable<Pair<CPos, SubCell>> occupied;

        public Immobile(ActorInitializer init,ImmobileInfo info)
        {
            location = init.Get<LocationInit, CPos>();
            position = init.World.Map.CenterOfCell(location);


        }

        public CPos TopLeft { get { return location; } }

        public WPos CenterPosition { get { return position; } }

        public IEnumerable<Pair<CPos,SubCell>> OccupiedCells() {  return occupied;  }

        public void AddedToWorld(Actor self) { }

        public void RemovedFromWorld(Actor self) { }
    }
}