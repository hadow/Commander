using System;
using System.Collections.Generic;
using System.Drawing;
namespace EW.Graphics
{
    /// <summary>
    ///  ¿ΩÁ‰÷»æ∆˜
    /// </summary>
    public sealed class WorldRenderer:IDisposable
    {

        public readonly Size TileSize;

        public readonly World World;

        public readonly Theater Theater;

        readonly TerrainRenderer terrainRenderer;

        internal WorldRenderer(ModData mod,World world)
        {
            World = world;
            TileSize = World.Map
        }
        public void Dispose() { }
    }
}