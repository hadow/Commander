using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{


    public class GrantConditionOnPrerequisiteManagerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new GrantConditionOnPrerequisiteManager(init); }
    }

    public class GrantConditionOnPrerequisiteManager
    {


        public GrantConditionOnPrerequisiteManager(ActorInitializer init)
        {

        }
    }
}