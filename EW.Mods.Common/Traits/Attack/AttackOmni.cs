using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AttackOmniInfo : AttackBaseInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackOmni(init.Self, this);
        }
    }
    public class AttackOmni:AttackBase
    {


        public AttackOmni(Actor self,AttackOmniInfo info) : base(self, info) { }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            throw new NotImplementedException();
        }
    }
}