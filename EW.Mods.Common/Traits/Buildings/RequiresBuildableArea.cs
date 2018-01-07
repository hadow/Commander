using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class RequiresBuildableAreaInfo:TraitInfo<RequiresBuildableArea>,Requires<BuildingInfo>{

        public readonly HashSet<string> AreaTypes = new HashSet<string>();


        /// <summary>
        /// Maximum range from the actor with 'GivesBuildableArea this can be placed at.
        /// </summary>
        public readonly int Adjacent = 2;


    }

    public class RequiresBuildableArea
    {
        public RequiresBuildableArea()
        {
        }
    }
}
