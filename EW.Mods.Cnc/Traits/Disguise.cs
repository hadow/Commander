using System;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Cnc.Traits
{

    class DisguiseTooltipInfo : TooltipInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new DisguiseTooltip();
        }
    }

    class DisguiseTooltip
    {

    }


    class DisguiseInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Disguise();
        }
    }

    class Disguise
    {

    }
}