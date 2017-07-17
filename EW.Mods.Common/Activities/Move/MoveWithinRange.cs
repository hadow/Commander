using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class MoveWithinRange:MoveAdjacentTo
    {
        public MoveWithinRange(Actor self,Target target,WDist minRange,WDist maxRange) : base(self, target)
        {

        }



    }
}