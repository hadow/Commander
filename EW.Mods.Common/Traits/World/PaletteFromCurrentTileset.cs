using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Loads the palette specified in the tileset definition
    /// </summary>
    public class PaletteFromCurrentTilesetInfo : ITraitInfo
    {
        [FieldLoader.Require,PaletteDefinition]
        public readonly string Name = null;

        /// <summary>
        /// Map listed indices to shadow.Ignores previous color.
        /// </summary>
        public readonly int[] ShadowIndex = { };

        public readonly bool AllowModifiers = true;

        public object Create(ActorInitializer init)
        {
            return new PaletteFromCurrentTileset(init.World,this);
        }
    }

    public class PaletteFromCurrentTileset:ILoadsPalettes,IProvidesAssetBrowserPalettes
    {

        readonly World world;
        readonly PaletteFromCurrentTilesetInfo info;

        public PaletteFromCurrentTileset(World world,PaletteFromCurrentTilesetInfo info)
        {
            this.world = world;
            this.info = info;
        }


        public void LoadPalettes(WorldRenderer wr)
        {
            wr.AddPalette(info.Name, new ImmutablePalette(wr.World.Map.Open(world.Map.Rules.TileSet.Palette), info.ShadowIndex), info.AllowModifiers);
        }

        public IEnumerable<string> PaletteNames { get
            {
                yield return info.Name;
            } }
    }
}