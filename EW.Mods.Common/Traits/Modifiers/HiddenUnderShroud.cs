using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class HiddenUnderShroudInfo:ITraitInfo{

        public virtual object Create(ActorInitializer init){
            return new HiddenUnderShroud(this);
        }

    }
    public class HiddenUnderShroud
    {
        protected readonly HiddenUnderShroudInfo Info;


        public HiddenUnderShroud(HiddenUnderShroudInfo info)
        {
            Info = info;
        }


    }
}
