using System;
using EW.Graphics;
using EW.Traits;
using System.Collections.Generic;
using EW.Framework;
using System.Drawing;
namespace EW.Mods.Common.Traits
{
    using GUtil = EW.Graphics.Util;

    /// <summary>
    /// Used for bursted one-colored whole screen effects.
    /// </summary>
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

            var frac = (float)remainingFrames / Info.Length;

            foreach(var pal in palettes)
            {
                for(var x = 0; x < Palette.Size; x++)
                {
                    var orig = pal.Value.GetColor(x);
                    var c = Info.Color;
                    var color = Color.FromArgb(orig.A, ((int)c.R).Clamp(0, 255), ((int)c.G).Clamp(0, 255), ((int)c.B).Clamp(0, 255));
                    var final = GUtil.PremultipliedColorLerp(frac, orig, GUtil.PremultiplyAlpha(Color.FromArgb(orig.A, color)));
                    pal.Value.SetColor(x, final);
                }
            }
        }


        public void Tick(Actor self)
        {
            if (remainingFrames > 0)
                remainingFrames--;
        }
    }
}