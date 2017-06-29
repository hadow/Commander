using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{

    class PaletteFromFileInfo : ITraitInfo
    {
        [FieldLoader.Require,PaletteDefinition]
        public readonly string Name = null;

        /// <summary>
        /// If defined,load the palette only for this tileset.
        /// </summary>
        public readonly string Tileset = null;

        [FieldLoader.Require]
        public readonly string Filename = null;

        /// <summary>
        /// Map listed indices to shadow.Ignores previous color.
        /// </summary>
        public readonly int[] ShadowIndex = { };

        public readonly bool AllowModifiers = true;

        public object Create(ActorInitializer init)
        {
            return new PaletteFromFile(init.World, this);
        }
    }
    class PaletteFromFile:ILoadsPalettes,IProvidesAssetBrowserPalettes
    {
        readonly World world;

        readonly PaletteFromFileInfo info;

        public PaletteFromFile(World world,PaletteFromFileInfo info)
        {
            this.world = world;
            this.info = info;
        }

        public void LoadPalettes(WorldRenderer wr)
        {
            if (info.Tileset == null || info.Tileset.ToLowerInvariant() == world.Map.Tileset.ToLowerInvariant())
                wr.AddPalette(info.Name, new ImmutablePalette(world.Map.Open(info.Filename), info.ShadowIndex), info.AllowModifiers);
        }

        public IEnumerable<string> PaletteNames
        {
            get
            {
                //Only expose the palette if it is available for the shellmap's tileset (which is a requirement for its use).
                if (info.Tileset == null || info.Tileset == world.Map.Rules.TileSet.Id)
                    yield return info.Name;
            }
        }


    }
}