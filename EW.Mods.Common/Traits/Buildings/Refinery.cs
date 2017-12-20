using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// 精练厂
    /// </summary>
    public class RefineryInfo : Requires<WithSpriteBodyInfo>,IAcceptResourcesInfo
    {
        public virtual object Create(ActorInitializer init) { return new Refinery(init.Self,this); }
    }
    public class Refinery
    {

        public Refinery(Actor self,RefineryInfo info)
        {

        }
    }


}