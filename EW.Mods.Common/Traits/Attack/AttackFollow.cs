using System;
using EW.Activities;
using EW.Traits;
using EW.NetWork;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Actor will follow units until in range to attack them.
    /// </summary>
    public class AttackFollowInfo : AttackBaseInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackFollow(init.Self, this);
        }
    }
    public class AttackFollow:AttackBase,ITick,INotifyOwnerChanged
    {
        public Target Target { get; protected set; }
        public AttackFollow(Actor self,AttackBaseInfo info) : base(self, info){}

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new AttackActivity(self, newTarget, allowMove, forceAttack);
        }


        void ITick.Tick(Actor self){
            Tick(self);
        }
        public virtual void Tick(Actor self)
        {
            if(IsTraitDisabled)
            {
                Target = Target.Invalid;
                return;
            }

            DoAttack(self, Target);
            IsAniming = Target.IsValidFor(self);
        }

        public void OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            Target = Target.Invalid;
        }
        class AttackActivity : Activity
        {
            readonly AttackFollow attack;
            readonly IMove move;
            readonly Target target;
            readonly bool forceAttack;

            bool hasTicked;

            public AttackActivity(Actor self,Target target,bool allowMove,bool forceAttack)
            {
                attack = self.Trait<AttackFollow>();
                move = allowMove ? self.TraitOrDefault<IMove>() : null;

                var mobile = move as Mobile;

                this.target = target;
                this.forceAttack = forceAttack;
            }
            public override Activity Tick(Actor self)
            {
                if (IsCanceled || !target.IsValidFor(self))
                    return NextActivity;

                if (attack.IsTraitPaused)
                    return this;

                var weapon = attack.ChooseArmamentsForTarget(target, forceAttack).FirstEnabledTraitOrDefault();
                if(weapon != null)
                {
                    var targetIsMobile = (target.Type == TargetT.Actor && target.Actor.Info.HasTraitInfo<IMoveInfo>())
                        || (target.Type == TargetT.FrozenActor && target.FrozenActor.Info.HasTraitInfo<IMoveInfo>());

                    var modifiedRange = weapon.MaxRange();
                    var maxRange = targetIsMobile ? new WDist(Math.Max(weapon.Weapon.MinRange.Length, modifiedRange.Length - 1024)) : modifiedRange;

                    if (hasTicked && attack.Target.Type == TargetT.Invalid)
                        return NextActivity;

                    attack.Target = target;
                    hasTicked = true;

                    if (move != null)
                        return ActivityUtils.SequenceActivities(move.MoveFollow(self, target, weapon.Weapon.MinRange, maxRange), this);

                    if (target.IsInRange(self.CenterPosition, weapon.MaxRange()) && !target.IsInRange(self.CenterPosition, weapon.Weapon.MinRange))
                        return this;


                        
                }

                attack.Target = Target.Invalid;

                return NextActivity;
            }


        }
    }
}