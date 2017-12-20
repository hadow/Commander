using System;

namespace EW.FileFormats
{
    // Run length encoded sequences of zeros (aka Format2)
    public static class RLEZerosCompression
    {
        public static void DecodeInto(byte[] src, byte[] dest, int destIndex)
        {
            var r = new FastByteReader(src);

            while (!r.Done())
            {
                var cmd = r.ReadByte();
                if (cmd == 0)
                {
                    var count = r.ReadByte();
                    while (count-- > 0)
                        dest[destIndex++] = 0;
                }
                else
                    dest[destIndex++] = cmd;
            }
        }
    }
}