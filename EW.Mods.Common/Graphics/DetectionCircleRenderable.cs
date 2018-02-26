using System;
using System.Drawing;
using EW.Graphics;

namespace EW.Mods.Common.Graphics
{
    public struct DetectionCircleRenderable:IRenderable,IFinalizedRenderable
    {
        readonly WPos centerPosition;
        readonly WDist radius;
        readonly int zOffset;
        readonly int trailCount;
        readonly WAngle trailSeparation;
        readonly WAngle trailAngle;
        readonly Color color;
        readonly Color contrastColor;

        public DetectionCircleRenderable(WPos centerPosition,WDist radius,int zOffset,
            int lineTrails,WAngle trailSeparation,WAngle trailAngle,Color color,Color contrastColor)
        {
            this.centerPosition = centerPosition;
            this.radius = radius;
            this.zOffset = zOffset;
            trailCount = lineTrails;
            this.trailSeparation = trailSeparation;
            this.trailAngle = trailAngle;
            this.color = color;
            this.contrastColor = contrastColor;
        }


        public WPos Pos { get { return centerPosition; } }

        public PaletteReference Palette { get { return null; } }

        public int ZOffset { get { return zOffset; } }

        public bool IsDecoration { get { return true; } }

        public IRenderable WithPalette(PaletteReference newPalette)
        {
            return new DetectionCircleRenderable(centerPosition, radius, zOffset, trailCount, trailSeparation, trailAngle, color, contrastColor);
        }

        public IRenderable WithZOffset(int newOffset)
        {
            return new DetectionCircleRenderable(centerPosition, radius, newOffset, trailCount, trailSeparation, trailAngle, color, contrastColor);
        }

        public IRenderable AsDecoration() { return this; }


        public IRenderable OffsetBy(WVec vec)
        {
            return new DetectionCircleRenderable(centerPosition + vec, radius, zOffset, trailCount, trailSeparation, trailAngle, color, contrastColor);
        }
        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

        void IFinalizedRenderable.Render(WorldRenderer wr)
        {
            var wcr = WarGame.Renderer.WorldRgbaColorRenderer;
            var center = wr.Screen3DPosition(centerPosition);

            for(var i = 0; i < trailCount; i++)
            {
                var angle = trailAngle - new WAngle(i * (trailSeparation.Angle <= 512 ? 1 : -1));
                var length = radius.Length * new WVec(angle.Cos(), angle.Sin(), 0) / 1024;
                var end = wr.Screen3DPosition(centerPosition + length);
                var alpha = color.A - i * color.A / trailCount;

                wcr.DrawLine(center, end, 3, Color.FromArgb(alpha, contrastColor));
                wcr.DrawLine(center, end, 1, Color.FromArgb(alpha, color));
            }
        }

        void IFinalizedRenderable.RenderDebugGeometry(WorldRenderer wr)
        {

        }

        Rectangle IFinalizedRenderable.ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }


    }
}