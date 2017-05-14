using System;


namespace EW.Mods.Common.Traits
{


    public abstract class TooltipInfoBase : ITraitInfo
    {
        public abstract object Create(ActorInitializer init);
    }

    public class TooltipInfo : TooltipInfoBase
    {
        public override object Create(ActorInitializer init)
        {
            return new Tooltip(init.Self,this);
        }
    }
    public class Tooltip
    {

        public Tooltip(Actor self,TooltipInfo info)
        {

        }
    }
}