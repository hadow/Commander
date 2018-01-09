using System;
using System.Collections.Generic;
using EW.Primitives;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class HuskInfo : ITraitInfo,IOccupySpaceInfo
    {
        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            var occupied = new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } };
            return new ReadOnlyDictionary<CPos, SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

        public object Create(ActorInitializer init) { return new Husk(); }
    }



    public class Husk:IPositionable,IFacing,ISync,INotifyCreated,INotifyAddedToWorld,INotifyRemovedFromWorld,IDeathActorInitModifier
    {



        public int TurnSpeed { get { return 0; } }

        [Sync]
        public CPos TopLeft { get; private set; }

        [Sync]
        public WPos CenterPosition { get; private set; }

        [Sync]
        public int Facing { get; set; }


        public IEnumerable<Pair<CPos,SubCell>> OccupiedCells()
        {
            return new[] { Pair.New(TopLeft, SubCell.FullCell) };
        }

        public bool CanEnterCell(CPos a,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            return false;
        }


        public void SetPosition(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {

        }

        public void SetPosition(Actor self,WPos pos)
        {

        }

        public void SetVisualPosition(Actor self,WPos pos)
        {

        }

        public void Created(Actor self)
        {

        }


        public bool IsLeavingCell(CPos location,SubCell subCell = SubCell.Any) { return false; }



        public void AddedToWorld(Actor self)
        {

        }

        public void RemovedFromWorld(Actor self)
        {

        }

        public void ModifyDeathActorInit(Actor self,TypeDictionary init)
        {

        }

    }
}