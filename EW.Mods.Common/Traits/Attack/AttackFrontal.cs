using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AttackFrontalInfo : AttackBaseInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }

    public class AttackFrontal:AttackBase
    {

        public AttackFrontal(Actor self, AttackFrontalInfo info):base(self,info)
        {

        }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new Activities.Attack(self, newTarget, allowMove, forceAttack);
        }
    }
}