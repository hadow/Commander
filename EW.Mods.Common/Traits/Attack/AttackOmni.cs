using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// 全方位作战
    /// </summary>
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
            return new SetTarget(this, newTarget, allowMove);
        }


        protected class SetTarget : Activity
        {
            readonly Target target;
            readonly AttackOmni attack;
            readonly bool allowMove;

            public SetTarget(AttackOmni attack,Target target,bool allowMove)
            {
                this.target = target;
                this.attack = attack;
                this.allowMove = allowMove;
            }


            public override Activity Tick(Actor self)
            {
                if (IsCanceled || !target.IsValidFor(self) || !attack.IsReachableTarget(target, allowMove))
                    return NextActivity;

                attack.DoAttack(self, target);
                return this;
            }
        }
    }
}