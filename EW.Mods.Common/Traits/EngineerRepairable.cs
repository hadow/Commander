using System;
using System.Collections.Generic;
using EW.Traits;

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