using System;
using System.Collections.Generic;
using EW.OpenGLES;
using EW.Primitives;
using EW.Graphics;
using EW.Traits;
using System.Drawing;
namespace EW.Mods.Common.Traits
{

    public class CloakPaletteEffectInfo : TraitInfo<CloakPaletteEffect>{}


    public class CloakPaletteEffect:IPaletteModifier,ITick
    {

        float t = 0;
        string paletteName = "cloak";


        Color[] colors =
        {
            Color.FromArgb(55,205,205,220),
            Color.FromArgb(120,205,205,230),
            Color.FromArgb(192,180,180,255),
            Color.FromArgb(178,205,250,220)
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> b)
        {
            var i = (int)t;
            var p = b[paletteName];

            for(var j = 0; j < colors.Length; j++)
            {
                var k = (i + j) % 16 + 0xb0;
                p.SetColor(k, colors[j]);
            }
        }

        public void Tick(Actor self)
        {
            t += 0.25f;
            if (t >= 256)
                t = 0;
        }

    }
}