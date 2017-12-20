using System;

namespace EW.Mods.Common.Traits
{

    class GiveMcvCrateActionInfo : GiveUnitCrateActionInfo
    {

    }

    class GiveMcvCrateAction:GiveUnitCrateAction
    {

        public GiveMcvCrateAction(Actor self,GiveMcvCrateActionInfo info):base(self,info)
        {

        }
    }
}