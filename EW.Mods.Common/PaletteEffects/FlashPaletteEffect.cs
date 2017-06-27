using System;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class FlashPaletteEffectInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new FlashPaletteEffect();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class FlashPaletteEffect:IPaletteModifier,ITick
    {

        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> palettes)
        {

        }


        public void Tick(Actor self)
        {

        }
    }
}