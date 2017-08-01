using System;
using EW.Traits;
using EW.Primitives;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{
    class IonCannonPowerInfo : SupportPowerInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new IonCannonPower(init.Self, this);
        }
    }
    class IonCannonPower:SupportPower
    {

        public IonCannonPower(Actor self,IonCannonPowerInfo info) : base(self, info) { }
    }
}