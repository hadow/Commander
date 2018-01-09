using System;

namespace EW.Mods.Common.Traits.Render
{
    public class RenderUtils
    {

        public static int ZOffsetFromCenter(Actor self,WPos pos,int offset)
        {
            var delta = self.CenterPosition - pos;
            return delta.Y + delta.Z + offset;
        }
    }
}