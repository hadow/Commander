using System;
using System.Collections.Generic;
using System.IO;
namespace EW
{
    public static class StreamExts
    {
        public static byte[] ReadBytes(this Stream s,int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Non-negagive number required");

            var buffer = new byte[count];
            s.ReadBytes(buffer, 0, count);
            return buffer;
        }

        public static void ReadBytes(this Stream s,byte[] buffer,int offset,int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException("count", "Non-negagive number required");
            while (count > 0)
            {
                int bytesRead;
                if ((bytesRead = s.Read(buffer, offset, count)) == 0)
                    throw new EndOfStreamException();
                offset += bytesRead;
                count -= bytesRead;
            }
        }

        public static byte ReadUInt8(this Stream s)
        {
            var b = s.ReadByte();
            if (b == -1)
                throw new EndOfStreamException();
            return (byte)b;
        }

        public static ushort ReadUInt16(this Stream s)
        {
            return BitConverter.ToUInt16(s.ReadBytes(2), 0);
        }

        public static uint ReadUInt32(this Stream s)
        {
            return BitConverter.ToUInt32(s.ReadBytes(4), 0);
        }



    }
}