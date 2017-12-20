using System;
using EW.Traits;

namespace EW.Mods.Cnc.Traits.Render
{
    public class WithVoxelUnloadBodyInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new WithVoxelUnloadBody();
        }
    }

    public class WithVoxelUnloadBody
    {

    }
}