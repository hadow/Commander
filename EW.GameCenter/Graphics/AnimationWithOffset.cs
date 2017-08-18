using System;
using System.Collections.Generic;

namespace EW.Graphics
{
    public class AnimationWithOffset
    {


        public readonly Animation Animation;
        public readonly Func<WVec> OffsetFunc;
        public readonly Func<bool> DisableFunc;
        public readonly Func<WPos, int> ZOffset;


        public AnimationWithOffset(Animation a,Func<WVec> offset,Func<bool> disable,Func<WPos,int> zOffset)
        {
            Animation = a;
            OffsetFunc = offset;
            DisableFunc = disable;
            ZOffset = zOffset;
        }

        public static implicit operator AnimationWithOffset(Animation a)
        {
            return new AnimationWithOffset(a,null,null,null);
        }
    }
}