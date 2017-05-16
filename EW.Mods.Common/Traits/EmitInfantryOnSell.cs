using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class EmitInfantryOnSellInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new EmitInfantryOnSell();
        }
    }

    public class EmitInfantryOnSell
    {
    }
}