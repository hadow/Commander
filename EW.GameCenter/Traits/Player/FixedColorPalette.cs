using System;
using EW.Graphics;
namespace EW.Traits
{
    /// <summary>
    /// Add this to the World actor definition.
    /// </summary>
    public class FixedColorPaletteInfo: ITraitInfo
    {

        [PaletteReference]
        public readonly string Base = TileSet.TerrainPaletteInternalName;

        [PaletteDefinition]
        public readonly string Name = "resources";

        public readonly int[] RemapIndex = { };


        public readonly HSLColor Color;

        /// <summary>
        /// Luminosity range to span.
        /// 亮度范围跨越
        /// </summary>
        public readonly float Ramp = 0.05f;

        /// <summary>
        /// Allow palette modifiers to change the palette.
        /// </summary>
        public readonly bool AllowModifiers = true;

        

        public object Create(ActorInitializer init)
        {
            return new FixedColorPalette(this);
        }
    }
    public class FixedColorPalette:ILoadsPalettes
    {

        readonly FixedColorPaletteInfo info;

        public FixedColorPalette(FixedColorPaletteInfo info)
        {
            this.info = info;
        }
        public void LoadPalettes(WorldRenderer wr)
        {
            var remap = new PlayerColorRemap(info.RemapIndex, info.Color, info.Ramp);
            wr.AddPalette(info.Name, new ImmutablePalette(wr.Palette(info.Base).Palette, remap), info.AllowModifiers);
        }

    }
}