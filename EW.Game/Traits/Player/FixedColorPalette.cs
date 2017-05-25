using System;
using EW.Graphics;
namespace EW.Traits
{
    public class FixedColorPaletteInfo: ITraitInfo
    {

        [PaletteReference]
        public readonly string Base = TileSet.TerrainPaletteInternalName;

        [PaletteDefinition]
        public readonly string Name = "resources";

        public readonly int[] RemapIndex = { };

        /// <summary>
        /// 
        /// </summary>
        public readonly float Ramp = 0.05f;

        /// <summary>
        /// 
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

        }

    }
}