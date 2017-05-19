using System;
using System.Collections.Generic;


namespace EW.Graphics
{
    public static class Util
    {

        static readonly int[] ChannelMasks = { 2, 1, 0, 3 };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void FastCopyIntoChannel(Sprite dest,byte[] src)
        {
            var data = new byte[dest.Sheet.Width * dest.Sheet.Height * 4];
            dest.Sheet.GetData(data);

            var srcStride = dest.Bounds.Width;
            var destStride = dest.Sheet.Width * 4;
            var destOffset = destStride * dest.Bounds.Top + dest.Bounds.Left * 4 + ChannelMasks[(int)dest.Channel];
            var destSkip = destStride - 4 * srcStride;
            var height = dest.Bounds.Height;

            var srcOffset = 0;
            for(var j = 0; j < height; j++)
            {
                for(var i = 0; i < srcStride; i++, srcOffset++)
                {
                    data[destOffset] = src[srcOffset];
                    destOffset += 4;
                }
                destOffset += destSkip;
            }

        }

    }
}