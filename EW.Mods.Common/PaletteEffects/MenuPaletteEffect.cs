using System;


namespace EW.Mods.Common.Traits
{
    public class MenuPaletteEffectInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new MenuPaletteEffect();
        }
    }
    public class MenuPaletteEffect
    {
    }
}