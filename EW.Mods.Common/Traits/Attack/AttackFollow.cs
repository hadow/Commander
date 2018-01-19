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
        public AttackFollow(Actor self,AttackBaseInfo info) : base(self, info)
        {

        }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new AttackActivity(self, newTarget, allowMove, forceAttack);
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

        }
        class AttackActivity : Activity
        {
            readonly AttackFollow attack;
            readonly IMove move;
            readonly Target target;
            readonly bool forceAttack;
            readonly bool onRailsHack;


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
                throw new NotImplementedException();
            }


        }
    }
}