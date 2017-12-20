using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{
    class AttackOrderPowerInfo:SupportPowerInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new AttackOrderPower(init.Self,this);
        }
    }

    class AttackOrderPower : SupportPower
    {
        public AttackOrderPower(Actor self,AttackOrderPowerInfo info):base(self,info)
        {

        }
    }
}