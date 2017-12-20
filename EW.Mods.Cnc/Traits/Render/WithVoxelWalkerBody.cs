using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Cnc.Traits.Render
{
    public class WithVoxelWalkerBodyInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new WithVoxelWalkerBody();
        }
    }

    public class WithVoxelWalkerBody
    {

    }
}