using System;
using System.Linq;
using System.Drawing;
using EW.Traits;
using EW.Graphics;
using EW.Mods.Common.Commands;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Renders a debug overlay showing the terrain cells,Attach this to the world actor.
    /// </summary>
    public class TerrainGeometryOverlayInfo : TraitInfo<TerrainGeometryOverlay>
    {

    }
    public class TerrainGeometryOverlay:IRenderAboveWorld,IWorldLoaded
    {
        const string CommandName = "terrainoverlay";
        const string CommandDesc = "Toggles the terrain geometry overlay";

        public bool Enabled;

        void IWorldLoaded.WorldLoaded(World w, EW.Graphics.WorldRenderer render)
        {
            var console = w.WorldActor.TraitOrDefault<ChatCommands>();
            var help = w.WorldActor.TraitOrDefault<HelpCommand>();

            if (console == null || help == null)
                return;

            Enabled = true;

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

            foreach(var uv in wr.ViewPort.AllVisibleCells.CandidateMapCoords)
            {
                if (!map.Height.Contains(uv))
                    continue;

                var height = (int)map.Height[uv];
                var tile = map.Tiles[uv];
                var ti = tileSet.GetTileInfo(tile);
                var ramp = ti != null ? ti.RampType : 0;

                var corners = map.Grid.CellCorners[ramp];
                var color = corners.Select(c => colors[height + c.Z / 512]).ToArray();
                var pos = map.CenterOfCell(uv.ToCPos(map));
                var screen = corners.Select(c => wr.Screen3DPxPosition(pos + c)).ToArray();
                var width = (uv == mouseCell ? 3 : 1) / wr.ViewPort.Zoom;

                //Colors change between points,so render separately
                for(var i = 0; i < 4; i++)
                {
                    var j = (i + 1) % 4;
                    wcr.DrawLine(screen[i], screen[j], width, color[i], color[j]);
                }
            }

            //Projected cell coordinates for the current cell.
            var projectedCorners = map.Grid.CellCorners[0];
            foreach(var puv in map.ProjectedCellsCovering(mouseCell))
            {
                var pos = map.CenterOfCell(((MPos)puv).ToCPos(map));
                var screen = projectedCorners.Select(c => wr.Screen3DPxPosition(pos + c - new WVec(0, 0, pos.Z))).ToArray();
                for(var i = 0; i < 4; i++)
                {
                    var j = (i + 1) % 4;
                    wcr.DrawLine(screen[i], screen[j], 3 / wr.ViewPort.Zoom, Color.Green);
                }
            }
            

        }

    }


}