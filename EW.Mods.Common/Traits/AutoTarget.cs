using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class AutoTargetInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            throw new NotImplementedException();
        }
    }
    public class AutoTarget
    {
    }


    class AutoTargetIgnoreInfo : TraitInfo<AutoTargetIgnore>
    {

    }

    class AutoTargetIgnore
    {

    }
}