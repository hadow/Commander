using System;
using System.Drawing;
using EW.Graphics;
using EW.Mods.Common.Projectiles;
using EW.Framework;
namespace EW.Mods.Common.Graphics
{
    public struct RailgunHelixRenderable : IRenderable, IFinalizedRenderable
    {
        readonly WPos pos;
        readonly int zOffset;
        readonly Railgun railgun;
        readonly RailgunInfo info;
        readonly WDist helixRadius;
        readonly int alpha;
        readonly int ticks;

        WAngle angle;

        public RailgunHelixRenderable(WPos pos, int zOffset, Railgun railgun, RailgunInfo railgunInfo, int ticks)
        {
            this.pos = pos;
            this.zOffset = zOffset;
            this.railgun = railgun;
            this.info = railgunInfo;
            this.ticks = ticks;

            helixRadius = info.HelixRadius + new WDist(ticks * info.HelixRadiusDeltaPerTick);
            alpha = (railgun.HelixColor.A + ticks * info.HelixAlphaDeltaPerTick).Clamp(0, 255);
            angle = new WAngle(ticks * info.HelixAngleDeltaPerTick.Angle);
        }

        public WPos Pos { get { return pos; } }
        public PaletteReference Palette { get { return null; } }
        public int ZOffset { get { return zOffset; } }
        public bool IsDecoration { get { return true; } }

        public IRenderable WithPalette(PaletteReference newPalette) { return new RailgunHelixRenderable(pos, zOffset, railgun, info, ticks); }
        public IRenderable WithZOffset(int newOffset) { return new RailgunHelixRenderable(pos, newOffset, railgun, info, ticks); }
        public IRenderable OffsetBy(WVec vec) { return new RailgunHelixRenderable(pos + vec, zOffset, railgun, info, ticks); }
        public IRenderable AsDecoration() { return this; }

        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
        public void Render(WorldRenderer wr)
        {
            if (railgun.ForwardStep == WVec.Zero)
                return;

            var screenWidth = wr.ScreenVector(new WVec(info.HelixThickness.Length, 0, 0))[0];

            // Move forward from self to target to draw helix
            var centerPos = this.pos;
            var points = new Vector3[railgun.CycleCount * info.QuantizationCount];
            for (var i = points.Length - 1; i >= 0; i--)
            {
                // Make it narrower near the end.
                var rad = i < info.QuantizationCount ? helixRadius / 4 :
                    i < 2 * info.QuantizationCount ? helixRadius / 2 :
                    helixRadius;

                // Note: WAngle.Sin(x) = 1024 * Math.Sin(2pi/1024 * x)
                var u = rad.Length * angle.Cos() * railgun.LeftVector / (1024 * 1024)
                    + rad.Length * angle.Sin() * railgun.UpVector / (1024 * 1024);
                points[i] = wr.Screen3DPosition(centerPos + u);

                centerPos += railgun.ForwardStep;
                angle += railgun.AngleStep;
            }

            WarGame.Renderer.WorldRgbaColorRenderer.DrawLine(points, screenWidth, Color.FromArgb(alpha, railgun.HelixColor));
        }

        public void RenderDebugGeometry(WorldRenderer wr) { }
        public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
    }
}