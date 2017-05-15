using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{

    public class GainsExperienceInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new GainsExperience(); }
    }

    public class GainsExperience
    {
    }
}