using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    [Desc("This actor requires another actor with 'GivesBuildableArea' trait around to be placed.")]

    public class RequiresBuildableAreaInfo:TraitInfo<RequiresBuildableArea>,Requires<BuildingInfo>{

        public readonly HashSet<string> AreaTypes = new HashSet<string>();


        /// <summary>
        /// Maximum range from the actor with 'GivesBuildableArea this can be placed at.
        /// </summary>
        public readonly int Adjacent = 2;


    }

    public class RequiresBuildableArea{}
}
