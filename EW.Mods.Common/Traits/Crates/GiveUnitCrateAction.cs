﻿using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{


    class GiveUnitCrateActionInfo : CrateActionInfo
    {

    }

    class GiveUnitCrateAction:CrateAction
    {

        public GiveUnitCrateAction(Actor self,GiveUnitCrateActionInfo info) : base(self, info) { }
    }
}