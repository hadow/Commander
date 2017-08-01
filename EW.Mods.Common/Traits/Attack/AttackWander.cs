using System;
using EW.Traits;
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
        public AttackWander(Actor self,AttackWanderInfo info) : base(self, info)
        {

        }
    }
}