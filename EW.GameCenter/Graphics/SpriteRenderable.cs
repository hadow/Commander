using System;
using System.Collections.Generic;
using System.Linq;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    public struct SpriteRenderable:IRenderable,IFinalizedRenderable
    {

        public static readonly IEnumerable<IRenderable> None = new IRenderable[0].AsEnumerable();

        readonly Sprite sprite;

        readonly WPos pos;

        readonly WVec offset;

        readonly int zOffset;

        readonly PaletteReference palette;

        readonly float scale;

        readonly bool isDecoration;

        public SpriteRenderable(Sprite sprite,WPos pos,WVec offset,int zOffset,PaletteReference palette,float scale,bool isDecoration)
        {
            this.sprite = sprite;
            this.pos = pos;
            this.offset = offset;
            this.zOffset = zOffset;
            this.palette = palette;
            this.scale = scale;
            this.isDecoration = isDecoration;
        }

        public WPos Pos { get { return pos + offset; } }

        public WVec Offset { get { return offset; } }

        public PaletteReference Palette { get { return palette; } }

        public int ZOffset { get { return zOffset; } }

        public bool IsDecoration { get { return isDecoration; } }

        public IRenderable WithPalette(PaletteReference newPalette)
        {
            return new SpriteRenderable(sprite, pos, offset, zOffset, newPalette, scale, isDecoration);
        }

        public IRenderable WithZOffset(int newOffset)
        {
            return new SpriteRenderable(sprite, pos, offset, newOffset, palette, scale, isDecoration);
        }

        public IRenderable OffsetBy(WVec vec)
        {
            return new SpriteRenderable(sprite, pos + vec, offset, zOffset, palette, scale, isDecoration);
        }

        public IRenderable AsDecoration()
        {
            return new SpriteRenderable(sprite, pos, offset, zOffset, palette, scale, true);
        }


        public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }

        public void Render(WorldRenderer wr)
        {

        }

        public void RenderDebugGeometry(WorldRenderer wr)
        {

        }

        public Rectangle ScreenBounds(WorldRenderer wr)
        {
            var screenOffset = ScreenPosition(wr);
        }

        Vector3 ScreenPosition(WorldRenderer wr)
        {
            var xy = wr.ScreenPxPosition(pos) + wr.ScreenPxOffset(offset) - (0.5f * scale * sprite.Size.XY).ToInt2();

            return new Vector3(xy, sprite.Offset.Z + wr.ScreenZPosition(pos, 0) - 0.5f * scale * sprite.Size.Z);
        }
    }
}