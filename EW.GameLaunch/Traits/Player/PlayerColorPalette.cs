using System;
using EW.Graphics;
namespace EW.Traits
{
    /// <summary>
    /// Add this to the Player actor definition.
    /// </summary>
    public class PlayerColorPaletteInfo : ITraitInfo
    {

        [PaletteReference]
        public readonly string BasePalette = null;

        /// <summary>
        /// The prefix for the resulting player palettes.
        /// </summary>
        public readonly string BaseName = "player";

        /// <summary>
        /// Remap these indices to player colors.
        /// </summary>
        public readonly int[] RemapIndex = { };


        /// <summary>
        /// Luminosity range to span.
        /// 光度范围跨度。
        /// </summary>
        public readonly float Ramp = 0.05f;


        /// <summary>
        /// Allow palette modifiers to change the palette.
        /// </summary>
        public readonly bool AllowModifiers = true;


        public object Create(ActorInitializer init)
        {
            return new PlayerColorPalette(this);
        }
    }
    public class PlayerColorPalette:ILoadsPlayerPalettes
    {


        readonly PlayerColorPaletteInfo info;

        public PlayerColorPalette(PlayerColorPaletteInfo info)
        {
            this.info = info;
        }


        public void LoadPlayerPalettes(WorldRenderer wr,string playerName,HSLColor color,bool replaceExisting)
        {
            var remap = new PlayerColorRemap(info.RemapIndex, color, info.Ramp);
            var pal = new ImmutablePalette(wr.Palette(info.BasePalette).Palette, remap);
            wr.AddPalette(info.BaseName + playerName, pal, info.AllowModifiers, replaceExisting);
        }
    }
}