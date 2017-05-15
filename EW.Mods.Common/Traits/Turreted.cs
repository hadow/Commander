using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class TurretedInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new Turreted(); }
    }
    public class Turreted
    {
    }
}