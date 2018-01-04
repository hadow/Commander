using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Will open and be passable for actors that appear friendly when there are no enemies in range.
    /// </summary>
    public class GateInfo:BuildingInfo
    {
        public readonly string OpeningSound = null;

        public readonly string ClosingSound = null;

        public readonly int CloseDelay = 150;

        public readonly int TransitionDelay = 33;

        public readonly WDist BlocksProjectilesHeight = new WDist(640);


        public override object Create(ActorInitializer init)
        {
            return new Gate(init, this);
        }

    }

    public class Gate : Building,ITick,ISync
    {

        readonly GateInfo info;
        readonly Actor self;

        public readonly int OpenPosition;

        IEnumerable<CPos> blockedPositions;

        [Sync]
        public int Position { get; private set; }

        int desiredPosition;
        int remainingOpenTime;

        public Gate(ActorInitializer init,GateInfo info):base(init,info)
        {
            this.info = info;
            self = init.Self;
            OpenPosition = info.TransitionDelay;

        }


        void ITick.Tick(Actor self)
        {
            if (self.IsDisabled() || Locked || !BuildComplete)
                return;

            if (desiredPosition < Position)
            {
                //Gate was fully open
                if(Position == OpenPosition)
                {
                    WarGame.Sound.Play(SoundType.World, info.ClosingSound, self.CenterPosition);
                    self.World.ActorMap.AddInfluence(self, this);
                }

                Position--;
            }
            else if(desiredPosition > Position)
            {

            }

            if(Position == OpenPosition)
            {
                if (IsBlocked())
                    remainingOpenTime = info.CloseDelay;
                else if (--remainingOpenTime <= 0)
                    desiredPosition = 0;
            }
        }

        public override void AddedToWorld(Actor self)
        {
            base.AddedToWorld(self);
            blockedPositions = info.Tiles(self.Location);
        }

        bool IsBlocked()
        {
            return blockedPositions.Any(loc => self.World.ActorMap.GetActorsAt(loc).Any(a => a != self));
        }
    }
}