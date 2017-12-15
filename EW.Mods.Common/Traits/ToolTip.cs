using System;

using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public abstract class TooltipInfoBase : ConditionalTraitInfo
    {
    }

    public class TooltipInfo : TooltipInfoBase
    {
        public override object Create(ActorInitializer init)
        {
            return new Tooltip(init.Self,this);
        }
    }


    public class EditorOnlyTooltipInfo : TooltipInfoBase
    {
        public override object Create(ActorInitializer init)
        {
            return this;
        }
    }
    public class Tooltip
    {

        public Tooltip(Actor self,TooltipInfo info)
        {

        }
    }
}