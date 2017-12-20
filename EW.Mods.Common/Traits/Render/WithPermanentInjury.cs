using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{
    public class WithPermanentInjuryInfo:ITraitInfo
    {

        public object Create(ActorInitializer init)
        {
            return new WithPermanentInjury();
        }

    }

    public class WithPermanentInjury
    {

    }
}