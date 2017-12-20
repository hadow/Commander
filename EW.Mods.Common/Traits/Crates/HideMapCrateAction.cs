using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class HideMapCrateActionInfo:CrateActionInfo
    {

    }

    class HideMapCrateAction : CrateAction
    {
        public HideMapCrateAction(Actor self,HideMapCrateActionInfo info) : base(self, info)
        {

        }
    }
}