﻿using System;
using System.IO;
using EW.Mods.Common.FileFormats;

namespace EW.Mods.Common.AudioLoaders
{
    public class AudLoader:ISoundLoader
    {

        bool IsAud(Stream s)
        {
            var start = s.Position;
            s.Position += 10;
            var readFlag = s.ReadByte();
            var readFormat = s.ReadByte();
            s.Position = start;

            if (!Enum.IsDefined(typeof(SoundFlags), readFlag))
                return false;

            return Enum.IsDefined(typeof(SoundFormat), readFormat);
        }

        bool ISoundLoader.TryParseSound(Stream stream, out ISoundFormat sound)
        {
            try
            {
                if (IsAud(stream))
                {
                    sound = new AudFormat(stream);
                    return true;
                }
            }
            catch
            {
                //Not a supported AUD
            }

            sound = null;
            return false;
        }

    }

    public sealed class AudFormat : ISoundFormat
    {
        public int Channels { get { return 1; } }
        public int SampleBits { get { return 16; } }
        public int SampleRate { get { return sampleRate; } }
        public float LengthInSeconds { get { return AudReader.SoundLength(sourceStream); } }
        public Stream GetPCMInputStream() { return audStreamFactory(); }
        public void Dispose() { sourceStream.Dispose(); }

        readonly Stream sourceStream;
        readonly Func<Stream> audStreamFactory;
        readonly int sampleRate;

        public AudFormat(Stream stream)
        {
            sourceStream = stream;

            if (!AudReader.LoadSound(stream, out audStreamFactory, out sampleRate))
                throw new InvalidDataException();
        }
    }
}