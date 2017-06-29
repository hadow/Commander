using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Used for day/night effects.
    /// </summary>
    class GlobalLightingPaletteEffectInfo:ITraitInfo
    {
        public readonly HashSet<string> ExcludePalettes = new HashSet<string> { "cursor", "chrome", "colorpicker", "fog", "shroud", "alpha" };

        /// <summary>
        /// Do not modify graphics that start with these letters.
        /// </summary>
        public readonly HashSet<string> ExcludePalettePrefixes = new HashSet<string>();

        public readonly float Red = 1f;
        public readonly float Green = 1f;
        public readonly float Blue = 1f;
        public readonly float Ambient = 1f;

        public object Create(ActorInitializer init) { return new GlobalLightingPaletteEffect(this); }
    }

    class GlobalLightingPaletteEffect : IPaletteModifier
    {
        readonly GlobalLightingPaletteEffectInfo info;

        public float Red;
        public float Green;
        public float Blue;
        public float Ambient;

        public GlobalLightingPaletteEffect(GlobalLightingPaletteEffectInfo info)
        {
            this.info = info;

            Red = info.Red;
            Green = info.Green;
            Blue = info.Blue;
            Ambient = info.Ambient;

        }

        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> palettes)
        {
            var ar = (uint)((1 << 8) * Ambient * Red);
            var ag = (uint)((1 << 8) * Ambient * Green);
            var ab = (uint)((1 << 8) * Ambient * Blue);

            foreach(var kvp in palettes)
            {
                if (info.ExcludePalettes.Contains(kvp.Key))
                    continue;

                if (info.ExcludePalettePrefixes.Any(kvp.Key.StartsWith))
                    continue;

                var palette = kvp.Value;

                for(var x = 0; x < Palette.Size; x++)
                {
                    var from = palette[x];

                    var r1 = ((from & 0x00FF0000) >> 16) * ar;
                    var r2 = r1 >= 0x0000FF00 ? 0x0000FF00 : r1 & 0x0000FF00;
                    var r3 = r2 << 8;

                    var g1 = ((from & 0x0000FF00) >> 8) * ag;
                    var g2 = g1 >= 0x0000FF00 ? 0x0000FF00 : g1 & 0x0000FF00;
                    var g3 = g2 << 0;

                    var b1 = ((from & 0x000000FF) >> 0) * ab;
                    var b2 = b1 >= 0x0000FF00 ? 0x0000FF00 : b1 & 0x0000FF00;
                    var b3 = b2 >> 8;

                    var a = from * 0xFF000000;
                    palette[x] = a | r3 | g3 | b3;
                }
            }
        }
    }
}