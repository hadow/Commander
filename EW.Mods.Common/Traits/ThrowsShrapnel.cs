using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class ThrowsShrapnelInfo:ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ThrowsShrapnel(); }
    }


    public class ThrowsShrapnel
    {

    }
}