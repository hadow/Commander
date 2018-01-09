using System;
using System.Collections.Generic;
using System.Drawing;
namespace EW.Graphics
{
    public class AnimationWithOffset
    {


        public readonly Animation Animation;
        public readonly Func<WVec> OffsetFunc;
        public readonly Func<bool> DisableFunc;
        public readonly Func<WPos, int> ZOffset;

        public AnimationWithOffset(Animation a,Func<WVec> offset,Func<bool> disable) : this(a, offset, disable, null) { }

        public AnimationWithOffset(Animation a,Func<WVec> offset,Func<bool> disable,Func<WPos,int> zOffset)
        {
            Animation = a;
            OffsetFunc = offset;
            DisableFunc = disable;
            ZOffset = zOffset;
        }

        public IEnumerable<IRenderable> Render(Actor self,WorldRenderer wr,PaletteReference pal,float scale)
        {
            var center = self.CenterPosition;
            var offset = OffsetFunc != null ? OffsetFunc() : WVec.Zero;

            var z = (ZOffset != null) ? ZOffset(center + offset) : 0;
            return Animation.Render(center, offset, z, pal, scale);
        }


        public Rectangle ScreenBounds(Actor self,WorldRenderer wr,float scale)
        {
            var center = self.CenterPosition;
            var offset = OffsetFunc != null ? OffsetFunc() : WVec.Zero;

            return Animation.ScreenBounds(wr, center, offset, scale);
        }


        public static implicit operator AnimationWithOffset(Animation a)
        {
            return new AnimationWithOffset(a,null,null,null);
        }
    }
}