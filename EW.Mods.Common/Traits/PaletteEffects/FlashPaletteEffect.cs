using System;
using EW.Graphics;
using EW.Traits;
using System.Collections.Generic;
using EW.Xna.Platforms;
namespace EW.Mods.Common.Traits
{
    public class FlashPaletteEffectInfo : ITraitInfo
    {

        public readonly HashSet<string> ExcludePalettes = new HashSet<string> { "cursor", "chrome", "colorpicker", "fog", "shroud" };

        /// <summary>
        /// Measured in ticks
        /// </summary>
        public readonly int Length = 20;

        public readonly Color Color = Color.White;

        public readonly string Type = null;



        public object Create(ActorInitializer init)
        {
            return new FlashPaletteEffect(this);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class FlashPaletteEffect:IPaletteModifier,ITick
    {
        public readonly FlashPaletteEffectInfo Info;

        int remainingFrames;

        public FlashPaletteEffect(FlashPaletteEffectInfo info)
        {
            Info = info;
        }
        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> palettes)
        {
            if (remainingFrames == 0)
                return;
        }


        public void Tick(Actor self)
        {
            if (remainingFrames > 0)
                remainingFrames--;
        }
    }
}