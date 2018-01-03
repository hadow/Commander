using System;
using System.IO;
using EW.FileSystem;
namespace EW.GameRules
{
    public class MusicInfo
    {
        public readonly string Filename;
        public readonly string Title;
        public readonly bool Hidden;


        public int Length { get; private set; } //seconds

        public bool Exists { get; private set; }
        public MusicInfo(string key,MiniYaml value)
        {
            Title = value.Value;

            var node = value.ToDictionary();
            if (node.ContainsKey("Hidden"))
                bool.TryParse(node["Hidden"].Value, out Hidden);

            var ext = node.ContainsKey("Extension") ? node["Extension"].Value : "aud";
            Filename = node.ContainsKey("Filename") ? node["Filename"].Value : key + "." + ext;
        }

        public void Load(IReadOnlyFileSystem fileSystem)
        {
            Stream stream;
            if(!fileSystem.TryOpen(Filename,out stream))
            {
                return;
            }

            try
            {
                Exists = true;
                foreach(var loader in WarGame.ModData.SoundLoaders)
                {
                    ISoundFormat soundFormat;
                    if(loader.TryParseSound(stream,out soundFormat))
                    {
                        Length = (int)soundFormat.LengthInSeconds;
                        soundFormat.Dispose();
                        break;
                    }
                }
            }
            finally
            {
                stream.Dispose();
            }

        }
    }
}