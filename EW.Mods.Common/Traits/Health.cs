using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class HealthInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Health();
        }
    }
    public class Health
    {
    }
}