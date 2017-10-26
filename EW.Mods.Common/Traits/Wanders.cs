using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WandersInfo : ConditionalTraitInfo,Requires<IMoveInfo>
    {
        public readonly int WanderMoveRadius = 1;

        public readonly int ReduceMoveRadiusDelay = 5;

        public readonly int MinMoveDelay = 0;

        public readonly int MaxMoveDelay = 0;



        public override object Create(ActorInitializer init)
        {
            return new Wanders(init.Self, this);
        }
    }
    public class Wanders:ConditionalTrait<WandersInfo>,INotifyIdle,INotifyBecomingIdle
    {
        readonly Actor self;

        readonly WandersInfo info;

        IResolveOrder move;

        int countDown;
        int ticksIdle;
        int effectiveMoveRadius;
        bool firstTick = true;

        public Wanders(Actor self,WandersInfo info) : base(info)
        {
            this.self = self;
            this.info = info;
            effectiveMoveRadius = info.WanderMoveRadius;
        }

        protected override void Created(Actor self)
        {
            move = self.Trait<IMove>() as IResolveOrder;
            base.Created(self);
        }


        protected virtual void TickIdle(Actor self)
        {
            if (IsTraitDisabled)
                return;
        }

        void INotifyIdle.TickIdle(Actor self)
        {
            TickIdle(self);
        }

        protected virtual void OnBecomingIdle(Actor self)
        {
            countDown = self.World.SharedRandom.Next(info.MinMoveDelay, info.MaxMoveDelay);
        }

        void INotifyBecomingIdle.OnBecomingIdle(Actor self)
        {
            OnBecomingIdle(self);
        }

    }
}