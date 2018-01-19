using System;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.NetWork;
namespace EW.Mods.Cnc.Traits
{
    class AttackOrderPowerInfo:SupportPowerInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new AttackOrderPower(init.Self,this);
        }
    }

    class AttackOrderPower : SupportPower,INotifyCreated,INotifyBurstComplete
    {
        readonly AttackOrderPowerInfo info;
        AttackBase attack;


        public AttackOrderPower(Actor self,AttackOrderPowerInfo info):base(self,info)
        {
            this.info = info;
        }

        public override void Activate(Actor self, Order order, SupportPowerManager manager)
        {
            base.Activate(self, order, manager);
            attack.AttackTarget(Target.FromCell(self.World, order.TargetLocation), false, false, true);
        }

        protected override void Created(Actor self)
        {
            attack = self.Trait<AttackBase>();
            base.Created(self);
        }

        void INotifyBurstComplete.FiredBurst(Actor self, Target target, Armament a)
        {
            self.World.IssueOrder(new Order("Stop", self, false));
        }
    }
}