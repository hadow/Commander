using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class HiddenUnderShroudInfo:ITraitInfo{

        public virtual object Create(ActorInitializer init){
            return new HiddenUnderShroud(this);
        }

    }
    public class HiddenUnderShroud:IDefaultVisibility
    {
        protected readonly HiddenUnderShroudInfo Info;


        public HiddenUnderShroud(HiddenUnderShroudInfo info)
        {
            Info = info;
        }

        
        public bool IsVisible(Actor self,Player byPlayer)
        {
            return true;
        }
        
    }
}
