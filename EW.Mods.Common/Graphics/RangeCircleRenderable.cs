using System;
using System.Collections.Generic;
using EW.Graphics;
using EW.Framework;
using System.Drawing;
namespace EW.Mods.Common.Graphics
{
    public struct RangeCircleRenderable:IRenderable,IFinalizedRenderable
    {
        const int RangeCircleSegments = 32;

        static readonly int[][] RangeCircleStartRotations = Exts.MakeArray(RangeCircleSegments, i => WRot.FromFacing(8 * i).AsMatrix());
        static readonly int[][] RangeCircleEndRotations = Exts.MakeArray(RangeCircleSegments, i => WRot.FromFacing(8 * i + 6).AsMatrix());



        readonly WPos centerPosition;
        readonly WDist radius;
        readonly int zOffset;
        readonly Color color;
        readonly Color contrastColor;


        public RangeCircleRenderable(WPos centerPosition, WDist radius, int zOffset,Color color,Color contrastColor)
        {

            this.centerPosition = centerPosition;
            this.radius = radius;
            this.zOffset = zOffset;
            this.color = color;
            this.contrastColor = contrastColor;

        }

        public IRenderable WithPalette(PaletteReference newPalette)
        { return new RangeCircleRenderable(centerPosition, radius, zOffset, color, contrastColor); }


        public IRenderable WithZOffset(int newOffset) { return new RangeCircleRenderable(centerPosition, radius, newOffset, color, contrastColor); }

        public IRenderable OffsetBy(WVec vec) { return new RangeCircleRenderable(centerPosition + vec, radius, zOffset, color, contrastColor); }

        public IRenderable AsDecoration() { return this; }

        public WPos Pos{ get { return centerPosition; }}

        public PaletteReference Palette{ get { return null; }}

        public int ZOffset{ get { return zOffset; }}


        public bool IsDecoration{ get { return true; }}


        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }


        public void Render(WorldRenderer wr){

            DrawRangeCircle(wr, centerPosition, radius, 1, color, 3, contrastColor);

        }


        public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }


        public void RenderDebugGeometry(WorldRenderer wr){}

        public static void DrawRangeCircle(WorldRenderer wr, WPos centerPosition, WDist radius,
            float width, Color color, float contrastWidth, Color contrastColor)
        {
            var wcr = WarGame.Renderer.WorldRgbaColorRenderer;
            var offset = new WVec(radius.Length, 0, 0);
            for (var i = 0; i < RangeCircleSegments; i++)
            {
                var a = wr.Screen3DPosition(centerPosition + offset.Rotate(RangeCircleStartRotations[i]));
                var b = wr.Screen3DPosition(centerPosition + offset.Rotate(RangeCircleEndRotations[i]));

                if (contrastWidth > 0)
                    wcr.DrawLine(a, b, contrastWidth / wr.ViewPort.Zoom, contrastColor);

                if (width > 0)
                    wcr.DrawLine(a, b, width / wr.ViewPort.Zoom, color);
            }
        }
    }
}