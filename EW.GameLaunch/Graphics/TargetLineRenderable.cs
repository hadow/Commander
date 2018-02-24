using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using EW.Framework;
namespace EW.Graphics
{
    public struct TargetLineRenderable:IRenderable,IFinalizedRenderable
    {

        readonly Color color;
        readonly IEnumerable<WPos> waypoints;

        public TargetLineRenderable(IEnumerable<WPos> waypoints,Color color)
        {
            this.waypoints = waypoints;
            this.color = color;
        }
            
        public WPos Pos { get { return waypoints.First(); } }

        public PaletteReference Palette { get { return null; } }

        public int ZOffset { get { return 0; } }

        public bool IsDecoration { get { return true; } }

        public IRenderable WithPalette(PaletteReference newPalette)
        {
            return new TargetLineRenderable(waypoints, color);
        }

        public IRenderable WithZOffset(int newOffset)
        {
            return new TargetLineRenderable(waypoints, color);
        }

        public IRenderable OffsetBy(WVec vec)
        {
            return new TargetLineRenderable(waypoints.Select(w => w + vec), color);
        }

        public IRenderable AsDecoration() { return this; }

        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

        public void Render(WorldRenderer wr)
        {
            if (!waypoints.Any())
                return;

            var iz = 1 / wr.ViewPort.Zoom;
            var first = wr.Screen3DPosition(waypoints.First());
            var a = first;

            foreach(var b in waypoints.Skip(1).Select(pos=>wr.Screen3DPosition(pos)))
            {
                WarGame.Renderer.WorldRgbaColorRenderer.DrawLine(a, b, iz, color);
                DrawTargetMarker(wr, color, b);
                a = b;
            }

            DrawTargetMarker(wr, color, first);


        }

        public static void DrawTargetMarker(WorldRenderer wr,Color color,Vector3 location)
        {
            var iz = 1 / wr.ViewPort.Zoom;
            var offset = new Vector2(iz, iz);
            var tl = location - offset;
            var br = location + offset;

            WarGame.Renderer.WorldRgbaColorRenderer.FillRect(tl, br, color);
        }

        public void RenderDebugGeometry(WorldRenderer wr) { }

        public Rectangle ScreenBounds(WorldRenderer wr)
        {
            return Rectangle.Empty;
        }

    }

}