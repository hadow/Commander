using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AttackBomberInfo : AttackBaseInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackBomber(init.Self, this);
        }
    }
    public class AttackBomber:AttackBase
    {
        public AttackBomber(Actor self,AttackBomberInfo info) : base(self, info) { }


        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            throw new NotImplementedException();
        }
    }
}