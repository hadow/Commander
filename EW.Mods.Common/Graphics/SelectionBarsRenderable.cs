using System;
using System.Drawing;
using EW.Graphics;
using EW.Traits;
using EW.Framework;

namespace EW.Mods.Common.Graphics
{
    public struct SelectionBarsRenderable:IRenderable,IFinalizedRenderable
    {

        readonly WPos pos;
        readonly Actor actor;
        readonly bool displayHealth;
        readonly bool displayExtra;
        readonly Rectangle decorationBounds;


        public SelectionBarsRenderable(Actor actor,Rectangle decorationBounds,bool displayHealth,bool displayExtra):this(actor.CenterPosition,actor,decorationBounds)
        {
            this.displayHealth = displayHealth;
            this.displayExtra = displayExtra;

        }

        public SelectionBarsRenderable(WPos pos,Actor actor,Rectangle decorationBounds) : this()
        {
            this.pos = pos;
            this.actor = actor;
            this.decorationBounds = decorationBounds;
        }

        public WPos Pos { get { return pos; } }

        public bool DisplayHealth { get { return displayHealth; } }

        public bool DisplayExtra { get { return displayExtra; } }

        public PaletteReference Palette { get { return null; } }

        public int ZOffset { get { return 0; } }

        public bool IsDecoration { get { return true; } }

        public IRenderable WithPalette(PaletteReference newPalette) { return this; }

        public IRenderable WithZOffset(int newOffset) { return this; }

        public IRenderable OffsetBy(WVec vec) { return new SelectionBarsRenderable(pos + vec, actor, decorationBounds); }

        public IRenderable AsDecoration() { return this; }


        public void Render(WorldRenderer wr)
        {
            if (!actor.IsInWorld || actor.IsDead)
                return;

            var health = actor.TraitOrDefault<IHealth>();

            var screenPos = wr.Screen3DPxPosition(pos);

            var start = new Vector3(decorationBounds.Left + 1, decorationBounds.Top, screenPos.Z);
            var end = new Vector3(decorationBounds.Right - 1, decorationBounds.Top, screenPos.Z);

            if (DisplayHealth)
            {
                DrawHealthBar(wr, health, start, end);
            }
            if (DisplayExtra)
            {

            }
        }


        void DrawExtraBars(WorldRenderer wr,Vector3 start,Vector3 end)
        {
            foreach(var extraBar in actor.TraitsImplementing<ISelectionBar>())
            {
                var value = extraBar.GetValue();
                if(value != 0 || extraBar.DisplayWhenEmpty)
                {
                    var offset = new Vector3(0,(int)(4/wr.ViewPort.Zoom),0);
                    start += offset;
                    end += offset;

                }
            }
        }

        void DrawHealthBar(WorldRenderer wr,IHealth health,Vector3 start,Vector3 end)
        {
            if (health == null || health.IsDead)
                return;

            var c = Color.FromArgb(128, 30, 30, 30);
            var c2 = Color.FromArgb(128, 10, 10, 10);

            var iz = 1 / wr.ViewPort.Zoom;

            var p = new Vector2(0, -4 * iz);
            var q = new Vector2(0, -3 * iz);
            var r = new Vector2(0, -2 * iz);

            var healthColor = GetHealthColor(health);
            var healthColor2 = Color.FromArgb(255, healthColor.R / 2, healthColor.G / 2, healthColor.B / 2);

            var z = Vector3.Lerp(start, end, (float)health.HP / health.MaxHP);

            var wcr = WarGame.Renderer.WorldRgbaColorRenderer;
            wcr.DrawLine(start + p, end + p, iz, c);
            wcr.DrawLine(start + q, end + q, iz, c2);
            wcr.DrawLine(start + r, end + r, iz, c);

            wcr.DrawLine(start + p, z + p, iz, healthColor2);
            wcr.DrawLine(start + q, z + q, iz, healthColor);
            wcr.DrawLine(start + r, z + r, iz, healthColor);

        }


        Color GetHealthColor(IHealth health)
        {
            if (WarGame.Settings.Game.UsePlayerStanceColor)
                return actor.Owner.PlayerStanceColor(actor);
            else
                return health.DamageState == DamageState.Critical ? Color.Red : health.DamageState == DamageState.Heavy ? Color.Yellow : Color.LimeGreen;
        }
        public void RenderDebugGeometry(WorldRenderer wr) { }

        public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }

        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

    }
}