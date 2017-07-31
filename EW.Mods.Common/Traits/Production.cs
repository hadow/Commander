using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class ProductionInfo : ITraitInfo
    {

        public virtual object Create(ActorInitializer init) { return new Production(); }
    }


    class Production
    {
    }
}