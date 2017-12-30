using System;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Create player palettes by applying alpha transparency to another player palette.
    /// </summary>
    class PaletteFromPlayerPaletteWithAlphaInfo:ITraitInfo
    {
        [FieldLoader.Require]
        /// <summary>
        /// The prefix for the resulting player palettes
        /// </summary>
        /// 
        [PaletteDefinition(true)]
        public readonly string BaseName = null;

        /// <summary>
        /// The name of the player palette to base off.
        /// </summary>
        /// 
        [FieldLoader.Require]
        public readonly string BasePalette = null;


        /// <summary>
        /// Allow palette modifiers to change the palette
        /// </summary>
        public readonly bool AllowModifiers = true;

        /// <summary>
        /// Alpha component that is applied to the base palette.
        /// </summary>
        public readonly float Alpha = 1.0f;

        /// <summary>
        /// Premultiply color by the alpha component.
        /// </summary>
        public readonly bool Premultiply = true;

        public object Create(ActorInitializer init)
        {
            return new PaletteFromPlayerPaletteWithAlpha(this);
        }

    }

    class PaletteFromPlayerPaletteWithAlpha:ILoadsPlayerPalettes
    {
        readonly PaletteFromPlayerPaletteWithAlphaInfo info;
        public PaletteFromPlayerPaletteWithAlpha(PaletteFromPlayerPaletteWithAlphaInfo info)
        {
            this.info = info;
        }


        public void LoadPlayerPalettes(WorldRenderer wr,string playerName,HSLColor color,bool replaceExisting)
        {
            var remap = new AlphaPaletteRemap(info.Alpha, info.Premultiply);
            var pal = new ImmutablePalette(wr.Palette(info.BasePalette + playerName).Palette, remap);
            wr.AddPalette(info.BaseName + playerName, pal, info.AllowModifiers, replaceExisting);
        }
    }
}