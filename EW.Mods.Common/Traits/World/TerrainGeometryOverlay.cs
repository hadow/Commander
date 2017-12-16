using System;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits
{


    public class TerrainGeometryOverlayInfo : TraitInfo<TerrainGeometryOverlay>
    {

    }
    public class TerrainGeometryOverlay:IRenderAboveWorld,IWorldLoaded
    {

        public bool Enabled;

        void IWorldLoaded.WorldLoaded(World w, EW.Graphics.WorldRenderer render)
        {



        }


        void IRenderAboveWorld.RenderAboveWorld(Actor self, EW.Graphics.WorldRenderer wr)
        {
            if (!Enabled)
                return;

            var map = wr.World.Map;
            var tileSet = wr.World.Map.Rules.TileSet;

            var wcr = WarGame.Renderer.WorldRgbaColorRenderer;
            var colors = tileSet.HeightDebugColors;

            var mouseCell = wr.ViewPort.ViewToWorld(GameViewPort.LastMousePos).ToMPos(wr.World.Map);


        }

    }


}