using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{
    class EngineerRepairableInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new EngineerRepairable(); }
    }
    class EngineerRepairable
    {
    }
}