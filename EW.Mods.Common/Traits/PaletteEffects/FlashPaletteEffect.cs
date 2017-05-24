using System;

namespace EW.Mods.Common.Traits
{
    public class FlashPaletteEffectInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new FlashPaletteEffect();
        }
    }
    public class FlashPaletteEffect
    {
    }
}