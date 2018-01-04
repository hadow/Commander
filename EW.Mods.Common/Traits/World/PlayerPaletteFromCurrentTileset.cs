using System;
using EW.Graphics;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class PlayerPaletteFromCurrentTilesetInfo:ITraitInfo
    {

        [FieldLoader.Require,PaletteDefinition(true)]
        public readonly string Name = null;

        public readonly int[] ShadowIndex = { };

        public readonly bool AllowModifiers = true;


        public object Create(ActorInitializer init) { return new PlayerPaletteFromCurrentTileset(init.World, this); }

    }



    class PlayerPaletteFromCurrentTileset : ILoadsPalettes
    {

        readonly World world;
        readonly PlayerPaletteFromCurrentTilesetInfo info;
        public PlayerPaletteFromCurrentTileset(World world,PlayerPaletteFromCurrentTilesetInfo info)
        {
            this.world = world;
            this.info = info;
        }


        public void LoadPalettes(WorldRenderer wr)
        {
            var filename = world.Map.Rules.TileSet.PlayerPalette ?? world.Map.Rules.TileSet.Palette;
            wr.AddPalette(info.Name, new ImmutablePalette(wr.World.Map.Open(filename), info.ShadowIndex), info.AllowModifiers);
        }


    }
}