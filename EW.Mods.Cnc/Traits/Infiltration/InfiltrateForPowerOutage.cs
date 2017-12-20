using System;
using EW.Traits;

namespace EW.Mods.Cnc.Traits
{
    class InfiltrateForPowerOutageInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new InfiltrateForPowerOutage(init.Self, this);
        }

    }

    class InfiltrateForPowerOutage
    {
        public InfiltrateForPowerOutage(Actor self,InfiltrateForPowerOutageInfo info)
        {

        }
    }
}