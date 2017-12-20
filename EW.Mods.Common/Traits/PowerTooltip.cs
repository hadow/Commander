using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PowerTooltipInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new PowerTooltip();
        }
    }


    public class PowerTooltip
    {

    }
}