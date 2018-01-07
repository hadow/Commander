using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class GivesBuildableAreaInfo : ConditionalTraitInfo
    {

        public readonly HashSet<string> AreaTypes = new HashSet<string>();

        public override object Create(ActorInitializer init)
        {
            return new GivesBuildableArea(this);
        }
    }
    public class GivesBuildableArea:ConditionalTrait<GivesBuildableAreaInfo>
    {

        readonly HashSet<string> noAreaTypes = new HashSet<string>();

        public HashSet<string> AreaTypes{
            get{
                return IsTraitDisabled ? noAreaTypes : Info.AreaTypes; 
            }
        }
        public GivesBuildableArea(GivesBuildableAreaInfo info):base(info){


        }
    }
}
