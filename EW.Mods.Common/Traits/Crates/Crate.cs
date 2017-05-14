using System;


namespace EW.Mods.Common.Traits
{


    class CrateInfo : ITraitInfo, Requires<RenderSpritesInfo>
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