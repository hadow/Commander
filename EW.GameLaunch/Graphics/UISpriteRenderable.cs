using System;
using System.Drawing;
using EW.Framework;
namespace EW.Graphics
{
    public struct UISpriteRenderable:IRenderable,IFinalizedRenderable
    {
        readonly Sprite sprite;
        readonly WPos effectiveWorldPos;
        readonly Int2 screenPos;
        readonly int zOffset;
        readonly PaletteReference palette;
        readonly float scale;

        public UISpriteRenderable(Sprite sprite,WPos effectiveWorldPos,Int2 screenPos,int zOffset,PaletteReference palette,float scale)
        {
            this.sprite = sprite;
            this.effectiveWorldPos = effectiveWorldPos;
            this.screenPos = screenPos;
            this.zOffset = zOffset;
            this.palette = palette;
            this.scale = scale;

        }

        public WPos Pos { get { return effectiveWorldPos; } }

        public WVec Offset { get { return WVec.Zero; } }

        public bool IsDecoration { get { return true; } }


        public PaletteReference Palette { get { return palette; } }

        public int ZOffset { get { return zOffset; } }

        public IRenderable WithPalette(PaletteReference newPalette) { return new UISpriteRenderable(sprite, effectiveWorldPos, screenPos, zOffset, newPalette, scale); }

        public IRenderable WithZOffset(int newOffset) { return this; }

        public IRenderable OffsetBy(WVec vec) { return this; }

        public IRenderable AsDecoration() { return this; }


        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

        public void Render(WorldRenderer wr)
        {
            WarGame.Renderer.SpriteRenderer.DrawSprite(sprite, screenPos, palette, scale * sprite.Size);
        }


        public void RenderDebugGeometry(WorldRenderer wr)
        {

        }


        public Rectangle ScreenBounds(WorldRenderer wr)
        {
            var offset = screenPos + sprite.Offset;
            return new Rectangle((int)offset.X, (int)offset.Y, (int)sprite.Size.X, (int)sprite.Size.Y);
        }





    }
}