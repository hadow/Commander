using System;
using System.Collections.Generic;
using EW.Graphics;
using System.Drawing;
namespace EW.Mods.Common.Graphics
{
    public class SpriteActorPreview:IActorPreview
    {
        readonly Animation animation;
        readonly Func<WVec> offset;
        readonly Func<int> zOffset;
        readonly PaletteReference pr;
        readonly float scale;

        public SpriteActorPreview(Animation animation,Func<WVec> offset,Func<int> zOffset,PaletteReference pr,float scale)
        {
            this.animation = animation;
            this.offset = offset;
            this.zOffset = zOffset;
            this.pr = pr;
            this.scale = scale;
        }

        public void Tick() { animation.Tick(); }

        public IEnumerable<IRenderable> Render(WorldRenderer wr,WPos pos)
        {
            return animation.Render(pos, offset(), zOffset(), pr, scale);
        }

        public IEnumerable<Rectangle> ScreenBounds(WorldRenderer wr,WPos pos)
        {
            yield return animation.ScreenBounds(wr, pos, offset(), scale);
        }

    }
}