using System;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits
{
    public class ShroudRendererInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new ShroudRenderer();
        }
    }
    public class ShroudRenderer
    {
    }
}