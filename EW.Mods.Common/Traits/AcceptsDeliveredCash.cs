using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AcceptsDeliveredCashInfo:ITraitInfo
    {

        public object Create(ActorInitializer init) { return new AcceptsDeliveredCash(); }

    }


    public class AcceptsDeliveredCash
    {

    }
}