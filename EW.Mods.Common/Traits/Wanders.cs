using EW.Traits;
using EW.NetWork;
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

            if (firstTick)
            {
                countDown = self.World.SharedRandom.Next(info.MinMoveDelay, info.MaxMoveDelay);
                firstTick = false;
                return;
            }

            if (--countDown > 0)
                return;

            var targetCell = PickTargetLocation();
            if (targetCell != CPos.Zero)
                DoAction(self, targetCell);
        }

        CPos PickTargetLocation()
        {
            var target = self.CenterPosition + new WVec(0, -1024 * effectiveMoveRadius, 0).Rotate(WRot.FromFacing(self.World.SharedRandom.Next(255)));
            var targetCell = self.World.Map.CellContaining(target);

            if (!self.World.Map.Contains(targetCell))
            {

                if (++ticksIdle % info.ReduceMoveRadiusDelay == 0)
                    effectiveMoveRadius--;

                return CPos.Zero;
            }

            ticksIdle = 0;
            effectiveMoveRadius = info.WanderMoveRadius;

            return targetCell;
        }

        public virtual void DoAction(Actor self,CPos targetCell)
        {
            move.ResolveOrder(self, new Order("Move", self, Target.FromCell(self.World, targetCell), false));
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