using System;
using EW.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Unit got to face the target.
    /// </summary>
    public class AttackFrontalInfo : AttackBaseInfo,Requires<IFacingInfo>
    {
        //误差
        public readonly int FacingTolerance = 0;

        public override object Create(ActorInitializer init)
        {
            return new AttackFrontal(init.Self, this);
        }

        public override void RulesetLoaded(Ruleset rules, ActorInfo info)
        {
            base.RulesetLoaded(rules, info);

            if (FacingTolerance < 0 || FacingTolerance > 128)
                throw new YamlException("Facing tolerance must be in range of [0,128],128 covers 360 degrees.");
        }
    }

    public class AttackFrontal:AttackBase
    {
        readonly AttackFrontalInfo info;

        public AttackFrontal(Actor self, AttackFrontalInfo info):base(self,info)
        {
            this.info = info;
        }

        protected override bool CanAttack(Actor self, Target target)
        {
            if (!base.CanAttack(self, target))
                return false;

            var pos = self.CenterPosition;
            var targetedPosition = GetTargetPosition(pos, target);
            var delta = targetedPosition - pos;

            if (delta.HorizontalLengthSquared == 0)
                return true;

            return Util.FacingWithinTolerance(facing.Facing, delta.Yaw.Facing, info.FacingTolerance);
        }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new Activities.Attack(self, newTarget, allowMove, forceAttack,info.FacingTolerance);
        }
    }
}