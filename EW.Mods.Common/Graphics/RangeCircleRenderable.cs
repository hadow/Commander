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


        }

        public EW.Framework.Rectangle ScreenBounds(WorldRenderer wr) { return EW.Framework.Rectangle.Empty; }


        public void RenderDebugGeometry(WorldRenderer wr){}
    }
}