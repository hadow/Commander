using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{


    class CrateInfo : ITraitInfo,IPositionableInfo,Requires<RenderSpritesInfo>
    {
        public object Create(ActorInitializer init)
        {
            return new Crate();
        }
    }
    class Crate
    {
    }
}