using System;
using System.Collections.Generic;
using EW.Traits;
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


        public bool CanEnterCell(CPos cell,Actor ignoreActor = null,bool checkTransientActors = true) { return true; }


        public void SetPosition(Actor self,WPos pos)
        {

            
        }


        #endregion

        #region Implement IMove

        #endregion
    }
}