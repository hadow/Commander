using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class EntersTunnelsInfo:ITraitInfo
    {
        public object Create(ActorInitializer init) { return new EntersTunnels(); }
    }


    public class EntersTunnels
    {

    }
}