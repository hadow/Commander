using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Graphics;
using EW.Widgets;
using EW.Framework;
namespace EW.Mods.Common.Graphics
{
    public struct TextRenderable:IRenderable,IFinalizedRenderable
    {
        readonly SpriteFont font;
        readonly WPos pos;
        readonly int zOffset;
        readonly Color color;
        readonly Color bgDark;
        readonly Color bgLight;
        readonly string text;


        public TextRenderable(SpriteFont font,WPos pos,int zOffset,Color color,string text) : this(font, pos, zOffset, color, ChromeMetrics.Get<Color>("TextContrastColorDark"), ChromeMetrics.Get<Color>("TextContrastColorLight"), text) { }


        public TextRenderable(SpriteFont font,WPos pos,int zOffset,Color color,Color bgDark,Color bgLight,string text)
        {
            this.font = font;
            this.pos = pos;
            this.zOffset = zOffset;
            this.color = color;
            this.bgDark = bgDark;
            this.bgLight = bgLight;
            this.text = text;
        }


        public WPos Pos { get { return pos; } }

        public PaletteReference Palette { get { return null; } }

        public int ZOffset { get { return zOffset; } }

        public bool IsDecoration { get { return true; } }

        public IRenderable WithPalette(PaletteReference newPalette) { return new TextRenderable(font, pos, zOffset, color, text); }

        public IRenderable WithZOffset(int newOffset) { return new TextRenderable(font, pos, zOffset, color, text); }

        public IRenderable OffsetBy(WVec vec) { return new TextRenderable(font, pos + vec, zOffset, color, text); }


        public IRenderable AsDecoration() { return this; }

        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

        public void Render(WorldRenderer wr)
        {
            var screenPos = wr.ViewPort.Zoom * (wr.ScreenPosition(pos) - wr.ViewPort.TopLeft.ToVector2()) - 0.5f * font.Measure(text).ToVector2();
            var screenPxPos = new Vector2((float)Math.Round(screenPos.X), (float)Math.Round(screenPos.Y));
            font.DrawTextWithContrast(text, screenPxPos, color, bgDark, bgLight, 1);
        }

        public void RenderDebugGeometry(WorldRenderer wr)
        {

        }


        public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
    }
}