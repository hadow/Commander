using System;
using EW.Activities;
using EW.Traits;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{

    public class AttackHeliInfo : AttackFrontalInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackHeli(init.Self, this);
        }
    }
    public class AttackHeli:AttackFrontal
    {

        public AttackHeli(Actor self,AttackHeliInfo info) : base(self, info) { }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new HeliAttack(self, newTarget);
        }

    }
}