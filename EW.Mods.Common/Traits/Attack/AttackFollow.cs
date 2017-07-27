using System;
using EW.Activities;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

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
        public override void ResolveOrder(Actor self, Order order)
        {
            base.ResolveOrder(self, order);

            if (order.OrderString == "Stop")
                Target = Target.Invalid;
        }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new AttackActivity(self, newTarget, allowMove, forceAttack);
        }

        public virtual void Tick(Actor self)
        {

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