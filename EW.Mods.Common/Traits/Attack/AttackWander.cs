using System;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    class AttackWanderInfo : WandersInfo, Requires<AttackMoveInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackWander(init.Self, this);
        }
    }
    class AttackWander:Wanders
    {
        readonly AttackMove attackMove;

        public AttackWander(Actor self,AttackWanderInfo info) : base(self, info)
        {
            attackMove = self.Trait<AttackMove>();
        }

        public override void DoAction(Actor self, CPos targetCell)
        {
            attackMove.ResolveOrder(self, new Order("AttackMove", self, Target.FromCell(self.World, targetCell),false));
        }
    }
}