using System;
using System.Drawing;
using EW.Graphics;
using EW.Framework;

namespace EW.Mods.Common.Graphics
{
    public struct SelectionBoxRenderable:IRenderable,IFinalizedRenderable
    {

        readonly WPos pos;
        readonly Rectangle decorationBounds;
        readonly Color color;

        public SelectionBoxRenderable(Actor actor,Rectangle decorationBounds,Color color) : this(actor.CenterPosition,decorationBounds,color){}

        public SelectionBoxRenderable(WPos pos,Rectangle decorationBounds,Color color)
        {
            this.pos = pos;
            this.decorationBounds = decorationBounds;
            this.color = color;
        }

        public WPos Pos { get { return pos; } }

        public PaletteReference Palette { get { return null; } }

        public int ZOffset { get { return 0; } }

        public bool IsDecoration { get { return true; } }


        public IRenderable WithPalette(PaletteReference newPalette) { return this; }

        public IRenderable WithZOffset(int newOffset) { return this; }

        public IRenderable OffsetBy(WVec vec) { return new SelectionBoxRenderable(pos + vec, decorationBounds, color); }

        public IRenderable AsDecoration() { return this; }


        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

        public void Render(WorldRenderer wr)
        {
            var iz = 1 / wr.ViewPort.Zoom;
            var screenDepth = wr.Screen3DPxPosition(pos).Z;
            var tl = new Vector3(decorationBounds.Left, decorationBounds.Top, screenDepth);
            var br = new Vector3(decorationBounds.Right, decorationBounds.Bottom, screenDepth);
            var tr = new Vector3(br.X, tl.Y, screenDepth);
            var bl = new Vector3(tl.X, br.Y, screenDepth);

            var u = new Vector2(4 * iz, 0);
            var v = new Vector2(0, 4 * iz);

            var wcr = WarGame.Renderer.WorldRgbaColorRenderer;
            wcr.DrawLine(new[] { tl + u, tl, tl + v }, iz, color, true);
            wcr.DrawLine(new[] { tr - u, tr, tr + v }, iz, color, true);
            wcr.DrawLine(new[] { br - u, br, br - v }, iz, color, true);
            wcr.DrawLine(new[] { bl + u, bl, bl - v }, iz, color, true);
        }

        public void RenderDebugGeometry(WorldRenderer wr) { }

        public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }

    }
}