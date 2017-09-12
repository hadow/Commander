using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public static int ReadInt32(this Stream s)
        {
            return BitConverter.ToInt32(s.ReadBytes(4), 0);
        }

        public static short ReadInt16(this Stream s)
        {
            return BitConverter.ToInt16(s.ReadBytes(2), 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(this Stream s)
        {
            using (s)
                return s.ReadBytes((int)(s.Length - s.Position));
        }

        public static string ReadAllText(this Stream s)
        {
            using (s)
            using (var sr = new StreamReader(s))
                return sr.ReadToEnd();
        }

        public static string ReadASCIIZ(this Stream s)
        {
            var bytes = new List<byte>();
            byte b;
            while ((b = s.ReadUInt8()) != 0)
                bytes.Add(b);
            return new string(Encoding.ASCII.GetChars(bytes.ToArray()));
        }

        public static string ReadASCII(this Stream s, int length)
        {
            return new string(Encoding.ASCII.GetChars(s.ReadBytes(length)));
        }

        public static int Peek(this Stream s)
        {
            var b = s.ReadByte();
            if (b == -1)
                return -1;
            s.Seek(-1, SeekOrigin.Current);
            return (byte)b;
        }

    }
}