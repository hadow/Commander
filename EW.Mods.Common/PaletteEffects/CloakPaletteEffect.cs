using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
using EW.Primitives;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    public class CloakPaletteEffectInfo : TraitInfo<CloakPaletteEffect>
    {

    }


    public class CloakPaletteEffect:IPaletteModifier,ITick
    {

        float t = 0;
        string paletteName = "cloak";

        public void AdjustPalette(IReadOnlyDictionary<string,MutablePalette> b)
        {

        }

        public void Tick(Actor self)
        {

        }

    }
}