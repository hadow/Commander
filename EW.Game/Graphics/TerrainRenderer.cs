using System;
using System.Collections.Generic;

namespace EW.Graphics
{
    /// <summary>
    /// 地形渲染
    /// </summary>
    sealed class TerrainRenderer:IDisposable
    {

        readonly Map map;

        readonly Dictionary<string, TerrainSpriteLayer> spriteLayers = new Dictionary<string, TerrainSpriteLayer>();

        readonly Theater theater;


        public TerrainRenderer(World world,WorldRenderer wr)
        {

            map = world.Map;
            theater = wr.th

        }

        public void Dispose()
        {

        }

    }
}