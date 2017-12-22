using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Primitives;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// 固定的，不变的物体
    /// </summary>
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

            if (info.OccupiesSpace)
                occupied = new[] { Pair.New(TopLeft, SubCell.FullCell) };
            else
                occupied = new Pair<CPos, SubCell>[0];

        }

        public CPos TopLeft { get { return location; } }

        public WPos CenterPosition { get { return position; } }

        public IEnumerable<Pair<CPos,SubCell>> OccupiedCells() {  return occupied;  }

        public void AddedToWorld(Actor self)
        {
            //self.World.ActorMap.AddInfluence(self, this);
            //self.World.ActorMap.AddPosition(self, this);

            //if (!self.Bounds.Size.IsEmpty)
            //    self.World.ScreenMap.Add(self);

            self.World.AddToMaps(self, this);
        }

        public void RemovedFromWorld(Actor self)
        {
            //self.World.ActorMap.RemoveInfluence(self, this);
            //self.World.ActorMap.RemovePosition(self, this);

            //if (!self.Bounds.Size.IsEmpty)
            //    self.World.ScreenMap.Remove(self);
            self.World.RemoveFromMaps(self, this);
            
        }
    }
}