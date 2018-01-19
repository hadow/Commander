using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Will open and be passable for actors that appear friendly when there are no enemies in range.
    /// 
    /// </summary>
    public class GateInfo:PausableConditionalTraitInfo,IBlocksProjectilesInfo,Requires<BuildingInfo>
    {
        public readonly string OpeningSound = null;

        public readonly string ClosingSound = null;
        /// <summary>
        /// Ticks until the gate closes.
        /// </summary>
        public readonly int CloseDelay = 150;

        /// <summary>
        /// Ticks untile the gate is considered open.
        /// </summary>
        public readonly int TransitionDelay = 33;

        /// <summary>
        /// The height of the blocks projectiles.
        /// 阻止射弹高度
        /// </summary>
        public readonly WDist BlocksProjectilesHeight = new WDist(640);


        public override object Create(ActorInitializer init)
        {
            return new Gate(init, this);
        }

    }

    public class Gate :PausableConditionalTrait<GateInfo>,
    ITick,
        ISync,
    INotifyAddedToWorld,
        INotifyRemovedFromWorld,
        ITemporaryBlocker,
        IBlocksProjectiles,INotifyBlockingMove
    {

        readonly GateInfo info;
        readonly Actor self;
        readonly Building building;
        public readonly int OpenPosition;

        IEnumerable<CPos> blockedPositions;

        [Sync]
        public int Position { get; private set; }

        int desiredPosition;
        int remainingOpenTime;

        public readonly IEnumerable<CPos> Footprint;
        public Gate(ActorInitializer init,GateInfo info):base(info)
        {
            this.info = info;
            self = init.Self;
            building = self.Trait<Building>();
            OpenPosition = info.TransitionDelay;
            blockedPositions = building.Info.Tiles(self.Location);
            Footprint = blockedPositions;
        }


        void ITick.Tick(Actor self)
        {
            if (IsTraitDisabled || IsTraitPaused || building.Locked || !building.BuildComplete)
                return;

            if (desiredPosition < Position)
            {
                //Gate was fully open
                if(Position == OpenPosition)
                {
                    WarGame.Sound.Play(SoundType.World, info.ClosingSound, self.CenterPosition);
                    self.World.ActorMap.AddInfluence(self, building);
                }

                Position--;
            }
            else if(desiredPosition > Position)
            {
                //Gate was fully closed.
                if(Position == 0){
                    
                }

                Position++;

                //Gate is now fully open.
                if(Position == OpenPosition){
                    self.World.ActorMap.RemoveInfluence(self,building);
                    remainingOpenTime = Info.CloseDelay;

                }
            }

            if(Position == OpenPosition)
            {
                if (IsBlocked())
                    remainingOpenTime = info.CloseDelay;
                else if (--remainingOpenTime <= 0)
                    desiredPosition = 0;
            }
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            blockedPositions = Footprint;
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self){

            blockedPositions = Enumerable.Empty<CPos>();

        }

        bool ITemporaryBlocker.CanRemoveBlockage(Actor self,Actor blocking){
            return CanRemoveBlockage(self,blocking);
        }

        bool ITemporaryBlocker.IsBlocking(Actor self,CPos cell){
            return Position != OpenPosition && blockedPositions.Contains(cell);
        }


        bool CanRemoveBlockage(Actor self,Actor blocking){

            return !IsTraitDisabled && !IsTraitPaused && building.BuildComplete && !building.Locked && blocking.AppearsFriendlyTo(self);
        }



        bool IsBlocked()
        {
            return blockedPositions.Any(loc => self.World.ActorMap.GetActorsAt(loc).Any(a => a != self));
        }


        WDist IBlocksProjectiles.BlockingHeight{
            get{
                return new WDist(Info.BlocksProjectilesHeight.Length * (OpenPosition - Position) / OpenPosition);
            }
        }

        void INotifyBlockingMove.OnNotifyBlockingMove(Actor self, Actor blocking)
        {
            if (Position != OpenPosition && CanRemoveBlockage(self, blocking))
                desiredPosition = OpenPosition;
        }
    }
}