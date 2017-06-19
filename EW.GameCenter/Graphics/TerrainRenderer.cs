using System;
using System.Collections.Generic;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    /// <summary>
    /// 地形渲染
    /// </summary>
    sealed class TerrainRenderer:DrawableGameComponent
    {

        readonly Map map;

        readonly Dictionary<string, TerrainSpriteLayer> spriteLayers = new Dictionary<string, TerrainSpriteLayer>();

        readonly Theater theater;


        public TerrainRenderer(Game game,World world,WorldRenderer wr):base(game)
        {

            map = world.Map;
            theater = wr.Theater;
            //Enable use of "custom" palettes per tile Templates
            foreach(var template in map.Rules.TileSet.Templates)
            {
                var palette = template.Value.Palette ?? TileSet.TerrainPaletteInternalName;
                spriteLayers.GetOrAdd(palette, pal => new TerrainSpriteLayer(game,world, wr, theater.Sheet, BlendMode.Alpha, wr.Palette(palette), world.Type != WorldT.Editor));
            }

            foreach(var cell in map.AllCells)
            {
                UpdateCell(cell);
            }

            map.Tiles.CellEntryChanged += UpdateCell;
            map.Height.CellEntryChanged += UpdateCell;

        }

        /// <summary>
        /// 当地形数据更改时更新地图层
        /// </summary>
        /// <param name="cell"></param>
        public void UpdateCell(CPos cell)
        {
            var tile = map.Tiles[cell];
            var palette = TileSet.TerrainPaletteInternalName;
            if (map.Rules.TileSet.Templates.ContainsKey(tile.Type))
                palette = map.Rules.TileSet.Templates[tile.Type].Palette ?? palette;

            var sprite = theater.TileSprite(tile);

            foreach(var kv in spriteLayers)
            {
                kv.Value.Update(cell, palette == kv.Key ? sprite : null);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="wr"></param>
        /// <param name="viewPort"></param>
        public void Draw(WorldRenderer wr,GameViewPort viewPort)
        {
            foreach (var kv in spriteLayers.Values)
                kv.Draw(wr.ViewPort);

            foreach (var r in wr.World.WorldActor.TraitsImplementing<IRenderOverlay>())
                r.Render(wr);
        }
        

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            map.Height.CellEntryChanged -= UpdateCell;
            map.Tiles.CellEntryChanged -= UpdateCell;

            foreach (var kv in spriteLayers.Values)
                kv.Dispose();
        }

    }
}