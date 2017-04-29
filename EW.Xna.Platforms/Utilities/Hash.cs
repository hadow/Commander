using System;
using System.IO;
using System.Collections.Generic;

namespace EW.Xna.Platforms.Utilities
{
    internal static class Hash
    {
        internal static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                const int p = 16777619;
                var hash = (int)2166136261;

                for (var i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }


        internal static int ComputeHash(Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream.CanSeek);

            unchecked
            {
                const int p = 16777619;
                var hash = (int)2166136261;

                var prevPosition = stream.Position;
                stream.Position = 0;

                var data = new byte[1024];
                int length;
                while ((length = stream.Read(data, 0, data.Length)) != 0)
                {
                    for (var i = 0; i < length; i++)
                        hash = (hash ^ data[i]) * p;
                }

                // Restore stream position.
                stream.Position = prevPosition;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }


    }
}