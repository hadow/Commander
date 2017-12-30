using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
using EW.OpenGLES;
using System.Drawing;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Create a palette by applying alpha transparency to another palette.
    /// </summary>
    class PaletteFromPaletteWithAlphaInfo : ITraitInfo
    {
        [FieldLoader.Require,PaletteDefinition]
        public readonly string Name = null;

        [FieldLoader.Require,PaletteReference]
        public readonly string BasePalette = null;


        public readonly bool AllowModifiers = true;

        /// <summary>
        /// Alpha component that is applies to the base palette.
        /// </summary>
        public readonly float Alpha = 1.0f;


        /// <summary>
        /// Premultiply color by the alpha component.
        /// </summary>
        public readonly bool Premultiply = true;

        public object Create(ActorInitializer init) { return new PaletteFromPaletteWithAlpha(this); }
    }


    class PaletteFromPaletteWithAlpha:ILoadsPalettes,IProvidesAssetBrowserPalettes
    {
        readonly PaletteFromPaletteWithAlphaInfo info;

        public PaletteFromPaletteWithAlpha(PaletteFromPaletteWithAlphaInfo info) { this.info = info; }

        public void LoadPalettes(WorldRenderer wr)
        {
            var remap = new AlphaPaletteRemap(info.Alpha, info.Premultiply);
            wr.AddPalette(info.Name, new ImmutablePalette(wr.Palette(info.BasePalette).Palette, remap), info.AllowModifiers);
        }

        public IEnumerable<string> PaletteNames { get { yield return info.Name; } }
    }
    class AlphaPaletteRemap : IPaletteRemap
    {
        readonly float alpha;
        readonly bool premultiply;
        public AlphaPaletteRemap(float alpha,bool premultiply)
        {
            this.alpha = alpha;
            this.premultiply = premultiply;
        }

        public Color GetRemappedColor(Color original,int index)
        {
            var a = (int)(original.A * alpha).Clamp(0, 255);
            var r = premultiply ? (int)(alpha * original.R + 0.5f).Clamp(0, 255) : original.R;
            var g = premultiply ? (int)(alpha * original.G + 0.5f).Clamp(0, 255) : original.G;
            var b = premultiply ? (int)(alpha * original.B + 0.5f).Clamp(0, 255) : original.B;

            return Color.FromArgb(a, r, g, b);
        }
    }
}