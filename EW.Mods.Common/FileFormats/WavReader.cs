using System;
using System.IO;

namespace EW.Mods.Common.FileFormats
{
    /// <summary>
    /// 
    /// </summary>
    public class WavReader
    {
        public int FileSize;

        public string Format;

        public int FmtChunkSize;

        public int AudioFormat;

        public int Channels;

        public int SampleRate;

        public int ByteRate;

        public int BlockAlign;

        public int BitsPerSample;

        public int UncompressedSize;

        public int DataSize;

        public byte[] RawOutput;

        public enum WaveT { Pcm=0x1,ImaAdpcm = 0x11}

        public static WaveT Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool LoadSound(Stream s)
        {
            var type = s.ReadASCII(4);
            if (type != "RIFF")
                return false;

            FileSize = s.ReadInt32();
            Format = s.ReadASCII(4);
            if (Format != "WAVE")
                return false;
            while (s.Position < s.Length)
            {
                if ((s.Position & 1) == 1)
                    s.ReadByte();           //Alignment

                type = s.ReadASCII(4);
                switch (type)
                {
                    case "fmt":
                        FmtChunkSize = s.ReadInt32();
                        AudioFormat = s.ReadInt16();
                        Type = (WaveT)AudioFormat;

                        if (!Enum.IsDefined(typeof(WaveT), Type))
                            throw new NotSupportedException("Compression type {0} is not supported.".F(AudioFormat));

                        Channels = s.ReadInt16();
                        SampleRate = s.ReadInt32();
                        ByteRate = s.ReadInt32();
                        BlockAlign = s.ReadInt16();
                        BitsPerSample = s.ReadInt16();

                        s.ReadBytes(FmtChunkSize - 16);
                        break;
                    case "fact":
                        var chunkSize = s.ReadInt32();
                        UncompressedSize = s.ReadInt32();
                        s.ReadBytes(chunkSize - 4);
                        break;
                    case "data":
                        DataSize = s.ReadInt32();
                        RawOutput = s.ReadBytes(DataSize);
                        break;
                    default:
                        var unknownChunkSize = s.ReadInt32();
                        s.ReadBytes(unknownChunkSize);
                        break;
                }
            }

            if(Type == WaveT.ImaAdpcm)
            {
                
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static float WaveLength(Stream s)
        {
            s.Position = 12;
            var fmt = s.ReadASCII(4);

            if (fmt != "fmt")
                return 0;

            s.Position = 22;
            var channels = s.ReadInt16();
            var sampleRate = s.ReadInt32();

            s.Position = 34;
            var bitsPerSample = s.ReadInt16();
            var length = s.Length * 8;

            return length / (channels * sampleRate * bitsPerSample);
        }
    }
}