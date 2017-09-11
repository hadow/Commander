using System;
using System.IO;
using EW.Mods.Common.FileFormats;

namespace EW.Mods.Common.AudioLoaders
{
    public class WavLoader:ISoundLoader
    {
        bool IsWave(Stream s)
        {
            var start = s.Position;
            var type = s.ReadASCII(4);
            s.Position += 4;
            var format = s.ReadASCII(4);
            s.Position = start;

            return type == "RIFF" && format == "WAVE";
        }




        bool ISoundLoader.TryParseSound(Stream stream, out ISoundForamt sound)
        {
            try
            {
                if (IsWave(stream))
                {
                    sound = new WavFormat(stream);
                    return true;
                }
            }
            catch
            {

            }

            sound = null;
            return false;
        }



    }


    public sealed class WavFormat : ISoundForamt
    {
        Lazy<WavReader> reader;

        readonly Stream stream;

        public WavFormat(Stream stream)
        {
            this.stream = stream;

            var position = stream.Position;
            reader = Exts.Lazy(() =>
            {
                var wavReader = new WavReader();
                try
                {
                    if (!wavReader.LoadSound(stream))
                        throw new InvalidDataException();
                }
                finally
                {
                    stream.Position = position;
                }
                return wavReader;
            });
        }
    }
}