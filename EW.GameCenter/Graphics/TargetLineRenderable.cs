using System;
using System.Collections.Generic;
using EW.OpenGLES;
using System.Linq;
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

        }

        public void RenderDebugGeometry(WorldRenderer wr) { }

        public Rectangle ScreenBounds(WorldRenderer wr)
        {
            return Rectangle.Empty;
        }

    }

}