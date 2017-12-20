using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{

    public class GrantExternalConditionCrateActionInfo : CrateActionInfo
    {

    }

    public class GrantExternalConditionCrateAction:CrateAction
    {
        public GrantExternalConditionCrateAction(Actor self,GrantExternalConditionCrateActionInfo info) : base(self, info)
        {

        }
    }
}