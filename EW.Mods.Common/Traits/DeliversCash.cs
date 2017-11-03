using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    class DeliversCashInfo:ITraitInfo
    {


        public object Create(ActorInitializer init) { return new DeliversCash(); }
    }

    class DeliversCash
    {

    }
}