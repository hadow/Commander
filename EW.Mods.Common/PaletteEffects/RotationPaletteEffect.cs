using System;


namespace EW.Mods.Common.Traits
{
    class RotationPaletteEffectInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new RotationPaletteEffect();
        }
    }
    class RotationPaletteEffect
    {
    }
}