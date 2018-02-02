using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Primitives;
using EW.Activities;
namespace EW.Mods.Common.Traits
{

    public class AircraftInfo : ITraitInfo
    {
        //巡航高度
        public readonly WDist CruiseAltitude = new WDist(1280);
        public readonly WDist IdealSeparation = new WDist(1706);


        public readonly int InitialFacing = 0;

        public readonly int TurnSpeed = 255;

        public readonly int Speed = 1;

        public readonly HashSet<string> LandableTerrainTypes = new HashSet<string>();

        public readonly bool MoveIntoShroud = true;

        [VoiceReference]
        public readonly string Voice = "Action";


        public object Create(ActorInitializer init)
        {
            return new Aircraft();
        }
    }

    public class Aircraft:ITick,ISync,IFacing,IPositionable,IMove,IDeathActorInitModifier,INotifyCreated,INotifyAddedToWorld,INotifyRemovedFromWorld,INotifyActorDisposing
    {

        static readonly Pair<CPos, SubCell>[] NoCells = { };

        public readonly AircraftInfo Info;

        readonly Actor self;

        ConditionManager conditionManager;

        bool isMoving;
        bool isMovingVertically;
        WPos cachedPosition;
        bool? landNow;
        
        [Sync]
        public int Facing { get; set; }

        [Sync]
        public WPos CenterPosition { get; private set; }


        public int TurnSpeed { get { return Info.TurnSpeed; } }

        public CPos TopLeft { get { return self.World.Map.CellContaining(CenterPosition); } }

        public Pair<CPos,SubCell>[] OccupiedCells() { return NoCells; }
        void INotifyCreated.Created(Actor self)
        {
            conditionManager = self.TraitOrDefault<ConditionManager>();

        }

        void ITick.Tick(Actor self)
        {

        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            AddedToWorld(self);
        }

        protected virtual void AddedToWorld(Actor self)
        {
            self.World.AddToMaps(self, this);

        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {

        }

        void INotifyActorDisposing.Disposing(Actor self)
        {

        }

        void IDeathActorInitModifier.ModifyDeathActorInit(Actor self, Primitives.TypeDictionary inits)
        {
            inits.Add(new FacingInit(Facing));
        }



        #region Implement IPositionable

        public bool IsLeavingCell(CPos location,SubCell subCell = SubCell.Any) { return false; }

        public bool CanEnterCell(CPos cell,Actor ignoreActor = null,bool checkTransientActors = true) { return true; }

        public SubCell GetValidSubCell(SubCell preferred) { return SubCell.Invalid; }

        public SubCell GetAvailableSubCell(CPos a,SubCell preferredSubCell = SubCell.Any,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            return SubCell.Invalid;
        }


        public void SetVisualPosition(Actor self,WPos pos) { SetPosition(self, pos); }

        public void SetPosition(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {

        }

        public void SetPosition(Actor self,WPos pos)
        {

            
        }



        #endregion

        #region Implement IMove

        public Activity MoveTo(CPos cell,int nearEnough)
        {
            throw new NotImplementedException();
        }

        public Activity MoveTo(CPos cell,Actor ignoreActor)
        {
            throw new NotImplementedException();
        }

        public Activity MoveWithinRange(Target target,WDist range)
        {
            throw new NotImplementedException();
        }

        public Activity MoveWithinRange(Target target,WDist minRange,WDist maxRange)
        {
            throw new NotImplementedException();
        }


        public Activity MoveFollow(Actor self,Target target,WDist minRange,WDist maxRange)
        {
            throw new NotImplementedException();
        }

        public Activity MoveIntoWorld(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {
            throw new NotImplementedException();
        }

        public Activity MoveToTarget(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public Activity MoveIntoTarget(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public Activity VisualMove(Actor self,WPos fromPos,WPos toPos)
        {
            throw new NotImplementedException();
        }


        public CPos NearestMoveableCell(CPos cell) { return cell; }

        public bool IsMoving { get { return isMoving; }set { } }

        public bool CanEnterTargetNow(Actor self,Target target)
        {
            return true;
        }
        #endregion
    }
}