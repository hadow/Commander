using System;

namespace EW.Mods.Common.Traits
{
    class HealUnitsCrateActionInfo:CrateActionInfo
    {

    }

    class HealUnitsCrateAction : CrateAction
    {
        public HealUnitsCrateAction(Actor self,HealUnitsCrateActionInfo info) : base(self, info) { }
    }
}